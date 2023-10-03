using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OBSNotifier.Modules.Event.UserControls
{
    /// <summary>
    /// Interaction logic for OpenFileOverlayActions.xaml
    /// </summary>
    public partial class OpenFileActions : UserControl
    {
        public static readonly DependencyProperty ForegroundElementsColorProperty = DependencyProperty.Register("ForegroundElementsColor", typeof(Brush), typeof(OpenFileActions), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public Brush ForegroundElementsColor
        {
            get => (Brush)GetValue(ForegroundElementsColorProperty);
            set => SetValue(ForegroundElementsColorProperty, value);
        }

        string filePath = null;
        public string FilePath
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

        public OpenFileActions()
        {
            InitializeComponent();
        }

        private void btn_open_folder_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPreview && FilePath != null)
            {
                try
                {
                    Process.Start(Path.GetFullPath(Path.GetDirectoryName(FilePath)));
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                }
            }
        }

        private void btn_open_file_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPreview && FilePath != null)
            {
                try
                {
                    Process.Start(Path.GetFullPath(FilePath));
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                }
            }
        }
    }
}
