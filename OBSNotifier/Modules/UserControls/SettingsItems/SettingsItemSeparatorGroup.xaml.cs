﻿using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls.SettingsItems
{
    public partial class SettingsItemSeparatorGroup : UserControl
    {
        public SettingsItemSeparatorGroup() { }

        public SettingsItemSeparatorGroup(string categoryName)
        {
            InitializeComponent();

            tb_category_text.Text = categoryName;
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
