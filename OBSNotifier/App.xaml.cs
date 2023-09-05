using OBSNotifier.Modules;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public const string AppName = "OBSNotifier";
        public const string AppNameSpaced = "OBS Notifier";
        private const string appGUID = "EAC71402-ACC2-40F1-A75A-4060C19E1F9F";
        Mutex mutex = new Mutex(false, "Global\\" + appGUID);

        public enum ConnectionState
        {
            Connected,
            Disconnected,
            TryingToReconnect,
        }

        static Logger logger;
        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        public static event EventHandler<ConnectionState> ConnectionStateChanged;
        public static OBSWebsocket obs;
        public static ModuleManager modules;
        public static NotificationManager notifications;
        public static ConnectionState CurrentConnectionState { get; private set; }
        public static bool IsNeedToSkipNextConnectionNotifications = false;
        public DeferredAction gc_collect = new DeferredAction(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);

        static System.Windows.Forms.NotifyIcon trayIcon;
        static DispatcherOperation close_reconnect;
        static Task reconnectThread;
        static CancellationTokenSource reconnectCancellationToken;
        static bool isNeedToSkipDisconnectErrorPrinting = false;

        VersionCheckerGitHub versionCheckerGitHub = new VersionCheckerGitHub("DmitriySalnikov", "OBSNotifier", AppName);
        SettingsWindow settingsWindow;
        AboutBox1 aboutBox;

        // TODO mb can be fixed by updating obs-websocket-dotnet, but currently 1 click on "Connect" can spawn 3 Auth Failed boxes
        Dictionary<int, FrequentMsgPair> frequentMessageBoxBlocker = new Dictionary<int, FrequentMsgPair>();
        class FrequentMsgPair
        {
            public bool IsNotFrequent;
            public DeferredAction ResetAction;

            public FrequentMsgPair()
            {
                IsNotFrequent = true;
                ResetAction = new DeferredAction(() => IsNotFrequent = true, 2500);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs ee)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // Initialize Settings
            Settings.Load();

            LanguageChanged += App_LanguageChanged;
            var start_ui_lang = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = DefaultLanguage;
            SelectLanguageOnStart(start_ui_lang);

            if (!mutex.WaitOne(0, false))
            {
                if (!Environment.CommandLine.Contains("--force_close"))
                    ShowMessageBox(Utils.TrFormat("message_box_app_already_running", AppNameSpaced), Utils.Tr("message_box_app_already_running_title"));

                mutex.Dispose();
                mutex = null;

                Environment.ExitCode = -1;
                Shutdown();
                return;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            logger = new Logger("logs/log.txt");

            // Just log the message if the autorun script exists
            if (AutostartScriptManager.IsScriptExists())
                AutostartScriptManager.IsFileNeedToUpdate();

            // Global exception handlers
            // https://stackoverflow.com/a/10203030/8980874
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            // Fix the current directory if app starts using autorun (in System32...)
            if (Environment.CurrentDirectory.ToLower() == Environment.GetFolderPath(Environment.SpecialFolder.System).ToLower())
                Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);

            CurrentConnectionState = ConnectionState.Disconnected;

            obs = new OBSWebsocket();
            obs.WSTimeout = TimeSpan.FromMilliseconds(500);
            obs.Connected += Obs_Connected;
            obs.Disconnected += Obs_Disconnected;
            obs.ExitStarted += Obs_ExitStarted;

            modules = new ModuleManager();
            notifications = new NotificationManager(this, obs);

            // Clear unused
            if (Settings.Instance.ClearUnusedModuleSettings())
                Settings.Instance.Save();

            // Select current module
            if (!modules.SelectCurrent(Settings.Instance.NotificationModule))
            {
                // Select the default module if the previously used module is not found
                Settings.Instance.NotificationModule = "Default";
                Settings.Instance.Save();
                modules.SelectCurrent(Settings.Instance.NotificationModule);
            }

            // Update old settings
            Settings.Instance.PatchSavedSettings();

            // Create tray icon
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_64px;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += Menu_OpenSettingsWindow;

            ReconstructTrayMenu();

            if (Settings.Instance.FirstRun)
            {
                Settings.Instance.FirstRun = false;
                Settings.Instance.DisplayID = WPFScreens.Primary.DeviceName;
                Settings.Instance.Save();

                trayIcon.ShowBalloonTip(3000, $"{AppNameSpaced} Info", $"The {AppNameSpaced} will always be in the tray while it's running", System.Windows.Forms.ToolTipIcon.Info);
                Menu_OpenSettingsWindow(this, null);
            }
            else
            {
                // Connect to obs if previously connected
                if (Settings.Instance.IsManuallyConnected && !obs.IsConnected)
                {
                    IsNeedToSkipNextConnectionNotifications = true;
                    ChangeConnectionState(ConnectionState.TryingToReconnect);
                }
            }

            UpdateTrayStatus();

            versionCheckerGitHub.MessageBoxShown += VersionCheckerGitHub_MessageBoxShown;
            versionCheckerGitHub.VersionSkippedByUser += VersionCheckerGitHub_VersionSkippedByUser;

            // Get SkipVersion for updater
            try { versionCheckerGitHub.SkipVersion = new Version(Settings.Instance.SkipVersion); }
            catch (Exception ex) { logger.Write("The SkipVersion string for the updater could not be parsed."); logger.Write(ex.Message); }

            versionCheckerGitHub.CheckForUpdates(true);

            // Debug print all languages
#if false
            Log(string.Join(", ", TranslationProgress
                .OrderBy((i)=> new CultureInfo(i.Key).EnglishName)
                .OrderByDescending((i) => i.Value.Height)
                .OrderByDescending((i)=>i.Value.Width)
                .Select((i)=> new CultureInfo(i.Key).EnglishName)
                .Prepend(DefaultLanguage.EnglishName)
                .Take(8))
                + $" and {Languages.Count - 8} other languages");
#endif
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Log("App exit");
            StopReconnection();

            if (obs != null)
            {
                obs.Connected -= Obs_Connected;
                obs.Disconnected -= Obs_Disconnected;
                obs.Disconnect();
                obs = null;
            }

            gc_collect.Dispose();
            gc_collect = null;

            settingsWindow?.Close();
            settingsWindow = null;

            aboutBox?.Close();
            aboutBox?.Dispose();
            aboutBox = null;

            modules?.Dispose();
            modules = null;

            trayIcon?.Dispose();
            trayIcon = null;

            close_reconnect?.Abort();
            close_reconnect = null;

            versionCheckerGitHub.Dispose();
            versionCheckerGitHub = null;

            logger?.Dispose();
            logger = null;

            Settings.Instance?.Save(true);
            mutex?.Dispose();
        }

        void Menu_ShowAboutWindow(object sender, EventArgs e)
        {
            if (aboutBox != null)
                return;

            aboutBox = new AboutBox1();
            aboutBox.FormClosed += (s, ev) => aboutBox = null;
            aboutBox.ShowDialog();
        }

        void Menu_OpenSettingsWindow(object sender, EventArgs e)
        {
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Closed += (ss, evv) => { settingsWindow = null; gc_collect.CallDeferred(); };
                settingsWindow.Show();
            }
            else
            {
                settingsWindow.Close();
            }
        }

        void Menu_CheckForUpdates(object sender, EventArgs e)
        {
            versionCheckerGitHub.CheckForUpdates();
        }

        void Menu_OpenLogsFolder(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(AppDataFolder, "logs"));
        }

        public static void Log(string txt)
        {
            logger?.Write(txt);
        }

        public static void Log(Exception ex)
        {
            logger.Write(ex);
        }

        void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log("--- The program crashed due to an exception ---");
            Log((Exception)e.ExceptionObject);
        }

        public static MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            logger?.Write($"MessageBox shown. Text: '{messageBoxText}', Caption: '{caption}', Button: '{button}', Icon: '{icon}', DefaultResult: '{defaultResult}', Options: '{options}'");
            MessageBoxResult res = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            logger?.Write($"MessageBox result for box with Text: '{messageBoxText}' and Caption: '{caption}' is: '{res}'");

            return res;
        }

        void SelectLanguageOnStart(CultureInfo start_ui_info)
        {
            // Select saved language, save system language or select available language
            if (Settings.Instance.Language != null && Languages.Contains(Settings.Instance.Language))
            {
                Language = Settings.Instance.Language;
            }
            else
            {
                // Find the exact language
                if (Languages.Contains(start_ui_info))
                {
                    Language = start_ui_info;
                }
                // Find a similar language
                else
                {
                    var similarLang = Languages.Where((lang) => lang.TwoLetterISOLanguageName == start_ui_info.TwoLetterISOLanguageName).First();

                    if (similarLang != null)
                    {
                        Language = similarLang;
                    }
                }
            }
        }

        internal void ReconstructTrayMenu()
        {
            if (trayIcon != null)
            {
                // Create lang menu
                var lang_menu = new System.Windows.Forms.MenuItem(Utils.Tr("tray_menu_languages"));

                // Sort all except default one, so skip(1)
                var sorted_langs = Languages.Skip(1).ToArray();
                // Sort by codes
                Array.Sort(sorted_langs, (CultureInfo x, CultureInfo y) => x.Name.CompareTo(y.Name));

                var lang_hint = lang_menu.MenuItems.Add(Utils.Tr("tray_menu_languages_completion_hint"));
                lang_hint.Enabled = false;

                // Don't forget to prepend(default)
                foreach (var l in sorted_langs.Prepend(Languages.First()))
                {
                    var progress_tr = "~";
                    var progress_apr = "~";

                    if (TranslationProgress.ContainsKey(l.Name))
                    {
                        var sz = TranslationProgress[l.Name];
                        progress_tr = sz.Width.ToString();
                        progress_apr = sz.Height.ToString();
                    }

                    var lang_item_str = l.Name == DefaultLanguage.Name ? l.NativeName : Utils.TrFormat("tray_menu_languages_completion_template", l.NativeName, progress_tr, progress_apr);
                    var item = lang_menu.MenuItems.Add(lang_item_str, (s, e) => Language = l);

                    // Highlight the currently selected language
                    if (Language.Equals(l))
                    {
                        item.DefaultItem = true;
                    }
                }

                // Link to the translations page
                lang_menu.MenuItems.Add("Help with translations!", (s, e) => Process.Start("https://crowdin.com/project/obs-notifier"));


                trayIcon.ContextMenu?.MenuItems.Clear();

                trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] {
                    new System.Windows.Forms.MenuItem(Utils.Tr("tray_menu_open_settings"), Menu_OpenSettingsWindow),
                    new System.Windows.Forms.MenuItem("-"),
                    lang_menu,
                    new System.Windows.Forms.MenuItem(Utils.Tr("tray_menu_check_updates"), Menu_CheckForUpdates),
                    new System.Windows.Forms.MenuItem(Utils.Tr("tray_menu_open_logs_folder"), Menu_OpenLogsFolder),
                    new System.Windows.Forms.MenuItem(Utils.TrFormat("tray_menu_about", AppNameSpaced), Menu_ShowAboutWindow),
                    new System.Windows.Forms.MenuItem("-"),
                    new System.Windows.Forms.MenuItem(Utils.Tr("tray_menu_exit"), (s,e) => Shutdown()),
                });

                trayIcon.ContextMenu.MenuItems[0].DefaultItem = true;
            }
        }

        static void ChangeConnectionState(ConnectionState newState)
        {
            if (CurrentConnectionState != newState)
            {
                CurrentConnectionState = newState;

                if (newState == ConnectionState.TryingToReconnect)
                {
                    close_reconnect?.Abort();
                    StopReconnection();

                    reconnectCancellationToken = new CancellationTokenSource();
                    reconnectThread = Task.Run(ReconnectionThread, reconnectCancellationToken.Token);
                }
                else
                {
                    close_reconnect?.Abort();
                    close_reconnect = Current.InvokeAction(() => StopReconnection());
                }

                Current.InvokeAction(() => ConnectionStateChanged?.Invoke(Current, newState));
                UpdateTrayStatus();
            }
        }

        static void UpdateTrayStatus()
        {
            switch (CurrentConnectionState)
            {
                case ConnectionState.Connected:
                    trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_connected_64px;
                    trayIcon.Text = $"{AppNameSpaced}:\n{Utils.Tr("tray_menu_status_connected")}";
                    break;
                case ConnectionState.Disconnected:
                    trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_64px;
                    trayIcon.Text = $"{AppNameSpaced}:\n{Utils.Tr("tray_menu_status_not_connected")}";
                    break;
                case ConnectionState.TryingToReconnect:
                    trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_reconnect_64px;
                    trayIcon.Text = $"{AppNameSpaced}:\n{Utils.Tr("tray_menu_status_trying_to_reconnect")}";
                    break;
            }
        }

        static async void ReconnectionThread()
        {
            var attempts = 0;
            isNeedToSkipDisconnectErrorPrinting = false;
            Log("Reconnection Thread started.");

            while (true)
            {
                attempts++;
                if (reconnectCancellationToken == null || reconnectCancellationToken.IsCancellationRequested)
                {
                    isNeedToSkipDisconnectErrorPrinting = false;
                    return;
                }

                try
                {
                    await ConnectToOBS(Settings.Instance.ServerAddress, Utils.DecryptString(Settings.Instance.Password));
                }
                catch (Exception ex)
                {
                    if (attempts < 5)
                    {
                        Log(ex.Message);
                    }
                    else if (attempts == 5)
                    {
                        Log(ex.Message);
                        Log("5 Connection errors are printed. The next errors will not be displayed.");
                        isNeedToSkipDisconnectErrorPrinting = true;
                    }

                }

                if (obs.IsConnected || (reconnectCancellationToken == null || reconnectCancellationToken.IsCancellationRequested))
                {
                    isNeedToSkipDisconnectErrorPrinting = false;
                    return;
                }

                Thread.Sleep(4000);
            }
        }

        static void StopReconnection()
        {
            if (reconnectCancellationToken != null)
            {
                reconnectCancellationToken.Cancel();
                reconnectThread.Wait();

                reconnectCancellationToken.Dispose();
                reconnectThread.Dispose();
                reconnectThread = null;
                reconnectCancellationToken = null;
            }
        }

        internal static Task ConnectToOBS(string adr, string pas)
        {
            var adrs = adr;
            try
            {
                if (string.IsNullOrWhiteSpace(adrs))
                    adrs = "ws://localhost:4455";
                if (!adrs.StartsWith("ws://"))
                    adrs = "ws://" + adrs;
                var pass = pas;

                if (CurrentConnectionState != ConnectionState.TryingToReconnect)
                    Settings.Instance.Save();
                return obs.ConnectAsync(adrs, pass);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message, Utils.Tr("message_box_error_title"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return Task.CompletedTask;
        }

        internal static void DisconnectFromOBS()
        {
            try
            {
                obs.Disconnect();
            }
            catch (Exception ex)
            {
                Log(ex);
            }

            if (CurrentConnectionState == ConnectionState.TryingToReconnect)
                IsNeedToSkipNextConnectionNotifications = true;

            ChangeConnectionState(ConnectionState.Disconnected);
            Settings.Instance.Save();
        }

        private void Obs_Connected(object sender, EventArgs e)
        {
            Log($"Connected to OBS");
            ChangeConnectionState(ConnectionState.Connected);
            Settings.Instance.IsManuallyConnected = true;
        }

        private void Obs_Disconnected(object sender, ObsDisconnectionInfo e)
        {
            if (isNeedToSkipDisconnectErrorPrinting)
                return;

            if ((int)e.ObsCloseCode < 4000)
            {
                var ee = (System.Net.WebSockets.WebSocketCloseStatus)e.ObsCloseCode;
                Log($"Disconnected from OBS: {ee}");

                switch (ee)
                {
                    case System.Net.WebSockets.WebSocketCloseStatus.NormalClosure:
                    case System.Net.WebSockets.WebSocketCloseStatus.EndpointUnavailable:
                    case System.Net.WebSockets.WebSocketCloseStatus.ProtocolError:
                    case System.Net.WebSockets.WebSocketCloseStatus.InvalidMessageType:
                    case System.Net.WebSockets.WebSocketCloseStatus.Empty:
                    case System.Net.WebSockets.WebSocketCloseStatus.InvalidPayloadData:
                    case System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation:
                    case System.Net.WebSockets.WebSocketCloseStatus.MessageTooBig:
                    case System.Net.WebSockets.WebSocketCloseStatus.MandatoryExtension:
                    case System.Net.WebSockets.WebSocketCloseStatus.InternalServerError:
                        // no need to mark connection as manually disconnected and prevent reconnect
                        // Settings.Instance.IsConnected = false;
                        break;
                }
            }
            else
            {
                Log($"Disconnected from OBS: {e.ObsCloseCode}");

                switch (e.ObsCloseCode)
                {
                    case ObsCloseCodes.UnknownReason:
                    case ObsCloseCodes.MessageDecodeError:
                    case ObsCloseCodes.MissingDataField:
                    case ObsCloseCodes.InvalidDataFieldType:
                    case ObsCloseCodes.InvalidDataFieldValue:
                    case ObsCloseCodes.UnknownOpCode:
                    case ObsCloseCodes.NotIdentified:
                    case ObsCloseCodes.AlreadyIdentified:
                    case ObsCloseCodes.UnsupportedRpcVersion:
                    case ObsCloseCodes.SessionInvalidated:
                    case ObsCloseCodes.UnsupportedFeature:
                        break;
                    case ObsCloseCodes.AuthenticationFailed:
                        if (IsNotTooFrequentMessage((int)e.ObsCloseCode))
                            ShowMessageBox(Utils.Tr("message_box_app_auth_failed"), Utils.Tr("message_box_error_title"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        StopReconnection();

                        Settings.Instance.IsManuallyConnected = false;
                        DisconnectFromOBS();
                        break;
                }
            }

            if (Settings.Instance.IsManuallyConnected)
                ChangeConnectionState(ConnectionState.TryingToReconnect);
            else
                ChangeConnectionState(ConnectionState.Disconnected);
        }

        // TODO it can be deleted if the "authorization error" error does not appear many times when the connection button is pressed once
        bool IsNotTooFrequentMessage(int messageId)
        {
            if (!frequentMessageBoxBlocker.ContainsKey(messageId))
            {
                frequentMessageBoxBlocker.Add(messageId, new FrequentMsgPair());
            }

            if (frequentMessageBoxBlocker[messageId].IsNotFrequent)
            {
                frequentMessageBoxBlocker[messageId].IsNotFrequent = false;
                frequentMessageBoxBlocker[messageId].ResetAction.CallDeferred();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Obs_ExitStarted(object sender, EventArgs e)
        {
            Log("OBS is about to close");

            if (Settings.Instance.IsCloseOnOBSClosing && settingsWindow == null)
            {
                StopReconnection();
                this.InvokeAction(() => Shutdown());
            }
        }

        private void VersionCheckerGitHub_VersionSkippedByUser(object sender, VersionCheckerGitHub.VersionSkipByUserData e)
        {
            Settings.Instance.SkipVersion = e.SkippedVersion.ToString();
            Settings.Instance.Save();
        }

        private void VersionCheckerGitHub_MessageBoxShown(object sender, VersionCheckerGitHub.ShowMessageBoxEventData e)
        {
            logger?.Write($"MessageBox shown. Text: '{e.Message}', Caption: '{e.Caption}', Button: '{e.Button}', Icon: '{e.Icon}', DefaultResult: '{e.DefaultResult}', Options: '{e.Options}', Result: '{e.Result}'");
        }
    }
}
