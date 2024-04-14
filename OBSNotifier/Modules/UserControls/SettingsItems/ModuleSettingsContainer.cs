using OBSNotifier.Modules.Event;
using OBSNotifier.Modules.UserControls.SettingsItems.Additional;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    internal class ModuleSettingsContainer : StackPanel
    {
        public event EventHandler<ModuleManager.ModuleData>? ValueChanged;
        readonly List<SettingsItemModuleData> settingsItems = [];
        bool isNotifyValueChange = true;
        ModuleManager.ModuleData moduleData;

        public ModuleSettingsContainer(ModuleManager.ModuleData moduleData) : base()
        {
            this.moduleData = moduleData;
        }

        public void AddResetAllButton()
        {
            Children.Add(new SettingsItemResetAll(() =>
            {
                isNotifyValueChange = false;

                foreach (var item in settingsItems)
                    item.ResetToDefault();

                isNotifyValueChange = true;
                ValueChanged?.Invoke(this, moduleData);
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
                    ValueChanged?.Invoke(this, moduleData);
            };
            settingsItems.Add(item);
        }
    }
}
