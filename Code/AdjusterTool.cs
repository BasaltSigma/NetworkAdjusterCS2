using Game.Common;
using Game.Input;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace NetworkAdjusterCS2.Code
{
    public partial class AdjusterTool : ObjectToolBaseSystem
    {
        internal static AdjusterTool instance { get; private set; }
        internal AdjusterUISystem m_UISystem;

        private RaycastSystem raycastSystem;
        internal JobHandle m_inputDeps;
        internal EntityQuery m_tempQuery;

        internal ProxyAction m_primaryAction;
        internal ProxyAction m_secondaryAction;

        internal RaycastTerrain m_raycastTerrain;

        public override string toolID => "NetworkAdjusterTool";
        public override PrefabBase GetPrefab() => null;
        public override bool TrySetPrefab(PrefabBase prefab) => false;

        protected override void OnCreate()
        {
            base.OnCreate();
            instance = this;
            Enabled = false;

            m_UISystem = World.GetOrCreateSystemManaged<AdjusterUISystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();

            m_primaryAction = InputManager.instance.FindAction("Tool", "Apply");
            m_secondaryAction = InputManager.instance.FindAction("Tool", "Secondary Apply");
            m_tempQuery = new EntityQueryBuilder(WorldUpdateAllocator).WithAll<Temp, Transform>().WithNone<Owner>().Build(EntityManager);
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            m_primaryAction.shouldBeEnabled = true;
            m_secondaryAction.shouldBeEnabled = true;

        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            m_primaryAction.shouldBeEnabled = false;
            m_secondaryAction.shouldBeEnabled = false;
        }

        protected override void OnDestroy()
        {
            m_tempQuery.Dispose();
            base.OnDestroy();
        }

        public void RequestEnable()
        {
            if (m_ToolSystem.activeTool != this && m_ToolSystem.activeTool == m_DefaultToolSystem)
            {
                m_ToolSystem.selected = Entity.Null;
                m_ToolSystem.activeTool = this;
            }
        }

        public void RequestDisable()
        {
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.collisionMask = (CollisionMask.OnGround | CollisionMask.ExclusiveGround);
            m_ToolRaycastSystem.typeMask = TypeMask.Net;
            m_ToolRaycastSystem.raycastFlags = 0;
            m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.TrainTrack | Layer.TramTrack | Layer.SubwayTrack | Layer.Pathway;
            m_ToolRaycastSystem.iconLayerMask = Game.Notifications.IconLayerMask.None;

            m_raycastTerrain = new RaycastTerrain(World);
        }

        internal float GetTerrainHeight(float3 position)
        {
            m_TerrainSystem.AddCPUHeightReader(m_inputDeps);
            TerrainHeightData data = m_TerrainSystem.GetHeightData(false);
            return TerrainUtils.SampleHeight(ref data, position);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_inputDeps = base.OnUpdate(inputDeps);

            return m_inputDeps;
        }
    }
}
