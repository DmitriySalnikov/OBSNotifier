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
            AdditionalData = new DefaultCustomNotifBlockSettings(),
            Option = Positions.BottomRight,
            Offset = new Point(0, 0),
            OnScreenTime = 3000,
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

