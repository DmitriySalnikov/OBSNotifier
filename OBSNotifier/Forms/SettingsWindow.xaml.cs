using OBSNotifier.Modules;
using OBSNotifier.Modules.Event;
using OBSNotifier.Modules.UserControls.SettingsItems;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OBSNotifier
{
    public partial class SettingsWindow : Window
    {
        bool IsChangedByCode = false;
        bool IsConnecting = false;
        readonly Dictionary<string, FrameworkElement> module_id_to_text_block_map = [];
        ModuleSettingsContainer? moduleSettings = null;

        static SettingsWindow? Instance = null;
        public static Popup CPP { get => Instance?.colorPicker_popup ?? throw new NullReferenceException(nameof(Instance)); }
        public static ColorPicker.StandardColorPicker CP { get => Instance?.colorPicker ?? throw new NullReferenceException(nameof(Instance)); }

        public SettingsWindow()
        {
            InitializeComponent();

            Instance = this;
            Title = App.AppNameSpaced;

            UpdateConnectButton();
            IsChangedByCode = true;
            App.ConnectionStateChanged += App_ConnectionStateChanged;
            App.LanguageChanged += App_LanguageChanged; ;

            UpdateWindowAppearance();
            UpdateModulesMenu();

            if (Settings.Instance.ActiveModules.Count > 0)
            {
                OnModuleChanged(Settings.Instance.ActiveModules[0]);
            }

            IsChangedByCode = false;
        }

        void UpdateWindowAppearance()
        {
            FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        void UpdateModulesMenu()
        {
            var prevId = cb_notification_modules.SelectedItem != null ? (string)((FrameworkElement)cb_notification_modules.SelectedItem).Tag : "";
            if (string.IsNullOrWhiteSpace(prevId) && Settings.Instance.ActiveModules.Count > 0)
            {
                prevId = Settings.Instance.ActiveModules[0];
            }

            cb_notification_modules.ToolTip = "";

            cb_notification_modules.Items.Clear();
            lb_available_modules.Items.Clear();
            module_id_to_text_block_map.Clear();

            foreach (var p in App.Modules.LoadedModules)
            {
                var tb = new TextBlock()
                {
                    Text = p.instance.ModuleName,
                    Tag = p.instance.ModuleID,
                };

                cb_notification_modules.Items.Add(tb);
                module_id_to_text_block_map.Add(p.instance.ModuleID, tb);

                if (p.instance.ModuleID == prevId)
                {
                    cb_notification_modules.SelectedItem = tb;
                    cb_notification_modules.ToolTip = p.instance.ModuleDescription;
                }

                var cb = new CheckBox()
                {
                    Content = p.instance.ModuleName,
                    Tag = p.instance.ModuleID,
                    IsChecked = Settings.Instance.ActiveModules.Contains(p.instance.ModuleID),
                };
                cb.Checked += (s, e) => UpdateActiveModules(p.instance.ModuleID, true);
                cb.Unchecked += (s, e) => UpdateActiveModules(p.instance.ModuleID, false);

                lb_available_modules.Items.Add(cb);
            }
        }

        void UpdateActiveModules(string moduleId, bool add)
        {
            if (add && !Settings.Instance.ActiveModules.Contains(moduleId))
            {
                Settings.Instance.ActiveModules.Add(moduleId);
            }
            else
            {
                Settings.Instance.ActiveModules.Remove(moduleId);
            }
            App.Modules.UpdateActiveModules();
            Settings.Instance.Save();

            UpdateNotification();
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

        void UpdateNotification(ModuleManager.ModuleData? moduleData = null)
        {
            if (cb_preview.IsChecked == true)
            {
                if (!moduleData.HasValue)
                {
                    foreach (var mod in App.Modules.ActiveModules)
                    {
                        mod.instance.ShowPreview();
                    }
                }
                else
                {
                    moduleData.Value.instance.ShowPreview();
                }
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
                cb_autostart_text.Text = not_available_text;
            }
            else
            {
                cb_autostart_text.Text = def_text;
                if (cb_autostart.IsChecked.HasValue && cb_autostart.IsChecked.Value)
                {
                    try
                    {
                        cb_autostart_text.Text = AutostartManager.GetAutostartPath(App.AppName) == System.Windows.Forms.Application.ExecutablePath ? def_text : dif_path_text;
                    }
                    catch
                    {
                        cb_autostart_text.Text = not_available_text;
                    }
                }
            }
        }

        void UpdateAutostartScriptButton()
        {
            if (AutostartManager.IsScriptExists() && AutostartManager.IsFileNeedToUpdate(true))
            {
                tb_autostart_button_text.Text = $"{Utils.Tr("settings_window_run_with_obs_button")}\n{Utils.Tr("settings_window_run_with_obs_button_outdated")}";
                tb_autostart_button_text.ToolTip = $"{Utils.Tr("settings_window_run_with_obs_hint")}\n{Utils.TrFormat("settings_window_run_with_obs_hint_outdated", App.AppNameSpaced)}";
            }
            else
            {
                tb_autostart_button_text.Text = Utils.Tr("settings_window_run_with_obs_button");
                tb_autostart_button_text.ToolTip = Utils.Tr("settings_window_run_with_obs_hint");
            }
        }

        void OnEventModuleSettingChanged(object? sender, ModuleManager.ModuleData mod)
        {
            UpdateNotification(mod);
            Settings.Instance.Save();
        }

        void OnModuleChanged(string moduleId)
        {
            IsChangedByCode = true;

            var mod = App.Modules.GetModuleById(moduleId);
            if (mod.HasValue)
            {
                var moduleData = mod.Value;

                if (moduleSettings != null)
                {
                    moduleSettings.ValueChanged -= OnEventModuleSettingChanged;
                    moduleSettings.Children.Clear();
                }

                moduleSettings = SettingsMenuGenerator.GenerateMenu(moduleData);
                moduleSettings.ValueChanged += OnEventModuleSettingChanged;
                module_settings_container.Children.Clear();
                module_settings_container.Children.Add(moduleSettings);
            }

            IsChangedByCode = false;
        }

        private void Window_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            IsChangedByCode = true;

            if ((bool)e.NewValue)
            {
                // Update size
                if (Settings.Instance.SettingsWindowRect.Size != new Size())
                {
                    Width = Settings.Instance.SettingsWindowRect.Size.Width;
                    Height = Settings.Instance.SettingsWindowRect.Size.Height;
                }

                // Update position
                if (Settings.Instance.SettingsWindowRect.Location != new Point(-1, -1))
                {
                    Left = Settings.Instance.SettingsWindowRect.Location.X;
                    Top = Settings.Instance.SettingsWindowRect.Location.Y;

                    var screen = WPFScreens.GetScreenFrom(this);
                    if (screen != null && !screen.DeviceBounds.Contains(Left, Top))
                    {
                        Left = 0;
                        Top = 0;

                        Settings.Instance.SettingsWindowRect = new Rect(new Point(), Settings.Instance.SettingsWindowRect.Size);
                        Settings.Instance.Save();
                    }
                }

                Utils.FixWindowLocation(this, WPFScreens.GetScreenFrom(this));

                // Server & Password
                tb_address.Text = Settings.Instance.ServerAddress;
                tb_password.Password = Utils.DecryptString(Settings.Instance.Password, App.EncryptionKey) ?? "";

                // checkboxes
                cb_close_on_closing.IsChecked = Settings.Instance.IsCloseOnOBSClosing;

                // autostart
                try
                {
                    cb_autostart.IsChecked = !string.IsNullOrEmpty(AutostartManager.GetAutostartPath(App.AppName));
                    UpdateAutostartCheckbox();
                }
                catch
                {
                    UpdateAutostartCheckbox(true);
                }

                UpdateAutostartScriptButton();

                // Bring to top
                Activate();
            }
            else
            {
                Instance = null;

                // hide preview
                foreach (var mod in App.Modules.ActiveModules)
                {
                    mod.instance.HidePreview();
                }
                Settings.Instance.IsPreviewShowing = false;

                // Save window size
                Settings.Instance.SettingsWindowRect = new Rect((int)Left, (int)Top, (int)Width, (int)Height);
                Settings.Instance.Save();
            }

            IsChangedByCode = false;
        }

        private void SupportTextBlock_MouseLeftButtonUp(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Utils.ProcessStartShell("https://boosty.to/dmitriysalnikov/donate");
        }

        private void ReportErrorTextBlock_MouseLeftButtonUp(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Utils.ProcessStartShell("https://github.com/DmitriySalnikov/OBSNotifier/issues");
        }

        private void App_ConnectionStateChanged(object? sender, App.ConnectionState e)
        {
            IsConnecting = false;
            UpdateConnectButton();
        }

        private void App_LanguageChanged(object? sender, EventArgs e)
        {
            UpdateWindowAppearance();
            UpdateModulesMenu();
            UpdateConnectButton();
            UpdateAutostartCheckbox();
            UpdateAutostartScriptButton();
        }

        private async void btn_connect_Click(object? sender, RoutedEventArgs e)
        {
            if (!App.obs.IsAuthorized)
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
                    Settings.Instance.Password = Utils.EncryptString(tb_password.Password, App.EncryptionKey);
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

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Close();
        }

        private void cb_autostart_Checked(object? sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            try
            {
                AutostartManager.SetAutostart(App.AppName);
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

        private void cb_autostart_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            try
            {
                AutostartManager.RemoveAutostart(App.AppName);
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

        private void btn_autostart_script_create_update_Click(object? sender, RoutedEventArgs e)
        {
            var isScriptExists = AutostartManager.IsScriptExists();
            if (AutostartManager.CreateScript())
            {
                Clipboard.SetText(AutostartManager.ScriptPath);
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

        private void cb_close_on_closing_Checked(object? sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.IsCloseOnOBSClosing = true;
            Settings.Instance.Save();
        }

        private void cb_close_on_closing_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;

            Settings.Instance.IsCloseOnOBSClosing = false;
            Settings.Instance.Save();
        }

        private void cb_notification_modules_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            if (cb_notification_modules.SelectedItem == null) return;

            var module_id = (string)((FrameworkElement)cb_notification_modules.SelectedItem).Tag;
            var mod = App.Modules.GetModuleById(module_id);

            if (mod.HasValue)
            {
                var moduleData = mod.Value;
                cb_notification_modules.ToolTip = moduleData.instance.ModuleDescription;

                OnModuleChanged(module_id);
                UpdateNotification(moduleData);
            }
        }

        private void cb_preview_Checked(object? sender, RoutedEventArgs e)
        {
            UpdateNotification();
        }

        private void cb_preview_Unchecked(object? sender, RoutedEventArgs e)
        {
            foreach (var mod in App.Modules.ActiveModules)
            {
                mod.instance.HidePreview();
            }
            Settings.Instance.IsPreviewShowing = false;
        }
    }
}
