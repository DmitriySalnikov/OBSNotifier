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
    /// Interaction logic for StringParam.xaml
    /// </summary>
    public partial class StringParam : BaseConfigParam
    {
        public StringParam(VariableInfo info) : base(info)
        {
            InitializeComponent();

            tb_name.Text = info.GetName();
            tb_value.Text = info.GetValue<string>();
        }

        private void tb_value_TextChanged(object sender, TextChangedEventArgs e)
        {
            Info.SetValue(tb_value.Text);
            OnValueChanged(this, EventArgs.Empty);
        }
    }
}
