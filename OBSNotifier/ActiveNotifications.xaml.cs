using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for ActiveNotifications.xaml
    /// </summary>
    public partial class ActiveNotifications : Window
    {
        readonly Dictionary<NotificationType, CheckBox> activeNotification = new Dictionary<NotificationType, CheckBox>();
        readonly NotificationType currentNotifications;

        public ActiveNotifications(NotificationType currentNotifications)
        {
            InitializeComponent();

            this.currentNotifications = currentNotifications;
            {
                var type = typeof(NotificationType);
                var name = typeof(EnumNameAttribute);
                foreach (NotificationType e in Enum.GetValues(type))
                {
                    if (e == NotificationType.None)
                        continue;
                    if (e == NotificationType.Minimal)
                        break;

                    var member = type.GetMember(e.ToString())[0];
                    var cb = new CheckBox
                    {
                        Content = (Attribute.GetCustomAttribute(member, name) as EnumNameAttribute).Name,
                        IsChecked = currentNotifications.HasFlag(e)
                    };

                    lb_notifs.Items.Add(cb);
                    activeNotification.Add(e, cb);
                }
            }
        }

        void UpdateValues(NotificationType notifs)
        {
            foreach (var an in activeNotification)
            {
                an.Value.IsChecked = notifs.HasFlag(an.Key);
            }
        }

        public NotificationType GetActiveNotification()
        {
            var res = NotificationType.None;
            foreach (var n in activeNotification)
            {
                if (n.Value.IsChecked == true)
                    res |= n.Key;
            }
            return res;
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            UpdateValues(App.plugins.CurrentPlugin.plugin.DefaultActiveNotifications);
        }

        private void btn_reset_to_current_Click(object sender, RoutedEventArgs e)
        {
            UpdateValues(currentNotifications);
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btn_select_all_Click(object sender, RoutedEventArgs e)
        {
            UpdateValues(NotificationType.All);
        }

        private void btn_select_none_Click(object sender, RoutedEventArgs e)
        {
            UpdateValues(NotificationType.None);
        }
    }
}
