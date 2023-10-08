using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public class SettingsItemModuleData : UserControl
    {
        public string SettingName { get; }
        public object ValueOwner { get; }
        public PropertyInfo ValuePropertyInfo { get; }
        public object? Value
        {
            get
            {
                return ValuePropertyInfo.GetValue(ValueOwner);
            }
            set
            {
                ValuePropertyInfo.SetValue(ValueOwner, value, null);
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public object DefaultValue { get; set; }
        public event EventHandler? ValueChanged;

        protected SettingsItemModuleData()
        {
            SettingName = "Empty";
            ValueOwner = new();
            ValuePropertyInfo = typeof(SettingsItemModuleData).GetProperties()[0];
            DefaultValue = new();
        }

        protected SettingsItemModuleData(string settingName, object valueOwner, PropertyInfo propInfo, object defaultValue)
        {
            SettingName = settingName;
            ValueOwner = valueOwner;
            ValuePropertyInfo = propInfo;
            DefaultValue = defaultValue;
        }

        protected string GetPropertyName()
        {
            // TODO localization
            return string.IsNullOrWhiteSpace(SettingName) ? "[WIP] " + ValuePropertyInfo.Name : SettingName;
        }
    }
}
