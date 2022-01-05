using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        Timer close_timer = null;
        Action<string> logWriter = null;
        DefaultNotificationWindow window = null;

        public string PluginName => "Default";

        public string PluginAuthor => "Dmitriy Salnikov";

        public string PluginDescription => "This is the default notification plugin";

        OBSNotifierPluginSettings _pluginSettings = new OBSNotifierPluginSettings()
        {
            AdditionalData = "BackgroundColor = #FFFFFF\nForegroundColor = #000000",
            Position = Positions.TopLeft,
            Offset = new Point(),
            OnScreenTime = 2000,
        };

        public OBSNotifierPluginSettings PluginSettings
        {
            get => _pluginSettings;
            set => _pluginSettings = value;
        }

        public Type EnumPositionType => typeof(Positions);

        public bool PluginInit(Action<string> logWriter)
        {
            this.logWriter = logWriter;
            return true;
        }

        public void PluginDispose()
        {
            window?.Close();
        }

        public bool ShowNotification(NotificationType type, string title, string description)
        {
            if (window == null)
                window = new DefaultNotificationWindow(this);

            window.ShowNotif(title, description);

            close_timer?.Dispose();
            close_timer = new Timer((ev) =>
            {
                var w = window;
                window?.InvokeAction(() => w?.Close());
                window = null;
            }, null, _pluginSettings.OnScreenTime, Timeout.Infinite);

            return true;
        }

        public void ShowPreview()
        {
            if (window == null)
                window = new DefaultNotificationWindow(this);

            window.ShowPreview();
        }

        public void HidePreview()
        {
            window?.Close();
            window = null;
        }

        public void ForceCloseWindow()
        {
            window?.Close();
        }
    }
}

