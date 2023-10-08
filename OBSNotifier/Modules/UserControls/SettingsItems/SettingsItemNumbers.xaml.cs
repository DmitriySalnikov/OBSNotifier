using OBSNotifier.Modules.UserControls.SettingsItems.Parts;
using System.Windows;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemNumbers : SettingsItemModuleData
    {
        SettingsItemNumberRangeAttribute rangeAttribute;

        double number = 0;
        readonly List<SettingsItemNumbersRow> numbersRows = [];

        public SettingsItemNumbers() : base() { rangeAttribute = new(0, 0); InitializeComponent(); }

        public SettingsItemNumbers(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            if (Value == null)
                throw new NullReferenceException(nameof(Value));

            InitializeComponent();
            tb_text.Text = GetPropertyName();

            if (ValuePropertyInfo.GetCustomAttribute<SettingsItemNumberRangeAttribute>() is SettingsItemNumberRangeAttribute attr_range)
                rangeAttribute = attr_range;
            else
                rangeAttribute = new(0, 0);

            SetupRows();

            ValueChanged += (s, e) =>
            {
                foreach (var row in numbersRows)
                {
                    row.Update();
                }
            };
        }

        void SetupRows()
        {
            void addRow(Action<double> set, Func<double> get, Type valType, bool hasRevert)
            {
                var tmp_row = new SettingsItemNumbersRow(
                    set,
                    get,
                    valType,
                    rangeAttribute.Min,
                    rangeAttribute.Max,
                    rangeAttribute.Step,
                    hasRevert);

                control_container.Children.Add(tmp_row);
                numbersRows.Add(tmp_row);
            }

            if (SettingsMenuGenerator.NumericTypes.Contains(ValuePropertyInfo.PropertyType))
            {
                addRow(
                    v => Value = Convert.ChangeType(v, ValuePropertyInfo.PropertyType),
                    () => (double)Convert.ChangeType(Value!, typeof(double)),
                    ValuePropertyInfo.PropertyType,
                    true);
            }
            else if (ValuePropertyInfo.PropertyType == typeof(Point))
            {
                addRow(
                    v => Value = (Point)Value! with { X = v },
                    () => (double)Convert.ChangeType(((Point)Value!).X, typeof(double)),
                    typeof(double),
                    true);

                control_container.Children.Add(new SettingsItemSeparator());

                addRow(
                    v => Value = (Point)Value! with { Y = v },
                    () => (double)Convert.ChangeType(((Point)Value!).Y, typeof(double)),
                    typeof(double),
                    false);
            }
            else if (ValuePropertyInfo.PropertyType == typeof(Thickness))
            {
                addRow(
                    v => Value = (Thickness)Value! with { Left = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Left, typeof(double)),
                    typeof(double),
                    true);

                control_container.Children.Add(new SettingsItemSeparator());

                addRow(
                    v => Value = (Thickness)Value! with { Top = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Top, typeof(double)),
                    typeof(double),
                    false);

                control_container.Children.Add(new SettingsItemSeparator());

                addRow(
                    v => Value = (Thickness)Value! with { Right = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Right, typeof(double)),
                    typeof(double),
                    false);

                control_container.Children.Add(new SettingsItemSeparator());

                addRow(
                    v => Value = (Thickness)Value! with { Bottom = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Bottom, typeof(double)),
                    typeof(double),
                    false);
            }
            else
            {
                control_container.Children.Add(new SettingsItemNotImplemented($"{Utils.GetCallerFileLine()}\n{SettingName}", ValueOwner, ValuePropertyInfo, DefaultValue));
            }
        }
    }
}
