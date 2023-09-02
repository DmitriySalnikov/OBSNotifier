using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace OBSNotifier.Modules.NvidiaLike
{
    public partial class NvidiaNotificationWindow : Window
    {
        const string default_icon_path = "pack://application:,,,/Modules/Nvidia-like/obs.png";
        const double default_window_width = 300;
        const double default_window_height = 90;

        NvidiaNotification owner = null;
        int addDataHash = -1;

        NvidiaCustomAnimationConfig currentParams;
        NvidiaCustomAnimationConfig previousParams;
        readonly NvidiaCustomAnimationConfig defaultParams = new NvidiaCustomAnimationConfig();

        bool IsPositionedOnTop { get => (NvidiaNotification.Positions)owner.ModuleSettings.Option == NvidiaNotification.Positions.TopRight; }
        DeferredAction hide_delay;
        BeginStoryboard anim_nv;
        BeginStoryboard anim_f;

        bool[] animation_finished = new bool[] { true, true };

        public NvidiaNotificationWindow(NvidiaNotification module)
        {
            InitializeComponent();

            anim_nv = (Resources["nvidia_anim"] as BeginStoryboard);
            anim_f = (Resources["fileOpen_anim"] as BeginStoryboard);
            currentParams = new NvidiaCustomAnimationConfig();

            hide_delay = new DeferredAction(() => Hide(), 200, this);
            owner = module;
            i_icon.SizeChanged += I_icon_SizeChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utils.RemoveWindowFromAltTab(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            owner = null;
            StopAnimNV();
            StopAnimFile();
            hide_delay.Dispose();
            hide_delay = null;

            base.OnClosed(e);
        }

        void PlayAnimNV()
        {
            anim_nv.Storyboard.Begin(this, true);
            animation_finished[0] = false;
        }

        void PlayAnimFile()
        {
            anim_f.Storyboard.Begin(this, true);
            animation_finished[1] = false;
        }

        void StopAnimNV()
        {
            anim_nv.Storyboard.Stop(this);
            animation_finished[0] = true;
        }

        void StopAnimFile()
        {
            anim_f.Storyboard.Stop(this);
            animation_finished[1] = true;
        }

        void UpdateParameters()
        {
            // Additional Params
            if (owner.ModuleSettings.AdditionalData != null && owner.ModuleSettings.AdditionalData.GetHashCode() != addDataHash)
            {
                addDataHash = owner.ModuleSettings.AdditionalData.GetHashCode();

                // Recreate but remember preview state
                bool prev = currentParams.IsPreviewNotif;
                currentParams = new NvidiaCustomAnimationConfig()
                {
                    IsPreviewNotif = prev,
                };
                Utils.ConfigParseString(owner.ModuleSettings.AdditionalData, ref currentParams);
            }

            // General params
            currentParams.Duration = owner.ModuleSettings.OnScreenTime;
            currentParams.IsOnRightSide = (NvidiaNotification.Positions)owner.ModuleSettings.Option == NvidiaNotification.Positions.TopRight;

            fileOpenOverlay.IsPreview = currentParams.IsPreviewNotif;
            fileOpenOverlay.HorizontalAlignment = currentParams.IsOnRightSide ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            fileOpen_colored_line.Visibility = currentParams.ShowQuickActionsColoredLine ? Visibility.Visible : Visibility.Hidden;

            // Preview max path
            if (currentParams.IsPreviewNotif)
            {
                var path = @"D:\Lorem\ipsum\dolor\sit\amet\consectetur\adipiscing\elit.\Donec\pharetra\lorem\turpis\nec\fringilla\leo\interdum\sit\amet.\Mauris\in\placerat\nulla\in\laoreet\Videos\OBS\01.01.01\Replay_01-01-01.mkv";

                if (currentParams.ShowQuickActions)
                {
                    g_fileOpen.Visibility = Visibility.Visible;
                    l_desc.Text = Utils.GetShortPath(path, currentParams.MaxPathChars);
                    fileOpenOverlay.FilePath = path;
                }
                else
                {
                    g_fileOpen.Visibility = Visibility.Collapsed;
                    l_desc.Text = Utils.Tr("notification_events_preview_2nd_line");
                    fileOpenOverlay.FilePath = null;
                }
            }

            // Colors
            g_back.Background = currentParams.BackgroundColor;
            g_front.Background = currentParams.ForegroundColor;
            l_title.Foreground = currentParams.TextColor;
            l_desc.Foreground = currentParams.TextColor;

            // Sizes
            fileOpen_viewbox.Width = Math.Ceiling(fileOpenOverlay.Width * currentParams.Scale);
            fileOpen_viewbox.Height = Math.Ceiling(fileOpenOverlay.Height * currentParams.Scale);
            fileOpen_sep.Height = Math.Ceiling(Math.Round(currentParams.QuickActionsOffset * currentParams.Scale));

            Width = Math.Ceiling(default_window_width * currentParams.Scale);
            Height = Math.Ceiling(default_window_height * currentParams.Scale) + (currentParams.ShowQuickActions ? Math.Ceiling(fileOpen_viewbox.Height + fileOpen_sep.Height) : 0);
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
            var pe = (NvidiaNotification.Positions)owner.ModuleSettings.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetWindowPosition(anchor, new Size(Width, Height), owner.ModuleSettings.Offset);

            Left = pos.X;
            Top = pos.Y;
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

            var timeline = (anim_nv.Storyboard.Children[0] as ParallelTimeline);
            var anim_front = (timeline.Children[0] as ThicknessAnimationUsingKeyFrames);
            var anim_back = (timeline.Children[1] as ThicknessAnimationUsingKeyFrames);
            var keys_front = anim_front.KeyFrames;
            var keys_back = anim_back.KeyFrames;

            var timeline_file = (anim_f.Storyboard.Children[0] as ParallelTimeline);
            var anim_file = (timeline_file.Children[0] as ThicknessAnimationUsingKeyFrames);
            var keys_file = anim_file.KeyFrames;

            var dur = TimeSpan.FromMilliseconds(currentParams.Duration);
            var end_time = TimeSpan.FromMilliseconds(100);
            var slide = TimeSpan.FromMilliseconds(currentParams.SlideDuration);
            var offset = TimeSpan.FromMilliseconds(currentParams.SlideOffset);

            var visible = new Thickness(0, 0, 0, 0);
            var visible_front = new Thickness(currentParams.IsOnRightSide ? currentParams.LineWidth : 0, 0, currentParams.IsOnRightSide ? 0 : currentParams.LineWidth, 0);
            var hidden = new Thickness(currentParams.IsOnRightSide ? g_back.Width : -g_back.Width, 0, currentParams.IsOnRightSide ? -g_back.Width : g_back.Width, 0);

            // Colored Background
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

            // Main Panel
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

            // Open File Panel
            anim_file.Duration = slide + dur + offset + slide + end_time;
            keys_file[0].Value = hidden;
            keys_file[1].Value = visible;
            keys_file[2].Value = visible;
            keys_file[3].Value = hidden;
            keys_file[4].Value = hidden;
            keys_file[1].KeyTime = slide;
            keys_file[2].KeyTime = slide + dur + offset;
            keys_file[3].KeyTime = slide + dur + offset + slide;

            // for preview
            keys_file[4].KeyTime = anim_file.Duration.TimeSpan - TimeSpan.FromMilliseconds(2);
            keys_file[5].KeyTime = anim_file.Duration.TimeSpan - TimeSpan.FromMilliseconds(1);

            // Finalize
            timeline.Duration = anim_back.Duration;
            timeline_file.Duration = anim_file.Duration;

            // If preview mode is enabled, then at the end of the animation the window should be shown again.
            if (currentParams.IsPreviewNotif)
            {
                keys_back[5].Value = visible;
                keys_front[6].Value = visible_front;

                keys_file[5].Value = visible;
            }
            else
            {
                keys_back[5].Value = hidden;
                keys_front[6].Value = hidden;

                keys_file[5].Value = hidden;
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

            var need_to_run_file_group = false;
            if (NotificationType.WithFilePaths.HasFlag(type))
            {
                l_desc.Text = Utils.GetShortPath(desc, currentParams.MaxPathChars);

                g_fileOpen.Visibility = Visibility.Visible;
                fileOpenOverlay.FilePath = currentParams.ShowQuickActions ? desc : null;
                need_to_run_file_group = true;
            }
            else
            {
                if (animation_finished[1])
                {
                    g_fileOpen.Visibility = Visibility.Collapsed;
                    fileOpenOverlay.FilePath = null;
                }

                l_desc.Text = desc;
            }
            l_desc.Visibility = string.IsNullOrWhiteSpace(desc) ? Visibility.Collapsed : Visibility.Visible;

            StopAnimNV();
            if (need_to_run_file_group)
                StopAnimFile();

            UpdateAnimationParameters();

            PlayAnimNV();
            if (need_to_run_file_group)
                PlayAnimFile();

            ShowWithLocationFix();
        }

        public void ShowPreview()
        {
            previousParams = currentParams.Duplicate();
            currentParams.IsPreviewNotif = true;
            UpdateParameters();

            l_title.Text = Utils.Tr("notification_events_preview");
            l_desc.Visibility = string.IsNullOrWhiteSpace(l_desc.Text) ? Visibility.Collapsed : Visibility.Visible;

            // update animation and force it to change values
            if (UpdateAnimationParameters())
            {
                PlayAnimNV();
                PlayAnimFile();
            }

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
                PlayAnimNV();
                anim_nv.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);

                PlayAnimFile();
                anim_f.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
            }
        }

        private void AnimationNV_Finished(object sender, EventArgs e)
        {
            animation_finished[0] = true;
            CheckForAllAnimationsToFinish();
        }

        private void AnimationFile_Finished(object sender, EventArgs e)
        {
            animation_finished[1] = true;
            CheckForAllAnimationsToFinish();

            if (!currentParams.IsPreviewNotif)
            {
                g_fileOpen.Visibility = Visibility.Collapsed;
                fileOpenOverlay.FilePath = null;
            }
        }

        private void CheckForAllAnimationsToFinish()
        {
            if (!currentParams.IsPreviewNotif)
            {
                if (animation_finished[0] &&
                    animation_finished[1])
                {
                    hide_delay.CallDeferred();
                }
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!currentParams.IsPreviewNotif)
            {
                anim_nv.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
                anim_f.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
            }
        }
    }
}
