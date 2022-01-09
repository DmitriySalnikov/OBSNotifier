using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace OBSNotifier.Plugins.Default
{
    [Export(typeof(IOBSNotifierPlugin))]
    public partial class DefaultNotification : IOBSNotifierPlugin
    {
        internal enum Positions
        {
            TopLeft, TopRight, BottomLeft, BottomRight,
            //Left, Right, Top, Bottom,
            Center
        }

        Action<string> logWriter = null;
        DefaultNotificationWindow window = null;
        DefaultNotificationWindow previewWindow = null;

        public string PluginName => "Default";

        public string PluginAuthor => "Dmitriy Salnikov";

        public string PluginDescription => "This is the default notification plugin";

        public DefaultPluginSettings AvailableDefaultSettings => DefaultPluginSettings.AllNoCustomSettings;

        OBSNotifierPluginSettings _pluginSettings = new OBSNotifierPluginSettings()
        {
            AdditionalData = "Blocks = 3\nBackgroundColor = #4C4C4C\nForegroundColor = #D8D8D8\nOutlineColor = #59000000\nRadius = 4.0\nWidth = 180\nHeight = 52\nMargin = 4,4,4,4",
            Option = Positions.TopLeft,
            Offset = new Point(),
            OnScreenTime = 2000,
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
            previewWindow?.Close();

            window = null;
            previewWindow = null;
        }

        public bool ShowNotification(NotificationType type, string title, string description = null, object[] originalData = null)
        {
            if (window == null)
                window = new DefaultNotificationWindow(this);

            window.Closing += Window_Closing;
            window.ShowNotif(title, description);

            return true;
        }

        public void ShowPreview()
        {
            if (previewWindow == null)
                previewWindow = new DefaultNotificationWindow(this);
            previewWindow.ShowPreview();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (sender as DefaultNotificationWindow).Closing -= Window_Closing;

            if (window == sender)
                window = null;
        }

        public void HidePreview()
        {
            previewWindow?.Close();
            previewWindow = null;
        }

        public void ForceCloseAllRelativeToPlugin()
        {
            if (window != null)
            {
                window.Closing -= Window_Closing;
                window.Close();
            }
            window = null;

            previewWindow?.Close();
            previewWindow = null;
        }

        public void OpenCustomSettings() { }

        public string GetCustomSettingsDataToSave() => null;
    }
}

