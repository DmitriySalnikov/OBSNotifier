using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OBSNotifier.Modules.Event.UserControls
{
    public class ControlWithAdditionalForegroundColor : UserControl
    {
        public static readonly DependencyProperty ForegroundElementsColorBrushProperty = DependencyProperty.Register("ForegroundElementsColorBrush", typeof(Brush), typeof(ControlWithAdditionalForegroundColor), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        public Brush ForegroundElementsColorBrush
        {
            get => (Brush)GetValue(ForegroundElementsColorBrushProperty);
            set => SetValue(ForegroundElementsColorBrushProperty, value);
        }
    }

    /// <summary>
    /// Interaction logic for OpenFileOverlayActions.xaml
    /// </summary>
    public partial class OpenFileActions : ControlWithAdditionalForegroundColor
    {
        string? filePath = null;
        public string? FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                if (string.IsNullOrEmpty(value))
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
        }

        public bool IsPreview { get; set; } = false;

        public OpenFileActions() : base()
        {
            InitializeComponent();
        }

        private void btn_open_folder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsPreview && FilePath != null)
            {
                try
                {
                    Utils.ProcessStartShell(Path.GetFullPath(Path.GetDirectoryName(FilePath) ?? "C:/"));
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                }
            }
        }

        private void btn_open_file_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsPreview && FilePath != null)
            {
                try
                {
                    Utils.ProcessStartShell(Path.GetFullPath(FilePath));
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                }
            }
        }
    }
}
