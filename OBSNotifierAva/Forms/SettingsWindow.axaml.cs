using Avalonia.Interactivity;
using OBSNotifier.Modules.Event;
//using OBSNotifier.Modules.UserControls.SettingsItems;

namespace OBSNotifier
{
    public partial class SettingsWindow : NotifierWindow
    {
        bool IsChangedByCode = false;
        bool IsConnecting = false;
        readonly Dictionary<string, Control> module_id_to_text_block_map = [];
        // TODO ModuleSettingsContainer? moduleSettings = null;

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
            App.LanguageChanged += App_LanguageChanged;

            WindowStateProperty.Changed.Subscribe(Window_StateChanged);

            UpdateWindowAppearance();
            UpdateModulesMenu();

            if (Settings.Instance.ActiveModules.Count > 0)
            {
                OnModuleChanged(Settings.Instance.ActiveModules[0]);
            }

            IsChangedByCode = false;

            // TODO enable if switched to Dark theme
            // NativeUtils.UseDarkTitleBar(this.GetHandle(), true);
        }

        void UpdateWindowAppearance()
        {
            FlowDirection = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        void UpdateModulesMenu()
        {
            var prevId = cb_notification_modules.SelectedItem != null ? (string)((Control)cb_notification_modules.SelectedItem).Tag : "";
            if (string.IsNullOrWhiteSpace(prevId) && Settings.Instance.ActiveModules.Count > 0)
            {
                prevId = Settings.Instance.ActiveModules[0];
            }

            ToolTip.SetTip(cb_notification_modules, "");

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
                    ToolTip.SetTip(cb_notification_modules, p.instance.ModuleDescription);
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
                    btn_connect.Content = Tr.SettingsWindow.ConnectButtonDisconnect;
                    break;
                case App.ConnectionState.Disconnected:
                    btn_connect.Content = Tr.SettingsWindow.ConnectButtonConnect;
                    break;
                case App.ConnectionState.TryingToReconnect:
                    btn_connect.Content = Tr.SettingsWindow.ConnectButtonTryingToReconnect;
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
            var def_text = $"{Tr.SettingsWindow.RunWithWindows}";
            var not_available_text = $"{def_text}\n{Tr.SettingsWindow.RunWithWindowsAdminError}";
            var dif_path_text = $"{def_text}\n{Tr.SettingsWindow.RunWithWindowsDifferentPath}";

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
                        cb_autostart_text.Text = AutostartManager.IsAutostartPathUpdated(App.AppName) ? def_text : dif_path_text;
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
                tb_autostart_button_text.Text = $"{Tr.SettingsWindow.RunWithObsButton}\n{Tr.SettingsWindow.RunWithObsButtonOutdated}";
                ToolTip.SetTip(tb_autostart_button_text, $"{Tr.SettingsWindow.RunWithObsHint}\n{Tr.SettingsWindow.RunWithObsHintOutdated(App.AppNameSpaced)}");
            }
            else
            {
                tb_autostart_button_text.Text = Tr.SettingsWindow.RunWithObsButton;
                ToolTip.SetTip(tb_autostart_button_text, Tr.SettingsWindow.RunWithObsHint);
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

            /* TODO
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
            */

            IsChangedByCode = false;
        }

        protected override void IsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.IsVisibleChanged(e);
            IsChangedByCode = true;

            if (IsVisible)
            {
                // Update size
                if (Settings.Instance.SettingsWindowRect.Size != new PixelSize())
                {
                    Width = Settings.Instance.SettingsWindowRect.Size.Width;
                    Height = Settings.Instance.SettingsWindowRect.Size.Height;
                }

                // Update position
                if (Settings.Instance.SettingsWindowRect.Position != new PixelPoint(-1, -1))
                {
                    Position = new(Settings.Instance.SettingsWindowRect.Position.X, Settings.Instance.SettingsWindowRect.Position.Y);

                    var screen = Screens.ScreenFromWindow(this);
                    if (screen != null && !screen.Bounds.Contains(Position + new PixelPoint(Position.X / 2, Position.Y / 2)))
                    {
                        Position = new PixelPoint();

                        Settings.Instance.SettingsWindowRect = new PixelRect(new PixelPoint(), Settings.Instance.SettingsWindowRect.Size);
                        Settings.Instance.Save();
                    }
                }

                Utils.FixWindowLocation(this);

                // Server & Password
                tb_address.Text = Settings.Instance.ServerAddress;
                tb_password.Text = Utils.DecryptString(Settings.Instance.Password, App.EncryptionKey) ?? "";

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
                Settings.Instance.SettingsWindowRect = new PixelRect(Position.X, Position.Y, (int)Width, (int)Height);
                Settings.Instance.Save();
            }

            IsChangedByCode = false;
        }

