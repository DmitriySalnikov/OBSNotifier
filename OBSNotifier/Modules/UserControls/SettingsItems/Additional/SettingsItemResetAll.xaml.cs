using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems.Additional
{
    public partial class SettingsItemResetAll : UserControl
    {
        Action? resetAction;

        public SettingsItemResetAll() { InitializeComponent(); }

        public SettingsItemResetAll(Action reset)
        {
            resetAction = reset;
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            resetAction?.Invoke();
        }
    }
}
