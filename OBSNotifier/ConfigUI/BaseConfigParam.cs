using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OBSNotifier.ConfigUI
{
   public class BaseConfigParam : UserControl
    {
        public event EventHandler ValueChanged;
        VariableInfo info;

        public VariableInfo Info
        {
            get => info;
            set => info = value;
        }

        public BaseConfigParam(VariableInfo varInfo)
        {
            Info = varInfo;
        }

        protected void OnValueChanged(object sender, EventArgs e)
        {
            ValueChanged?.Invoke(sender, e);
        }
    }
}
