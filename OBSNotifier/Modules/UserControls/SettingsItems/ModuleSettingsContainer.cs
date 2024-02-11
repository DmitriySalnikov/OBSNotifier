using OBSNotifier.Modules.UserControls.SettingsItems.Additional;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    internal class ModuleSettingsContainer : StackPanel
    {
        public event EventHandler? ValueChanged;
        readonly List<SettingsItemModuleData> settingsItems = [];
        bool isNotifyValueChange = true;

        public ModuleSettingsContainer() : base()
        {
        }

        public void AddResetAllButton()
        {
            Children.Add(new SettingsItemResetAll(() =>
            {
                isNotifyValueChange = false;

                foreach (var item in settingsItems)
                    item.ResetToDefault();

                isNotifyValueChange = true;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }));
        }

        public void AddItem(UIElement item)
        {
            Children.Add(item);
        }

        public void AddSettingItem(SettingsItemModuleData item)
        {
            Children.Add(item);
            item.ValueChanged += (s, e) =>
            {
                if (isNotifyValueChange)
                    ValueChanged?.Invoke(this, EventArgs.Empty);
            };
            settingsItems.Add(item);
        }
    }
}
