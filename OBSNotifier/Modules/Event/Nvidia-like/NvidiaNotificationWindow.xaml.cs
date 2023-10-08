using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OBSNotifier.Modules.Event.NvidiaLike
{
    internal partial class NvidiaNotificationWindow : Window
    {
        const string default_icon_path = "pack://application:,,,/Modules/Event/Nvidia-like/obs.png";
        const double default_window_width = 300;
        const double default_window_height = 90;

        NvidiaNotification owner = null;

        NvidiaCustomAnimationConfig previousParams;
        readonly NvidiaCustomAnimationConfig defaultParams = new NvidiaCustomAnimationConfig();

        bool IsPositionedOnTop { get => owner.SettingsTyped.Option == NvidiaNotification.Positions.TopRight; }
        DeferredActionWPF hide_delay;
        readonly BeginStoryboard anim_nv;
        readonly BeginStoryboard anim_f;

        readonly bool[] animation_finished = [true, true];

        public NvidiaNotificationWindow(NvidiaNotification module)
        {
            InitializeComponent();

            anim_nv = (Resources["nvidia_anim"] as BeginStoryboard);
            anim_f = (Resources["fileOpen_anim"] as BeginStoryboard);

            hide_delay = new DeferredActionWPF(() => Hide(), 200, this);
            owner = module;
            i_icon.SizeChanged += I_icon_SizeChanged;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = this.GetHandle();
            UtilsWinApi.SetWindowIgnoreFocus(hwnd, true);
            UtilsWinApi.SetWindowTopmost(hwnd, true);
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
            // if (owner.ModuleSettings.AdditionalData != null && owner.ModuleSettings.AdditionalData.GetHashCode() != addDataHash)
            {
                //    addDataHash = owner.ModuleSettings.AdditionalData.GetHashCode();

                // Recreate but remember preview state
                bool prev = owner.SettingsTyped.IsPreviewNotif;
                //owner.SettingsTyped = new NvidiaCustomAnimationConfig()
                {
                    // IsPreviewNotif = prev,
                };
                // TODO remove?
                //Utils.ConfigParseString(owner.ModuleSettings.AdditionalData, ref owner.SettingsTyped);
            }

            // General params
            owner.SettingsTyped.OnScreenTime = owner.SettingsTyped.OnScreenTime;
            owner.SettingsTyped.IsOnRightSide = owner.SettingsTyped.Option == NvidiaNotification.Positions.TopRight;

            UtilsWinApi.SetWindowIgnoreMouse(this.GetHandle(), owner.SettingsTyped.ClickThrough && !owner.SettingsTyped.ShowQuickActions);

            fileOpenOverlay.IsPreview = owner.SettingsTyped.IsPreviewNotif;
            fileOpenOverlay.HorizontalAlignment = owner.SettingsTyped.IsOnRightSide ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            fileOpen_colored_line.Visibility = owner.SettingsTyped.ShowQuickActionsColoredLine ? Visibility.Visible : Visibility.Hidden;

            // Preview max path
            if (owner.SettingsTyped.IsPreviewNotif)
            {
                var path = @"D:\Lorem\ipsum\dolor\sit\amet\consectetur\adipiscing\elit.\Donec\pharetra\lorem\turpis\nec\fringilla\leo\interdum\sit\amet.\Mauris\in\placerat\nulla\in\laoreet\Videos\OBS\01.01.01\Replay_01-01-01.mkv";

                if (owner.SettingsTyped.ShowQuickActions)
                {
                    g_fileOpen.Visibility = Visibility.Visible;
                    l_desc.Text = Utils.GetShortPath(path, owner.SettingsTyped.MaxPathChars);
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
            g_back.Background = new SolidColorBrush(owner.SettingsTyped.BackgroundColor);
            g_front.Background = new SolidColorBrush(owner.SettingsTyped.ForegroundColor);
            l_title.Foreground = new SolidColorBrush(owner.SettingsTyped.TextColor);
            l_desc.Foreground = new SolidColorBrush(owner.SettingsTyped.TextColor);

            // Sizes
            fileOpen_viewbox.Width = Math.Ceiling(fileOpenOverlay.Width * owner.SettingsTyped.Scale);
            fileOpen_viewbox.Height = Math.Ceiling(fileOpenOverlay.Height * owner.SettingsTyped.Scale);
            fileOpen_sep.Height = Math.Ceiling(Math.Round(owner.SettingsTyped.QuickActionsOffset * owner.SettingsTyped.Scale));

            Width = Math.Ceiling(default_window_width * owner.SettingsTyped.Scale);
            Height = Math.Ceiling(default_window_height * owner.SettingsTyped.Scale) + (owner.SettingsTyped.ShowQuickActions ? Math.Ceiling(fileOpen_viewbox.Height + fileOpen_sep.Height) : 0);
            g_front.Width = Math.Max(1, default_window_width - owner.SettingsTyped.LineWidth);

            // Icon
            try
            {
                if (owner.SettingsTyped.IconPath != defaultParams.IconPath)
                    i_icon.Source = Utils.GetBitmapImage(owner.SettingsTyped.IconPath, GetType().Assembly);
                else
                    i_icon.Source = Utils.GetBitmapImage(default_icon_path);
            }
            catch
            {
                i_icon.Source = Utils.GetBitmapImage(default_icon_path);
            }

            i_icon.Height = owner.SettingsTyped.IconHeight;
            i_icon.HorizontalAlignment = owner.SettingsTyped.IsOnRightSide ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            if (owner.SettingsTyped.IconHeight == 0)
            {
                i_icon.Visibility = Visibility.Collapsed;
                i_icon.Margin = new Thickness(0);
            }
            else
            {
                i_icon.Visibility = Visibility.Visible;
                i_icon.Margin = new Thickness(owner.SettingsTyped.IsOnRightSide ? 8 : 0, 0, owner.SettingsTyped.IsOnRightSide ? 0 : 8, 0);
            }
            I_icon_SizeChanged(null, null);

            // Position
            var pe = owner.SettingsTyped.Option;
            var anchor = (Utils.AnchorPoint)Enum.Parse(typeof(Utils.AnchorPoint), pe.ToString());
            Point pos = Utils.GetWindowPosition(owner.SettingsTyped.DisplayID, anchor, new Size(Width, Height), owner.SettingsTyped.Offset, owner.SettingsTyped.UseSafeDisplayArea);

            Left = pos.X;
            Top = pos.Y;
        }

        private void I_icon_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (owner.SettingsTyped.IconHeight == 0)
                sp_text.Margin = new Thickness(6, 0, 6, 0);
            else
                sp_text.Margin = new Thickness(owner.SettingsTyped.IsOnRightSide ? i_icon.ActualWidth + 14 : 6, 0, owner.SettingsTyped.IsOnRightSide ? 6 : i_icon.ActualWidth + 14, 0);
        }

        bool UpdateAnimationParameters()
        {
            if (owner.SettingsTyped.IsAnimParamsEqual(previousParams))
                return false;

            var timeline = (anim_nv.Storyboard.Children[0] as ParallelTimeline);
            var anim_front = (timeline.Children[0] as ThicknessAnimationUsingKeyFrames);
            var anim_back = (timeline.Children[1] as ThicknessAnimationUsingKeyFrames);
            var keys_front = anim_front.KeyFrames;
            var keys_back = anim_back.KeyFrames;

            var timeline_file = (anim_f.Storyboard.Children[0] as ParallelTimeline);
            var anim_file = (timeline_file.Children[0] as ThicknessAnimationUsingKeyFrames);
            var keys_file = anim_file.KeyFrames;

            var dur = TimeSpan.FromSeconds(owner.SettingsTyped.OnScreenTime);
            var end_time = TimeSpan.FromMilliseconds(100);
            var slide = TimeSpan.FromSeconds(owner.SettingsTyped.SlideDuration);
            var offset = TimeSpan.FromSeconds(owner.SettingsTyped.SlideOffset);

            var visible = new Thickness(0, 0, 0, 0);
            var visible_front = new Thickness(owner.SettingsTyped.IsOnRightSide ? owner.SettingsTyped.LineWidth : 0, 0, owner.SettingsTyped.IsOnRightSide ? 0 : owner.SettingsTyped.LineWidth, 0);
            var hidden = new Thickness(owner.SettingsTyped.IsOnRightSide ? g_back.Width : -g_back.Width, 0, owner.SettingsTyped.IsOnRightSide ? -g_back.Width : g_back.Width, 0);

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
            if (owner.SettingsTyped.IsPreviewNotif)
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
            if (owner.SettingsTyped.IsPreviewNotif)
                return;

            previousParams = (NvidiaCustomAnimationConfig)owner.SettingsTyped.Clone();
            UpdateParameters();

            l_title.Text = title;

            var need_to_run_file_group = false;
            if (NotificationType.WithFilePaths.HasFlag(type))
            {
                l_desc.Text = Utils.GetShortPath(desc, owner.SettingsTyped.MaxPathChars);

                g_fileOpen.Visibility = Visibility.Visible;
                fileOpenOverlay.FilePath = owner.SettingsTyped.ShowQuickActions ? desc : null;
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
            previousParams = (NvidiaCustomAnimationConfig)owner.SettingsTyped.Clone();
            owner.SettingsTyped.IsPreviewNotif = true;
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
            if (owner.SettingsTyped.IsPreviewNotif)
            {
                previousParams = (NvidiaCustomAnimationConfig)owner.SettingsTyped.Clone();
                owner.SettingsTyped.IsPreviewNotif = false;

                // update animation and force it to change values
                UpdateAnimationParameters();
                PlayAnimNV();
                anim_nv.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);

                PlayAnimFile();
                anim_f.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
            }
        }

        private void AnimationNV_Finished(object? sender, EventArgs e)
        {
            animation_finished[0] = true;
            CheckForAllAnimationsToFinish();
        }

        private void AnimationFile_Finished(object? sender, EventArgs e)
        {
            animation_finished[1] = true;
            CheckForAllAnimationsToFinish();

            if (!owner.SettingsTyped.IsPreviewNotif)
            {
                g_fileOpen.Visibility = Visibility.Collapsed;
                fileOpenOverlay.FilePath = null;
            }
        }

        private void CheckForAllAnimationsToFinish()
        {
            if (!owner.SettingsTyped.IsPreviewNotif)
            {
                if (animation_finished[0] &&
                    animation_finished[1])
                {
                    hide_delay.CallDeferred();
                }
            }
        }

        private void Window_MouseDown(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!owner.SettingsTyped.IsPreviewNotif)
            {
                anim_nv.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
                anim_f.Storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.Duration);
            }
        }
    }
}
