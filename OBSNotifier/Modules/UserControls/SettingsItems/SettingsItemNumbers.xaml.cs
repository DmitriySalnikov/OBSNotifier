using OBSNotifier.Modules.UserControls.SettingsItems.Additional;
using OBSNotifier.Modules.UserControls.SettingsItems.Parts;
using System.Windows;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemNumbers : SettingsItemModuleData
    {
        public enum ValueSetAlignment
        {
            Min,
            Center,
            Max,
        }

        readonly SettingsItemNumberRangeAttribute? rangeAttribute;
        readonly List<SettingsItemNumbersRow> numbersRows = [];
        public SettingsItemNumbers() : base() { InitializeComponent(); }

        public SettingsItemNumbers(string settingName, object valueOwner, PropertyInfo valueInfo, object defaultValue)
            : base(settingName, valueOwner, valueInfo, defaultValue)
        {
            if (Value == null)
                throw new NullReferenceException(nameof(Value));

            InitializeComponent();
            tb_text.Text = GetPropertyName();

            if (ValuePropertyInfo.GetCustomAttribute<SettingsItemNumberRangeAttribute>() is SettingsItemNumberRangeAttribute attr_range)
                rangeAttribute = attr_range;

            SetupRows();

            ValueChanged += (s, e) =>
            {
                foreach (var row in numbersRows)
                {
                    row.Update();
                }
            };
        }

        public override void SetMinValue()
        {
            SetCustomValue(ValueSetAlignment.Min);
        }

        public override void SetCenterValue()
        {
            SetCustomValue(ValueSetAlignment.Center);
        }

        public override void SetMaxValue()
        {
            SetCustomValue(ValueSetAlignment.Max);
        }

        public void SetCustomValue(ValueSetAlignment pos)
        {
            if (ValuePropertyInfo.PropertyType == typeof(Size))
            {
                if (ValuePropertyInfo.GetCustomAttribute<SettingsItemNumberRangeMaxDisplayAttribute>() is SettingsItemNumberRangeMaxDisplayAttribute)
                {
                    var s = WPFScreens.GetMaxSize();
                    switch (pos)
                    {
                        case ValueSetAlignment.Min:
                            Value = new Size(0, 0);
                            break;
                        case ValueSetAlignment.Center:
                            Value = new Size(s.Width * 0.5, s.Height * 0.5);
                            break;
                        case ValueSetAlignment.Max:
                            Value = new Size(s.Width, s.Height);
                            break;
                    }
                    return;
                }
            }

            double? new_value = null;
            if (rangeAttribute != null && (rangeAttribute.Min != 0 || rangeAttribute.Max != 0))
            {
                switch (pos)
                {
                    case ValueSetAlignment.Min:
                        new_value = rangeAttribute.Min;
                        break;
                    case ValueSetAlignment.Center:
                        new_value = (rangeAttribute.Min + rangeAttribute.Max) * 0.5;
                        break;
                    case ValueSetAlignment.Max:
                        new_value = rangeAttribute.Max;
                        break;
                }
            }

            if (new_value != null)
            {
                if (SettingsMenuGenerator.NumericTypes.Contains(ValuePropertyInfo.PropertyType))
                {
                    Value = new_value;
                }
                else if (typeof(Point) == ValuePropertyInfo.PropertyType)
                {
                    Value = new Point(new_value.Value, new_value.Value);
                }
                else if (typeof(Size) == ValuePropertyInfo.PropertyType)
                {
                    new_value = Math.Max(new_value.Value, 0);
                    Value = new Size(new_value.Value, new_value.Value);
                }
                else if (typeof(Thickness) == ValuePropertyInfo.PropertyType)
                {
                    Value = new Thickness(new_value.Value, new_value.Value, new_value.Value, new_value.Value);
                }
            }
        }

        void SetupRows()
        {
            void addRow(Action<double> set, Func<double> get, Type valType, RowRightActionButtonType button, double? customMax = null, double? customMin = null, double? customStep = null)
            {
                var tmp_row = new SettingsItemNumbersRow(
                    set,
                    get,
                    valType,
                    customMin ?? rangeAttribute?.Min ?? 0,
                    customMax ?? rangeAttribute?.Max ?? 100,
                    customStep ?? rangeAttribute?.Step ?? 1,
                    button);

                control_container.Children.Add(tmp_row);
                numbersRows.Add(tmp_row);
            }

            if (SettingsMenuGenerator.NumericTypes.Contains(ValuePropertyInfo.PropertyType))
            {
                addRow(
                    v => Value = Convert.ChangeType(v, ValuePropertyInfo.PropertyType),
                    () => (double)Convert.ChangeType(Value!, typeof(double)),
                    ValuePropertyInfo.PropertyType,
                    RowRightActionButtonType.Revert);
            }
            // POINT
            else if (typeof(Point) == ValuePropertyInfo.PropertyType)
            {
                addRow(
                        v => Value = (Point)Value! with { X = v },
                        () => (double)Convert.ChangeType(((Point)Value!).X, typeof(double)),
                        typeof(double),
                        RowRightActionButtonType.Revert);

                control_container.Children.Add(new SettingsItemSeparator(true));

                addRow(
                    v => Value = (Point)Value! with { Y = v },
                    () => (double)Convert.ChangeType(((Point)Value!).Y, typeof(double)),
                    typeof(double),
                    RowRightActionButtonType.Center);
            }
            // SIZE
            else if (typeof(Size) == ValuePropertyInfo.PropertyType)
            {
                double? width = null;
                double? height = null;
                var step = rangeAttribute?.Step;

                if (ValuePropertyInfo.GetCustomAttribute<SettingsItemNumberRangeMaxDisplayAttribute>() is SettingsItemNumberRangeMaxDisplayAttribute)
                {
                    var s = WPFScreens.GetMaxSize();
                    width = s.Width;
                    height = s.Height;
                    step = 1;
                }

                addRow(
                        v => Value = (Size)Value! with { Width = v },
                        () => (double)Convert.ChangeType(((Size)Value!).Width, typeof(double)),
                        typeof(double),
                        RowRightActionButtonType.Revert, customMax: width, customStep: step);

                control_container.Children.Add(new SettingsItemSeparator(true));

                addRow(
                    v => Value = (Size)Value! with { Height = v },
                    () => (double)Convert.ChangeType(((Size)Value!).Height, typeof(double)),
                    typeof(double),
                    RowRightActionButtonType.Center, customMax: height, customStep: step);
            }
            // THICKNESS
            else if (typeof(Thickness) == ValuePropertyInfo.PropertyType)
            {
                addRow(
                    v => Value = (Thickness)Value! with { Left = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Left, typeof(double)),
                    typeof(double),
                    RowRightActionButtonType.Revert);

                control_container.Children.Add(new SettingsItemSeparator(true));

                addRow(
                    v => Value = (Thickness)Value! with { Top = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Top, typeof(double)),
                    typeof(double),
                    RowRightActionButtonType.None);

                control_container.Children.Add(new SettingsItemSeparator(true));

                addRow(
                    v => Value = (Thickness)Value! with { Right = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Right, typeof(double)),
                    typeof(double),
                    RowRightActionButtonType.None);

                control_container.Children.Add(new SettingsItemSeparator(true));

                addRow(
                    v => Value = (Thickness)Value! with { Bottom = v },
                    () => (double)Convert.ChangeType(((Thickness)Value!).Bottom, typeof(double)),
                    typeof(double),
                    RowRightActionButtonType.None);
            }
            else
            {
                control_container.Children.Add(new SettingsItemNotImplemented($"{Utils.GetCallerFileLine()}\n{SettingName}", ValueOwner, ValuePropertyInfo, DefaultValue));
            }
        }
    }
}
