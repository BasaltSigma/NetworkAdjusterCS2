using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace NetworkAdjusterCS2.Code
{
    internal partial class AdjusterToolSystem : ToolBaseSystem
    {
        public override string toolID => "networkadjustertool";

        private ProxyAction m_applyAction;
        private ProxyAction m_secondaryApplyAction;
        private NetPrefab m_prefab;
        
        public override PrefabBase GetPrefab()
        {
            return m_prefab;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            NetPrefab netPrefab = prefab as NetPrefab;
            if (netPrefab != null)
            {
                m_prefab = netPrefab;
                return true;
            }
            return false;
        }

        protected override void OnCreate()
        {
            m_applyAction = InputManager.instance.FindAction("Tool", "Apply");
            m_secondaryApplyAction = InputManager.instance.FindAction("Tool", "Secondary Apply");

            base.OnCreate();
            Enabled = false;
        }

        protected override void OnStartRunning()
        {
            m_applyAction.shouldBeEnabled = true;
            m_secondaryApplyAction.shouldBeEnabled = true;
        }

        protected override void OnStopRunning()
        {
            m_applyAction.shouldBeEnabled = false;
            m_secondaryApplyAction.shouldBeEnabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return base.OnUpdate(inputDeps);
        }

        #region Raycasting
        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.typeMask = TypeMask.Net;
            m_ToolRaycastSystem.netLayerMask = Layer.All;
            m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround;
            m_ToolRaycastSystem.raycastFlags |= RaycastFlags.SubElements;
            m_ToolRaycastSystem.typeMask |= TypeMask.MovingObjects;
            m_ToolRaycastSystem.raycastFlags |= RaycastFlags.Placeholders | RaycastFlags.Decals;
        }

        protected override bool GetRaycastResult(out ControlPoint controlPoint)
        {
            if (GetRaycastResult(out Entity entity, out RaycastHit hit))
            {
                if (EntityManager.HasChunkComponent<Node>(entity) && EntityManager.HasComponent<Edge>(hit.m_HitEntity))
                {
                    entity = hit.m_HitEntity;
                }
                controlPoint = new ControlPoint(entity, hit);
                return true;
            }
            controlPoint = default;
            return false;
        }

        protected override bool GetRaycastResult(out ControlPoint controlPoint, out bool forceUpdate)
        {
            if (GetRaycastResult(out var entity, out var hit, out forceUpdate))
            {
                if (EntityManager.HasComponent<Node>(entity) && EntityManager.HasComponent<Edge>(hit.m_HitEntity))
                {
                    entity = hit.m_HitEntity;
                }
                controlPoint = new ControlPoint(entity, hit);
                return true;
            }
            controlPoint = default;
            return false;
        }
        #endregion
    }
}
