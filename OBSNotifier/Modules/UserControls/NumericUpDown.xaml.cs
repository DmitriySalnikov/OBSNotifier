using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OBSNotifier.Modules.UserControls
{
    public partial class NumericUpDown : UserControl
    {
        public event EventHandler<double>? ValueChanged;

        bool is_changed_by_code = true;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, Math.Max(Math.Min(value, Maximum), Minimum));
                UpdateTextBox();

                ValueChanged?.Invoke(this, value);
            }
        }

        public static readonly DependencyProperty UpDownWidthProperty = DependencyProperty.Register(nameof(UpDownWidth), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double UpDownWidth
        {
            get => (double)GetValue(UpDownWidthProperty);
            set => SetValue(UpDownWidthProperty, value);
        }

        public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register(nameof(Precision), typeof(uint), typeof(NumericUpDown), new FrameworkPropertyMetadata((uint)3, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public uint Precision
        {
            get => (uint)GetValue(PrecisionProperty);
            set
            {
                SetValue(PrecisionProperty, value);
                UpdatePrecision();
                UpdateTextBox();
            }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(-1.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public NumericUpDown()
        {
            InitializeComponent();

            is_changed_by_code = false;
            UpdatePrecision();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                UpdateTextBox();
            }
            else
            {
                tb_number.SetBinding(TextBox.TextProperty, new Binding(nameof(Value)) { StringFormat = $"N{Precision}", Mode = BindingMode.OneWay });
            }
        }

        void UpdatePrecision()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var bind = BindingOperations.GetBinding(tb_number, TextBox.TextProperty);
                if (bind != null)
                    bind.StringFormat = $"N{Precision}";
            }
        }

        void UpdateTextBox()
        {
            if (is_changed_by_code)
                return;

            is_changed_by_code = true;
            tb_number.Text = Value.ToString($"F{Precision}", CultureInfo.InvariantCulture);
            is_changed_by_code = false;
        }

        double GetCurrentStep()
        {
            var s = Step;
            if (s < 0)
                return 1;
            else
                return s;
        }

        private void btn_up_Click(object? sender, RoutedEventArgs e)
        {
            Value += GetCurrentStep();
        }

        private void btn_down_Click(object? sender, RoutedEventArgs e)
        {
            Value -= GetCurrentStep();
        }

        private void tb_number_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (is_changed_by_code)
                return;

            if (double.TryParse(tb_number.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out double num))
            {
                is_changed_by_code = true;
                Value = num;
                is_changed_by_code = false;
            }
        }

        private void tb_number_MouseWheel(object? sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Required to work in conjunction with PreviewMouseWheel
        }

        private void tb_number_PreviewMouseWheel(object? sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (tb_number.IsFocused)
            {
                e.Handled = true;

                var sign = Math.Sign(e.Delta);
                var mult = Math.Pow(10, Utils.Clamp(Precision, 0, 5));

                Value += sign * (1 / mult);
            }
        }
    }
}
