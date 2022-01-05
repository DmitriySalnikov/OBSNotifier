using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace OBSNotifier
{
    class Settings
    {
        [JsonIgnore]
        static public Settings Instance { get; private set; } = null;

        [JsonIgnore]
        static string SaveFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "settings.json");

        public Point SettingsWindowSize { get; set; } = new Point(0, 0);
        public Point SettingsWindowPosition { get; set; } = new Point(-1, -1);
        public string ServerAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayID { get; set; } = string.Empty;
        // SEPARATE SETTINGS FOR ALL PLUGINS
        public string NotificationStyle { get; set; } = string.Empty;
        public string NotificationPosition { get; set; } = string.Empty;
        public PointF NotificationOffset { get; set; } = new PointF(0, 0);
        public string AdditionalData { get; set; } = null;

        #region Save/Load

        Settings()
        {
            if (Instance == null)
                Instance = this;
        }

        public void Save()
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
