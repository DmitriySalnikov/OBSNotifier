using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBSNotifier.ConfigUI
{
    /// <summary>
    /// Interaction logic for BoolParam.xaml
    /// </summary>
    public partial class BoolParam : BaseConfigParam
    {
        public BoolParam(VariableInfo info) : base(info)
        {
            InitializeComponent();

            cb_name.Content = info.GetName();
            cb_name.IsChecked = info.GetValue<bool>();
        }

        private void cb_name_Checked(object sender, RoutedEventArgs e)
        {
            Info.SetValue(true);
            OnValueChanged(this, EventArgs.Empty);
        }

        private void cb_name_Unchecked(object sender, RoutedEventArgs e)
        {
            Info.SetValue(false);
            OnValueChanged(this, EventArgs.Empty);
        }
    }
}
