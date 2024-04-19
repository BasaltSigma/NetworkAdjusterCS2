using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI;
using Game;
using Game.Modding;
using Game.SceneFlow;
using System.Reflection;
using System.IO;
using NetworkAdjusterCS2.Code;
using Unity.Entities;
using HarmonyLib;
using System.Linq;

namespace NetworkAdjusterCS2
{
    /// <summary>
    /// NOTE: Mod API structure and understanding helped by the following mod repos:
    /// <list type="bullet">
    /// <item>https://github.com/ST-Apps/CS2-ExtendedRoadUpgrades/tree/master</item>
    /// <item>https://github.com/AlphaGaming7780/ExtraLib</item>
    /// <item>https://github.com/kosch104/CS2-ExtraNetworksAndAreas</item>
    /// <item>https://github.com/Quboid/CS2-MoveIt</item>
    /// </list>
    /// Thank you very much for making your code public on Github, it was very helpful to help me learn the API as no documentation exists
    /// as of writing this. 
    /// </summary>
    public sealed class Mod : IMod
    {
        public const string MOD_NAME = "Network Adjuster";
        public const string MOD_ICONS_ID = "nacs2";

        public static ILog log = LogManager.GetLogger($"{nameof(NetworkAdjusterCS2)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        internal Setting m_activeSettings { get; private set; }

        private Harmony harmony;
        internal static string ResourcesIcons { get; private set; }
        internal static Mod Instance { get; private set; }

        private string s_assemblyPath = null;
        public string AssemblyPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(s_assemblyPath))
                {
                    string assemblyName = Assembly.GetExecutingAssembly().FullName;
                    ExecutableAsset modAsset = AssetDatabase.global.GetAsset(SearchFilter<ExecutableAsset>.ByCondition(x => x.definition?.FullName.Equals(assemblyName) ?? false));
                    if (modAsset is null)
                    {
                        log.Error("Mod executable asset not found");
                        return null;
                    }
                    s_assemblyPath = Path.GetDirectoryName(modAsset.GetMeta().path);
                }
                return s_assemblyPath;
            }
        }

        /// <summary>
        /// Called by game when mod is loaded
        /// </summary>
        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            Instance = this;

            log.Info($"Loading {MOD_NAME} version {Assembly.GetExecutingAssembly().GetName().Version}");

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info($"Current mod asset at {asset.path}");
            }

            FileInfo fileInfo = new FileInfo(asset.path);
            ResourcesIcons = Path.Combine(fileInfo.DirectoryName, "Icons");

            UIManager.defaultUISystem.AddHostLocation("uil", AssemblyPath + "/Icons/");
            updateSystem.World.GetOrCreateSystem<AdjusterToolSystem>();
            updateSystem.UpdateAt<AdjusterToolSystem>(SystemUpdatePhase.ToolUpdate);

            AdjustmentInstaller.Install();

            harmony = new Harmony($"{nameof(NetworkAdjusterCS2)}.{nameof(Mod)}");
            harmony.PatchAll(typeof(Mod).Assembly);
            var patchedMethods = harmony.GetPatchedMethods().ToArray();
            foreach (var patchedMethod in patchedMethods)
            {
                log.Info($"Patched Method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
            }

            // create and configure settings for loading localization
            m_activeSettings = new Setting(this);
            Localization.LoadTranslations(m_activeSettings, log);
            UIManager.defaultUISystem.AddHostLocation(MOD_ICONS_ID, AssemblyPath + "/Icons/");
        }

        /// <summary>
        /// Called by the base game when the mod gets disposed of
        /// </summary>
        public void OnDispose()
        {
            log.Info($"{MOD_NAME}: {nameof(OnDispose)}");
            if (m_activeSettings != null)
            {
                m_activeSettings.UnregisterInOptionsUI();
                m_activeSettings = null;
            }
            harmony.UnpatchAll($"{nameof(NetworkAdjusterCS2)}.{nameof(Mod)}");
        }
    }
}
