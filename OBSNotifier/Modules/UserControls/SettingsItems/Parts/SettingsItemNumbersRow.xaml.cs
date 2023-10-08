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

        double number = 0;

        bool IsUsingRange
        {
            get => min != 0 || max != 0;
        }

        public SettingsItemNumbersRow() { valueType = typeof(double); set = _ => { }; get = () => 0; InitializeComponent(); }

        public SettingsItemNumbersRow(Action<double> set, Func<double> get, Type valType, double min, double max, double step, bool hasRevert)
        {
            this.set = set;
            this.get = get;
            valueType = valType;
            this.min = min;
            this.max = max;

            InitializeComponent();

            btn_revert.Visibility = hasRevert ? Visibility.Visible : Visibility.Hidden;

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

            number = slider_value.Value;

            is_changed_by_code = true;
            set.Invoke(number);
            nud_value.Value = number;
            is_changed_by_code = false;
        }

        private void nud_value_ValueChanged(object? sender, double e)
        {
            if (is_changed_by_code)
                return;

            number = nud_value.Value;

            is_changed_by_code = true;
            set(number);
            slider_value.Value = number;
            is_changed_by_code = false;
        }
    }
}
