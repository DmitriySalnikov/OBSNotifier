using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    internal class ModuleSettingsContainer : StackPanel
    {
        public event EventHandler? ValueChanged;

        public ModuleSettingsContainer() : base()
        {
        }

        public void AddSettingItem(SettingsItemModuleData item)
        {
            Children.Add(item);
            item.ValueChanged += (s, e) => ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
