using System;
using System.Collections.Generic;
using System.Globalization;
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
using OBSNotifier.Plugins.Default;
using OBSWebsocketDotNet;

namespace OBSNotifier
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            UpdateConnectButton();

            // Plugins list
            {
                foreach (var p in App.plugins.LoadedPlugins)
                    cb_notification_styles.Items.Add(p.plugin.PluginName);

                if (cb_notification_styles.Items.Contains(Settings.Instance.NotificationStyle))
                    cb_notification_styles.SelectedItem = Settings.Instance.NotificationStyle;
                else
                    cb_notification_styles.SelectedItem = "Default";
            }
        }

        void UpdateConnectButton()
        {
            if (App.obs.IsConnected)
                btn_connect.Content = "Disconnect";
            else
                btn_connect.Content = "Connect";
        }

        void OnPluginChanged()
        {
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

        private void Obs_Connected(object sender, EventArgs e)
        {
            UpdateConnectButton();
        }

        private void Obs_Disconnected(object sender, EventArgs e)
        {
            UpdateConnectButton();
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            if (!App.obs.IsConnected)
            {
                if (App.ConnectToOBS(tb_address.Text, tb_password.Password))
                {
                    Settings.Instance.IsConnected = true;
                    Settings.Instance.ServerAddress = tb_address.Text;
                    Settings.Instance.Password = Utils.EncryptString(tb_password.Password);
                    Settings.Instance.Save();
                }
            }
            else
            {
                App.obs.Disconnect();

                Settings.Instance.IsConnected = false;
                Settings.Instance.Save();
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // OBS Events
                App.obs.Connected += Obs_Connected;
                App.obs.Disconnected += Obs_Disconnected;

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
                    }
                }

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
                        cb_display_to_show.SelectedItem = WPFScreens.Primary.DeviceName;
                }
                cb_use_safe_area.IsChecked = Settings.Instance.UseSafeDisplayArea;
            }
            else
            {
                // OBS Disconnect
                App.obs.Connected -= Obs_Connected;
                App.obs.Disconnected -= Obs_Disconnected;

                // hide preview
                App.plugins.CurrentPlugin.plugin.HidePreview();
                Settings.Instance.IsPreviewShowing = false;

                // Save window size
                Settings.Instance.SettingsWindowRect = new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
                Settings.Instance.Save();
            }
        }

        private void cb_display_to_show_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                Settings.Instance.DisplayID = e.AddedItems[0].ToString();
            else
                Settings.Instance.DisplayID = string.Empty;

            Settings.Instance.Save();
            UpdateNotification();
        }

        private void cb_use_safe_area_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Instance.UseSafeDisplayArea = false;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void cb_use_safe_area_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Instance.UseSafeDisplayArea = true;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void cb_notification_styles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var name = (string)cb_notification_styles.SelectedItem;
            if (App.plugins.SelectCurrent(name))
            {
                Settings.Instance.NotificationStyle = name;
                Settings.Instance.Save();

                OnPluginChanged();
                UpdateNotification();
            }
        }

        private void cb_notification_options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Instance.CurrentPluginSettings.SelectedOption = (string)cb_notification_options.SelectedItem;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_position_offset_x_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.CurrentPluginSettings.Offset = new System.Drawing.PointF((float)e.NewValue, Settings.Instance.CurrentPluginSettings.Offset.Y);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_position_offset_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.CurrentPluginSettings.Offset = new System.Drawing.PointF(Settings.Instance.CurrentPluginSettings.Offset.X, (float)e.NewValue);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void sldr_fade_delay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.CurrentPluginSettings.OnScreenTime = (int)Math.Round(e.NewValue / 100) * 100;
            l_delay_sec.Text = (e.NewValue / 1000.0).ToString("F1", CultureInfo.InvariantCulture);
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Instance.CurrentPluginSettings.AdditionalData = tb_additional_data.Text;
            Settings.Instance.Save();

            UpdateNotification();
        }

        private void btn_select_active_notifications_Click(object sender, RoutedEventArgs e)
        {
            var notifs = Settings.Instance.CurrentPluginSettings.ActiveNotificationTypes??App.plugins.CurrentPlugin.plugin.DefaultActiveNotifications;

            var an = new ActiveNotifications(notifs);
            an.Left = Left + Width / 2 - an.Width / 2;
            an.Top = Top + Height / 2 - an.Height / 2;

            var screen = WPFScreens.GetScreenFrom(this);
            if (screen != null && !screen.DeviceBounds.Contains(an.Left, an.Top))
                an.Top = Top;

            if (an.ShowDialog() == true)
            {
                Settings.Instance.CurrentPluginSettings.ActiveNotificationTypes = an.GetActiveNotification();
                Settings.Instance.Save();

                UpdateNotification();
            }

            an.Close();
        }

        private void btn_reset_style_Click(object sender, RoutedEventArgs e)
        {
            cb_notification_styles.SelectedItem = "Default";
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
            sldr_position_offset_x.Value = 0;
            sldr_position_offset_y.Value = 0;

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
            sldr_fade_delay.Value = 2000;
        }

        private void btn_open_plugin_settings_Click(object sender, RoutedEventArgs e)
        {
            if (App.plugins.CurrentPlugin.plugin!=null)
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