        private void SupportTextBlock_MouseLeftButtonUp(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Utils.ProcessStartShell("https://boosty.to/dmitriysalnikov/donate");
        }

        private void ReportErrorTextBlock_MouseLeftButtonUp(object? sender, Avalonia.Input.PointerPressedEventArgs e)
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
            if (Design.IsDesignMode) return;

            if (!App.OBS.IsAuthorized)
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
                        await App.ConnectToOBS(tb_address.Text, tb_password.Text);
                    }
                    catch (Exception ex) { App.Log(ex); }

                    IsConnecting = false;
                    UpdateConnectButton();

                    Settings.Instance.ServerAddress = tb_address.Text;
                    Settings.Instance.Password = Utils.EncryptString(tb_password.Text, App.EncryptionKey);
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

        private void Window_StateChanged(AvaloniaPropertyChangedEventArgs<WindowState> e)
        {
            if (e.NewValue.Value == WindowState.Minimized)
                Close();
        }

        private void cb_autostart_Checked(object? sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode) return;
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
            if (Design.IsDesignMode) return;
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
            if (Design.IsDesignMode) return;

            var isScriptExists = AutostartManager.IsScriptExists();
            if (AutostartManager.CreateScript())
            {
                Clipboard?.SetTextAsync(AutostartManager.ScriptPath).Wait();

                App.ShowMessageBox(string.Join("\n",
                    (isScriptExists ? Tr.MessageBoxAutostartScript.Updated : Tr.MessageBoxAutostartScript.Created),
                    Tr.MessageBoxAutostartScript.PathCopied,
                    "",
                    (isScriptExists ? Tr.MessageBoxAutostartScript.AlreadyAdded : Tr.MessageBoxAutostartScript.NeedToAdd),
                    "",
                    Tr.MessageBoxAutostartScript.Instruction(App.AppNameSpaced)),
                    Tr.MessageBoxAutostartScript.Title, MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                UpdateAutostartScriptButton();
            }
        }

        private void cb_close_on_closing_Checked(object? sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode) return;
            if (IsChangedByCode) return;

            Settings.Instance.IsCloseOnOBSClosing = true;
            Settings.Instance.Save();
        }

        private void cb_close_on_closing_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode) return;
            if (IsChangedByCode) return;

            Settings.Instance.IsCloseOnOBSClosing = false;
            Settings.Instance.Save();
        }

        private void cb_notification_modules_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            if (cb_notification_modules.SelectedItem == null) return;

            var module_id = (string)((Control)cb_notification_modules.SelectedItem).Tag;
            var mod = App.Modules.GetModuleById(module_id);

            if (mod.HasValue)
            {
                var moduleData = mod.Value;
                ToolTip.SetTip(cb_notification_modules, moduleData.instance.ModuleDescription);

                OnModuleChanged(module_id);
                UpdateNotification(moduleData);
            }
        }

        private void cb_preview_Checked(object? sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode) return;
            UpdateNotification();
        }

        private void cb_preview_Unchecked(object? sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode) return;
            foreach (var mod in App.Modules.ActiveModules)
            {
                mod.instance.HidePreview();
            }
            Settings.Instance.IsPreviewShowing = false;
        }
    }
}
