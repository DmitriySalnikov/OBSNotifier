using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace OBSNotifier.Modules.UserControls
{
    /// <summary>
    /// Interaction logic for RevertButton.xaml
    /// </summary>
    public partial class CustomColorPickerButton : UserControl
    {
        public event EventHandler<Color>? ColorChanged;

        static readonly StyledProperty<bool> UseAlphaProperty = AvaloniaProperty.Register<CustomColorPickerButton, bool>(nameof(UseAlpha), true);
        public bool UseAlpha
        {
            get => (bool)GetValue(UseAlphaProperty);
            set => SetValue(UseAlphaProperty, value);
        }

        static readonly StyledProperty<Color> SelectedColorProperty = AvaloniaProperty.Register<CustomColorPickerButton, Color>(nameof(SelectedColor), Colors.White);
        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        static readonly StyledProperty<Color> SecondaryColorProperty = AvaloniaProperty.Register<CustomColorPickerButton, Color>(nameof(SecondaryColor), Colors.Black);
        public Color SecondaryColor
        {
            get => (Color)GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }

        public CustomColorPickerButton()
        {
            InitializeComponent();
        }

        private void CP_ColorChanged(object? sender, RoutedEventArgs e)
        {
            SelectedColor = SettingsWindow.CP.SelectedColor;
            SecondaryColor = SettingsWindow.CP.SecondaryColor;
            ColorChanged?.Invoke(this, SelectedColor);
        }

        private void CPP_Opened(object? sender, EventArgs e)
        {
            SettingsWindow.CP.SelectedColor = SelectedColor;
            SettingsWindow.CP.SecondaryColor = SecondaryColor;
            SettingsWindow.CP.ShowAlpha = UseAlpha;

            SettingsWindow.CP.ColorChanged += CP_ColorChanged;
        }

        private void ToggleButton_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            SettingsWindow.CPP.PlacementTarget = toggleButton;
            SettingsWindow.CPP.SetBinding(Popup.IsOpenProperty, new Binding("IsChecked") { Source = toggleButton, Mode = BindingMode.TwoWay });
            toggleButton.Bind(ToggleButton.IsEnabledProperty, new Binding("IsOpen") { Source = SettingsWindow.CPP, Converter = new BoolInvertedConverter() });

            SettingsWindow.CPP.Opened += CPP_Opened;
        }

        // TODO replace by observable/dispose
        private void ToggleButton_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            //   if (BindingOperations.GetBinding(SettingsWindow.CPP, Popup.IsOpenProperty).Source != toggleButton || !SettingsWindow.CPP.IsOpen)
            //   {
            //       ToggleButton_IsCheckedChanged1(this, default);
            //   }
        }

        private void ToggleButton_IsCheckedChanged1(object? sender, RoutedEventArgs e)
        {
            //   if (toggleButton.IsEnabled)
            //   {
            //       if (BindingOperations.GetBinding(SettingsWindow.CPP, Popup.IsOpenProperty).Source == toggleButton)
            //           BindingOperations.ClearBinding(SettingsWindow.CPP, Popup.IsOpenProperty);
            //       BindingOperations.ClearBinding(toggleButton, ToggleButton.IsEnabledProperty);
            //
            //       SettingsWindow.CPP.Opened -= CPP_Opened;
            //       SettingsWindow.CP.ColorChanged -= CP_ColorChanged;
            //   }
        }
    }
}
