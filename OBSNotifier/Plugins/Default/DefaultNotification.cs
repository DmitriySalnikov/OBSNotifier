using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSNotifier.Plugins.Default
{
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
            enum Positions
            {
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight,
            }

            Timer close_timer = null;
            Action<string> logWriter = null;

            public string PluginName => "Default";

            public string PluginAuthor => "Dmitriy Salnikov";

            public string PluginDescription => "This is the default notification plugin";

            OBSNotifierPluginSettings _pluginSettings = new OBSNotifierPluginSettings()
            {
                AdditionalData = "BackgroundColor = #FFFFFF\nForegroundColor = #000000",
                Position = Positions.TopLeft,
                OffsetX = 0,
                OffsetY = 0,
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
              //  Close();
            }

            public bool ShowNotification(NotificationType type, string title, string description)
            {
                close_timer  = new Timer((ev) =>
                {
                    this.InvokeAction(() => Hide());
                }, null, 2000, Timeout.Infinite);

                return true;
            }

            public void ShowPreview()
            {
              //  Show();
            }

            public void HidePreview()
            {
              //  Hide();
            }

            public void ForceCloseWindow()
            {
                throw new NotImplementedException();
            }
        }
    }

}
