using OBSNotifier.Modules.Event;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems.Parts
{
    /// <summary>
    /// Interaction logic for ActiveNotifications.xaml
    /// </summary>
    public partial class ActiveNotifications : Window
    {
        readonly Dictionary<NotificationType, CheckBox> activeNotifications = [];
        readonly NotificationType currentNotifications;
        readonly ModuleManager.ModuleData moduleData;

        public ActiveNotifications(ModuleManager.ModuleData moduleData, NotificationType currentNotifications)
        {
            InitializeComponent();

            this.currentNotifications = currentNotifications;
            this.moduleData = moduleData;

            foreach (var e in NotificationManager.NotificationsData)
            {
                var cb = new CheckBox
                {
                    Content = e.Value.Name,
                    IsChecked = currentNotifications.HasFlag(e.Key)
                };

                lb_notifs.Items.Add(cb);
                activeNotifications.Add(e.Key, cb);
            }

            FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        void UpdateValues(NotificationType notifs)
        {
            foreach (var an in activeNotifications)
            {
                an.Value.IsChecked = notifs.HasFlag(an.Key);
            }
        }

        public NotificationType GetActiveNotifications()
        {
            var res = NotificationType.None;
            foreach (var n in activeNotifications)
            {
                if (n.Value.IsChecked == true)
                    res |= n.Key;
            }
            return res;
        }

        private void btn_reset_Click(object? sender, RoutedEventArgs e)
        {
            UpdateValues(moduleData.instance.DefaultActiveNotifications);
        }

        private void btn_reset_to_current_Click(object? sender, RoutedEventArgs e)
        {
            UpdateValues(currentNotifications);
        }

        private void btn_save_Click(object? sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btn_select_all_Click(object? sender, RoutedEventArgs e)
        {
            UpdateValues(NotificationType.All);
        }

        private void btn_select_none_Click(object? sender, RoutedEventArgs e)
        {
            UpdateValues(NotificationType.None);
        }
    }
}
