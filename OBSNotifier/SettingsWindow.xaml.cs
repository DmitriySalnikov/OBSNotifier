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

        public SettingsWindow()
        {
            InitializeComponent();

            UpdateConnectButton();
            IsChangedByCode = true;
            App.ConnectionStateChanged += App_ConnectionStateChanged;

            // Plugins list
            {
                foreach (var p in App.plugins.LoadedPlugins)
                    cb_notification_styles.Items.Add(p.plugin.PluginName);

                if (cb_notification_styles.Items.Contains(Settings.Instance.NotificationStyle))
                {
                    cb_notification_styles.SelectedItem = Settings.Instance.NotificationStyle;
                }
                else
                {
                    Settings.Instance.NotificationStyle = "Default";
                    Settings.Instance.Save();

                    cb_notification_styles.SelectedItem = "Default";
                    App.plugins.SelectCurrent((string)cb_notification_styles.SelectedItem);
                }

                cb_notification_styles.ToolTip = App.plugins.CurrentPlugin.plugin.PluginDescription;
            }
            OnPluginChanged();

            IsChangedByCode = false;
        }

        void UpdateConnectButton()
        {
            btn_connect.IsEnabled = !IsConnecting;

            switch (App.CurrentConnectionState)
            {
                case App.ConnectionState.Connected:
                    btn_connect.Content = "Disconnect";
                    break;
                case App.ConnectionState.Disconnected:
                    btn_connect.Content = "Connect";
                    break;
                case App.ConnectionState.TryingToReconnect:
                    btn_connect.Content = "Trying to reconnect... Cancel?";
                    break;
            }
        }

        void UpdateNotification()
        {
            App.plugins.UpdateCurrentPluginSettings();
            if (cb_preview.IsChecked == true)
            {
                App.plugins.CurrentPlugin.plugin.ShowPreview();
                Settings.Instance.IsPreviewShowing = true;
            }
        }

        void UpdateAutostartCheckbox(bool isError = false)
        {
            var def_text = "Run at Windows startup";
            var not_available_text = "Run at Windows startup\n(unavailable due to an error.\nTry to run as administrator)";
            var dif_path_text = "Run at Windows startup\n(a different path is used now)";

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

        void OnPluginChanged()
        {
            IsChangedByCode = true;

            var pluginData = App.plugins.CurrentPlugin;
            if (pluginData.plugin != null)
            {
                // Update options list
                cb_notification_options.Items.Clear();

                if (pluginData.plugin.EnumOptionsType != null)
                {

                    var names = Enum.GetNames(pluginData.plugin.EnumOptionsType);

                    foreach (var e in names)
                        cb_notification_options.Items.Add(e);

                    if (names.Contains(Settings.Instance.CurrentPluginSettings.SelectedOption))
                        cb_notification_options.SelectedItem = Settings.Instance.CurrentPluginSettings.SelectedOption;
                    else
                        cb_notification_options.SelectedItem = Enum.GetName(pluginData.plugin.EnumOptionsType, pluginData.defaultSettings.Option);
                }

                // additional data
                if (Settings.Instance.CurrentPluginSettings.AdditionalData == null)
                    tb_additional_data.Text = pluginData.defaultSettings.AdditionalData;
                else
                    tb_additional_data.Text = pluginData.plugin.PluginSettings.AdditionalData;

                // offset
                sldr_position_offset_x.Value = pluginData.plugin.PluginSettings.Offset.X;
                sldr_position_offset_y.Value = pluginData.plugin.PluginSettings.Offset.Y;

                // fade time
                sldr_fade_delay.Value = pluginData.plugin.PluginSettings.OnScreenTime;
                sldr_fade_delay_ValueChanged(null, new RoutedPropertyChangedEventArgs<double>(0, pluginData.plugin.PluginSettings.OnScreenTime));

                // Update visibility of settings groups
                var groups_map = new Dictionary<Plugins.DefaultPluginSettings, FrameworkElement>()
                {
                    {Plugins.DefaultPluginSettings.Options, group_options},
                    {Plugins.DefaultPluginSettings.Offset, group_offset},
                    {Plugins.DefaultPluginSettings.FadeDelay, group_delay},
                    {Plugins.DefaultPluginSettings.AdditionalData, group_additional_data},
                    {Plugins.DefaultPluginSettings.CustomSettings, group_open_plugin_settings},
                };

                foreach (var p in groups_map)
                {
                    p.Value.Visibility = pluginData.plugin.AvailableDefaultSettings.HasFlag(p.Key) ? Visibility.Visible : Visibility.Collapsed;
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
                cb_use_safe_area.IsChecked = Settings.Instance.UseSafeDisplayArea;

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
            }
            else
            {
                // hide preview
                App.plugins.CurrentPlugin.plugin.HidePreview();
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

            Settings.Instance.UseSafeDisplayArea = false;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void cb_use_safe_area_Checked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.UseSafeDisplayArea = true;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void cb_notification_styles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;

            var name = (string)cb_notification_styles.SelectedItem;
            if (App.plugins.SelectCurrent(name))
            {
                Settings.Instance.NotificationStyle = name;
                Settings.Instance.Save();
                cb_notification_styles.ToolTip = App.plugins.CurrentPlugin.plugin.PluginDescription;

                OnPluginChanged();
                UpdateNotification();
            }
        }

        private void cb_notification_options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentPluginSettings.SelectedOption = (string)cb_notification_options.SelectedItem;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_position_offset_x_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentPluginSettings.Offset = new Point(e.NewValue, Settings.Instance.CurrentPluginSettings.Offset.Y);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_position_offset_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentPluginSettings.Offset = new Point(Settings.Instance.CurrentPluginSettings.Offset.X, e.NewValue);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_fade_delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            l_delay_sec.Text = (e.NewValue / 1000.0).ToString("F1", CultureInfo.InvariantCulture);

            if (IsChangedByCode) return;

            Settings.Instance.CurrentPluginSettings.OnScreenTime = (uint)Math.Round(e.NewValue / 100) * 100;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.CurrentPluginSettings.AdditionalData = tb_additional_data.Text;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void btn_select_active_notifications_Click(object sender, RoutedEventArgs e)
        {
            var notifs = Settings.Instance.CurrentPluginSettings.ActiveNotificationTypes ?? App.plugins.CurrentPlugin.plugin.DefaultActiveNotifications;

            var an = new ActiveNotifications(notifs);
            an.Left = Left + Width / 2 - an.Width / 2;
            an.Top = Top + Height / 2 - an.Height / 2;
            Utils.FixWindowLocation(an, WPFScreens.GetScreenFrom(this));

            if (an.ShowDialog() == true)
            {
                Settings.Instance.CurrentPluginSettings.ActiveNotificationTypes = an.GetActiveNotifications();
                Settings.Instance.Save();

                UpdateNotification();
            }

            an.Close();
        }

        private void btn_reset_options_Click(object sender, RoutedEventArgs e)
        {
            var pluginData = App.plugins.CurrentPlugin;
            if (pluginData.plugin != null)
                cb_notification_options.SelectedItem = Enum.GetName(pluginData.plugin.EnumOptionsType, pluginData.defaultSettings.Option);

            UpdateNotification();
        }

        private void btn_reset_position_offset_Click(object sender, RoutedEventArgs e)
        {
            var pluginData = App.plugins.CurrentPlugin;
            sldr_position_offset_x.Value = pluginData.defaultSettings.Offset.X;
            sldr_position_offset_y.Value = pluginData.defaultSettings.Offset.Y;

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
            var pluginData = App.plugins.CurrentPlugin;
            if (pluginData.plugin != null)
                tb_additional_data.Text = pluginData.defaultSettings.AdditionalData;

            UpdateNotification();
        }

        private void btn_reset_fade_delay_Click(object sender, RoutedEventArgs e)
        {
            sldr_fade_delay.Value = App.plugins.CurrentPlugin.defaultSettings.OnScreenTime;
        }

        private void btn_open_plugin_settings_Click(object sender, RoutedEventArgs e)
        {
            if (App.plugins.CurrentPlugin.plugin != null)
            {
                App.plugins.CurrentPlugin.plugin?.OpenCustomSettings();
                Settings.Instance.CurrentPluginSettings.CustomSettings = App.plugins.CurrentPlugin.plugin.GetCustomSettingsDataToSave();
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
            App.plugins.CurrentPlugin.plugin.HidePreview();
            Settings.Instance.IsPreviewShowing = false;
        }
    }
}
