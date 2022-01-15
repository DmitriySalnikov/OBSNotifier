using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace OBSNotifier
{
    class Settings
    {
        public class PluginSettings
        {
            public bool FirstLoad = true;
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
        static string SaveFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "settings.json");

        [JsonIgnore]
        DeferredAction saveSettings = new DeferredAction(() => Instance.SaveInternal(), 1000);
        [JsonIgnore]
        public bool IsPreviewShowing = false;

        public bool FirstRun { get; set; } = true;
        public string SkipVersion { get; set; } = "";

        public Rectangle SettingsWindowRect { get; set; } = new Rectangle(-1, -1, 0, 0);
        public string ServerAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayID { get; set; } = string.Empty;
        public bool IsCloseOnOBSClosing { get; set; } = false;
        public bool IsConnected { get; set; } = false;
        public bool UseSafeDisplayArea { get; set; } = true;
        public string NotificationStyle { get; set; } = string.Empty;

        public Dictionary<string, PluginSettings> PerPluginSettings { get; } = new Dictionary<string, PluginSettings>();

        [JsonIgnore]
        public PluginSettings CurrentPluginSettings
        {
            get
            {
                if (!PerPluginSettings.ContainsKey(App.plugins.CurrentPlugin.plugin.PluginName))
                    PerPluginSettings[App.plugins.CurrentPlugin.plugin.PluginName] = new PluginSettings();

                return PerPluginSettings[App.plugins.CurrentPlugin.plugin.PluginName];
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
            saveSettings.Dispose();
            saveSettings = null;
        }

        public bool ClearUnusedPluginSettings()
        {
            List<string> to_delete = new List<string>();
            foreach (var p in PerPluginSettings)
            {
                if (App.plugins.LoadedPlugins.FindIndex((i) => i.plugin.PluginName == p.Key) == -1)
                {
                    to_delete.Add(p.Key);
                }
            }

            foreach (var p in to_delete)
            {
                PerPluginSettings.Remove(p);
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
                File.WriteAllText(SaveFile, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public void Load()
        {
            try
            {
                if (File.Exists(SaveFile))
                    Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SaveFile));

                if (Instance == null)
                    Instance = new Settings();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
