using Colossal.Localization;
using Colossal.Logging;
using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdjusterCS2.Code
{
    /// <summary>
    /// Note this is based directly off https://github.com/ST-Apps/CS2-ExtendedRoadUpgrades/blob/feature/pdx-mods/Code/Localization.cs so I can figure out how to apply localizations
    /// </summary>
    public static class Localization
    {
        public static void LoadTranslations(Setting settings, ILog log)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = thisAssembly.GetManifestResourceNames();
            try
            {
                foreach (string localeID in GameManager.instance.localizationManager.GetSupportedLocales())
                {
                    string resourceName = $"{thisAssembly.GetName().Name}.l10n.{localeID}.csv";
                    if (resourceNames.Contains(resourceName))
                    {
                        try
                        {
                            log.Info($"Reading embedded translation file {resourceName}");
                            StreamReader reader = new StreamReader(thisAssembly.GetManifestResourceStream(resourceName));
                            Dictionary<string, string> translations = new Dictionary<string, string>();
                            StringBuilder builder = new StringBuilder();
                            string key = null;
                            bool parsingKey = true;
                            bool quoting = false;
                            while (true)
                            {
                                string line = reader.ReadLine();
                                if (line is null)
                                {
                                    break;
                                }
                                if (string.IsNullOrWhiteSpace(line) || line.Length == 0)
                                {
                                    continue;
                                }
                                for (int i = 0; i < line.Length; ++i)
                                {
                                    char thisChar = line[i];
                                    if (quoting)
                                    {
                                        if (thisChar == '"')
                                        {
                                            int j = i + 1;
                                            if (j < line.Length && line[j] == '"')
                                            {
                                                i = j;
                                                builder.Append('"');
                                                continue;
                                            }
                                            quoting = false;
                                            if (!parsingKey)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            builder.Append(thisChar);
                                        }
                                    }
                                    else
                                    {
                                        if (thisChar == '\t' || thisChar == ',')
                                        {
                                            if (!parsingKey)
                                            {
                                                break;
                                            }
                                            parsingKey = false;
                                            key = UnpackOptionsKey(builder.ToString(), settings);
                                            builder.Length = 0;
                                        }
                                        else if (thisChar == '"' && builder.Length == 0)
                                        {
                                            quoting = true;
                                        }
                                        else
                                        {
                                            builder.Append(thisChar);
                                        }
                                    }
                                }
                                if (quoting)
                                {
                                    builder.AppendLine();
                                    continue;
                                }
                                parsingKey = true;
                                if (string.IsNullOrWhiteSpace(key))
                                {
                                    log.Info($"Invalid key in line {line}");
                                    continue;
                                }
                                if (builder.Length == 0)
                                {
                                    log.Info($"No value field found in line {line}");
                                    continue;
                                }
                                string value = builder.ToString();
                                builder.Length = 0;
                                if (!translations.ContainsKey(key))
                                {
                                    translations.Add(key, value);
                                }
                            }
                            log.Info($"Adding translation for {localeID} with {translations.Count} entries");
                            GameManager.instance.localizationManager.AddSource(localeID, new MemorySource(translations));
                        }
                        catch (Exception e)
                        {
                            log.Error(e, $"Exception reading localization from embedded file {resourceName}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, $"Exception reading embedded settings localization file");
            }
        }

        private static string UnpackOptionsKey(string translationKey, Setting settings)
        {
            int divider = translationKey.IndexOf(':');
            if (divider < 0)
            {
                return translationKey;
            }
            string context = translationKey.Remove(divider);
            string key = translationKey.Substring(divider + 1);
            switch (context)
            {
                case "Options.OPTION":
                    return settings.GetOptionLabelLocaleID(key);
                case "Options.OPTION_DESCRIPTION":
                    return settings.GetOptionDescLocaleID(key);
                case "Options.WARNING":
                    return settings.GetOptionWarningLocaleID(key);
                default:
                    return settings.GetSettingsLocaleID();
            }
        }
    }
}
