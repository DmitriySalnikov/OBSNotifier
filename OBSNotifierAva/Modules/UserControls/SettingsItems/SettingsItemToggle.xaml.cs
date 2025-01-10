namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemToggle : SettingsItemModuleData
    {
        bool is_changed_by_code = true;

        public SettingsItemToggle() : base() { InitializeComponent(); }

        public SettingsItemToggle(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            cb_text.Text = GetPropertyName();

            ValueChanged += (s, e) =>
            {
                if (is_changed_by_code)
                    return;

                is_changed_by_code = true;
                cb_value.IsChecked = (bool)(Value ?? false);
                is_changed_by_code = false;
            };

            is_changed_by_code = true;
            cb_value.IsChecked = (bool)(Value ?? false);
            is_changed_by_code = false;
        }

        private void cb_value_Toggled(object? sender, System.Windows.RoutedEventArgs e)
        {
            if (is_changed_by_code)
                return;

            is_changed_by_code = true;
            Value = cb_value.IsChecked ?? false;
            is_changed_by_code = false;
        }
    }
}
