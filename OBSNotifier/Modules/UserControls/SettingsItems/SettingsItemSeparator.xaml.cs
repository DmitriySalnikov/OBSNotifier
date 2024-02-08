using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemSeparator : UserControl
    {
        public SettingsItemSeparator()
        {
            InitializeComponent();
        }

        public SettingsItemSeparator(bool isSubItem = false)
        {
            InitializeComponent();

            if (isSubItem)
            {
                invisible_separator.Height *= 0.5;
            }
        }
    }
}
