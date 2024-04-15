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

namespace NetworkAdjusterCS2
{
    /// <summary>
    /// NOTE: Mod API structure and understanding helped by https://github.com/ST-Apps/CS2-ExtendedRoadUpgrades/tree/master <br />
    /// Thank you very much for making your code public on Github, it was very helpful to help me learn the API as no documentation exists
    /// as of writing this. <br /><br />
    /// Main mod class for Network Adjuster
    /// </summary>
    public sealed class Mod : IMod
    {
        public const string MOD_NAME = "Network Adjuster";
        public const string MOD_ICONS_ID = "nacs2";

        private string s_assemblyPath = null;

        public static Mod Instance { get; private set; }

        public string m_assemblyPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(s_assemblyPath))
                {
                    string assemblyName = Assembly.GetExecutingAssembly().FullName;
                    ExecutableAsset modAsset = AssetDatabase.global.GetAsset(SearchFilter<ExecutableAsset>.ByCondition(x => x.definition?.FullName.Equals(assemblyName) ?? false));
                    if (modAsset is null)
                    {
                        log.Error("Mod executable asset was not found");
                        return null;
                    }
                    s_assemblyPath = Path.GetDirectoryName(modAsset.GetMeta().path);
                }
                return s_assemblyPath;
            }
        }

        public static ILog log = LogManager.GetLogger($"{nameof(NetworkAdjusterCS2)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        internal Setting m_activeSettings { get; private set; }

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

            // create and configure settings
            m_activeSettings = new Setting(this);
            m_activeSettings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_activeSettings));
            AssetDatabase.global.LoadSettings(nameof(NetworkAdjusterCS2), m_activeSettings, new Setting(this));
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
        }
    }
}
