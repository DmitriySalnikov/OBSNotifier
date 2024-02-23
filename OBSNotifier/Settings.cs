using System.Text.Json;
using System.Text.Json.Serialization;
using OBSNotifier.Modules.Event;
using System.Text.Json.Serialization.Metadata;
using System.Windows;

namespace OBSNotifier
{
    partial class Settings
    {
        [JsonIgnore]
        static public Settings Instance { get; private set; } = null!;

        [JsonIgnore]
        const string SAVE_FILE_NAME = "settings.json";
        [JsonIgnore]
        static readonly string SaveFile = Path.Combine(App.AppDataFolder, SAVE_FILE_NAME);
        [JsonIgnore]
        static readonly string SaveFileBackup = SaveFile + ".backup";

        [JsonIgnore]
        readonly DeferredActionWPF saveSettings = new(() => Instance.SaveInternal(), 1000, App.Current);
        [JsonIgnore]
        public bool IsPreviewShowing = false;

        public CultureInfo? Language { get; set; } = null;
        public bool FirstRun { get; set; } = true;
        public string SkipVersion { get; set; } = "";

        public Rect SettingsWindowRect { get; set; } = new(-1, -1, 0, 0);
        public string ServerAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayID { get; set; } = string.Empty;
        public bool IsCloseOnOBSClosing { get; set; } = false;
        public bool IsManuallyConnected { get; set; } = false;
        public string NotificationModule { get; set; } = string.Empty;

        [JsonPropertyOrder(100)]
        public Dictionary<string, OBSModuleSettings> PerModuleSettings
        {
            get
            {
                return App.Modules.LoadedModules.Select(m => (m.instance.ModuleID, m.instance.Settings)).ToDictionary(k => k.ModuleID, v => v.Settings);
            }
            set
            {
                foreach (var s in value)
                {
                    bool found = false;
                    foreach (var module in App.Modules.LoadedModules)
                    {
                        if (module.instance.ModuleID == s.Key)
                        {
                            found = true;
                            module.instance.Settings = s.Value;
                            break;
                        }
                    }
                    if (!found)
                    {
                        App.Log($"{s.Key} settings will not be used and will be erased from the settings file.");
                    }
                }
            }
        }

        #region Temp Update Flags

        //[JsonProperty(Order = 129)]
        //public string NotificationStyle { get; set; } = null;

        #endregion

        #region Save/Load

        static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = new ModuleSettingsJsonTypeInfoResolver(),
            Converters = {
                new FloatJsonConverter(),
                new DoubleJsonConverter(),

                new CultureInfoJsonConverter(),

                new ThicknessJsonConverter(),
                new RectJsonConverter(),

                new PointJsonConverter(),
                new SizeJsonConverter(),

                new ColorJsonConverter(),
            }
        };

        //Settings()
        //{
        //    Instance ??= this;
        //}

        //~Settings()
        //{
        //    if (saveSettings.IsTimerActive())
        //        SaveInternal();

        //    saveSettings.Dispose();
        //}

        public bool ClearUnusedModuleSettings()
        {
            List<string> to_delete = [];
            foreach (var p in PerModuleSettings)
            {
                if (App.Modules.LoadedModules.FindIndex((i) => i.instance.ModuleID == p.Key) == -1)
                {
                    to_delete.Add(p.Key);
                }
            }

            foreach (var p in to_delete)
            {
                PerModuleSettings.Remove(p);
            }

            return to_delete.Count > 0;
        }

        public void Save(bool forceSave = false)
        {
            if (forceSave)
                SaveInternal();
            else
                saveSettings.CallDeferred();
        }

        void SaveInternal()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(SaveFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(SaveFile) ?? string.Empty);

                try
                {
                    // Remove old backup
                    if (File.Exists(SaveFile) && File.Exists(SaveFileBackup))
                    {
                        File.Delete(SaveFileBackup);
                    }

                    // Create new backup
                    if (File.Exists(SaveFile))
                    {
                        File.Move(SaveFile, SaveFileBackup);
                    }
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                }

                File.WriteAllText(SaveFile, JsonSerializer.Serialize(this, jsonOptions));
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }
        }

        static public void Load()
        {
            static bool tryLoad(string file)
            {
                if (File.Exists(file))
                {
                    var fileText = File.ReadAllText(file);

                    if (string.IsNullOrWhiteSpace(fileText))
                    {
                        throw new FileLoadException($"Save file (\"{file}\") is empty");
                    }

                    Instance = JsonSerializer.Deserialize<Settings>(fileText, jsonOptions) ?? new Settings();
                    return true;
                }
                return false;
            }

            var is_tried_to_load_backup = false;
            try
            {
                // Attempt to load a regular file
                if (tryLoad(SaveFile))
                {
                    return;
                }
                else
                {
                    // Attempt to load a backup file
                    is_tried_to_load_backup = true;
                    if (tryLoad(SaveFileBackup))
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log(ex);

                // Attempt to load a backup file
                if (!is_tried_to_load_backup)
                {
                    try
                    {
                        if (tryLoad(SaveFileBackup))
                        {
                            return;
                        }
                    }
                    catch (Exception iex)
                    {
                        App.Log(iex);

                    }
                }
            }

            Instance = new Settings();
        }

        #endregion

        /// <summary>
        /// The simplest mechanism for bringing new flags to default values after an update.
        /// </summary>
        public void PatchSavedSettings()
        {
            /*
            if (IsScreenshotSavedJustAdded)
            {
                IsScreenshotSavedJustAdded = false;

                foreach (var pp in PerModuleSettings)
                {
                    if (pp.Value.ActiveNotificationTypes != null)
                    {
                        if (App.modules.LoadedModules.Any((p) => p.instance.ModuleID == pp.Key))
                        {
                            var module = App.modules.LoadedModules.First((p) => p.instance.ModuleID == pp.Key);
                            pp.Value.ActiveNotificationTypes |= module.instance.DefaultActiveNotifications & NotificationType.ScreenshotSaved;
                        }
                    }
                }

                Instance.Save();
            }
            */

            /*
            if (!string.IsNullOrEmpty(NotificationStyle))
            {
                NotificationModule = NotificationStyle;
                NotificationStyle = null;
            }
            */
        }

        // Dynamic loading of module settings, but only for loaded modules
        class ModuleSettingsJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
        {
            public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

                if (jsonTypeInfo.Type == typeof(OBSModuleSettings))
                {
                    var types = App.Modules.LoadedModules.Select(m => new JsonDerivedType(m.defaultSettings.GetType(), m.defaultSettings.GetType().Name)).ToList();

                    jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                    {
                        TypeDiscriminatorPropertyName = "$settings-type",
                        IgnoreUnrecognizedTypeDiscriminators = true,
                        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    };

                    foreach (var t in types)
                    {
                        jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(t);
                    }
                }

                return jsonTypeInfo;
            }
        }
    }
}
