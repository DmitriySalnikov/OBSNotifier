using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;

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
                tb_autostart_button_text.ToolTip = $"{Utils.Tr("settings_window_run_with_obs_hint")}\n{Utils.TrFormat("settings_window_run_with_obs_hint_outdated", App.AppNameSpaced)}";
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

                // Bring to top
                Activate();

                cb_enable_audio_alerts.IsChecked = Settings.Instance.EnableAudioAlerts;

                var ring = GetRingSounds();
                UpdateEventSoundUI("screenshot", cb_type_screenshot, cb_file_screenshot, panel_random_screenshot, ic_random_screenshot,
                    Settings.Instance.SoundTypeForScreenshot, Settings.Instance.SoundFileForScreenshot, Settings.Instance.RandomPoolForScreenshot, ring, GetTtsSoundsForOp("screenshot"));
                UpdateEventSoundUI("replay", cb_type_replay, cb_file_replay, panel_random_replay, ic_random_replay,
                    Settings.Instance.SoundTypeForReplaySaved, Settings.Instance.SoundFileForReplaySaved, Settings.Instance.RandomPoolForReplaySaved, ring, GetTtsSoundsForOp("replay"));
                UpdateEventSoundUI("rec_start", cb_type_rec_start, cb_file_rec_start, panel_random_rec_start, ic_random_rec_start,
                    Settings.Instance.SoundTypeForRecordingStarted, Settings.Instance.SoundFileForRecordingStarted, Settings.Instance.RandomPoolForRecordingStarted, ring, GetTtsSoundsForOp("rec_start"));
                UpdateEventSoundUI("rec_stop", cb_type_rec_stop, cb_file_rec_stop, panel_random_rec_stop, ic_random_rec_stop,
                    Settings.Instance.SoundTypeForRecordingStopped, Settings.Instance.SoundFileForRecordingStopped, Settings.Instance.RandomPoolForRecordingStopped, ring, GetTtsSoundsForOp("rec_stop"));
                UpdateEventSoundUI("connected", cb_type_connected, cb_file_connected, panel_random_connected, ic_random_connected,
                    Settings.Instance.SoundTypeForConnected, Settings.Instance.SoundFileForConnected, Settings.Instance.RandomPoolForConnected, ring, GetTtsSoundsForOp("connected"));
                UpdateEventSoundUI("disconnected", cb_type_disconnected, cb_file_disconnected, panel_random_disconnected, ic_random_disconnected,
                    Settings.Instance.SoundTypeForDisconnected, Settings.Instance.SoundFileForDisconnected, Settings.Instance.RandomPoolForDisconnected, ring, GetTtsSoundsForOp("disconnected"));
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

        List<string> GetAvailableSounds()
        {
            var list = new List<string>();
            var baseDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(baseDir);
            var candidates = new List<string>();
            candidates.Add(System.IO.Path.Combine(exeDir, "sounds"));
            candidates.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(exeDir, "..", "..", "..", "sounds")));
            candidates.Add(System.IO.Path.Combine(Environment.CurrentDirectory, "sounds"));

            foreach (var dir in candidates)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        var files = Directory.GetFiles(dir, "*.mp3");
                        list.AddRange(files.Select(f => System.IO.Path.GetFileName(f)));
                        var wavs = Directory.GetFiles(dir, "*.wav");
                        list.AddRange(wavs.Select(f => System.IO.Path.GetFileName(f)));
                        break;
                    }
                }
                catch { }
            }

            list = SortNaturally(list.Distinct());
            return list;
        }

        List<string> GetRingSounds()
        {
            var list = new List<string>();
            var baseDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(baseDir);
            var candidates = new List<string>();
            candidates.Add(System.IO.Path.Combine(exeDir, "sounds", "ring_sound"));
            candidates.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(exeDir, "..", "..", "..", "sounds", "ring_sound")));
            candidates.Add(System.IO.Path.Combine(Environment.CurrentDirectory, "sounds", "ring_sound"));
            foreach (var dir in candidates)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        foreach (var p in Directory.GetFiles(dir, "*.mp3")) list.Add("ring_sound/" + System.IO.Path.GetFileName(p));
                        foreach (var p in Directory.GetFiles(dir, "*.wav")) list.Add("ring_sound/" + System.IO.Path.GetFileName(p));
                        break;
                    }
                }
                catch { }
            }
            return SortNaturally(list.Distinct());
        }

        string MapOpToTtsFolder(string op)
        {
            switch (op)
            {
                case "connected": return "tts_sound_connection";
                case "disconnected": return "tts_sound_disconnection";
                case "rec_start": return "tts_sound_recording_on";
                case "rec_stop": return "tts_sound_recording_off";
                case "replay": return "tts_sound_replay";
                case "screenshot": return "tts_sound_screenshot";
            }
            return null;
        }

        List<string> GetTtsSoundsForOp(string op)
        {
            var folder = MapOpToTtsFolder(op);
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(folder)) return list;
            var baseDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(baseDir);
            var candidates = new List<string>();
            candidates.Add(System.IO.Path.Combine(exeDir, "sounds", folder));
            candidates.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(exeDir, "..", "..", "..", "sounds", folder)));
            candidates.Add(System.IO.Path.Combine(Environment.CurrentDirectory, "sounds", folder));
            foreach (var dir in candidates)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        foreach (var p in Directory.GetFiles(dir, "*.wav")) list.Add(folder + "/" + System.IO.Path.GetFileName(p));
                        foreach (var p in Directory.GetFiles(dir, "*.mp3")) list.Add(folder + "/" + System.IO.Path.GetFileName(p));
                        break;
                    }
                }
                catch { }
            }
            return SortNaturally(list.Distinct());
        }

        class NaturalComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return StrCmpLogicalW(x, y);
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        static extern int StrCmpLogicalW(string x, string y);

        List<string> SortNaturally(IEnumerable<string> src)
        {
            return src.OrderBy(s => s, new NaturalComparer()).ToList();
        }

        void UpdateEventSoundUI(string key,
            ComboBox cbType,
            ComboBox cbFile,
            FrameworkElement randomPanel,
            ItemsControl randomItems,
            string type,
            string selected,
            List<string> randomPool,
            List<string> ring,
            List<string> voice)
        {
            SelectType(cbType, type);
            FillFileCombo(cbFile, type, selected, ring, voice);
            if (type == "Random")
            {
                randomPanel.Visibility = Visibility.Visible;
                FillRandomItems(randomItems, randomPool, ring, voice, Settings.Instance.CustomAudioFiles);
            }
            else
            {
                randomPanel.Visibility = Visibility.Collapsed;
            }
        }

        void SelectType(ComboBox cbType, string type)
        {
            foreach (var it in cbType.Items)
            {
                var cbi = it as ComboBoxItem;
                if (cbi != null && (string)cbi.Tag == type)
                {
                    cbType.SelectedItem = cbi;
                    break;
                }
            }
        }

        void FillFileCombo(ComboBox cbFile, string type, string selected, List<string> ring, List<string> voice)
        {
            cbFile.Items.Clear();
            var none = new ComboBoxItem() { Content = Utils.Tr("audio_alerts_sound_none"), Tag = null };
            if (type == "Random")
            {
                cbFile.IsEnabled = false;
                cbFile.Items.Add(none);
                cbFile.SelectedIndex = 0;
                return;
            }
            cbFile.IsEnabled = true;
            cbFile.Items.Add(none);
            if (type == "Ring")
            {
                foreach (var s in ring) cbFile.Items.Add(new ComboBoxItem() { Content = System.IO.Path.GetFileName(s), Tag = s });
            }
            else if (type == "Voice")
            {
                foreach (var s in voice) cbFile.Items.Add(new ComboBoxItem() { Content = System.IO.Path.GetFileName(s), Tag = s });
            }
            else if (type == "Custom")
            {
                foreach (var p in Settings.Instance.CustomAudioFiles)
                {
                    var name = System.IO.Path.GetFileName(p);
                    cbFile.Items.Add(new ComboBoxItem() { Content = name, Tag = p });
                }
            }

            if (!string.IsNullOrWhiteSpace(selected))
            {
                bool found = false;
                foreach (var it in cbFile.Items)
                {
                    var cbi = it as ComboBoxItem;
                    if (cbi != null && (string)cbi.Tag == selected)
                    {
                        cbFile.SelectedItem = cbi;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var display = System.IO.Path.GetFileName(selected);
                    var custom = new ComboBoxItem() { Content = display, Tag = selected };
                    cbFile.Items.Add(custom);
                    cbFile.SelectedItem = custom;
                }
            }
            else
            {
                cbFile.SelectedIndex = 0;
            }
        }

        void FillRandomItems(ItemsControl ic, List<string> pool, List<string> ring, List<string> voice, List<string> custom)
        {
            ic.Items.Clear();
            var all = new List<string>();
            all.AddRange(ring);
            all.AddRange(voice);
            all.AddRange(custom);
            foreach (var s in SortNaturally(all.Distinct()))
            {
                var cb = new CheckBox();
                cb.Content = System.IO.Path.GetFileName(s);
                cb.Tag = s;
                cb.IsChecked = pool != null && pool.Contains(s);
                cb.Checked += (o, e) => { ApplyRandomItems(ic, pool); };
                cb.Unchecked += (o, e) => { ApplyRandomItems(ic, pool); };
                ic.Items.Add(cb);
            }
        }

        void ApplyRandomItems(ItemsControl ic, List<string> pool)
        {
            var list = new List<string>();
            foreach (var it in ic.Items)
            {
                var cb = it as CheckBox;
                if (cb != null && cb.IsChecked == true)
                {
                    list.Add((string)cb.Tag);
                }
            }
            pool.Clear();
            pool.AddRange(list);
            Settings.Instance.Save();
        }

        private void SupportTextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://boosty.to/dmitriysalnikov/donate");
        }

        private void ReportErrorTextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/DmitriySalnikov/OBSNotifier/issues");
        }

        private void App_ConnectionStateChanged(object sender, App.ConnectionState e)
        {
            IsConnecting = false;
            UpdateConnectButton();
        }

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            UpdateWindowAppearance();
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
                Clipboard.SetDataObject(AutostartScriptManager.ScriptPath);
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

        private void cb_enable_audio_alerts_Checked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;
            Settings.Instance.EnableAudioAlerts = true;
            Settings.Instance.Save();
        }

        private void cb_enable_audio_alerts_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsChangedByCode) return;
            Settings.Instance.EnableAudioAlerts = false;
            Settings.Instance.Save();
        }

        void PreviewSound(string type, string file, List<string> pool)
        {
            string target = file;
            if (type == "Random")
            {
                var src = pool != null ? pool.Where(s => !string.IsNullOrWhiteSpace(s)).ToList() : new List<string>();
                if (src.Count > 0)
                {
                    var r = new Random();
                    target = src[r.Next(src.Count)];
                }
            }
            if (!string.IsNullOrWhiteSpace(target))
            {
                TryPlaySimpleSound(target);
            }
        }

        private void btn_preview_screenshot_Click(object sender, RoutedEventArgs e)
        {
            PreviewSound(Settings.Instance.SoundTypeForScreenshot, Settings.Instance.SoundFileForScreenshot, Settings.Instance.RandomPoolForScreenshot);
        }

        private void btn_preview_replay_Click(object sender, RoutedEventArgs e)
        {
            PreviewSound(Settings.Instance.SoundTypeForReplaySaved, Settings.Instance.SoundFileForReplaySaved, Settings.Instance.RandomPoolForReplaySaved);
        }

        private void btn_preview_rec_start_Click(object sender, RoutedEventArgs e)
        {
            PreviewSound(Settings.Instance.SoundTypeForRecordingStarted, Settings.Instance.SoundFileForRecordingStarted, Settings.Instance.RandomPoolForRecordingStarted);
        }

        private void btn_preview_rec_stop_Click(object sender, RoutedEventArgs e)
        {
            PreviewSound(Settings.Instance.SoundTypeForRecordingStopped, Settings.Instance.SoundFileForRecordingStopped, Settings.Instance.RandomPoolForRecordingStopped);
        }

        private void btn_preview_connected_Click(object sender, RoutedEventArgs e)
        {
            PreviewSound(Settings.Instance.SoundTypeForConnected, Settings.Instance.SoundFileForConnected, Settings.Instance.RandomPoolForConnected);
        }
        private void btn_preview_disconnected_Click(object sender, RoutedEventArgs e)
        {
            PreviewSound(Settings.Instance.SoundTypeForDisconnected, Settings.Instance.SoundFileForDisconnected, Settings.Instance.RandomPoolForDisconnected);
        }

        private void sldr_tts_rate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsChangedByCode) return;
            Settings.Instance.TTSRate = (int)Math.Round(e.NewValue);
            Settings.Instance.Save();
        }

        private void sldr_tts_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsChangedByCode) return;
            Settings.Instance.TTSVolume = (int)Math.Round(e.NewValue);
            Settings.Instance.Save();
        }

        void BrowseAndAssign(ComboBox combo, Action<string> assign)
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Audio files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*";
                dlg.CheckFileExists = true;
                dlg.Multiselect = false;
                if (dlg.ShowDialog() == true)
                {
                    var path = dlg.FileName;
                    assign(path);
                    if (!Settings.Instance.CustomAudioFiles.Contains(path))
                        Settings.Instance.CustomAudioFiles.Add(path);
                    Settings.Instance.Save();

                    var display = System.IO.Path.GetFileName(path);
                    var custom = new ComboBoxItem() { Content = display, Tag = path };
                    combo.Items.Add(custom);
                    combo.SelectedItem = custom;
                }
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }
        }

        private void btn_browse_screenshot_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SoundTypeForScreenshot = "Custom";
            Settings.Instance.Save();
            BrowseAndAssign(cb_file_screenshot, (p) => Settings.Instance.SoundFileForScreenshot = p);
        }
        private void btn_browse_replay_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SoundTypeForReplaySaved = "Custom";
            Settings.Instance.Save();
            BrowseAndAssign(cb_file_replay, (p) => Settings.Instance.SoundFileForReplaySaved = p);
        }
        private void btn_browse_rec_start_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SoundTypeForRecordingStarted = "Custom";
            Settings.Instance.Save();
            BrowseAndAssign(cb_file_rec_start, (p) => Settings.Instance.SoundFileForRecordingStarted = p);
        }
        private void btn_browse_rec_stop_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SoundTypeForRecordingStopped = "Custom";
            Settings.Instance.Save();
            BrowseAndAssign(cb_file_rec_stop, (p) => Settings.Instance.SoundFileForRecordingStopped = p);
        }

        private void btn_browse_connected_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SoundTypeForConnected = "Custom";
            Settings.Instance.Save();
            BrowseAndAssign(cb_file_connected, (p) => Settings.Instance.SoundFileForConnected = p);
        }
        private void btn_browse_disconnected_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SoundTypeForDisconnected = "Custom";
            Settings.Instance.Save();
            BrowseAndAssign(cb_file_disconnected, (p) => Settings.Instance.SoundFileForDisconnected = p);
        }

        private void btn_restore_audio_defaults_Click(object sender, RoutedEventArgs e)
        {
            IsChangedByCode = true;
            Settings.Instance.SoundTypeForConnected = "Ring";
            Settings.Instance.SoundFileForConnected = null;
            Settings.Instance.RandomPoolForConnected.Clear();
            Settings.Instance.SoundTypeForDisconnected = "Ring";
            Settings.Instance.SoundFileForDisconnected = null;
            Settings.Instance.RandomPoolForDisconnected.Clear();
            Settings.Instance.SoundTypeForScreenshot = "Ring";
            Settings.Instance.SoundFileForScreenshot = "ring_sound/shutter1.mp3";
            Settings.Instance.RandomPoolForScreenshot.Clear();
            Settings.Instance.SoundTypeForReplaySaved = "Ring";
            Settings.Instance.SoundFileForReplaySaved = "ring_sound/notification-4.mp3";
            Settings.Instance.RandomPoolForReplaySaved.Clear();
            Settings.Instance.SoundTypeForRecordingStarted = "Ring";
            Settings.Instance.SoundFileForRecordingStarted = "ring_sound/pluck-on.mp3";
            Settings.Instance.RandomPoolForRecordingStarted.Clear();
            Settings.Instance.SoundTypeForRecordingStopped = "Ring";
            Settings.Instance.SoundFileForRecordingStopped = "ring_sound/pluck-off.mp3";
            Settings.Instance.RandomPoolForRecordingStopped.Clear();
            Settings.Instance.Save();

            var ring = GetRingSounds();
            UpdateEventSoundUI("connected", cb_type_connected, cb_file_connected, panel_random_connected, ic_random_connected,
                Settings.Instance.SoundTypeForConnected, Settings.Instance.SoundFileForConnected, Settings.Instance.RandomPoolForConnected, ring, GetTtsSoundsForOp("connected"));
            UpdateEventSoundUI("disconnected", cb_type_disconnected, cb_file_disconnected, panel_random_disconnected, ic_random_disconnected,
                Settings.Instance.SoundTypeForDisconnected, Settings.Instance.SoundFileForDisconnected, Settings.Instance.RandomPoolForDisconnected, ring, GetTtsSoundsForOp("disconnected"));
            UpdateEventSoundUI("screenshot", cb_type_screenshot, cb_file_screenshot, panel_random_screenshot, ic_random_screenshot,
                Settings.Instance.SoundTypeForScreenshot, Settings.Instance.SoundFileForScreenshot, Settings.Instance.RandomPoolForScreenshot, ring, GetTtsSoundsForOp("screenshot"));
            UpdateEventSoundUI("replay", cb_type_replay, cb_file_replay, panel_random_replay, ic_random_replay,
                Settings.Instance.SoundTypeForReplaySaved, Settings.Instance.SoundFileForReplaySaved, Settings.Instance.RandomPoolForReplaySaved, ring, GetTtsSoundsForOp("replay"));
            UpdateEventSoundUI("rec_start", cb_type_rec_start, cb_file_rec_start, panel_random_rec_start, ic_random_rec_start,
                Settings.Instance.SoundTypeForRecordingStarted, Settings.Instance.SoundFileForRecordingStarted, Settings.Instance.RandomPoolForRecordingStarted, ring, GetTtsSoundsForOp("rec_start"));
            UpdateEventSoundUI("rec_stop", cb_type_rec_stop, cb_file_rec_stop, panel_random_rec_stop, ic_random_rec_stop,
                Settings.Instance.SoundTypeForRecordingStopped, Settings.Instance.SoundFileForRecordingStopped, Settings.Instance.RandomPoolForRecordingStopped, ring, GetTtsSoundsForOp("rec_stop"));
            IsChangedByCode = false;
        }

        private void cb_type_screenshot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var type = (cb_type_screenshot.SelectedItem as ComboBoxItem)?.Tag as string;
            if (type == null) return;
            Settings.Instance.SoundTypeForScreenshot = type;
            Settings.Instance.Save();
            var ring = GetRingSounds();
            var voice = GetTtsSoundsForOp("screenshot");
            UpdateEventSoundUI("screenshot", cb_type_screenshot, cb_file_screenshot, panel_random_screenshot, ic_random_screenshot,
                Settings.Instance.SoundTypeForScreenshot, Settings.Instance.SoundFileForScreenshot, Settings.Instance.RandomPoolForScreenshot, ring, voice);
        }

        private void cb_type_replay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var type = (cb_type_replay.SelectedItem as ComboBoxItem)?.Tag as string;
            if (type == null) return;
            Settings.Instance.SoundTypeForReplaySaved = type;
            Settings.Instance.Save();
            var ring = GetRingSounds();
            var voice = GetTtsSoundsForOp("replay");
            UpdateEventSoundUI("replay", cb_type_replay, cb_file_replay, panel_random_replay, ic_random_replay,
                Settings.Instance.SoundTypeForReplaySaved, Settings.Instance.SoundFileForReplaySaved, Settings.Instance.RandomPoolForReplaySaved, ring, voice);
        }

        private void cb_type_rec_start_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var type = (cb_type_rec_start.SelectedItem as ComboBoxItem)?.Tag as string;
            if (type == null) return;
            Settings.Instance.SoundTypeForRecordingStarted = type;
            Settings.Instance.Save();
            var ring = GetRingSounds();
            var voice = GetTtsSoundsForOp("rec_start");
            UpdateEventSoundUI("rec_start", cb_type_rec_start, cb_file_rec_start, panel_random_rec_start, ic_random_rec_start,
                Settings.Instance.SoundTypeForRecordingStarted, Settings.Instance.SoundFileForRecordingStarted, Settings.Instance.RandomPoolForRecordingStarted, ring, voice);
        }

        private void cb_type_rec_stop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var type = (cb_type_rec_stop.SelectedItem as ComboBoxItem)?.Tag as string;
            if (type == null) return;
            Settings.Instance.SoundTypeForRecordingStopped = type;
            Settings.Instance.Save();
            var ring = GetRingSounds();
            var voice = GetTtsSoundsForOp("rec_stop");
            UpdateEventSoundUI("rec_stop", cb_type_rec_stop, cb_file_rec_stop, panel_random_rec_stop, ic_random_rec_stop,
                Settings.Instance.SoundTypeForRecordingStopped, Settings.Instance.SoundFileForRecordingStopped, Settings.Instance.RandomPoolForRecordingStopped, ring, voice);
        }

        private void cb_type_connected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var type = (cb_type_connected.SelectedItem as ComboBoxItem)?.Tag as string;
            if (type == null) return;
            Settings.Instance.SoundTypeForConnected = type;
            Settings.Instance.Save();
            var ring = GetRingSounds();
            var voice = GetTtsSoundsForOp("connected");
            UpdateEventSoundUI("connected", cb_type_connected, cb_file_connected, panel_random_connected, ic_random_connected,
                Settings.Instance.SoundTypeForConnected, Settings.Instance.SoundFileForConnected, Settings.Instance.RandomPoolForConnected, ring, voice);
        }

        private void cb_type_disconnected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var type = (cb_type_disconnected.SelectedItem as ComboBoxItem)?.Tag as string;
            if (type == null) return;
            Settings.Instance.SoundTypeForDisconnected = type;
            Settings.Instance.Save();
            var ring = GetRingSounds();
            var voice = GetTtsSoundsForOp("disconnected");
            UpdateEventSoundUI("disconnected", cb_type_disconnected, cb_file_disconnected, panel_random_disconnected, ic_random_disconnected,
                Settings.Instance.SoundTypeForDisconnected, Settings.Instance.SoundFileForDisconnected, Settings.Instance.RandomPoolForDisconnected, ring, voice);
        }

        private void cb_file_screenshot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var sel = cb_file_screenshot.SelectedItem as ComboBoxItem;
            Settings.Instance.SoundFileForScreenshot = sel?.Tag as string;
            Settings.Instance.Save();
        }
        private void cb_file_replay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var sel = cb_file_replay.SelectedItem as ComboBoxItem;
            Settings.Instance.SoundFileForReplaySaved = sel?.Tag as string;
            Settings.Instance.Save();
        }
        private void cb_file_rec_start_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var sel = cb_file_rec_start.SelectedItem as ComboBoxItem;
            Settings.Instance.SoundFileForRecordingStarted = sel?.Tag as string;
            Settings.Instance.Save();
        }
        private void cb_file_rec_stop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var sel = cb_file_rec_stop.SelectedItem as ComboBoxItem;
            Settings.Instance.SoundFileForRecordingStopped = sel?.Tag as string;
            Settings.Instance.Save();
        }
        private void cb_file_connected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var sel = cb_file_connected.SelectedItem as ComboBoxItem;
            Settings.Instance.SoundFileForConnected = sel?.Tag as string;
            Settings.Instance.Save();
        }
        private void cb_file_disconnected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsChangedByCode) return;
            var sel = cb_file_disconnected.SelectedItem as ComboBoxItem;
            Settings.Instance.SoundFileForDisconnected = sel?.Tag as string;
            Settings.Instance.Save();
        }

        void TryPlaySimpleSound(string file)
        {
            try
            {
                if (System.IO.Path.IsPathRooted(file) && File.Exists(file))
                {
                    var mp = new System.Windows.Media.MediaPlayer();
                    mp.Open(new Uri(file));
                    mp.Volume = 1.0;
                    mp.Play();
                    return;
                }
                var baseDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exeDir = System.IO.Path.GetDirectoryName(baseDir);
                var candidates = new List<string>();
                candidates.Add(System.IO.Path.Combine(exeDir, "sounds", file));
                candidates.Add(System.IO.Path.GetFullPath(System.IO.Path.Combine(exeDir, "..", "..", "..", "sounds", file)));
                candidates.Add(System.IO.Path.Combine(Environment.CurrentDirectory, "sounds", file));

                foreach (var p in candidates)
                {
                    if (File.Exists(p))
                    {
                        var mp = new System.Windows.Media.MediaPlayer();
                        mp.Open(new Uri(p));
                        mp.Volume = 1.0;
                        mp.Play();
                        break;
                    }
                }
            }
            catch { }
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
