using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OBSNotifier.Plugins.Default
{
    /// <summary>
    /// Interaction logic for NotificationBlock.xaml
    /// </summary>
    internal partial class DefaultNotificationBlock : UserControl, IDisposable
    {
        BeginStoryboard notifFade;
        bool isPreview = false;
        int prevMaxPathChars = -1;

        public delegate void VoidHandler(object sender, EventArgs e);
        public event VoidHandler Finished;
        uint prevDuration = 0;

        public DefaultNotificationBlock()
        {
            InitializeComponent();

            notifFade = g_notif.Resources["FadeAnimBoard"] as BeginStoryboard;
        }

        public void Dispose()
        {
            notifFade.Storyboard.Stop(g_notif);
            notifFade.Storyboard.Completed -= Animation_Completed;
            MouseDown -= Window_MouseDown;
            notifFade = null;
            Finished = null;
        }

        public void ShowPreview(DefaultCustomNotifBlockSettings settings, double opacity)
        {
            isPreview = true;
            notifFade.Storyboard.Stop(g_notif);
            g_notif.Opacity = opacity;
            Visibility = Visibility.Visible;

            // Preview max path
            var desc = "Some description";
            var notif = NotificationType.None;
            if (isPreview && prevMaxPathChars != -1 && prevMaxPathChars != settings.MaxPathChars)
            {
                desc = Utils.GetShortPath(@"D:\Lorem\ipsum\dolor\sit\amet\consectetur\adipiscing\elit.\Donec\pharetra\lorem\turpis\nec\fringilla\leo\interdum\sit\amet.\Mauris\in\placerat\nulla\in\laoreet\Videos\OBS\01.01.01\Replay_01-01-01.mkv", settings.MaxPathChars);
                notif = NotificationType.RecordingStopped;
            }

            UpdateParams(settings, notif, "Preview Notification", desc);
        }

        public void HidePreview()
        {
            isPreview = false;
            HideNotif();
        }

        public void HideNotif()
        {
            notifFade.Storyboard.Stop(g_notif);
            g_notif.Opacity = 0;
            Visibility = Visibility.Collapsed;
        }

        public void SetupNotif(DefaultCustomNotifBlockSettings settings, NotificationType type, string title, string desc)
        {
            UpdateParams(settings, type, title, desc);

            Visibility = Visibility.Visible;
            notifFade.Storyboard.Stop(g_notif);

            if (settings.Duration != prevDuration)
            {
                prevDuration = settings.Duration;

                var d_anim = (notifFade.Storyboard.Children[0] as DoubleAnimationUsingKeyFrames);
                var keys = d_anim.KeyFrames;
                var dur = TimeSpan.FromMilliseconds(settings.Duration);
                var init_time = keys[1].KeyTime.TimeSpan; // Use second key as init time
                var fade = keys[2].KeyTime.TimeSpan.Subtract(init_time); // Use third key as fade in/out time
                d_anim.Duration = init_time + fade + dur + fade + init_time;
                keys[3].KeyTime = init_time + fade + dur;
                keys[4].KeyTime = init_time + fade + dur + fade;
            }

            notifFade.Storyboard.Begin(g_notif, true);
        }

        void UpdateParams(DefaultCustomNotifBlockSettings settings, NotificationType type, string title, string desc)
        {
            r_notif.Fill = new SolidColorBrush(settings.BackgroundColor);
            r_notif.Stroke = new SolidColorBrush(settings.OutlineColor);
            r_notif.RadiusX = settings.Radius;
            r_notif.RadiusY = settings.Radius;
            l_title.Foreground = new SolidColorBrush(settings.TextColor);
            l_desc.Foreground = new SolidColorBrush(settings.TextColor);
            MainViewbox.Margin = settings.Margin;
            Width = settings.Width;
            Height = settings.Height;

            l_title.Text = title;

            if (NotificationType.WithFilePaths.HasFlag(type))
                l_desc.Text = Utils.GetShortPath(desc, (uint)settings.MaxPathChars);
            else
                l_desc.Text = desc;

            l_desc.Visibility = string.IsNullOrWhiteSpace(l_desc.Text) ? Visibility.Collapsed : Visibility.Visible;

            prevMaxPathChars = (int)settings.MaxPathChars;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private void Animation_Completed(object sender, EventArgs e)
        {
            HideNotif();
            Finished?.Invoke(this, EventArgs.Empty);
        }
    }
}
