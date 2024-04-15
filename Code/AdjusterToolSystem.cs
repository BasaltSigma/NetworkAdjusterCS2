using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colossal;
using Colossal.Json;
using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst.Intrinsics;

namespace NetworkAdjusterCS2.Code
{
    internal partial class AdjusterToolSystem : GameSystemBase
    {
        private ToolOutputBarrier m_toolOutputBarrier;
        private ToolSystem m_toolSystem;
        private EntityQuery m_UpdatedQuery;

        private bool canUpdate;
        private PrefabBase currentNetPrefab;
        private ToolBaseSystem currentTool;
        private PrefabBase CurrentNetPrefab
        {
            set
            {
                currentNetPrefab = value;
                UpdateCanUpdate();
            }
        }

        private ToolBaseSystem CurrentNetTool
        {
            set
            {
                currentTool = value;
                UpdateCanUpdate();
            }
        }

        private void UpdateCanUpdate()
        {
            canUpdate = currentTool is NetToolSystem && AdjusterUpgrades.Modes.Any(m => m.Id.Equals(currentNetPrefab?.name));
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_toolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_UpdatedQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Upgraded>(),
                        ComponentType.ReadOnly<Edge>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadWrite<Composition>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Hidden>(),
                    },
                },
            });
            m_UpdatedQuery.AddOrderVersionFilter();
            m_UpdatedQuery.AddChangedVersionFilter(ComponentType.ReadOnly<Edge>());
            m_UpdatedQuery.AddChangedVersionFilter(ComponentType.ReadOnly<Temp>());
            RequireForUpdate(m_UpdatedQuery);

            m_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_toolSystem.EventToolChanged += (Action<ToolBaseSystem>)Delegate.Combine(m_toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            m_toolSystem.EventPrefabChanged += (Action<PrefabBase>)Delegate.Combine(m_toolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
        }

        protected override void OnUpdate()
        {
            if (!canUpdate || m_UpdatedQuery.IsEmpty)
            {
                return;
            }

            
        }

        protected override void OnDestroy()
        {
            m_toolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Remove(m_toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            m_toolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Remove(m_toolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
            base.OnDestroy();
        }

        private void OnPrefabChanged(PrefabBase _prefabBase)
        {
            CurrentNetPrefab = _prefabBase;
        }

        private void OnToolChanged(ToolBaseSystem _toolSystem)
        {
            CurrentNetTool = _toolSystem;
        }

        private struct AdjusterUpdateFixerJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_entityType;
            [ReadOnly] public ComponentTypeHandle<Edge> m_edgeType;
            [ReadOnly] public ComponentLookup<Edge> m_edgeData;
            [ReadOnly] public ComponentLookup<Composition> m_compositionData;
            [ReadOnly] public ComponentLookup<NetCompositionData> m_netCompositionData;
            public BufferLookup<ConnectedEdge> m_connectedEdges;
            public EntityCommandBuffer m_commandBuffer;
            
            public enum SideOption
            {
                Left,
                Right,
                Both,
                Any
            }

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var updatedEdges = chunk.GetNativeArray(ref m_edgeType);
                var updatedEdgesEntities = chunk.GetNativeArray(m_entityType);
                for (int i = 0; i < updatedEdges.Length; i++)
                {
                    Entity currentEdgeEntity = updatedEdgesEntities[i];
                    Edge currentEdge = updatedEdges[i];
                    if (m_compositionData.TryGetComponent(currentEdgeEntity, out var currentEdgeComposition) &&
                        m_netCompositionData.TryGetComponent(currentEdgeComposition.m_Edge, out var currentEdgeCompositionData) && 
                        m_netCompositionData.TryGetComponent(currentEdgeComposition.m_StartNode, out var currentEdgeStartNodeCompositionData) &&
                        m_netCompositionData.TryGetComponent(currentEdgeComposition.m_EndNode, out var currentEdgeEndNodeCompositionData))
                    {
                        ProcessConnectedEdges(currentEdge.m_Start, currentEdgeComposition.m_Edge, currentEdgeComposition.m_StartNode,
                            currentEdgeStartNodeCompositionData, currentEdgeCompositionData, true);
                        ProcessConnectedEdges(currentEdge.m_End, currentEdgeComposition.m_Edge, currentEdgeComposition.m_EndNode, 
                            currentEdgeEndNodeCompositionData, currentEdgeCompositionData, false);
                    }
                }
            }

            private IEnumerable<Entity> ConnectedEdges(Entity nodeEntity)
            {
                if (!m_connectedEdges.TryGetBuffer(nodeEntity, out var connectedEdgesBuffer) && !connectedEdgesBuffer.IsCreated)
                {
                    yield break;
                }
                for (int i = 0; i < connectedEdgesBuffer.Length; i++)
                {
                    yield return connectedEdgesBuffer[i].m_Edge;
                }
            }

            private bool HasFlags(NetCompositionData _netCompositionData, CompositionFlags.Side _sideFlag, SideOption _sideOption)
            {
                switch (_sideOption)
                {
                    case SideOption.Left:
                        return _netCompositionData.m_Flags.m_Left.HasFlag(_sideFlag);
                    case SideOption.Right:
                        return _netCompositionData.m_Flags.m_Right.HasFlag(_sideFlag);
                    case SideOption.Both:
                        return _netCompositionData.m_Flags.m_Left.HasFlag(_sideFlag) && _netCompositionData.m_Flags.m_Right.HasFlag(_sideFlag);
                    case SideOption.Any:
                        return _netCompositionData.m_Flags.m_Left.HasFlag(_sideFlag) || _netCompositionData.m_Flags.m_Right.HasFlag(_sideFlag);
                    default:
                        return false;
                }
            }

            private bool HasFlags(NetCompositionData _netCompositionData, CompositionFlags.General _generalFlags)
            {
                return _netCompositionData.m_Flags.m_General.HasFlag(_generalFlags);
            }

            private void ProcessConnectedEdges(Entity _currentEdgeNodeEntity, Entity _currentCompositionEdgeEntity, Entity _currentCompositionNodeEntity, 
                NetCompositionData _currentNodeNetCompositionData, NetCompositionData _currentEdgeNetCompositionData, bool _isStartNode)
            {
                
            }
        }
    }
}
