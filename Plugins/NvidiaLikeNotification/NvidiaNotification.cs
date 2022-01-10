using OBSNotifier;
using OBSNotifier.Plugins;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace NvidiaLikeNotification
{
    [Export(typeof(IOBSNotifierPlugin))]
    public partial class NvidiaNotification : IOBSNotifierPlugin
    {
        internal enum Positions
        {
            TopLeft, TopRight,
        }

        Action<string> logWriter = null;
        NvidiaNotificationWindow window = null;

        public string PluginName => "Nvidia-Like";

        public string PluginAuthor => "Dmitriy Salnikov";

        public string PluginDescription => "Simple notification similar to Nvidia's notifications";

        public DefaultPluginSettings AvailableDefaultSettings => DefaultPluginSettings.FadeDelay | DefaultPluginSettings.Options | DefaultPluginSettings.Offset | DefaultPluginSettings.AdditionalData;

        OBSNotifierPluginSettings _pluginSettings = new OBSNotifierPluginSettings()
        {
            AdditionalData = "BackgroundColor = #2E48BD\nForegroundColor = #000000\nTextColor = #E4E4E4\nWidth = 300\nHeight = 90\nSlideDuration = 200\nSlideOffset = 140\nColoredLineWidth = 6\nMaxPathChars = 32",
            Option = Positions.TopRight,
            Offset = new Point(0, 0.1),
            OnScreenTime = 2500,
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
                window = new NvidiaNotificationWindow(this);

            window.Closing += Window_Closing;
            window.ShowNotif(type, title, description);

            return true;
        }

        public void ShowPreview()
        {
            if (window == null)
                window = new NvidiaNotificationWindow(this);

            window.ShowPreview();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (sender as NvidiaNotificationWindow).Closing -= Window_Closing;

            if (window == sender)
                window = null;
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
    }
}

