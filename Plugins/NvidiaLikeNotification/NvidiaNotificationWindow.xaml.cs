using NvidiaLikeNotification.Properties;
using OBSNotifier;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace NvidiaLikeNotification
{
    public partial class NvidiaNotificationWindow : Window
    {
        public NvidiaNotification owner = null;
        int addDataHash = -1;

        int MaxPathChars = 32;

        struct AnimationParams
        {
            public bool IsPreviewNotif;
            public uint Duration;
            public uint SlideDuration;
            public uint SlideOffset;
            public double LineWidth;
            public bool IsOnRightSide;
        }

        AnimationParams CurrentParams;
        AnimationParams PreviousParams;
        AnimationParams DefaultParams = new AnimationParams()
        {
            IsPreviewNotif = false,
            Duration = 2500,
            SlideDuration = 450,
            SlideOffset = 200,
            LineWidth = 6,
            IsOnRightSide = false,
        };

        bool IsPositionedOnTop { get => (NvidiaNotification.Positions)owner.PluginSettings.Option == NvidiaNotification.Positions.TopRight; }
        DeferredAction hide_delay;
        BeginStoryboard anim;

        public NvidiaNotificationWindow(NvidiaNotification plugin)
        {
            InitializeComponent();

            anim = (Resources["nvidia_anim"] as BeginStoryboard);
            CurrentParams = DefaultParams;

            hide_delay = new DeferredAction(() => Hide(), 200, this);
            owner = plugin;
            i_icon.SizeChanged += I_icon_SizeChanged;
        }

        protected override void OnClosed(EventArgs e)
        {
            owner = null;
            hide_delay.Dispose();
            hide_delay = null;

            base.OnClosed(e);
        }

        void UpdateParameters()
        {
            // Additional Params
            if (owner.PluginSettings.AdditionalData.GetHashCode() != addDataHash)
            {
                addDataHash = owner.PluginSettings.AdditionalData.GetHashCode();

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
                                        g_back.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(args[1].Trim()));
                                    }
                                    catch
                                    {
                                        g_back.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E48BD"));
                                    }
                                    break;
                                }
                            case "ForegroundColor":
                                {
                                    try
                                    {
                                        g_front.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(args[1].Trim()));
                                    }
                                    catch
                                    {
                                        g_front.Background = new SolidColorBrush(Colors.Black);
                                    }
                                    break;
                                }
                            case "TextColor":
                                {
                                    try
                                    {
                                        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(args[1].Trim()));
                                        l_title.Foreground = b;
                                        l_desc.Foreground = b;
                                    }
                                    catch
                                    {
                                        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4E4E4"));
                                        l_title.Foreground = b;
                                        l_desc.Foreground = b;
                                    }
                                    break;
                                }
                            case "Scale":
                                {
                                    if (double.TryParse(args[1].Trim(), out double val))
                                    {
                                        Width = 300 * val;
                                        Height = 90 * val;
                                    }
                                    else
                                    {
                                        Width = 300;
                                        Height = 90;
                                    }
                                    break;
                                }
                            case "SlideDuration":
                                {
                                    if (uint.TryParse(args[1].Trim(), out uint val))
                                        CurrentParams.SlideDuration = val;
                                    else
                                        CurrentParams.SlideDuration = 400;
                                    break;
                                }
                            case "SlideOffset":
                                {
                                    if (uint.TryParse(args[1].Trim(), out uint val))
                                        CurrentParams.SlideOffset = val;
                                    else
                                        CurrentParams.SlideOffset= 180;
                                    break;
                                }
                            case "MaxPathChars":
                                {
                                    if (int.TryParse(args[1].Trim(), out int val))
                                        MaxPathChars = val;
                                    else
                                        MaxPathChars = 32;
                                    break;
                                }
                            case "IconHeight":
                                {
                                    if (double.TryParse(args[1].Trim(), out double val))
                                        i_icon.Height = val;
                                    else
                                        i_icon.Height = 64;
                                    break;
                                }
                            case "IconPath":
                                {
                                    BitmapImage logo = new BitmapImage();
                                    try
                                    {
                                        try
                                        {
                                            logo.BeginInit();
                                            logo.UriSource = new Uri(args[1].Trim());
                                            logo.EndInit();
                                        }
                                        catch
                                        {
                                            logo = new BitmapImage();
                                            logo.BeginInit();
                                            logo.UriSource = new Uri(System.IO.Path.GetFullPath(args[1].Trim()));
                                            logo.EndInit();
                                        }
                                    }
                                    catch
                                    {
                                        logo = new BitmapImage();
                                        logo.BeginInit();
                                        logo.UriSource = new Uri("pack://application:,,,/NvidiaLikeNotification;component/Resources/obs.png");
                                        logo.EndInit();
                                    }
                                    i_icon.Source = logo;
                                    break;
                                }
                        }
                    }
                }
            }

            CurrentParams.Duration = owner.PluginSettings.OnScreenTime;
            CurrentParams.IsOnRightSide = (NvidiaNotification.Positions)owner.PluginSettings.Option == NvidiaNotification.Positions.TopRight;

            i_icon.HorizontalAlignment = CurrentParams.IsOnRightSide ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            i_icon.Margin = new Thickness(CurrentParams.IsOnRightSide ? 8 : 0, 0, CurrentParams.IsOnRightSide ? 0 : 8, 0);
            I_icon_SizeChanged(null, null);

            // Position
            var pe = (NvidiaNotification.Positions)owner.PluginSettings.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetWindowPosition(anchor, new Size(Width, Height), owner.PluginSettings.Offset);

            Left = pos.X;
            Top = pos.Y;
        }

        private void I_icon_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sp_text.Margin = new Thickness(CurrentParams.IsOnRightSide ? i_icon.ActualWidth + 14 : 6, 0, CurrentParams.IsOnRightSide ? 6 : i_icon.ActualWidth + 14, 0);
        }

        void UpdateAnimationParameters()
        {
            if (CurrentParams.Equals(PreviousParams))
                return;
            PreviousParams = CurrentParams;

            var timeline = (anim.Storyboard.Children[0] as ParallelTimeline);
            var anim_front = (timeline.Children[0] as ThicknessAnimationUsingKeyFrames);
            var anim_back = (timeline.Children[1]as ThicknessAnimationUsingKeyFrames);
            var keys_front = anim_front.KeyFrames;
            var keys_back = anim_back.KeyFrames;

            var dur = TimeSpan.FromMilliseconds(CurrentParams.Duration);
            var end_time = TimeSpan.FromMilliseconds(100);
            var slide = TimeSpan.FromMilliseconds(CurrentParams.SlideDuration);
            var offset = TimeSpan.FromMilliseconds(CurrentParams.SlideOffset);

            var visible = new Thickness(0, 0, 0, 0);
            var visible_front = new Thickness(CurrentParams.IsOnRightSide ? CurrentParams.LineWidth : 0, 0, CurrentParams.IsOnRightSide ? 0 : CurrentParams.LineWidth, 0);
            var hidden = new Thickness(CurrentParams.IsOnRightSide ? g_back.Width : -g_back.Width, 0, CurrentParams.IsOnRightSide ? -g_back.Width : g_back.Width, 0);

            anim_back.Duration = slide + dur + offset + slide + offset + end_time;
            timeline.Duration = CurrentParams.IsPreviewNotif ? TimeSpan.Zero : anim_back.Duration;

            if (CurrentParams.IsPreviewNotif)
            {
                keys_back[0].Value = visible;
                keys_front[0].Value = visible_front;
            }
            else
            {
                keys_back[0].Value = hidden;
                keys_back[1].Value = visible;
                keys_back[2].Value = visible;
                keys_back[3].Value = hidden;
                keys_back[1].KeyTime = slide;
                keys_back[2].KeyTime = slide + dur + offset;
                keys_back[3].KeyTime = slide + dur + offset + slide;

                anim_front.Duration = offset + slide + dur + slide;
                keys_front[0].Value = hidden;
                keys_front[1].Value = hidden;
                keys_front[2].Value = visible_front;
                keys_front[3].Value = visible_front;
                keys_front[4].Value = hidden;
                keys_front[1].KeyTime = offset;
                keys_front[2].KeyTime = offset + slide;
                keys_front[3].KeyTime = slide + dur;
                keys_front[4].KeyTime = slide + dur + slide;
            }
        }

        void ShowWithLocationFix()
        {
            hide_delay.Cancel();
            Show();
            if (!IsPositionedOnTop)
            {
                var delta = Height - ActualHeight;
                if (delta > 0)
                    Top += delta;
            }
        }

        public void ShowNotif(NotificationType type, string title, string desc)
        {
            if (CurrentParams.IsPreviewNotif)
                return;
            UpdateParameters();

            l_title.Text = title;

            if (NotificationType.WithFilePaths.HasFlag(type))
                l_desc.Text = Utils.GetShortPath(desc, (uint)MaxPathChars);
            else
                l_desc.Text = desc;
            l_desc.Visibility = string.IsNullOrWhiteSpace(desc) ? Visibility.Collapsed : Visibility.Visible;

            anim.Storyboard.Stop(this);
            UpdateAnimationParameters();
            anim.Storyboard.Begin(this, true);

            ShowWithLocationFix();
        }

        public void ShowPreview()
        {
            CurrentParams.IsPreviewNotif = true;
            UpdateParameters();

            l_title.Text = "Preview Notification";
            l_desc.Text = "Some description";
            l_desc.Visibility = string.IsNullOrWhiteSpace(l_desc.Text) ? Visibility.Collapsed : Visibility.Visible;

            // update animation and force it to change values
            UpdateAnimationParameters();
            anim.Storyboard.Begin(this, true);

            ShowWithLocationFix();
        }

        public void HidePreview()
        {
            CurrentParams.IsPreviewNotif = false;

            // update animation and force it to change values
            UpdateAnimationParameters();
            anim.Storyboard.Begin(this, true);
            anim.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
        }

        private void Animation_Finished(object sender, EventArgs e)
        {
            if (!CurrentParams.IsPreviewNotif)
                hide_delay.CallDeferred();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!CurrentParams.IsPreviewNotif)
                anim.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
            else
                HidePreview();
        }
    }
}
