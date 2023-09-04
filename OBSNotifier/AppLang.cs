using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        // based on https://github.com/Epsil0neR/WPF-Localization/blob/master/WPFLocalizationCSharp/App.xaml.cs
        public static CultureInfo DefaultLanguage = new CultureInfo("en");

        private static List<CultureInfo> m_Languages = new List<CultureInfo>() {
            new CultureInfo("en"),
            new CultureInfo("af"),
            new CultureInfo("ar"),
            new CultureInfo("ca"),
            new CultureInfo("cs"),
            new CultureInfo("da"),
            new CultureInfo("de"),
            new CultureInfo("el"),
            new CultureInfo("es-ES"),
            new CultureInfo("fi"),
            new CultureInfo("fr"),
            new CultureInfo("he"),
            new CultureInfo("hu"),
            new CultureInfo("it"),
            new CultureInfo("ja"),
            new CultureInfo("ko"),
            new CultureInfo("nl"),
            new CultureInfo("no"),
            new CultureInfo("pl"),
            new CultureInfo("pt-BR"),
            new CultureInfo("pt-PT"),
            new CultureInfo("ro"),
            new CultureInfo("ru"),
            new CultureInfo("sr"),
            new CultureInfo("sv-SE"),
            new CultureInfo("tr"),
            new CultureInfo("vi"),
            new CultureInfo("zh-CN"),
            new CultureInfo("zh-TW"),
        };

        public static List<CultureInfo> Languages
        {
            get
            {
                return m_Languages;
            }
        }

        public static event EventHandler LanguageChanged;
        static bool is_localization_initialized = false;

        public static CultureInfo Language
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value.Equals(Thread.CurrentThread.CurrentUICulture) && is_localization_initialized) return;
                if (!Languages.Contains(value))
                {
                    Log($"'{value}' language is not found in this assembly!");
                    return;
                }

                is_localization_initialized = true;
                Thread.CurrentThread.CurrentUICulture = value;

                ChangeLanguageResourceDictionaries(value, "Localization/lang");
                ChangeLanguageResourceDictionaries(value, "Localization/lang_modules");

                LanguageChanged?.Invoke(Current, new EventArgs());
            }
        }

        private static void ChangeLanguageResourceDictionaries(CultureInfo lang, string baseResourcePathNoExt)
        {
            // The default language should always exist
            ResourceDictionary dictDef = new ResourceDictionary();
            dictDef.Source = new Uri($"{baseResourcePathNoExt}.xaml", UriKind.Relative);

            ResourceDictionary dict = new ResourceDictionary();
            if (!lang.Equals(DefaultLanguage))
            {
                // Load localized
                try
                {
                    dict.Source = new Uri($"{baseResourcePathNoExt}.{lang.Name}.xaml", UriKind.Relative);
                }
                catch (IOException ex)
                {
                    Log(ex);
                    Thread.CurrentThread.CurrentUICulture = DefaultLanguage;
                    return;
                }
            }
            else
            {
                // Use default
                dict = dictDef;
            }

            // Find old ResourceDictionary, remove it and add new ResourceDictionary

            var old_dict = Current.Resources.MergedDictionaries.Where((i) => i.Source != null && i.Source.OriginalString.StartsWith($"{baseResourcePathNoExt}.")).ToArray();

            // Remove if there are 2 of them
            if (old_dict.Count() == 2)
            {
                // Remove the old localized language
                int idx = Current.Resources.MergedDictionaries.IndexOf(old_dict.Last());
                Current.Resources.MergedDictionaries.Remove(old_dict.Last());
                // Add a new localized language
                Current.Resources.MergedDictionaries.Insert(idx, dict);
            }
            else
            {
                // Otherwise rebuild everything from scratch
                foreach (var d in old_dict)
                {
                    Current.Resources.MergedDictionaries.Remove(d);
                }

                // Add default language
                Current.Resources.MergedDictionaries.Add(dictDef);

                // If necessary, add a localized language
                if (dictDef != dict)
                {
                    Current.Resources.MergedDictionaries.Add(dict);
                }
            }
        }

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            Settings.Instance.Language = Language;
            Settings.Instance.Save();

            ReconstructTrayMenu();
        }

        Dictionary<string, Size> translationProgress;
        Dictionary<string, Size> TranslationProgress
        {
            get
            {
                if (translationProgress != null)
                    return translationProgress;

                string text = OBSNotifier.Properties.Resources.localization_status;
                text = text.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("%", "");
                var lines = text.Split('\n').Select((l) => l.Trim());

                Dictionary<string, Size> status = new Dictionary<string, Size>();

                var is_approved = false;
                foreach (string line in lines)
                {
                    if (line.ToLower().StartsWith("translated"))
                    {
                        is_approved = false;
                        continue;
                    }
                    else if (line.ToLower().StartsWith("approved"))
                    {
                        is_approved = true;
                        continue;
                    }
                    else if (line.StartsWith("-"))
                    {
                        var lang_split = line.Substring(1).Split(':');
                        if (lang_split.Length == 2)
                        {
                            var lang = lang_split[0].Trim();
                            var lang_percent = int.Parse(lang_split[1].Trim());

                            if (!status.ContainsKey(lang))
                            {
                                status.Add(lang, new Size(0, 0));
                            }

                            var sz = status[lang];

                            if (is_approved)
                            {
                                sz.Height = lang_percent;
                            }
                            else
                            {
                                sz.Width = lang_percent;

                            }

                            status[lang] = sz;
                        }
                        continue;
                    }
                }

                translationProgress = status;
                return translationProgress;
            }
        }
    }
}
