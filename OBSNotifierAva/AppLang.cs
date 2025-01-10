using Avalonia.Markup.Xaml.Styling;

namespace OBSNotifier
{
    // TODO mb switch to JSON

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // based on https://github.com/Epsil0neR/WPF-Localization/blob/master/WPFLocalizationCSharp/App.xaml.cs
        public static CultureInfo DefaultLanguage = new("en");

        private static List<CultureInfo> m_Languages = [
            new("en"),
            new("af"),
            new("ar"),
            new("ca"),
            new("cs"),
            new("da"),
            new("de"),
            new("el"),
            new("es-ES"),
            new("fi"),
            new("fr"),
            new("he"),
            new("hu"),
            new("it"),
            new("ja"),
            new("ko"),
            new("nl"),
            new("no"),
            new("pl"),
            new("pt-BR"),
            new("pt-PT"),
            new("ro"),
            new("ru"),
            new("sr"),
            new("sv-SE"),
            new("tr"),
            new("vi"),
            new("zh-CN"),
            new("zh-TW"),
        ];

        public static List<CultureInfo> Languages
        {
            get
            {
                return m_Languages;
            }
        }

        public static event EventHandler? LanguageChanged;
        static bool is_localization_initialized = false;

        public static CultureInfo Language
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Equals(Thread.CurrentThread.CurrentUICulture) && is_localization_initialized) return;
                if (!Languages.Contains(value))
                {
                    LogError($"'{value}' language is not found in this assembly!");
                    return;
                }

                is_localization_initialized = true;
                Thread.CurrentThread.CurrentUICulture = value;

                LanguageChanged?.Invoke(Current, new EventArgs());
            }
        }

        private static void ChangeLanguageResourceDictionaries(CultureInfo lang, string baseResourcePathNoExt)
        {
            return;

            if (Current == null) throw new NullReferenceException(nameof(Current));

            // The default language should always exist
            var defUri = new Uri($"avares://{baseResourcePathNoExt}.axaml");
            ResourceInclude dictDef = new(defUri)
            {
                Source = defUri,
            };

            ResourceInclude? dict;
            if (!lang.Equals(DefaultLanguage))
            {
                // Load localized
                try
                {
                    dict = new(new Uri($"avares://{baseResourcePathNoExt}.{lang.Name}.axaml"));
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

            var old_dict = Current.Resources.MergedDictionaries.OfType<ResourceInclude>().Where((i) => i.Source != null && i.Source.OriginalString.StartsWith($"avares://{baseResourcePathNoExt}.")).ToArray();

            // Remove if there are 2 of them
            if (old_dict.Length == 2)
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

        private void App_LanguageChanged(object? sender, EventArgs e)
        {
            Settings.Instance.Language = Language;
            Settings.Instance.Save();

            ReconstructTrayMenu();
        }


        struct TranslationProgressData(double wholeProgress, double approvedProgress)
        {
            public double WholeProgress = wholeProgress;
            public double ApprovedProgress = approvedProgress;
        }

        Dictionary<string, TranslationProgressData>? translationProgress;
        Dictionary<string, TranslationProgressData> TranslationProgress
        {
            get
            {
                if (translationProgress != null)
                    return translationProgress;

                string text = Tr.TrResources.LocalizationStatus;
                text = text.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("%", "");
                var lines = text.Split('\n').Select((l) => l.Trim());

                Dictionary<string, TranslationProgressData> status = [];

                var is_approved = false;
                foreach (string line in lines)
                {
                    if (line.StartsWith("translated", StringComparison.CurrentCultureIgnoreCase))
                    {
                        is_approved = false;
                        continue;
                    }
                    else if (line.StartsWith("approved", StringComparison.CurrentCultureIgnoreCase))
                    {
                        is_approved = true;
                        continue;
                    }
                    else if (line.StartsWith('-'))
                    {
                        var lang_split = line[1..].Split(':');
                        if (lang_split.Length == 2)
                        {
                            var lang = lang_split[0].Trim();
                            var lang_percent = int.Parse(lang_split[1].Trim());

                            if (!status.TryGetValue(lang, out TranslationProgressData value))
                            {
                                value = new TranslationProgressData(0, 0);
                                status.Add(lang, value);
                            }

                            if (is_approved)
                            {
                                value.ApprovedProgress = lang_percent;
                            }
                            else
                            {
                                value.WholeProgress = lang_percent;
                            }

                            status[lang] = value;
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
