using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace OBSNotifier
{
    class Settings
    {
        public class PluginSettings
        {
            public int OnScreenTime { get; set; } = 2000;
            public string SelectedOption { get; set; } = string.Empty;
            public PointF Offset { get; set; } = new PointF(0, 0);
            public string AdditionalData { get; set; } = null;
            public string CustomSettings { get; set; } = null;
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

        public Rectangle SettingsWindowRect { get; set; } = new Rectangle(-1, -1, 0, 0);
        public string ServerAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayID { get; set; } = string.Empty;
        public bool IsConnected { get; set; } = false;
        public bool UseSafeDisplayArea { get; set; } = true;
        public string NotificationStyle { get; set; } = string.Empty;

        public Dictionary<string, PluginSettings> PerPluginSettings { get; set; } = new Dictionary<string, PluginSettings>();
        
        [JsonIgnore]
        public PluginSettings CurrentPluginSettings
        {
            get
            {
                if(!PerPluginSettings.ContainsKey(NotificationStyle))
                    PerPluginSettings[NotificationStyle] = new PluginSettings();

                return PerPluginSettings[NotificationStyle];
            }
        }

        #region Save/Load

        Settings()
        {
            if (Instance == null)
                Instance = this;
        }

        public void Save()
        {
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
