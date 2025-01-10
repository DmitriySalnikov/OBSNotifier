namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemNotImplemented : SettingsItemModuleData
    {
        public SettingsItemNotImplemented() : base() { InitializeComponent(); }

        public SettingsItemNotImplemented(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            InitializeComponent();
            tb_message.Text = $"Not Implemented!\n{settingName}({ValuePropertyInfo.PropertyType} {ValuePropertyInfo.ReflectedType?.Name}.{ValuePropertyInfo.Name})";
        }
    }
}
