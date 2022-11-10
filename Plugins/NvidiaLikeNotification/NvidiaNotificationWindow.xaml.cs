using OBSNotifier;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NvidiaLikeNotification
{
    public partial class NvidiaNotificationWindow : Window
    {
        const string default_icon_path = "pack://application:,,,/NvidiaLikeNotification;component/Resources/obs.png";
        const double default_window_width = 300;
        const double default_window_height = 90;

        NvidiaNotification owner = null;
        int addDataHash = -1;
        bool firstUpdate = true;

        CustomAnimationConfig currentParams;
        CustomAnimationConfig previousParams;
        readonly CustomAnimationConfig defaultParams = new CustomAnimationConfig();

        bool IsPositionedOnTop { get => (NvidiaNotification.Positions)owner.PluginSettings.Option == NvidiaNotification.Positions.TopRight; }
        DeferredAction hide_delay;
        BeginStoryboard anim;

        public NvidiaNotificationWindow(NvidiaNotification plugin)
        {
            InitializeComponent();

            anim = (Resources["nvidia_anim"] as BeginStoryboard);
            currentParams = new CustomAnimationConfig();

            hide_delay = new DeferredAction(() => Hide(), 200, this);
            owner = plugin;
            i_icon.SizeChanged += I_icon_SizeChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.RemoveWindowFromAltTab(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            owner = null;
            anim.Storyboard.Stop(this);
            hide_delay.Dispose();
            hide_delay = null;

            base.OnClosed(e);
        }

        void UpdateParameters()
        {
            // Additional Params
            if (owner.PluginSettings.AdditionalData != null && owner.PluginSettings.AdditionalData.GetHashCode() != addDataHash)
            {
                addDataHash = owner.PluginSettings.AdditionalData.GetHashCode();

                // Recreate but remember preview state
                bool prev = currentParams.IsPreviewNotif;
                currentParams = new CustomAnimationConfig()
                {
                    IsPreviewNotif = prev,
                };
                Utils.ConfigParseString(owner.PluginSettings.AdditionalData, ref currentParams);
            }

            // General params
            currentParams.Duration = owner.PluginSettings.OnScreenTime;
            currentParams.IsOnRightSide = (NvidiaNotification.Positions)owner.PluginSettings.Option == NvidiaNotification.Positions.TopRight;
            fileOpenOverlay.IsPreview = currentParams.IsPreviewNotif;

            // Preview max path
            if (currentParams.IsPreviewNotif)
            {
                var path = @"D:\Lorem\ipsum\dolor\sit\amet\consectetur\adipiscing\elit.\Donec\pharetra\lorem\turpis\nec\fringilla\leo\interdum\sit\amet.\Mauris\in\placerat\nulla\in\laoreet\Videos\OBS\01.01.01\Replay_01-01-01.mkv";
                if (!firstUpdate && (
                    (previousParams.MaxPathChars != currentParams.MaxPathChars) ||
                    (previousParams.ShowQuickActionsOnFileSave != currentParams.ShowQuickActionsOnFileSave)
                    ))
                {
                    fileOpenOverlay.FilePath = currentParams.ShowQuickActionsOnFileSave ? path : null;
                    l_desc.Text = Utils.GetShortPath(path, currentParams.MaxPathChars);
                }
                else
                {
                    fileOpenOverlay.FilePath = null;
                    l_desc.Text = "Some description";
                }

            }

            // Colors
            g_back.Background = currentParams.BackgroundColor;
            g_front.Background = currentParams.ForegroundColor;
            l_title.Foreground = currentParams.TextColor;
            l_desc.Foreground = currentParams.TextColor;

            // Sizes
            Width = default_window_width * currentParams.Scale;
            Height = default_window_height * currentParams.Scale;
            g_front.Width = Math.Max(1, default_window_width - currentParams.LineWidth);

            // Icon
            try
            {
                if (currentParams.IconPath != defaultParams.IconPath)
                    i_icon.Source = Utils.GetBitmapImage(currentParams.IconPath, GetType().Assembly);
                else
                    i_icon.Source = Utils.GetBitmapImage(default_icon_path);
            }
            catch
            {
                i_icon.Source = Utils.GetBitmapImage(default_icon_path);
            }

            i_icon.Height = currentParams.IconHeight;
            i_icon.HorizontalAlignment = currentParams.IsOnRightSide ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            if (currentParams.IconHeight == 0)
            {
                i_icon.Visibility = Visibility.Collapsed;
                i_icon.Margin = new Thickness(0);
            }
            else
            {
                i_icon.Visibility = Visibility.Visible;
                i_icon.Margin = new Thickness(currentParams.IsOnRightSide ? 8 : 0, 0, currentParams.IsOnRightSide ? 0 : 8, 0);
            }
            I_icon_SizeChanged(null, null);

            // Position
            var pe = (NvidiaNotification.Positions)owner.PluginSettings.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetWindowPosition(anchor, new Size(Width, Height), owner.PluginSettings.Offset);

            Left = pos.X;
            Top = pos.Y;

            firstUpdate = false;
        }

        private void I_icon_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (currentParams.IconHeight == 0)
                sp_text.Margin = new Thickness(6, 0, 6, 0);
            else
                sp_text.Margin = new Thickness(currentParams.IsOnRightSide ? i_icon.ActualWidth + 14 : 6, 0, currentParams.IsOnRightSide ? 6 : i_icon.ActualWidth + 14, 0);
        }

        bool UpdateAnimationParameters()
        {
            if (currentParams.IsAnimParamsEqual(previousParams))
                return false;

            var timeline = (anim.Storyboard.Children[0] as ParallelTimeline);
            var anim_front = (timeline.Children[0] as ThicknessAnimationUsingKeyFrames);
            var anim_back = (timeline.Children[1] as ThicknessAnimationUsingKeyFrames);
            var keys_front = anim_front.KeyFrames;
            var keys_back = anim_back.KeyFrames;

            var dur = TimeSpan.FromMilliseconds(currentParams.Duration);
            var end_time = TimeSpan.FromMilliseconds(100);
            var slide = TimeSpan.FromMilliseconds(currentParams.SlideDuration);
            var offset = TimeSpan.FromMilliseconds(currentParams.SlideOffset);

            var visible = new Thickness(0, 0, 0, 0);
            var visible_front = new Thickness(currentParams.IsOnRightSide ? currentParams.LineWidth : 0, 0, currentParams.IsOnRightSide ? 0 : currentParams.LineWidth, 0);
            var hidden = new Thickness(currentParams.IsOnRightSide ? g_back.Width : -g_back.Width, 0, currentParams.IsOnRightSide ? -g_back.Width : g_back.Width, 0);

            anim_back.Duration = slide + dur + offset + slide + end_time;
            keys_back[0].Value = hidden;
            keys_back[1].Value = visible;
            keys_back[2].Value = visible;
            keys_back[3].Value = hidden;
            keys_back[4].Value = hidden;
            keys_back[1].KeyTime = slide;
            keys_back[2].KeyTime = slide + dur + offset;
            keys_back[3].KeyTime = slide + dur + offset + slide;

            // for preview
            keys_back[4].KeyTime = anim_back.Duration.TimeSpan - TimeSpan.FromMilliseconds(2);
            keys_back[5].KeyTime = anim_back.Duration.TimeSpan - TimeSpan.FromMilliseconds(1);

            anim_front.Duration = anim_back.Duration;
            keys_front[0].Value = hidden;
            keys_front[1].Value = hidden;
            keys_front[2].Value = visible_front;
            keys_front[3].Value = visible_front;
            keys_front[4].Value = hidden;
            keys_front[5].Value = hidden;
            keys_front[1].KeyTime = offset;
            keys_front[2].KeyTime = offset + slide;
            keys_front[3].KeyTime = slide + dur;
            keys_front[4].KeyTime = slide + dur + slide;

            // for preview
            keys_front[5].KeyTime = anim_back.Duration.TimeSpan - TimeSpan.FromMilliseconds(2);
            keys_front[6].KeyTime = anim_back.Duration.TimeSpan - TimeSpan.FromMilliseconds(1);

            timeline.Duration = anim_back.Duration;

            // If preview mode is enabled, then at the end of the animation the window should be shown again.
            if (currentParams.IsPreviewNotif)
            {
                keys_back[5].Value = visible;
                keys_front[6].Value = visible_front;
            }
            else
            {
                keys_back[5].Value = hidden;
                keys_front[6].Value = hidden;
            }

            return true;
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
            if (currentParams.IsPreviewNotif)
                return;

            previousParams = currentParams.Duplicate();
            UpdateParameters();

            l_title.Text = title;

            if (NotificationType.WithFilePaths.HasFlag(type))
            {
                fileOpenOverlay.FilePath = currentParams.ShowQuickActionsOnFileSave ? desc : null;
                l_desc.Text = Utils.GetShortPath(desc, currentParams.MaxPathChars);
            }
            else
            {
                fileOpenOverlay.FilePath = null;
                l_desc.Text = desc;
            }
            l_desc.Visibility = string.IsNullOrWhiteSpace(desc) ? Visibility.Collapsed : Visibility.Visible;

            anim.Storyboard.Stop(this);
            UpdateAnimationParameters();
            anim.Storyboard.Begin(this, true);

            ShowWithLocationFix();
        }

        public void ShowPreview()
        {
            previousParams = currentParams.Duplicate();
            currentParams.IsPreviewNotif = true;
            UpdateParameters();

            l_title.Text = "Preview Notification";
            l_desc.Visibility = string.IsNullOrWhiteSpace(l_desc.Text) ? Visibility.Collapsed : Visibility.Visible;

            // update animation and force it to change values
            if (UpdateAnimationParameters())
                anim.Storyboard.Begin(this, true);

            ShowWithLocationFix();
        }

        public void HidePreview()
        {
            if (currentParams.IsPreviewNotif)
            {
                previousParams = currentParams.Duplicate();
                currentParams.IsPreviewNotif = false;

                // update animation and force it to change values
                UpdateAnimationParameters();
                anim.Storyboard.Begin(this, true);
                anim.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
            }
        }

        private void Animation_Finished(object sender, EventArgs e)
        {
            if (!currentParams.IsPreviewNotif)
                hide_delay.CallDeferred();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!currentParams.IsPreviewNotif)
                anim.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
        }
    }
}
