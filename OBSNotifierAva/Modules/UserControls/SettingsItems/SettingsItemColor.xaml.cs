using System.Windows.Media;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemColor : SettingsItemModuleData
    {
        bool is_changed_by_code = true;

        public SettingsItemColor() : base() { InitializeComponent(); }

        public SettingsItemColor(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            tb_name.Text = GetPropertyName();

            ValueChanged += (s, e) =>
            {
                if (is_changed_by_code)
                    return;

                is_changed_by_code = true;
                cp_value.SecondaryColor = cp_value.SelectedColor;
                cp_value.SelectedColor = (Color)defaultValue;
                is_changed_by_code = false;
            };

            is_changed_by_code = true;
            cp_value.SelectedColor = (Color)(Value ?? Colors.Black);
            cp_value.SecondaryColor = (Color)defaultValue;
            is_changed_by_code = false;
        }

        private void cp_value_ColorChanged(object? sender, Color e)
        {
            if (is_changed_by_code)
                return;

            if ((Color)(Value ?? Colors.Black) != cp_value.SelectedColor)
            {
                is_changed_by_code = true;
                Value = cp_value.SelectedColor;
                is_changed_by_code = false;
            }
        }
    }
}
