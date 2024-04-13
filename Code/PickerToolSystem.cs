using Game.Net;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace NetworkAdjusterCS2
{
    internal class PickerToolSystem : ToolBaseSystem
    {
        private ToolBaseSystem m_previousActiveTool;

        public ToolBaseSystem previousActiveTool
        {
            get => m_previousActiveTool;
            private set { m_previousActiveTool = value; }
        }

        public override string toolID => "Picker Tool";

        [ReadOnly] ComponentLookup<Edge> edgeDataLookup;
        [ReadOnly] ComponentLookup<Composition> compositionDataLookup;
        [ReadOnly] ComponentLookup<NetCompositionData> netCompositionDataLookup;

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            edgeDataLookup = GetComponentLookup<Edge>(true);
            compositionDataLookup = GetComponentLookup<Composition>(true);
            netCompositionDataLookup = GetComponentLookup<NetCompositionData>(true);
        }

        [Preserve]
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return base.OnUpdate(inputDeps);
        }

        public override PrefabBase GetPrefab()
        {
            return default;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }
    }
}
