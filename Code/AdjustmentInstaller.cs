using Game.Prefabs;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Game;
using Game.SceneFlow;
using UnityEngine;
using Colossal.Serialization.Entities;
using Colossal.Logging;
using Game.Tools;
using Colossal.UI;
using Game.UI;
using Colossal.UI.Binding;

namespace NetworkAdjusterCS2.Code
{
    internal static class AdjustmentInstaller
    {
        private static readonly string COUIBaseLocation = $"coui://{Mod.MOD_UI}";
        private static bool installed = false;
        private static bool postInstalled = false;

        private static World s_world;
        private static PrefabSystem s_prefabSystem;

        internal static void Install()
        {
            var logHeader = $"[{nameof(AdjustmentInstaller)}.{nameof(Install)}]";
            if (installed)
            {
                Mod.log.Info($"{logHeader} already installed, exiting");
                return;
            }

            s_world = Traverse.Create(GameManager.instance).Field<World>("m_World").Value;
            if (s_world == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving world, exiting");
                return;
            }

            s_prefabSystem = s_world.GetExistingSystemManaged<PrefabSystem>();
            if (s_prefabSystem == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving prefab system, exiting");
                return;
            }

            var prefabs = Traverse.Create(s_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            if (prefabs == null || !prefabs.Any())
            {
                Mod.log.Error($"{logHeader} Failed retrieving prefab list, exiting");
                return;
            }

            // clone off the grass upgrade prefab
            var grassUpgradePrefab = prefabs.FirstOrDefault(p => p.name.Equals("Grass"));
            if (grassUpgradePrefab == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving grass upgrade prefab, exiting");
                return;
            }

            var grassUpgradePrefabUIObject = grassUpgradePrefab.GetComponent<UIObject>();
            if (grassUpgradePrefabUIObject == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving grass upgrade UIObject component, exiting");
                return;
            }

            Mod.log.Info($"{logHeader} now injecting this mod's tools into the Road Upgrades toolbar");
            foreach (string toolId in AdjustmentToolData.toolIDs)
            {
                var clonedGrassPrefab = GameObject.Instantiate(grassUpgradePrefab);
                clonedGrassPrefab.name = toolId;
                clonedGrassPrefab.Remove<UIObject>();
                var clonedGrassPrefabUIObject = ScriptableObject.CreateInstance<UIObject>();
                clonedGrassPrefabUIObject.m_Icon = $"{COUIBaseLocation}/{toolId}.svg";
                clonedGrassPrefabUIObject.name = toolId;
                clonedGrassPrefabUIObject.m_IsDebugObject = grassUpgradePrefabUIObject.m_IsDebugObject;
                clonedGrassPrefabUIObject.m_Priority = grassUpgradePrefabUIObject.m_Priority;
                clonedGrassPrefabUIObject.m_Group = grassUpgradePrefabUIObject.m_Group;
                clonedGrassPrefabUIObject.active = grassUpgradePrefabUIObject.active;
                
                clonedGrassPrefab.AddComponentFrom(clonedGrassPrefabUIObject);
                if (!s_prefabSystem.AddPrefab(clonedGrassPrefab))
                {
                    return;
                }

            }
            GameManager.instance.onGameLoadingComplete += OnGameLoadingCompleted;
            installed = true;
            Mod.log.Info($"{logHeader} Successfully installed without any issues");
        }

        private static void OnGameLoadingCompleted(Purpose purpose, GameMode gameMode)
        {
            var logHeader = $"[{nameof(AdjustmentInstaller)}.{nameof(OnGameLoadingCompleted)}]";
            if (postInstalled)
            {
                Mod.log.Info($"{logHeader} Post install already completed, skipping");
                return;
            }

            var prefabs = Traverse.Create(s_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            if (prefabs == null || !prefabs.Any())
            {
                Mod.log.Error($"{logHeader} failed getting prefab base list, exiting.");
            }

            foreach (var modeID in AdjustmentToolData.toolIDs)
            {
                var clonedPrefab = prefabs.FirstOrDefault(pfb => pfb.name.Equals(modeID));
                if (clonedPrefab == null)
                {
                    Mod.log.Error($"{logHeader} Failed retreving our own prefab of id {modeID}, exiting");
                    return;
                }
#if DEBUG
                List<string> prefabComponentNames = DebugUtils.GetAllComponentNamesOfPrefab(clonedPrefab);
                foreach (string s in prefabComponentNames)
                {
                    Mod.log.Info($"{logHeader} Found component of type {s} on {clonedPrefab.GetType().Name}");
                }
#endif

                // remove unneeded components and their data
                s_prefabSystem.RemoveComponent<PlaceableNetData>(clonedPrefab);
                s_prefabSystem.RemoveComponent<ServiceObjectData>(clonedPrefab);
                s_prefabSystem.RemoveComponent<NetObjectData>(clonedPrefab);
                clonedPrefab.Remove<ServiceObject>();
                clonedPrefab.Remove<PlaceableNet>();
                clonedPrefab.Remove<Unlockable>();
                clonedPrefab.Remove<NetUpgrade>();
                clonedPrefab.Remove<NetObject>();
            }
            postInstalled = true;
            Mod.log.Info($"{logHeader} Post install succesfully completed without any issues");
        }
    }

    /// <summary>
    /// Contains all the data on each tool provided
    /// </summary>
    internal static class AdjustmentToolData
    {
        public static string[] toolIDs = new string[] { "OverpassAdjustment", "UnderpassAdjustment" };
    }
}
