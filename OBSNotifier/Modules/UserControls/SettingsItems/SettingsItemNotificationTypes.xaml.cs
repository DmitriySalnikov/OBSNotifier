using OBSNotifier.Modules.Event;
using OBSNotifier.Modules.UserControls.SettingsItems.Parts;
using System.Windows;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemNotificationTypes : SettingsItemModuleData
    {
        readonly ModuleManager.ModuleData moduleData;

        public SettingsItemNotificationTypes() : base() { InitializeComponent(); }

        public SettingsItemNotificationTypes(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue, ref ModuleManager.ModuleData moduleData)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            this.moduleData = moduleData;

            ValueChanged += (s, e) =>
            {
                UpdateIsDefaultState();
            };

            UpdateIsDefaultState();
        }

        void UpdateIsDefaultState()
        {
            tb_star_changed.FontSize = IsDefaultValue() ? 0.01 : SystemFonts.MessageFontSize;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var notifs = Value is NotificationType notif ? notif : moduleData.instance.DefaultActiveNotifications;

            var an = new ActiveNotifications(moduleData, notifs);
            var wnd = Window.GetWindow(this);
            an.Left = wnd.Left + wnd.Width / 2 - an.Width / 2;
            an.Top = wnd.Top + wnd.Height / 2 - an.Height / 2;
            Utils.FixWindowLocation(an, WPFScreens.GetScreenFrom(wnd));

            if (an.ShowDialog() == true)
            {
                Value = an.GetActiveNotifications();
            }

            an.Close();
        }
    }
}
