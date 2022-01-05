using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OBSNotifier.Plugins.Default
{
    public partial class DefaultNotificationWindow : Window
    {
        public DefaultNotification owner = null;
        bool isPreview = false;

        public DefaultNotificationWindow()
        {
            InitializeComponent();
        }

        public DefaultNotificationWindow(DefaultNotification plugin)
        {
            InitializeComponent();
            owner = plugin;
        }

        public void ShowNotif(string title, string desc)
        {
            isPreview = false;
            l_title.Content = title;
            l_desc.Content = desc;

            l_desc.Visibility = string.IsNullOrWhiteSpace((string)l_desc.Content) ? Visibility.Collapsed : Visibility.Visible;

            UpdateWindow();
            Show();
        }

        public void ShowPreview()
        {
            isPreview = true;
            UpdateWindow();
            Show();
        }

        void UpdateWindow()
        {
            // Position
            Point pos = new Point();
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), ((DefaultNotification.Positions)owner.PluginSettings.Position).ToString());
            pos = Utils.GetWindowPosition(anchor, new Point(Width, Height), owner.PluginSettings.Offset);

            Left = pos.X;
            Top = pos.Y;

            // Additional Params
            var lines = owner.PluginSettings.AdditionalData.Replace("\r", "").Split('\n');
            foreach (var line in lines)
            {
                var args = line.Split('=');
                if (args.Length == 2)
                {
                    switch (args[0].Trim())
                    {
                        case "BackgroundColor":
                            {
                                try
                                {
                                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(args[1].Trim()));
                                }
                                catch { }
                                break;
                            }
                        case "ForegroundColor":
                            {
                                try
                                {
                                    var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(args[1].Trim()));
                                    l_title.Foreground = b;
                                    l_desc.Foreground = b;
                                }
                                catch { }
                                break;
                            }
                    }
                }
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Hide();
        }
    }
}
