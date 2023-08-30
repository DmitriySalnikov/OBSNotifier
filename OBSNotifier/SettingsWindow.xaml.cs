using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OBSNotifier
{
    public partial class SettingsWindow : Window
    {
        bool IsChangedByCode = false;
        const string autostartKeyName = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        bool IsConnecting = false;
        Dictionary<string, TextBlock> module_id_to_text_block_map = new Dictionary<string, TextBlock>();

        public SettingsWindow()
        {
            InitializeComponent();

            Title = App.AppNameSpaced;

            UpdateConnectButton();
            IsChangedByCode = true;
            App.ConnectionStateChanged += App_ConnectionStateChanged;
            App.LanguageChanged += App_LanguageChanged; ;

            UpdateModulesMenu();
            OnModuleChanged();

            IsChangedByCode = false;
        }

        void UpdateModulesMenu()
        {
            cb_notification_modules.Items.Clear();
            module_id_to_text_block_map.Clear();

            foreach (var p in App.modules.LoadedModules)
            {
                var tb = new TextBlock()
                {
                    Text = p.instance.ModuleName,
                    Tag = p.instance.ModuleID,
                };
                cb_notification_modules.Items.Add(tb);
                module_id_to_text_block_map.Add(p.instance.ModuleID, tb);
            }

            if (module_id_to_text_block_map.ContainsKey(Settings.Instance.NotificationModule))
            {
                cb_notification_modules.SelectedItem = module_id_to_text_block_map[Settings.Instance.NotificationModule];
            }
            else
            {
                Settings.Instance.NotificationModule = "Default";
                Settings.Instance.Save();

                cb_notification_modules.SelectedItem = module_id_to_text_block_map[Settings.Instance.NotificationModule];
                App.modules.SelectCurrent(Settings.Instance.NotificationModule);
            }

            cb_notification_modules.ToolTip = App.modules.CurrentModule.instance.ModuleDescription;
        }

        void UpdateConnectButton()
        {
            btn_connect.IsEnabled = !IsConnecting;

            switch (App.CurrentConnectionState)
            {
                case App.ConnectionState.Connected:
                    btn_connect.Content = Utils.Tr("settings_window_connect_button_disconnect");
                    break;
                case App.ConnectionState.Disconnected:
                    btn_connect.Content = Utils.Tr("settings_window_connect_button_connect");
                    break;
                case App.ConnectionState.TryingToReconnect:
                    btn_connect.Content = Utils.Tr("settings_window_connect_button_trying_to_reconnect");
                    break;
            }
        }

        void UpdateNotification()
        {
            App.modules.UpdateCurrentModuleSettings();
            if (cb_preview.IsChecked == true)
            {
                App.modules.CurrentModule.instance.ShowPreview();
                Settings.Instance.IsPreviewShowing = true;
            }
        }

        void UpdateAutostartCheckbox(bool isError = false)
        {
            var def_text = $"{Utils.Tr("settings_window_run_with_windows")}";
            var not_available_text = $"{def_text}\n{Utils.Tr("settings_window_run_with_windows_admin_error")}";
            var dif_path_text = $"{def_text}\n{Utils.Tr("settings_window_run_with_windows_different_path")}";

            if (isError)
            {
                cb_autostart.Content = not_available_text;
            }
            else
            {
                cb_autostart.Content = def_text;
                if (cb_autostart.IsChecked.HasValue && cb_autostart.IsChecked.Value)
                {
                    try
                    {
                        using (var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true))
                            cb_autostart.Content = (string)rkApp.GetValue(App.AppName) == System.Windows.Forms.Application.ExecutablePath ? def_text : dif_path_text;
                    }
                    catch
                    {
                        cb_autostart.Content = not_available_text;
                    }
                }
            }
        }

        void UpdateAutostartScriptButton()
        {
            if (AutostartScriptManager.IsScriptExists() && AutostartScriptManager.IsFileNeedToUpdate(true))
            {
                tb_autostart_button_text.Text = $"{Utils.Tr("settings_window_run_with_obs_button")}\n{Utils.Tr("settings_window_run_with_obs_button_outdated")}";
                tb_autostart_button_text.ToolTip = $"{Utils.Tr("settings_window_run_with_obs_hint")}\n{Utils.Tr("settings_window_run_with_obs_hint_outdated")}";
            }
            else
            {
                tb_autostart_button_text.Text = Utils.Tr("settings_window_run_with_obs_button");
                tb_autostart_button_text.ToolTip = Utils.Tr("settings_window_run_with_obs_hint");
            }
        }

        void OnModuleChanged()
        {
            IsChangedByCode = true;

            var moduleData = App.modules.CurrentModule;
            if (moduleData.instance != null)
            {
                cb_use_safe_area.IsChecked = moduleData.instance.ModuleSettings.UseSafeDisplayArea;

                // Update options list
                cb_notification_options.Items.Clear();

                if (moduleData.instance.EnumOptionsType != null)
                {

                    var names = Enum.GetNames(moduleData.instance.EnumOptionsType);

                    foreach (var e in names)
                        cb_notification_options.Items.Add(e);

                    if (names.Contains(Settings.Instance.CurrentModuleSettings.SelectedOption))
                        cb_notification_options.SelectedItem = Settings.Instance.CurrentModuleSettings.SelectedOption;
                    else
                        cb_notification_options.SelectedItem = Enum.GetName(moduleData.instance.EnumOptionsType, moduleData.defaultSettings.Option);
                }

                // additional data
                if (Settings.Instance.CurrentModuleSettings.AdditionalData == null)
                    tb_additional_data.Text = moduleData.defaultSettings.AdditionalData;
                else
                    tb_additional_data.Text = moduleData.instance.ModuleSettings.AdditionalData;

                // offset
                sldr_position_offset_x.Value = moduleData.instance.ModuleSettings.Offset.X;
                sldr_position_offset_y.Value = moduleData.instance.ModuleSettings.Offset.Y;

                // fade time
                sldr_fade_delay.Value = moduleData.instance.ModuleSettings.OnScreenTime;
                sldr_fade_delay_ValueChanged(null, new RoutedPropertyChangedEventArgs<double>(0, moduleData.instance.ModuleSettings.OnScreenTime));

                // Update visibility of settings groups
                var groups_map = new Dictionary<Modules.AvailableModuleSettings, FrameworkElement>()
                {
                    {Modules.AvailableModuleSettings.UseSafeArea, group_safe_area},
                    {Modules.AvailableModuleSettings.Options, group_options},
                    {Modules.AvailableModuleSettings.Offset, group_offset},
                    {Modules.AvailableModuleSettings.FadeDelay, group_delay},
                    {Modules.AvailableModuleSettings.AdditionalData, group_additional_data},
                    {Modules.AvailableModuleSettings.CustomSettings, group_open_module_settings},
                    {Modules.AvailableModuleSettings.AdditionalDataFix, btn_fix_additional_data},
                };

                foreach (var p in groups_map)
                {
                    p.Value.Visibility = moduleData.instance.DefaultAvailableSettings.HasFlag(p.Key) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            IsChangedByCode = false;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsChangedByCode = true;

            if ((bool)e.NewValue)
            {
                // Update size
                if (Settings.Instance.SettingsWindowRect.Size != new System.Drawing.Size())
                {
                    Width = Settings.Instance.SettingsWindowRect.Size.Width;
                    Height = Settings.Instance.SettingsWindowRect.Size.Height;
                }

                // Update position
                if (Settings.Instance.SettingsWindowRect.Location != new System.Drawing.Point(-1, -1))
                {
                    Left = Settings.Instance.SettingsWindowRect.Location.X;
                    Top = Settings.Instance.SettingsWindowRect.Location.Y;

                    var screen = WPFScreens.GetScreenFrom(this);
                    if (screen != null && !screen.DeviceBounds.Contains(Left, Top))
                    {
                        Left = 0;
                        Top = 0;

                        Settings.Instance.SettingsWindowRect = new System.Drawing.Rectangle(new System.Drawing.Point(), Settings.Instance.SettingsWindowRect.Size);
                        Settings.Instance.Save();
                    }
                }

                Utils.FixWindowLocation(this, WPFScreens.GetScreenFrom(this));

                // Server & Password
                tb_address.Text = Settings.Instance.ServerAddress;
                tb_password.Password = Utils.DecryptString(Settings.Instance.Password);

                // Update monitors list
                {
                    bool selected = false;

                    foreach (var screen in WPFScreens.AllScreens())
                    {
                        cb_display_to_show.Items.Add(screen.DeviceName);

                        if (screen.DeviceName == Settings.Instance.DisplayID)
                        {
                            cb_display_to_show.SelectedItem = screen.DeviceName;
                            selected = true;
                        }
                    }

                    if (!selected)
                    {
                        cb_display_to_show.SelectedItem = WPFScreens.Primary.DeviceName;

                        Settings.Instance.DisplayID = WPFScreens.Primary.DeviceName;
                        Settings.Instance.Save();
                    }
                }

                // checkboxes
                cb_close_on_closing.IsChecked = Settings.Instance.IsCloseOnOBSClosing;

                // autostart
                try
                {
                    using (var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true))
                        cb_autostart.IsChecked = rkApp.GetValue(App.AppName) != null;
                    UpdateAutostartCheckbox();
                }
                catch
                {
                    UpdateAutostartCheckbox(true);
                }

                UpdateAutostartScriptButton();
            }
            else
            {
                // hide preview
                App.modules.CurrentModule.instance.HidePreview();
                Settings.Instance.IsPreviewShowing = false;

                // Save window size
                Settings.Instance.SettingsWindowRect = new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
                Settings.Instance.Save();
            }

            IsChangedByCode = false;
        }

        private void App_ConnectionStateChanged(object sender, App.ConnectionState e)
        {
            IsConnecting = false;
            UpdateConnectButton();
        }

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            UpdateModulesMenu();
            UpdateConnectButton();
            UpdateAutostartCheckbox();
            UpdateAutostartScriptButton();
        }

        private async void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            if (!App.obs.IsConnected)
            {
                if (App.CurrentConnectionState == App.ConnectionState.TryingToReconnect)
                {
                    Settings.Instance.IsManuallyConnected = false;
                    App.DisconnectFromOBS();
                }
                else
                {
                    IsConnecting = true;
                    UpdateConnectButton();

                    try
                    {
                        await App.ConnectToOBS(tb_address.Text, tb_password.Password);
                    }
                    catch (Exception ex) { App.Log(ex); }

                    IsConnecting = false;
                    UpdateConnectButton();

                    Settings.Instance.ServerAddress = tb_address.Text;
                    Settings.Instance.Password = Utils.EncryptString(tb_password.Password);
                    Settings.Instance.Save();
                }
            }
            else
            {
                Settings.Instance.IsManuallyConnected = false;
                App.DisconnectFromOBS();
            }

            UpdateConnectButton();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Close();
        }

        private void cb_display_to_show_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;

            if (e.AddedItems.Count > 0)
                Settings.Instance.DisplayID = e.AddedItems[0].ToString();
            else
                Settings.Instance.DisplayID = string.Empty;

            Settings.Instance.Save();
            UpdateNotification();
        }

        private void cb_autostart_Checked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            try
            {
                using (var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true))
                    rkApp.SetValue(App.AppName, System.Windows.Forms.Application.ExecutablePath);
                UpdateAutostartCheckbox();
            }
            catch
            {
                e.Handled = true;
                IsChangedByCode = true;
                cb_autostart.IsChecked = false;
                IsChangedByCode = false;
                UpdateAutostartCheckbox(true);
            }
        }

        private void cb_autostart_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            try
            {
                using (var rkApp = Registry.CurrentUser.OpenSubKey(autostartKeyName, true))
                    rkApp.DeleteValue(App.AppName, false);
                UpdateAutostartCheckbox();
            }
            catch
            {
                e.Handled = true;
                IsChangedByCode = true;
                cb_autostart.IsChecked = true;
                IsChangedByCode = false;
                UpdateAutostartCheckbox(true);
            }
        }

        private void btn_autostart_script_create_update_Click(object sender, RoutedEventArgs e)
        {
            var isScriptExists = AutostartScriptManager.IsScriptExists();
            if (AutostartScriptManager.CreateScript())
            {
                Clipboard.SetText(AutostartScriptManager.ScriptPath);
                App.ShowMessageBox(string.Join("\n",
                    (isScriptExists ? Utils.Tr("message_box_autostart_script_updated") : Utils.Tr("message_box_autostart_script_created")),
                    Utils.Tr("message_box_autostart_script_path_copied"),
                    "",
                    (isScriptExists ? Utils.Tr("message_box_autostart_script_already_added") : Utils.Tr("message_box_autostart_script_need_to_add")),
                    "",
                    Utils.TrFormat("message_box_autostart_script_instruction", App.AppNameSpaced)),
                    Utils.Tr("message_box_autostart_script_title"), MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateAutostartScriptButton();
            }
        }

        private void cb_close_on_closing_Checked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.IsCloseOnOBSClosing = true;
            Settings.Instance.Save();
        }

        private void cb_close_on_closing_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.IsCloseOnOBSClosing = false;
            Settings.Instance.Save();
        }

        private void cb_use_safe_area_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.UseSafeDisplayArea = false;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void cb_use_safe_area_Checked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.UseSafeDisplayArea = true;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void cb_notification_modules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            if (cb_notification_modules.SelectedItem == null) return;

            var module_id = (string)((TextBlock)cb_notification_modules.SelectedItem).Tag;
            if (App.modules.SelectCurrent(module_id))
            {
                Settings.Instance.NotificationModule = module_id;
                Settings.Instance.Save();
                cb_notification_modules.ToolTip = App.modules.CurrentModule.instance.ModuleDescription;

                OnModuleChanged();
                UpdateNotification();
            }
        }

        private void cb_notification_options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.SelectedOption = (string)cb_notification_options.SelectedItem;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_position_offset_x_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.Offset = new Point(e.NewValue, Settings.Instance.CurrentModuleSettings.Offset.Y);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_position_offset_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.Offset = new Point(Settings.Instance.CurrentModuleSettings.Offset.X, e.NewValue);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_fade_delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            l_delay_sec.Text = (e.NewValue / 1000.0).ToString("F1", CultureInfo.InvariantCulture);

            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.OnScreenTime = (uint)Math.Round(e.NewValue / 100) * 100;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentModuleSettings.AdditionalData = tb_additional_data.Text;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void btn_select_active_notifications_Click(object sender, RoutedEventArgs e)
        {
            var notifs = Settings.Instance.CurrentModuleSettings.ActiveNotificationTypes ?? App.modules.CurrentModule.instance.DefaultActiveNotifications;

            var an = new ActiveNotifications(notifs);
            an.Left = Left + Width / 2 - an.Width / 2;
            an.Top = Top + Height / 2 - an.Height / 2;
            Utils.FixWindowLocation(an, WPFScreens.GetScreenFrom(this));

            if (an.ShowDialog() == true)
            {
                Settings.Instance.CurrentModuleSettings.ActiveNotificationTypes = an.GetActiveNotifications();
                Settings.Instance.Save();

                UpdateNotification();
            }

            an.Close();
        }

        private void btn_reset_options_Click(object sender, RoutedEventArgs e)
        {
            var moduleData = App.modules.CurrentModule;
            if (moduleData.instance != null)
                cb_notification_options.SelectedItem = Enum.GetName(moduleData.instance.EnumOptionsType, moduleData.defaultSettings.Option);

            UpdateNotification();
        }

        private void btn_reset_position_offset_Click(object sender, RoutedEventArgs e)
        {
            var moduleData = App.modules.CurrentModule;
            sldr_position_offset_x.Value = moduleData.defaultSettings.Offset.X;
            sldr_position_offset_y.Value = moduleData.defaultSettings.Offset.Y;

            UpdateNotification();
        }

        private void btn_reset_position_offset_center_Click(object sender, RoutedEventArgs e)
        {
            sldr_position_offset_x.Value = 0.5;
            sldr_position_offset_y.Value = 0.5;

            UpdateNotification();
        }

        private void btn_reset_additional_data_Click(object sender, RoutedEventArgs e)
        {
            var moduleData = App.modules.CurrentModule;
            if (moduleData.instance != null)
                tb_additional_data.Text = moduleData.defaultSettings.AdditionalData;

            UpdateNotification();
        }

        private void btn_fix_additional_data_Click(object sender, RoutedEventArgs e)
        {
            var moduleData = App.modules.CurrentModule;
            if (moduleData.instance != null)
            {
                moduleData.defaultSettings.AdditionalData = moduleData.instance.GetFixedAdditionalData();
                tb_additional_data.Text = moduleData.defaultSettings.AdditionalData;
            }

            UpdateNotification();
        }

        private void btn_reset_fade_delay_Click(object sender, RoutedEventArgs e)
        {
            sldr_fade_delay.Value = App.modules.CurrentModule.defaultSettings.OnScreenTime;
        }

        private void btn_open_module_settings_Click(object sender, RoutedEventArgs e)
        {
            if (App.modules.CurrentModule.instance != null)
            {
                App.modules.CurrentModule.instance?.OpenCustomSettings();
                Settings.Instance.CurrentModuleSettings.CustomSettings = App.modules.CurrentModule.instance.GetCustomSettingsDataToSave();
                Settings.Instance.Save();

                UpdateNotification();
            }
        }

        private void cb_preview_Checked(object sender, RoutedEventArgs e)
        {
            UpdateNotification();
        }

        private void cb_preview_Unchecked(object sender, RoutedEventArgs e)
        {
            App.modules.CurrentModule.instance.HidePreview();
            Settings.Instance.IsPreviewShowing = false;
        }
    }
}
