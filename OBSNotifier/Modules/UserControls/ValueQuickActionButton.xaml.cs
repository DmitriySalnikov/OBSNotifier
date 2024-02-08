using OBSNotifier.Modules.UserControls.SettingsItems;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier.Modules.UserControls
{
    public enum RowRightActionButtonType
    {
        None,
        Revert,
        Min,
        Center,
        Max,
    }

    /// <summary>
    /// Interaction logic for RevertButton.xaml
    /// </summary>
    public partial class ValueQuickActionButton : UserControl
    {
        SettingsItemModuleData? owner = null;
        RowRightActionButtonType buttonType = RowRightActionButtonType.Revert;
        public RowRightActionButtonType ButtonType
        {
            get => buttonType;
            set
            {
                buttonType = value;
                UpdateButton();
            }
        }

        public ValueQuickActionButton()
        {
            InitializeComponent();
        }

        void UpdateButton()
        {
            Visibility = ButtonType != RowRightActionButtonType.None ? Visibility.Visible : Visibility.Hidden;

            switch (ButtonType)
            {
                case RowRightActionButtonType.Revert:
                    btn_action.Content = " ↩️ ";
                    btn_action.SetResourceReference(ToolTipProperty, "settings_window_reset_hint");
                    break;
                case RowRightActionButtonType.Min:
                    btn_action.Content = " ↓ ";
                    btn_action.SetResourceReference(ToolTipProperty, "settings_window_reset_min_hint");
                    break;
                case RowRightActionButtonType.Center:
                    btn_action.Content = " ↔️ ";
                    btn_action.SetResourceReference(ToolTipProperty, "settings_window_reset_center_hint");
                    break;
                case RowRightActionButtonType.Max:
                    btn_action.Content = " ↑ ";
                    btn_action.SetResourceReference(ToolTipProperty, "settings_window_reset_max_hint");
                    break;
            }
        }

        private void btn_action_Click(object? sender, RoutedEventArgs e)
        {
            switch (buttonType)
            {
                case RowRightActionButtonType.Revert:
                    owner?.ResetToDefault();
                    break;
                case RowRightActionButtonType.Min:
                    owner?.SetMinValue();
                    break;
                case RowRightActionButtonType.Center:
                    owner?.SetCenterValue();
                    break;
                case RowRightActionButtonType.Max:
                    owner?.SetMaxValue();
                    break;
            }
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DependencyObject ucParent = Parent;

            while (ucParent is not SettingsItemModuleData && ucParent != null)
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
