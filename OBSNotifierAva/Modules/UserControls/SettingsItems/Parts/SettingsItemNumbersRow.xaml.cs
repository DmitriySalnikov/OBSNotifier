using OBSNotifier.Modules.UserControls.SettingsItems.Additional;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems.Parts
{
    /// <summary>
    /// Interaction logic for SettingsItemNumberRow.xaml
    /// </summary>
    public partial class SettingsItemNumbersRow : UserControl
    {
        bool is_changed_by_code = true;

        readonly Action<double> set;
        readonly Func<double> get;
        readonly Type valueType;
        readonly double min;
        readonly double max;
        readonly uint precision = 0;

        double number = 0;

        bool IsUsingRange
        {
            get => min != 0 || max != 0;
        }

        public SettingsItemNumbersRow() { valueType = typeof(double); set = _ => { }; get = () => 0; InitializeComponent(); }

        public SettingsItemNumbersRow(Action<double> set, Func<double> get, Type valType, double min, double max, double step, RowRightActionButtonType buttonType)
        {
            this.set = set;
            this.get = get;
            valueType = valType;
            this.min = min;
            this.max = max;

            InitializeComponent();

            btn_quick_action.ButtonType = buttonType;

            if (IsUsingRange)
            {
                slider_value.Visibility = Visibility.Visible;

                slider_value.Minimum = min;
                nud_value.Minimum = min;

                slider_value.Maximum = max;
                nud_value.Maximum = max;
            }
            else
            {
                slider_value.Visibility = Visibility.Collapsed;

                nud_value.Maximum = 1000;
                nud_value.Minimum = 0;
            }

            is_changed_by_code = true;
            number = get.Invoke();
            slider_value.LargeChange = step < 0 ? (Math.Abs(max - min) > 100 ? 10 : 1) : step * 10;
            slider_value.Value = number;
            nud_value.Value = number;
            nud_value.Step = step;

            if (valueType == typeof(double))
            {
                nud_value.Precision = 3;
            }
            else if (valueType == typeof(float))
            {
                nud_value.Precision = 2;
            }
            else
            {
                nud_value.Precision = 0;
            }
            precision = nud_value.Precision;

            is_changed_by_code = false;
        }

        public void Update()
        {
            if (is_changed_by_code)
                return;

            number = get.Invoke();

            is_changed_by_code = true;
            slider_value.Value = number;
            nud_value.Value = number;
            is_changed_by_code = false;
        }

        private void slider_value_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (is_changed_by_code)
                return;

            var new_val = Math.Round(slider_value.Value, (int)precision);

            if (Utils.ApproxEqual(number, new_val, Math.Pow(0.1, precision)))
                return;

            number = new_val;

            is_changed_by_code = true;
            set.Invoke(number);
            nud_value.Value = number;
            is_changed_by_code = false;
        }

        private void nud_value_ValueChanged(object? sender, double e)
        {
            if (is_changed_by_code)
                return;

            var new_val = Math.Round(nud_value.Value, (int)precision);

            if (Utils.ApproxEqual(number, new_val, Math.Pow(0.1, precision)))
                return;

            number = new_val;

            is_changed_by_code = true;
            set(number);
            slider_value.Value = number;
            is_changed_by_code = false;
        }
    }
}
