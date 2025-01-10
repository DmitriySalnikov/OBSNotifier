using OBSNotifier.Modules.Event;
using OBSNotifier.Modules.UserControls.SettingsItems;
using OBSNotifier.Modules.UserControls.SettingsItems.Additional;

namespace OBSNotifier.Modules
{
    internal static class SettingsMenuGenerator
    {
        public static readonly Type[] NumericTypes = [
            typeof(float),
            typeof(double),

            typeof(sbyte),
            typeof(byte),
            typeof(char),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
        ];

        public static readonly Type[] NumericStructTypes = [
            typeof(Point),
            typeof(Size),
            typeof(Thickness),
        ];

        public static readonly Type[] NumericSupportingTypes =
        [
            .. NumericTypes,
            .. NumericStructTypes
        ];

        public static ModuleSettingsContainer GenerateMenu(Window window, ModuleManager.ModuleData moduleData)
        {
            object settingObject = moduleData.instance.Settings;
            object defaultSettings = moduleData.defaultSettings;

            ArgumentNullException.ThrowIfNull(settingObject);
            ArgumentNullException.ThrowIfNull(defaultSettings);

            if (settingObject.GetType() != defaultSettings.GetType())
                throw new ArgumentException($"Type {nameof(settingObject)}, not equal to {nameof(defaultSettings)}");

            var res = new ModuleSettingsContainer(moduleData);
            var members = settingObject.GetType().GetMembers();

            res.AddResetAllButton();
            res.Children.Add(new SettingsItemSeparator());

            foreach (var mem in members)
            {
                if (mem.MemberType == MemberTypes.Property && mem is PropertyInfo propInfo)
                {
                    // TODO maybe remove this limitation?
                    if (typeof(AvaloniaObject).IsAssignableFrom(propInfo.PropertyType))
                        App.LogError($"{propInfo.PropertyType.Name} {propInfo.Name} is a '{nameof(AvaloniaObject)}' that cannot be obtained from another thread in which settings are usually saved.");

                    if (mem.GetCustomAttribute<SettingsItemIgnoreAttribute>() is SettingsItemIgnoreAttribute attr_ignore)
                        continue;

                    if (mem.GetCustomAttribute<SettingsItemCategoryAttribute>() is SettingsItemCategoryAttribute attr_category)
                        res.AddItem(new SettingsItemSeparatorGroup(attr_category.CategoryName));

                    var item = CreateItem(window, ref moduleData, settingObject, propInfo, propInfo.GetValue(defaultSettings) ?? throw new NullReferenceException($"{defaultSettings} cannot have a null value."));
                    res.AddSettingItem(item);

                    if (mem.GetCustomAttribute<SettingsItemHintAttribute>() is SettingsItemHintAttribute attr_hint)
                        ToolTip.SetTip(item, Utils.Tr(attr_hint.HintText));

                    res.Children.Add(new SettingsItemSeparator());
                }
            }

            return res;
        }

        static SettingsItemModuleData CreateItem(Window window, ref ModuleManager.ModuleData moduleData, object owner, PropertyInfo propertyInfo, object defaultValue)
        {
            Type type = propertyInfo.PropertyType;

            var attrSettings = propertyInfo.GetCustomAttribute<SettingsItemAttribute>();
            string name = "";

            if (attrSettings != null)
            {
                name = attrSettings.Name;
            }

            if (type == typeof(bool))
            {
                return new SettingsItemToggle(name, owner, propertyInfo, defaultValue);
            }
            else if (NumericSupportingTypes.Contains(type))
            {
                return new SettingsItemNumbers(name, owner, propertyInfo, defaultValue);
            }
            else if (type == typeof(Color))
            {
                return new SettingsItemColor(name, owner, propertyInfo, defaultValue);
            }
            else if (type == typeof(string))
            {
                if (propertyInfo.GetCustomAttribute<SettingsItemStringPathAttribute>() is not null)
                    return new SettingsItemStringPath(name, owner, propertyInfo, defaultValue);
                if (propertyInfo.GetCustomAttribute<SettingsItemStringDisplayIDAttribute>() is not null)
                    // TODO start from 1?
                    return new SettingsItemEnum(name, owner, propertyInfo, defaultValue, Enumerable.Range(0, window.Screens.ScreenCount).Select(s => s.ToString()).ToArray());

                return new SettingsItemString(name, owner, propertyInfo, defaultValue);
            }
            else if (type == typeof(NotificationType))
            {
                return new SettingsItemNotificationTypes(name, owner, propertyInfo, defaultValue, ref moduleData);
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                return new SettingsItemEnum(name, owner, propertyInfo, defaultValue);
            }

            return new SettingsItemNotImplemented($"{Utils.GetCallerFileLine()}\n{name}", owner, propertyInfo, defaultValue);
        }
    }
}
