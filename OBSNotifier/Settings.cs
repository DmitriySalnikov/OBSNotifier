using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OBSNotifier
{
    class Settings
    {
        public class ModuleSettings
        {
            public bool FirstLoad = true;
            public bool UseSafeDisplayArea { get; set; } = true;
            public uint OnScreenTime { get; set; } = 2000;
            public string SelectedOption { get; set; } = string.Empty;
            public System.Windows.Point Offset { get; set; } = new System.Windows.Point(0, 0);
            public string AdditionalData { get; set; } = string.Empty;
            public string CustomSettings { get; set; } = string.Empty;
            public NotificationType? ActiveNotificationTypes { get; set; } = null;
        }

        [JsonIgnore]
        static public Settings Instance { get; private set; } = null;

        [JsonIgnore]
        const string SAVE_FILE_NAME = "settings.json";
        [JsonIgnore]
        static string SaveFile = Path.Combine(App.AppDataFolder, SAVE_FILE_NAME);
        [JsonIgnore]
        static string SaveFileBackup = SaveFile + ".backup";
        [JsonIgnore]
        static string OldSaveFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), SAVE_FILE_NAME);

        [JsonIgnore]
        DeferredAction saveSettings = new DeferredAction(() => Instance.SaveInternal(), 1000);
        [JsonIgnore]
        public bool IsPreviewShowing = false;

        public CultureInfo Language { get; set; } = null;
        public bool FirstRun { get; set; } = true;
        public string SkipVersion { get; set; } = "";

        public Rectangle SettingsWindowRect { get; set; } = new Rectangle(-1, -1, 0, 0);
        public string ServerAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayID { get; set; } = string.Empty;
        public bool IsCloseOnOBSClosing { get; set; } = false;
        public bool IsManuallyConnected { get; set; } = false;
        public string NotificationModule { get; set; } = string.Empty;

        // Audio Alerts Settings
        public bool EnableAudioAlerts { get; set; } = true;
        public string AudioMode { get; set; } = "Simple";
        public int TTSRate { get; set; } = 0;
        public int TTSVolume { get; set; } = 100;
        public string TTSVoiceName { get; set; } = null;
        public List<string> CustomAudioFiles { get; set; } = new List<string>();
        public string SoundTypeForConnected { get; set; } = "Ring";
        public string SoundFileForConnected { get; set; } = null;
        public List<string> RandomPoolForConnected { get; set; } = new List<string>();
        public string SoundTypeForDisconnected { get; set; } = "Ring";
        public string SoundFileForDisconnected { get; set; } = null;
        public List<string> RandomPoolForDisconnected { get; set; } = new List<string>();
        public string SoundTypeForScreenshot { get; set; } = "Ring";
        public string SoundFileForScreenshot { get; set; } = "ring_sound/shutter1.mp3";
        public List<string> RandomPoolForScreenshot { get; set; } = new List<string>();
        public string SoundTypeForReplaySaved { get; set; } = "Ring";
        public string SoundFileForReplaySaved { get; set; } = "ring_sound/notification-4.mp3";
        public List<string> RandomPoolForReplaySaved { get; set; } = new List<string>();
        public string SoundTypeForRecordingStarted { get; set; } = "Ring";
        public string SoundFileForRecordingStarted { get; set; } = "ring_sound/pluck-on.mp3";
        public List<string> RandomPoolForRecordingStarted { get; set; } = new List<string>();
        public string SoundTypeForRecordingStopped { get; set; } = "Ring";
        public string SoundFileForRecordingStopped { get; set; } = "ring_sound/pluck-off.mp3";
        public List<string> RandomPoolForRecordingStopped { get; set; } = new List<string>();

        [JsonProperty("PerModuleSettings", Order = 100)]
        private Dictionary<string, ModuleSettings> perModuleSettings = new Dictionary<string, ModuleSettings>();
        [JsonIgnore]
        public Dictionary<string, ModuleSettings> PerModuleSettings { get => perModuleSettings; }

        #region Temp Update Flags

        [JsonProperty(Order = 128)]
        public bool IsScreenshotSavedJustAdded { get; set; } = true;
        [JsonProperty(Order = 129)]
        public string NotificationStyle { get; set; } = null;
        [JsonProperty(Order = 130)]
        public Dictionary<string, ModuleSettings> PerPluginSettings { get; set; } = null;

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
            if (Instance == null)
                Instance = this;
        }

        ~Settings()
        {
            if (saveSettings.IsTimerActive())
                SaveInternal();

            saveSettings.Dispose();
            saveSettings = null;
        }

        public bool ClearUnusedModuleSettings()
        {
            List<string> to_delete = new List<string>();
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
                Directory.CreateDirectory(Path.GetDirectoryName(SaveFile));

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

        static public void Load()
        {
            Func<string, bool> tryLoad = (file) =>
            {
                if (File.Exists(file))
                {
                    var fileText = File.ReadAllText(file);

                    if (string.IsNullOrWhiteSpace(fileText))
                    {
                        throw new FileLoadException($"Save file (\"{file}\") is empty");
                    }

                    Instance = JsonConvert.DeserializeObject<Settings>(fileText);
                    if (Instance == null)
                        Instance = new Settings();

                    return true;
                }
                return false;
            };

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

            // The logic of updating from locally saved settings in the application folder to AppData
            try
            {
                if (File.Exists(OldSaveFile))
                {
                    // Load old settings
                    Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(OldSaveFile));
                    // Save to old file with new comments
                    File.WriteAllText(OldSaveFile, "// This file will no longer be used to store settings.\n// The current settings file is located in %Appdata%/OBSNotifier/\n\n" + JsonConvert.SerializeObject(Instance, Formatting.Indented));
                    // Create new file in appdata
                    Instance.SaveInternal();
                    return;
                }
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }

            Instance = new Settings();
        }

        #endregion

        /// <summary>
        /// The simplest mechanism for bringing new flags to default values after an update.
        /// </summary>
        public void PatchSavedSettings()
        {
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

            if (!string.IsNullOrEmpty(NotificationStyle))
            {
                NotificationModule = NotificationStyle;
                NotificationStyle = null;
            }

            if (PerPluginSettings != null)
            {
                perModuleSettings = PerPluginSettings;
                PerPluginSettings = null;
            }
        }
    }
}
