using OBSNotifier.Modules.UserControls.SettingsItems;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls
{
    /// <summary>
    /// Interaction logic for RevertButton.xaml
    /// </summary>
    public partial class RevertButton : UserControl
    {
        SettingsItemModuleData? owner = null;

        public RevertButton()
        {
            InitializeComponent();
        }

        private void btn_reset_Click(object? sender, RoutedEventArgs e)
        {
            if (owner != null)
            {
                owner.Value = owner.DefaultValue;
            }
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DependencyObject ucParent = Parent;

            while (!(ucParent is SettingsItemModuleData) && ucParent != null)
            {
                ucParent = LogicalTreeHelper.GetParent(ucParent);
            }

            if (ucParent != null)
            {
                owner = (SettingsItemModuleData)ucParent;
            }
            else
            {
                throw new NullReferenceException($"{nameof(ucParent)} is null.");
            }
        }
    }
}
