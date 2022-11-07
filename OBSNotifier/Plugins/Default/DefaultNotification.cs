using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace OBSNotifier.Plugins.Default
{
    [Export(typeof(IOBSNotifierPlugin))]
    internal partial class DefaultNotification : IOBSNotifierPlugin
    {
        internal enum Positions
        {
            TopLeft, TopRight, BottomLeft, BottomRight,
            //Left, Right, Top, Bottom,
            Center
        }

        Action<string> logWriter = null;
        DefaultNotificationWindow window = null;

        public string PluginName => "Default";

        public string PluginAuthor => "Dmitriy Salnikov";

        public string PluginDescription => "This is the default notification plugin";

        public DefaultPluginSettings AvailableDefaultSettings => DefaultPluginSettings.AllNoCustomSettings;

        OBSNotifierPluginSettings _pluginSettings = new OBSNotifierPluginSettings()
        {
            AdditionalData = "BackgroundColor = #FF4C4C4C\nOutlineColor = #59000000\nTextColor = #FFD8D8D8\nBlocks = 3\nRadius = 4.0\nWidth = 180.0\nHeight = 52.0\nMargin = 4,4,4,4\nMaxPathChars = 32",
            Option = Positions.BottomRight,
            Offset = new Point(0, 0),
            OnScreenTime = 2700,
        };

        public OBSNotifierPluginSettings PluginSettings
        {
            get => _pluginSettings;
            set => _pluginSettings = value;
        }

        public Type EnumOptionsType => typeof(Positions);

        public NotificationType DefaultActiveNotifications => NotificationType.All;

        public bool PluginInit(Action<string> logWriter)
        {
            this.logWriter = logWriter;
            return true;
        }

        public void PluginDispose()
        {
            window?.Close();
            window = null;
        }

        public bool ShowNotification(NotificationType type, string title, string description = null, object[] originalData = null)
        {
            if (window == null)
            {
                window = new DefaultNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowNotif(type, title, description);
            return true;
        }

        public void ShowPreview()
        {
            if (window == null)
            {
                window = new DefaultNotificationWindow(this);
                window.Closing += Window_Closing;
            }

            window.ShowPreview();
        }

        public void HidePreview()
        {
            window?.HidePreview();
        }

        public void ForceCloseAllRelativeToPlugin()
        {
            if (window != null)
            {
                window.Closing -= Window_Closing;
                window.Close();
            }
            window = null;
        }

        public void OpenCustomSettings() { }

        public string GetCustomSettingsDataToSave() => null;

        public string GetFixedAdditionalData()
        {
            return Utils.ConfigFixString<DefaultCustomNotifBlockSettings>(_pluginSettings.AdditionalData);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (sender as DefaultNotificationWindow).Closing -= Window_Closing;

            if (window == sender)
                window = null;
        }

        public void Log(string txt)
        {
            logWriter.Invoke(txt);
        }

        public void Log(Exception ex)
        {
            Log($"Exception:\n{ex.Message}\nStackTrace:\n{ex.StackTrace}");
        }
    }
}

