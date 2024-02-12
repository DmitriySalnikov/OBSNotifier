using OBSNotifier.Modules;
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
        readonly Dictionary<string, TextBlock> module_id_to_text_block_map = [];
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
            OnModuleChanged();

            IsChangedByCode = false;
        }

        void UpdateWindowAppearance()
        {
            FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
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

            if (module_id_to_text_block_map.TryGetValue(Settings.Instance.NotificationModule, out TextBlock? value))
            {
                cb_notification_modules.SelectedItem = value;
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

        void OnEventModuleSettingChanged(object? sender, EventArgs e)
        {
            UpdateNotification();
        }

        void OnModuleChanged()
        {
            IsChangedByCode = true;

            var moduleData = App.modules.CurrentModule;
            if (moduleData.instance != null)
            {
                if (moduleSettings != null)
                {
                    moduleSettings.ValueChanged -= OnEventModuleSettingChanged;
                    moduleSettings.Children.Clear();
                }

                // TODO animations does not update
                module_settings_container.Children.Clear();
                moduleSettings = SettingsMenuGenerator.GenerateMenu(moduleData.instance.Settings, moduleData.defaultSettings);
                moduleSettings.ValueChanged += OnEventModuleSettingChanged;
                module_settings_container.Children.Add(moduleSettings);

                //  cb_use_safe_area.IsChecked = moduleData.instance.ModuleSettings.UseSafeDisplayArea;

                // Update options list
                // cb_notification_options.Items.Clear();

                if (moduleData.instance.EnumOptionsType != null)
                {

                    var names = Enum.GetNames(moduleData.instance.EnumOptionsType);

                    // foreach (var e in names)
                    // cb_notification_options.Items.Add(e);

                    //  if (names.Contains(Settings.Instance.CurrentModuleSettings.SelectedOption))
                    //      cb_notification_options.SelectedItem = Settings.Instance.CurrentModuleSettings.SelectedOption;
                    // else
                    //      cb_notification_options.SelectedItem = Enum.GetName(moduleData.instance.EnumOptionsType, moduleData.defaultSettings.Option);
                }
            }

            IsChangedByCode = false;
        }

        private void Window_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
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
                App.modules.CurrentModule.instance.HidePreview();
                Settings.Instance.IsPreviewShowing = false;

                // Save window size
                Settings.Instance.SettingsWindowRect = new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
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

        private void cb_preview_Checked(object? sender, RoutedEventArgs e)
        {
            UpdateNotification();
        }

        private void cb_preview_Unchecked(object? sender, RoutedEventArgs e)
        {
            App.modules.CurrentModule.instance.HidePreview();
            Settings.Instance.IsPreviewShowing = false;
        }
    }
}
