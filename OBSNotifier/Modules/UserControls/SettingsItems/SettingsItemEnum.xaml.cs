using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemEnum : SettingsItemModuleData
    {
        bool is_changed_by_code = true;
        readonly bool isStringEnum = false;
        readonly Dictionary<string, int> enumIndexMap = [];
        readonly Dictionary<string, object> enumValueMap = [];

        public SettingsItemEnum() : base() { InitializeComponent(); }

        // TODO add localization support
        public SettingsItemEnum(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue, string[]? enumValues = null)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            tb_text.Text = GetPropertyName();

            isStringEnum = enumValues != null;
            FillComboBox(enumValues);

            ValueChanged += (s, e) =>
            {
                if (is_changed_by_code)
                    return;

                is_changed_by_code = true;
                var key1 = Value?.ToString() ?? string.Empty;
                if (enumIndexMap.TryGetValue(key1, out int value))
                    cb_value.SelectedIndex = value;
                else
                    cb_value.SelectedIndex = 0;
                is_changed_by_code = false;
            };

            is_changed_by_code = true;
            var key = Value?.ToString() ?? string.Empty;
            if (enumIndexMap.TryGetValue(key, out int value))
                cb_value.SelectedIndex = value;
            else
                cb_value.SelectedIndex = 0;
            is_changed_by_code = false;
        }

        void FillComboBox(string[]? enumValues = null)
        {
            cb_value.Items.Clear();

            if (enumValues == null)
            {
                var items = Enum.GetNames(ValuePropertyInfo.PropertyType);
                var values = Enum.GetValues(ValuePropertyInfo.PropertyType);
                for (int i = 0; i < items.Length; i++)
                {
                    enumIndexMap.Add(items[i], cb_value.Items.Count);
                    enumValueMap.Add(items[i], values.GetValue(i) ?? string.Empty);
                    cb_value.Items.Add(items[i]);
                }
            }
            else
            {
                foreach (var v in enumValues)
                {
                    enumIndexMap.Add(v, cb_value.Items.Count);
                    cb_value.Items.Add(v);
                }
            }
        }

        private void cb_value_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (is_changed_by_code)
                return;

            is_changed_by_code = true;
            if (isStringEnum)
            {
                Value = cb_value.SelectedItem;
            }
            else
            {
                Value = enumValueMap[(string)cb_value.SelectedItem];
            }
            is_changed_by_code = false;
        }
    }
}
