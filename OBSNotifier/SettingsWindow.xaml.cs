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
using OBSNotifier.Plugins.Default;
using OBSWebsocketDotNet;

namespace OBSNotifier
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateConnectButton();

            // TODO disconnect
            App.obs.Connected += Obs_Connected; ;
            App.obs.Disconnected += Obs_Disconnected;
            App.obs.ReplayBufferStateChanged += Obs_ReplayBufferStateChanged;

            foreach (var p in App.plugins.LoadedPlugins)
            {
                cb_notification_styles.Items.Add(p.plugin.PluginName);
            }

            if (cb_notification_styles.Items.Contains(Settings.Instance.NotificationStyle))
            {
                cb_notification_styles.SelectedItem = Settings.Instance.NotificationStyle;
            }
            else
            {
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
                cb_notification_position.Items.Clear();
                var names = Enum.GetNames(pluginData.plugin.EnumPositionType);

                foreach (var e in names)
                    cb_notification_position.Items.Add(e);

                if (names.Contains(Settings.Instance.NotificationPosition))
                    cb_notification_position.SelectedItem = Settings.Instance.NotificationPosition;
                else
                    cb_notification_position.SelectedItem = Enum.GetName(pluginData.plugin.EnumPositionType, pluginData.defaultSettings.Position);

                if (Settings.Instance.AdditionalData == null)
                    tb_additional_data.Text = pluginData.defaultSettings.AdditionalData;
            }
        }

        private void Obs_ReplayBufferStateChanged(OBSWebsocket sender, OBSWebsocketDotNet.Types.OutputState type)
        {
            this.InvokeAction(() =>
            {
                var plugin = App.plugins.CurrentPlugin.plugin;
                switch (type)
                {
                    case OBSWebsocketDotNet.Types.OutputState.Starting:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Started:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Stopping:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Stopped:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Saved:
                        plugin.ShowNotification(Plugins.NotificationType.ReplaySaved, "Replay Saved", "");
                        break;
                }
            });
        }

        private void Obs_Connected(object sender, EventArgs e)
        {
            UpdateConnectButton();
        }

        private void Obs_Disconnected(object sender, EventArgs e)
        {
            UpdateConnectButton();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!App.obs.IsConnected)
            {
                var adrs = tb_address.Text;
                try
                {
                    if (string.IsNullOrWhiteSpace(adrs))
                        adrs = "ws://localhost:4444";
                    if (!adrs.StartsWith("ws://"))
                        adrs = "ws://" + adrs;
                    var pass = tb_password.Password;

                    App.obs.Connect(adrs, pass);

                    Settings.Instance.ServerAddress = tb_address.Text;
                    Settings.Instance.Password = Utils.EncryptString(pass);
                }
                catch (AuthFailureException)
                {
                    MessageBox.Show("Authentication failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                catch (ErrorResponseException ex)
                {
                    MessageBox.Show("Connect failed : " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            else
            {
                App.obs.Disconnect();
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // Update size
                if (Settings.Instance.SettingsWindowSize != new System.Drawing.Point())
                {
                    Width = Settings.Instance.SettingsWindowSize.X;
                    Height = Settings.Instance.SettingsWindowSize.Y;
                }

                // Update position
                if (Settings.Instance.SettingsWindowPosition != new System.Drawing.Point(-1, -1))
                {
                    Left = Settings.Instance.SettingsWindowPosition.X;
                    Top = Settings.Instance.SettingsWindowPosition.Y;

                    var screen = WPFScreens.GetScreenFrom(this);
                    if (screen != null && !screen.DeviceBounds.Contains(Left, Top))
                    {
                        Left = 0;
                        Top = 0;

                        Settings.Instance.SettingsWindowPosition = new System.Drawing.Point();
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

                // Update additional data text
                tb_additional_data.Text = Settings.Instance.AdditionalData;
            }
            else
            {
                // Save window size
                Settings.Instance.SettingsWindowSize = new System.Drawing.Point((int)Width, (int)Height);
                Settings.Instance.SettingsWindowPosition = new System.Drawing.Point((int)Left, (int)Top);

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
        }

        private void cb_notification_styles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var name = (string)cb_notification_styles.SelectedItem;
            if (App.plugins.SelectCurrent(name))
            {
                Settings.Instance.NotificationStyle = name;
                Settings.Instance.Save();

                OnPluginChanged();
            }
        }

        private void cb_notification_position_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Instance.NotificationPosition = (string)cb_notification_position.SelectedItem;
            Settings.Instance.Save();
        }

        private void sldr_position_offset_x_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.NotificationOffset = new System.Drawing.PointF((float)e.NewValue, Settings.Instance.NotificationOffset.Y);
            // TODO Save with timer
            Settings.Instance.Save();
        }

        private void sldr_position_offset_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.NotificationOffset = new System.Drawing.PointF(Settings.Instance.NotificationOffset.X, (float)e.NewValue);
            // TODO Save with timer
            Settings.Instance.Save();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO Save with timer
            Settings.Instance.AdditionalData = tb_additional_data.Text;
            Settings.Instance.Save();
        }

        private void btn_reset_style_Click(object sender, RoutedEventArgs e)
        {
            cb_notification_styles.SelectedItem = "Default";
        }

        private void btn_reset_position_Click(object sender, RoutedEventArgs e)
        {
            var pluginData = App.plugins.CurrentPlugin;
            if (pluginData.plugin != null)
                cb_notification_position.SelectedItem = Enum.GetName(pluginData.plugin.EnumPositionType, pluginData.defaultSettings.Position);

        }

        private void btn_reset_position_offset_Click(object sender, RoutedEventArgs e)
        {
            sldr_position_offset_x.Value = 0;
            sldr_position_offset_y.Value = 0;
        }

        private void btn_reset_additional_data_Click(object sender, RoutedEventArgs e)
        {
            var pluginData = App.plugins.CurrentPlugin;
            if (pluginData.plugin != null)
                tb_additional_data.Text = pluginData.defaultSettings.AdditionalData;
        }
    }
}
