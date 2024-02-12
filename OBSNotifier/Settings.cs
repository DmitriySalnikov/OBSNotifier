using Newtonsoft.Json;
using OBSNotifier.Modules.Event;
using System.Drawing;

namespace OBSNotifier
{
    class Settings
    {
        public class ModuleSettings
        {
            public bool FirstLoad = true;
            [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
            public OBSModuleSettings? Data { get; set; } = null;
        }

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

        public Rectangle SettingsWindowRect { get; set; } = new Rectangle(-1, -1, 0, 0);
        public string ServerAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayID { get; set; } = string.Empty;
        public bool IsCloseOnOBSClosing { get; set; } = false;
        public bool IsManuallyConnected { get; set; } = false;
        public string NotificationModule { get; set; } = string.Empty;

        [JsonProperty(nameof(PerModuleSettings), Order = 100)]
        private readonly Dictionary<string, ModuleSettings> perModuleSettings = [];
        [JsonIgnore]
        public Dictionary<string, ModuleSettings> PerModuleSettings { get => perModuleSettings; }

        #region Temp Update Flags

        //[JsonProperty(Order = 129)]
        //public string NotificationStyle { get; set; } = null;

        #endregion

        [JsonIgnore]
        public ModuleSettings CurrentModuleSettings
        {
            get
            {
                if (!PerModuleSettings.ContainsKey(App.modules.CurrentModule.instance.ModuleID))
                    PerModuleSettings[App.modules.CurrentModule.instance.ModuleID] = new ModuleSettings();

                return PerModuleSettings[App.modules.CurrentModule.instance.ModuleID];
            }
        }

        #region Save/Load

        Settings()
        {
            Instance ??= this;
        }

        ~Settings()
        {
            if (saveSettings.IsTimerActive())
                SaveInternal();

            saveSettings.Dispose();
        }

        public bool ClearUnusedModuleSettings()
        {
            List<string> to_delete = [];
            foreach (var p in PerModuleSettings)
            {
                if (App.modules.LoadedModules.FindIndex((i) => i.instance.ModuleID == p.Key) == -1)
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

                File.WriteAllText(SaveFile, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }
        }

        // TODO load modules data
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

                    Instance = JsonConvert.DeserializeObject<Settings>(fileText) ?? new Settings();
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
    }
}
