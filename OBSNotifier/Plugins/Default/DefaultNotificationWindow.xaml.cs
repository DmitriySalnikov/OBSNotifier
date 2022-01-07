using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OBSNotifier.Plugins.Default
{
    public partial class DefaultNotificationWindow : Window
    {
        public DefaultNotification owner = null;
        public bool IsPreviewNotif = false;

        public static readonly RoutedEvent FadeInOutEvent = EventManager.RegisterRoutedEvent("FadeInOut", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(DefaultNotificationWindow));
        public event RoutedEventHandler FadeInOut { add => AddHandler(FadeInOutEvent, value); remove => RemoveHandler(FadeInOutEvent, value); }

        DeferredAction hide;

        public DefaultNotificationWindow(DefaultNotification plugin)
        {
            InitializeComponent();

            hide = new DeferredAction(() => this.InvokeAction(() => Hide()), 100);
            owner = plugin;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        void UpdateWindow()
        {
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
                        case "Width":
                            {
                                if (int.TryParse(args[1].Trim(), out int val))
                                    Width = val;
                                break;
                            }
                        case "Height":
                            {
                                if (int.TryParse(args[1].Trim(), out int val))
                                    Height = val;
                                break;
                            }
                        case "Margin":
                            {
                                var split = args[1].Trim().Split(',');
                                if (split.Length == 1)
                                {
                                    if (double.TryParse(split[0], out double val))
                                        MainViewbox.Margin = new Thickness(val);
                                }
                                else if (split.Length == 4)
                                {
                                    if (double.TryParse(split[0], out double val1) &&
                                        double.TryParse(split[1], out double val2) &&
                                        double.TryParse(split[2], out double val3) &&
                                        double.TryParse(split[3], out double val4))
                                    {
                                        MainViewbox.Margin = new Thickness(val1, val2, val3, val4);
                                    }
                                }
                                break;
                            }
                    }
                }
            }

            // Position
            Point pos = new Point();
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), ((DefaultNotification.Positions)owner.PluginSettings.Option).ToString());
            pos = Utils.GetWindowPosition(anchor, new Point(Width, Height), owner.PluginSettings.Offset);

            Left = pos.X;
            Top = pos.Y;

        }

        public void ShowNotif(string title, string desc)
        {
            IsPreviewNotif = false;
            l_title.Text = title;
            l_desc.Text = desc;

            l_desc.Visibility = string.IsNullOrWhiteSpace(l_desc.Text) ? Visibility.Collapsed : Visibility.Visible;

            FadeAnimBoard.Storyboard.Stop(this);

            var d_anim = (FadeAnimBoard.Storyboard.Children[0] as DoubleAnimationUsingKeyFrames);
            var keys = d_anim.KeyFrames;
            var dur = TimeSpan.FromMilliseconds(owner.PluginSettings.OnScreenTime);
            var init_time = keys[1].KeyTime.TimeSpan; // Use second key as init time
            var fade = keys[2].KeyTime.TimeSpan; // Use third key as fade in/out time
            d_anim.Duration = init_time + fade + dur + fade + init_time;
            keys[3].KeyTime = init_time + fade + dur;
            keys[4].KeyTime = init_time + fade + dur + fade;

            FadeAnimBoard.Storyboard.Begin(this, true);

            UpdateWindow();
            Show();
        }

        public void ShowPreview()
        {
            IsPreviewNotif = true;
            UpdateWindow();
            Show();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsPreviewNotif)
            {
                FadeAnimBoard.Storyboard.Seek(TimeSpan.Zero, TimeSeekOrigin.Duration);
                FadeAnimBoard.Storyboard.Stop(this);
                Opacity = 0;
                hide.CallDeferred();
            }
        }

        private void DoubleAnimation_Completed_out(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
