namespace OBSNotifier.Modules.UserControls.SettingsItems.Additional
{
    public partial class SettingsItemSeparatorGroup : UserControl
    {
        public SettingsItemSeparatorGroup() { }

        public SettingsItemSeparatorGroup(string categoryName)
        {
            InitializeComponent();

            tb_category_text.Text = Utils.Tr(categoryName);
            if (string.IsNullOrWhiteSpace(tb_category_text.Text))
            {
                grid_main.Visibility = System.Windows.Visibility.Collapsed;
                sep_solo.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                grid_main.Visibility = System.Windows.Visibility.Visible;
                sep_solo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
