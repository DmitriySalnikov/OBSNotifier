using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemString : SettingsItemModuleData
    {
        bool is_changed_by_code = true;

        public SettingsItemString() : base() { InitializeComponent(); }

        public SettingsItemString(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            tb_text.Text = GetPropertyName();

            ValueChanged += (s, e) =>
            {
                if (is_changed_by_code)
                    return;

                is_changed_by_code = true;
                tb_value.Text = (string)(Value ?? string.Empty);
                is_changed_by_code = false;
            };

            is_changed_by_code = true;
            tb_value.Text = (string)(Value ?? string.Empty);
            is_changed_by_code = false;
        }

        private void tb_value_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (is_changed_by_code)
                return;

            is_changed_by_code = true;
            Value = tb_value.Text;
            is_changed_by_code = false;
        }
    }
}
