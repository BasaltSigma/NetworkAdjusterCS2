using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Game.SceneFlow;
using Colossal.Json;
using UnityEngine;
using System.Reflection;

namespace NetworkAdjusterCS2.Code
{
    internal static class AdjustmentManager
    {
        private static readonly string COUI_BASE_LOCATION = $"coui://{Mod.MOD_ICONS_ID}";

        private static bool installed;
        private static bool postInstalled;
        private static PrefabSystem m_prefabSytsem;
        private static World world;

        internal static void Install(UpdateSystem updateSystem)
        {
            var logHeader = $"[{nameof(AdjustmentManager)}.{nameof(Install)}]";
            if (installed)
            {
                Mod.log.Info($"{logHeader} Mod is installed, skipping");
                return;
            }
            Mod.log.Info($"{logHeader} Installing Mod");

            world = updateSystem.World;
            if (world == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving world instance, exiting.");
                return;
            }

            m_prefabSytsem = world.GetExistingSystemManaged<PrefabSystem>();
            if (m_prefabSytsem == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving PrefabSystem instance, exiting.");
                return;
            }

            List<PrefabBase> prefabs = null;
            try
            {
                m_prefabSytsem.GetType().GetField("m_Prefabs").GetValue(prefabs);
            }
            catch (Exception exc)
            {
                Mod.log.Error($"{logHeader} Error thrown when getting prefabs: {exc.Message}");
                prefabs = null;
            }
            if (prefabs == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving prefab list from PrefabSystem, exiting.");
                return;
            }

            var grassUpgradePrefab = prefabs.FirstOrDefault(p => p.name.Equals("Grass"));
            if (grassUpgradePrefab == null)
            {
                Mod.log.Error($"{logHeader} Failed retrieving original Grass prefab instance, exiting.");
                return;
            }

            var grassUpgradePrefabUIObject = grassUpgradePrefab.GetComponent<UIObject>();
            if (grassUpgradePrefabUIObject == null)
            {
                Mod.log.Error($"{logHeader} Failed retreiving the grass prefab's UIObject instance, exiting.");
                return;
            }

            foreach (var adjusterMode in AdjusterUpgrades.Modes)
            {
                var clonedGrassUpgradePrefab = GameObject.Instantiate(grassUpgradePrefab);
                clonedGrassUpgradePrefab.name = adjusterMode.Id;
                clonedGrassUpgradePrefab.Remove<UIObject>();

                var clonedGrassUpgradeUIObject = ScriptableObject.CreateInstance<UIObject>();
                clonedGrassUpgradeUIObject.m_Icon = $"{COUI_BASE_LOCATION}/{adjusterMode.Id}.svg";
                clonedGrassUpgradeUIObject.name = grassUpgradePrefabUIObject.name.Replace("Grass", adjusterMode.Id);
                clonedGrassUpgradeUIObject.m_IsDebugObject = grassUpgradePrefabUIObject.m_IsDebugObject;
                clonedGrassUpgradeUIObject.m_Priority = grassUpgradePrefabUIObject.m_Priority;
                clonedGrassUpgradeUIObject.m_Group = grassUpgradePrefabUIObject.m_Group;
                clonedGrassUpgradeUIObject.active = grassUpgradePrefabUIObject.active;

                clonedGrassUpgradePrefab.AddComponentFrom(clonedGrassUpgradeUIObject);
                if (!m_prefabSytsem.AddPrefab(clonedGrassUpgradePrefab))
                {
                    Mod.log.Error($"{logHeader} [{adjusterMode.Id}] failed adding the cloned prefab to PrefabSystem, exiting.");
                    return;
                }
            }
            GameManager.instance.onGameLoadingComplete += OnGameLoadingComplete;
            installed = true;
            Mod.log.Info($"{logHeader} Installation completed successfully");
        }

        private static void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            var logHeader = $"[{nameof(AdjustmentManager)}.{nameof(OnGameLoadingComplete)}]";
            if (postInstalled)
            {
                Mod.log.Info($"{logHeader} Already executed before, skipping");
                return;
            }

            if (mode != Game.GameMode.Game && mode != Game.GameMode.Editor)
            {
                Mod.log.Info($"{logHeader} Game mode is {mode}, skipping");
                return;
            }

            List<PrefabBase> prefabs = null;
            try
            {
                m_prefabSytsem.GetType().GetField("m_Prefabs").GetValue(prefabs);
            }
            catch (Exception exc) 
            {
                Mod.log.Error($"{logHeader} Error thrown retreving prefabs list: {exc.Message}");
                prefabs = null;
            }
            if (prefabs == null)
            {
                Mod.log.Error($"{logHeader} Failed retreving Prefabs list, exiting.");
                return;
            }

            var grassUpgradePrefab = prefabs.FirstOrDefault(p => p.name.Equals("Grass"));
            if (grassUpgradePrefab == null)
            {
                Mod.log.Error($"{logHeader} Failed retreiving original grass prefab instance, exiting.");
                return;
            }

            var grassUpgradePrefabData = m_prefabSytsem.GetComponentData<PlaceableNetData>(grassUpgradePrefab);
            if (grassUpgradePrefabData.Equals(default(PlaceableNetData)))
            {
                Mod.log.Error($"{logHeader} Failed retreiving original prefab's PlaceableNetData instance, exiting.");
                return;
            }

            foreach (var adjustmentMode in AdjusterUpgrades.Modes)
            {
                var clonedGrassUpgradePrefab = prefabs.FirstOrDefault(p => p.name.Equals(adjustmentMode.Id));
                if (clonedGrassUpgradePrefab == null)
                {
                    Mod.log.Error($"{logHeader} [{adjustmentMode.Id}] Failed retrieving cloned grass prefab instance, exiting.");
                    return;
                }
                var clonedGrassUpgradePrefabData = m_prefabSytsem.GetComponentData<PlaceableNetData>(clonedGrassUpgradePrefab);
                if (clonedGrassUpgradePrefabData.Equals(default(PlaceableNetData)))
                {
                    Mod.log.Error($"{logHeader} [{adjustmentMode.Id}] Failed retreving cloned prefab's PlaceableNetDat, exiting.");
                    return;
                }
                clonedGrassUpgradePrefabData.m_SetUpgradeFlags = adjustmentMode.m_SetUpgradeFlags;
                m_prefabSytsem.AddComponentData(clonedGrassUpgradePrefab, clonedGrassUpgradePrefabData);
            }
            postInstalled = true;
            Mod.log.Info($"{logHeader} Post installation completed.");
        }
    }
}
