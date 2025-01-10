using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace OBSNotifier.Modules.Event.UserControls
{
    /// <summary>
    /// Interaction logic for OpenFileActionsCustomButton.xaml
    /// </summary>
    [ContentProperty("InnerContent")]
    public partial class OpenFileActionsCustomButton : UserControl
    {
        public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerContent", typeof(object), typeof(OpenFileActionsCustomButton));

        public object InnerContent
        {
            get { return GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        public OpenFileActionsCustomButton()
        {
            InitializeComponent();
            UpdateColors();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ForegroundProperty)
            {
                UpdateColors();
            }
        }

        void UpdateColors()
        {
            tmp_border_hover.BorderBrush = new SolidColorBrush(((SolidColorBrush)Foreground).Color with { ScA = 0.4f });
            tmp_border_hover.Background = new SolidColorBrush(((SolidColorBrush)Foreground).Color with { ScA = 0.15f });
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            tmp_border_hover.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            tmp_border_hover.Visibility = Visibility.Hidden;
        }
    }
}
