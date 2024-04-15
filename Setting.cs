using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace NetworkAdjusterCS2
{
    [FileLocation(nameof(NetworkAdjusterCS2))]
    public class Setting : ModSetting
    {

        public Setting(IMod mod) : base(mod)
        {

        }

        public override void SetDefaults()
        {
            
        }
    }
    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                
            };
        }

        public void Unload()
        {
            m_Setting.UnregisterInOptionsUI();
        }
    }
}
