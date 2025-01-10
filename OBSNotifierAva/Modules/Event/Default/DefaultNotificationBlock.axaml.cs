using Avalonia.Interactivity;

namespace OBSNotifier.Modules.Event.Default
{
    /// <summary>
    /// Interaction logic for NotificationBlock.xaml
    /// </summary>
    public partial class DefaultNotificationBlock : UserControl, IDisposable
    {
        //   readonly BeginStoryboard notifFade;
        bool isPreview = false;
        int prevMaxPathChars = -1;
        bool? prevShowQuickActions = null;

        public delegate void VoidHandler(object? sender, EventArgs e);
        public event VoidHandler? Finished;
        double prevDuration = 0;

        public DefaultNotificationBlock()
        {
            InitializeComponent();

            //      notifFade = (BeginStoryboard)g_notif.Resources["FadeAnimBoard"];
        }

        public void Dispose()
        {
            //       notifFade.Storyboard.Stop(g_notif);
            //       notifFade.Storyboard.Completed -= Animation_Completed;
            //        MouseDown -= Window_MouseDown;
            Finished = null;
        }

        public void ShowPreview(DefaultCustomNotifBlockSettings settings, double opacity)
        {
            isPreview = true;
            //  notifFade.Storyboard.Stop(g_notif);
            g_notif.Opacity = opacity;
            IsVisible = true;

            // Preview max path
            var desc = Utils.Tr("notification_events_preview_2nd_line");
            var notif = NotificationType.None;
            if (isPreview && (
                (prevMaxPathChars != -1 && prevMaxPathChars != settings.MaxPathChars) ||
                (prevShowQuickActions != null && prevShowQuickActions != settings.ShowQuickActions)
               ))
            {
                desc = Utils.GetShortPath(Utils.PreviewPathString, settings.MaxPathChars);
                notif = NotificationType.RecordingStopped;
            }

            UpdateParams(settings, notif, Utils.Tr("notification_events_preview"), desc);
        }

        public void HidePreview()
        {
            isPreview = false;
            HideNotif();
        }

        public void HideNotif()
        {
            //   notifFade.Storyboard.Stop(g_notif);
            g_notif.Opacity = 0;
            IsVisible = false;
        }

        public void SetupNotif(DefaultCustomNotifBlockSettings settings, NotificationType type, string title, string desc)
        {
            UpdateParams(settings, type, title, desc);

            IsVisible = true;
            //    notifFade.Storyboard.Stop(g_notif);

            //if (settings.OnScreenTime != prevDuration)
            //{
            //prevDuration = settings.OnScreenTime;

            //var d_anim = (DoubleAnimationUsingKeyFrames)notifFade.Storyboard.Children[0];
            //var keys = d_anim.KeyFrames;
            //var dur = TimeSpan.FromSeconds(settings.OnScreenTime);
            //var init_time = keys[1].KeyTime.TimeSpan; // Use second key as init time
            //var fade = keys[2].KeyTime.TimeSpan.Subtract(init_time); // Use third key as fade in/out time
            //d_anim.Duration = init_time + fade + dur + fade + init_time;
            //keys[3].KeyTime = init_time + fade + dur;
            //keys[4].KeyTime = init_time + fade + dur + fade;
            //}

            //notifFade.Storyboard.Begin(g_notif, true);
        }

        void UpdateParams(DefaultCustomNotifBlockSettings settings, NotificationType type, string title, string desc)
        {
            b_background.Background = new SolidColorBrush(settings.BackgroundColor);
            b_background.BorderBrush = new SolidColorBrush(settings.OutlineColor);
            b_background.CornerRadius = new CornerRadius(settings.BorderRadius);
            b_background.BorderThickness = new Thickness(settings.BorderThickness);
            l_title.Foreground = l_desc.Foreground = new SolidColorBrush(settings.TextColor);
            mainBox.Margin = settings.Margin;
            Width = settings.BlockSize.Width;
            Height = settings.BlockSize.Height;

            l_title.Text = title;

            //    fileOpenOverlay.IsPreview = isPreview;
            if (type != NotificationType.None && NotificationType.WithFilePaths.HasFlag(type))
            {
                //        fileOpenOverlay.FilePath = settings.ShowQuickActions ? desc : null;
                l_desc.Text = Utils.GetShortPath(desc, settings.MaxPathChars);
            }
            else
            {
                //       fileOpenOverlay.FilePath = null;
                l_desc.Text = desc;
            }

            l_desc.IsVisible = !string.IsNullOrWhiteSpace(l_desc.Text);

            prevMaxPathChars = (int)settings.MaxPathChars;
            prevShowQuickActions = settings.ShowQuickActions;
        }

        private void Window_MouseDown(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (!isPreview)
            {
                HideNotif();
                Finished?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                HidePreview();
            }
        }

        private void Animation_Completed(object? sender, EventArgs e)
        {
            HideNotif();
            Finished?.Invoke(this, EventArgs.Empty);
        }
    }
}
