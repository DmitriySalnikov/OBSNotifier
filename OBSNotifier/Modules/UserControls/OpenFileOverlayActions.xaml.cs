using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls
{
    /// <summary>
    /// Interaction logic for OpenFileOverlayActions.xaml
    /// </summary>
    public partial class OpenFileOverlayActions : UserControl
    {
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

        public OpenFileOverlayActions()
        {
            InitializeComponent();
        }

        private void btn_open_folder_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPreview && FilePath != null)
            {
                Process.Start(Path.GetFullPath(Path.GetDirectoryName(FilePath)));
            }
        }

        private void btn_open_file_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPreview && FilePath != null)
            {
                Process.Start(Path.GetFullPath(FilePath));
            }
        }
    }
}
