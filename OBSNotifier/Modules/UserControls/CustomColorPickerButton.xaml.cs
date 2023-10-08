using OBSNotifier.Modules.UserControls.SettingsItems;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace OBSNotifier.Modules.UserControls
{
    /// <summary>
    /// Interaction logic for RevertButton.xaml
    /// </summary>
    public partial class CustomColorPickerButton : UserControl
    {
        public event EventHandler<Color>? ColorChanged;

        static DependencyProperty UseAlphaProperty = DependencyProperty.Register(nameof(UseAlpha), typeof(bool), typeof(CustomColorPickerButton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool UseAlpha
        {
            get => (bool)GetValue(UseAlphaProperty);
            set => SetValue(UseAlphaProperty, value);
        }

        static DependencyProperty SelectedColorProperty = DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(CustomColorPickerButton), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.AffectsRender));
        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        static DependencyProperty SecondaryColorProperty = DependencyProperty.Register(nameof(SecondaryColor), typeof(Color), typeof(CustomColorPickerButton), new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender));
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

        private void toggleButton_MouseEnter(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            SettingsWindow.CPP.PlacementTarget = toggleButton;
            SettingsWindow.CPP.SetBinding(Popup.IsOpenProperty, new Binding("IsChecked") { Source = toggleButton, Mode = BindingMode.TwoWay });
            toggleButton.SetBinding(ToggleButton.IsEnabledProperty, new Binding("IsOpen") { Source = SettingsWindow.CPP, Converter = new BoolInvertedConverter() });

            SettingsWindow.CPP.Opened += CPP_Opened;
        }

        private void toggleButton_IsEnabledChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (toggleButton.IsEnabled)
            {
                if (BindingOperations.GetBinding(SettingsWindow.CPP, Popup.IsOpenProperty).Source == toggleButton)
                    BindingOperations.ClearBinding(SettingsWindow.CPP, Popup.IsOpenProperty);
                BindingOperations.ClearBinding(toggleButton, ToggleButton.IsEnabledProperty);

                SettingsWindow.CPP.Opened -= CPP_Opened;
                SettingsWindow.CP.ColorChanged -= CP_ColorChanged;
            }
        }

        private void toggleButton_MouseLeave(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            if (BindingOperations.GetBinding(SettingsWindow.CPP, Popup.IsOpenProperty).Source != toggleButton || !SettingsWindow.CPP.IsOpen)
            {
                toggleButton_IsEnabledChanged(this, default);
            }
        }
    }
}
