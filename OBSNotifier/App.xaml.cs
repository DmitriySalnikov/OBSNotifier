using Ninja.WebSocketClient;
using OBSNotifier.Modules.Event;
using OBSWebsocketSharp;
using System.Windows;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public const string AppName = "OBSNotifier";
        public const string AppNameSpaced = "OBS Notifier";
        internal const string EncryptionKey = "0E38B5E89F6C4C7E9FC653E179F98E56";
        private const string appGUID = "EAC71402-ACC2-40F1-A75A-4060C19E1F9F";
        Mutex mutex = new(false, "Global\\" + appGUID);

        public enum ConnectionState
        {
            Connected,
            Disconnected,
            TryingToReconnect,
        }

        static Logger logger = null!;
        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        public static event EventHandler<ConnectionState>? ConnectionStateChanged;
        public static OBSWebsocket obs = null!;
        public static ModuleManager Modules = null!;
        public static NotificationManager? notifications;
        public static ConnectionState CurrentConnectionState { get; private set; }
        public static bool IsNeedToSkipNextConnectionNotifications = false;
        public DeferredActionWPF gc_collect = new(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);

        static Forms.NotifyIcon trayIcon = null!;
        static DispatcherOperation? close_reconnect;
        static Task? reconnectThread;
        static CancellationTokenSource? reconnectCancellationToken;
        static bool isNeedToSkipDisconnectErrorPrinting = false;

        VersionCheckerGitHub versionCheckerGitHub = new("DmitriySalnikov", "OBSNotifier", AppName, ShowMessageBoxForVersionCheck);
        SettingsWindow? settingsWindow;
        AboutBox1? aboutBox;

        private void Application_Startup(object? sender, StartupEventArgs ee)
        {
#if DEBUG
            UtilsWinApi.CreateUnicodeConsole();
#endif

            Forms.Application.EnableVisualStyles();
            Forms.Application.SetCompatibleTextRenderingDefault(false);
            logger ??= new Logger("logs/log.txt");

            // To load the module settings, it must know the available types of settings
            Modules = new ModuleManager();
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
                else
                    LogError(Utils.TrFormat("message_box_app_already_running", AppNameSpaced));

                mutex.Dispose();

                Environment.ExitCode = -1;
                Shutdown();
                return;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // Just log the message if the autorun script exists
            if (AutostartManager.IsScriptExists())
                AutostartManager.IsFileNeedToUpdate();

            // Global exception handlers
            // https://stackoverflow.com/a/10203030/8980874
            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            // Fix the current directory if app starts using autorun (in System32...)
            if (Environment.CurrentDirectory.Equals(Environment.GetFolderPath(Environment.SpecialFolder.System), StringComparison.CurrentCultureIgnoreCase))
            {
                string? path = Path.GetDirectoryName(GetType().Assembly.Location);
                if (path != null)
                {
                    Environment.CurrentDirectory = path;
                }
            }

            CurrentConnectionState = ConnectionState.Disconnected;

            obs = new(OBSEventInvoke);
            obs.Authorized += Obs_Connected;
            obs.Disconnected += Obs_Disconnected;
            obs.Events.ExitStarted += Obs_ExitStarted;

            notifications = new NotificationManager(this, obs);

            // Clear unused
            if (Settings.Instance.ClearUnusedModuleSettings())
                Settings.Instance.Save();

            // Select active modules
            Modules.UpdateActiveModules();

            // Update old settings
            Settings.Instance.PatchSavedSettings();

            // Create tray icon
            trayIcon = new()
            {
                Icon = AppImages.obs_notifier_64px,
                Visible = true
            };
            trayIcon.DoubleClick += Menu_OpenSettingsWindow;

            ReconstructTrayMenu();

            if (Settings.Instance.FirstRun)
            {
                Settings.Instance.FirstRun = false;
                Settings.Instance.DisplayID = WPFScreens.Primary.DeviceName;
                Settings.Instance.Save();

                trayIcon.ShowBalloonTip(3000, $"{AppNameSpaced} Info", $"The {AppNameSpaced} will always be in the tray while it's running", System.Windows.Forms.ToolTipIcon.Info);
                Menu_OpenSettingsWindow(this, EventArgs.Empty);
            }
            else
            {
                // Connect to obs if previously connected
                if (Settings.Instance.IsManuallyConnected && !obs.IsAuthorized)
                {
                    IsNeedToSkipNextConnectionNotifications = true;
                    ChangeConnectionState(ConnectionState.TryingToReconnect);
                }
            }

            UpdateTrayStatus();

            SetupVersionChecker(true);

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

        void OBSEventInvoke(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        private void Application_Exit(object? sender, ExitEventArgs e)
        {
            Log("App exiting");
            StopReconnection();
            Log("Force Saving");
            Settings.Instance?.Save(true);

            Log("Disconnecting");
            if (obs != null)
            {
                obs.Connected -= Obs_Connected;
                obs.Disconnected -= Obs_Disconnected;
                obs.Events.ExitStarted -= Obs_ExitStarted;
                _ = obs.Disconnect();
            }

            Log("Clearing variables");
            gc_collect.Dispose();

            settingsWindow?.Close();
            settingsWindow = null;

            aboutBox?.Close();
            aboutBox?.Dispose();
            aboutBox = null;

            Modules?.Dispose();

            trayIcon?.Dispose();

            close_reconnect?.Abort();
            close_reconnect = null;

            versionCheckerGitHub?.Dispose();

            Log("App closed");
            logger?.Dispose();
            mutex?.Dispose();

#if DEBUG
            UtilsWinApi.DeleteConsole();
#endif
        }

        internal void SetupVersionChecker(bool isSilent = true)
        {
            versionCheckerGitHub.VersionSkippedByUser += (s, e) =>
            {
                Settings.Instance.SkipVersion = e.SkippedVersion.ToString();
                Settings.Instance.Save();
            };

            // Get SkipVersion for updater
            try
            {
                if (!string.IsNullOrWhiteSpace(Settings.Instance.SkipVersion))
                    versionCheckerGitHub.SkipVersion = new Version(Settings.Instance.SkipVersion);
            }
            catch (Exception ex)
            {
                LogError("The SkipVersion string for the updater could not be parsed.");
                Log(ex);
            }

            versionCheckerGitHub.CheckForUpdates(isSilent);
        }

        static VersionCheckerGitHub.MSGDialogResult ShowMessageBoxForVersionCheck(VersionCheckerGitHub.MSGType type, Dictionary<string, string> customData, Exception? ex)
        {
            string text = "";
            string caption = "";
            MessageBoxImage icon = MessageBoxImage.None;
            MessageBoxButton buttons = MessageBoxButton.OK;

            switch (type)
            {
                case VersionCheckerGitHub.MSGType.InfoUpdateAvailable:
                    text = Utils.TrFormat("message_box_version_check_new_version_available", customData["current_version"], customData["new_version"]);
                    caption = Utils.TrFormat("message_box_version_check_new_version_available_title", AppNameSpaced);
                    icon = MessageBoxImage.Information;
                    buttons = MessageBoxButton.YesNoCancel;
                    break;
                case VersionCheckerGitHub.MSGType.InfoUsingLatestVersion:
                    text = Utils.TrFormat("message_box_version_check_latest_version", customData["current_version"]);
                    caption = Utils.TrErrorTitle();
                    icon = MessageBoxImage.Information;
                    break;
                case VersionCheckerGitHub.MSGType.FailedToRequestInfo:
                    text = $"{Utils.Tr("message_box_version_check_failed_request")}\n\n{ex?.Message ?? string.Empty}";
                    caption = Utils.TrErrorTitle();
                    icon = MessageBoxImage.Error;
                    break;
                case VersionCheckerGitHub.MSGType.FailedToGetInfo:
                    text = $"{Utils.Tr("message_box_version_check_failed_parse_info")}\n\n{ex?.Message ?? string.Empty}";
                    caption = Utils.TrErrorTitle();
                    icon = MessageBoxImage.Error;
                    break;
                case VersionCheckerGitHub.MSGType.FailedToProcessData:
                    text = $"{Utils.Tr("message_box_version_check_failed_to_check")}\n\n{ex?.Message ?? string.Empty}";
                    caption = Utils.TrErrorTitle();
                    icon = MessageBoxImage.Error;
                    break;
            }

            MessageBoxResult res = MessageBox.Show(text, caption, buttons, icon, MessageBoxResult.Cancel);

            return res switch
            {
                MessageBoxResult.None => VersionCheckerGitHub.MSGDialogResult.OK,
                MessageBoxResult.OK => VersionCheckerGitHub.MSGDialogResult.OK,
                MessageBoxResult.Cancel => VersionCheckerGitHub.MSGDialogResult.Cancel,
                MessageBoxResult.Yes => VersionCheckerGitHub.MSGDialogResult.Yes,
                MessageBoxResult.No => VersionCheckerGitHub.MSGDialogResult.No,
                _ => VersionCheckerGitHub.MSGDialogResult.Cancel,
            };
        }

        void Menu_ShowAboutWindow(object? sender, EventArgs e)
        {
            if (aboutBox != null)
                return;

            aboutBox = new AboutBox1();
            aboutBox.FormClosed += (s, ev) => aboutBox = null;
            aboutBox.ShowDialog();
        }

        void Menu_OpenSettingsWindow(object? sender, EventArgs e)
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

        void Menu_CheckForUpdates(object? sender, EventArgs e)
        {
            versionCheckerGitHub.CheckForUpdates();
        }

        void Menu_OpenLogsFolder(object? sender, EventArgs e)
        {
            Utils.ProcessStartShell(Path.Combine(AppDataFolder, "logs"));
        }

        public static void Log(string txt)
        {
            logger.Write(txt);
        }

        public static void LogError(string txt)
        {
            logger.WriteError(txt);
        }

        public static void Log(Exception ex)
        {
            logger.Write(ex);
        }

        void GlobalUnhandledExceptionHandler(object? sender, UnhandledExceptionEventArgs e)
        {
            LogError("--- The program crashed due to an exception ---");
            Log((Exception)e.ExceptionObject);
            logger.Dispose();
        }

        public static MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            logger.Write($"MessageBox shown. Text: '{messageBoxText}', Caption: '{caption}', Button: '{button}', Icon: '{icon}', DefaultResult: '{defaultResult}', Options: '{options}'");
            MessageBoxResult res = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            logger.Write($"MessageBox result for box with Text: '{messageBoxText}' and Caption: '{caption}' is: '{res}'");

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
            if (trayIcon == null)
                return;

            // Create lang menu
            var lang_menu = new Forms.ToolStripMenuItem(Utils.Tr("tray_menu_languages"));

            // Sort all except default one, so skip(1)
            var sorted_langs = Languages.Skip(1).ToArray();
            // Sort by codes
            Array.Sort(sorted_langs, (CultureInfo x, CultureInfo y) => x.Name.CompareTo(y.Name));

            var lang_hint = lang_menu.DropDownItems.Add(Utils.Tr("tray_menu_languages_completion_hint"));
            lang_hint.Enabled = false;

            // Don't forget to prepend(default)
            foreach (var l in sorted_langs.Prepend(Languages.First()))
            {
                var progress_tr = "~";
                var progress_apr = "~";

                if (TranslationProgress.TryGetValue(l.Name, out Size value))
                {
                    progress_tr = value.Width.ToString();
                    progress_apr = value.Height.ToString();
                }

                var lang_item_str = l.Name == DefaultLanguage.Name ? l.NativeName : Utils.TrFormat("tray_menu_languages_completion_template", l.NativeName, progress_tr, progress_apr);
                var item = lang_menu.DropDownItems.Add(lang_item_str, null, (s, e) => Language = l);

                // Highlight the currently selected language
                if (Language.Equals(l))
                {
                    Utils.UpdateContextItemStyle(item, System.Drawing.FontStyle.Bold);
                }
            }

            // Link to the translations page
            lang_menu.DropDownItems.Add("Help with translations!", null, (s, e) => Utils.ProcessStartShell("https://crowdin.com/project/obs-notifier"));

            trayIcon.ContextMenuStrip?.Dispose();
            trayIcon.ContextMenuStrip = new();

            trayIcon.ContextMenuStrip.Items.AddRange(new Forms.ToolStripItem[] {
                    new Forms.ToolStripMenuItem(Utils.Tr("tray_menu_open_settings"), null, Menu_OpenSettingsWindow),
                    new Forms.ToolStripSeparator(),
                    lang_menu,
                    new Forms.ToolStripMenuItem(Utils.Tr("tray_menu_check_updates"), null, Menu_CheckForUpdates),
                    new Forms.ToolStripMenuItem(Utils.Tr("tray_menu_open_logs_folder"),  null, Menu_OpenLogsFolder),
                    new Forms.ToolStripMenuItem(Utils.TrFormat("tray_menu_about", AppNameSpaced), null, Menu_ShowAboutWindow),
                    new Forms.ToolStripSeparator(),
                    new Forms.ToolStripMenuItem(Utils.Tr("tray_menu_exit"), null, (s, e) => Shutdown())
                });

            Utils.UpdateContextItemStyle(trayIcon.ContextMenuStrip.Items[0], System.Drawing.FontStyle.Bold);
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
            if (trayIcon == null)
                return;

            try
            {
                // max 128 chars
                switch (CurrentConnectionState)
                {
                    case ConnectionState.Connected:
                        trayIcon.Icon = AppImages.obs_notifier_connected_64px;
                        trayIcon.Text = $"{AppNameSpaced}:\n{Utils.Tr("tray_menu_status_connected")}";
                        break;
                    case ConnectionState.Disconnected:
                        trayIcon.Icon = AppImages.obs_notifier_64px;
                        trayIcon.Text = $"{AppNameSpaced}:\n{Utils.Tr("tray_menu_status_not_connected")}";
                        break;
                    case ConnectionState.TryingToReconnect:
                        trayIcon.Icon = AppImages.obs_notifier_reconnect_64px;
                        trayIcon.Text = $"{AppNameSpaced}:\n{Utils.Tr("tray_menu_status_trying_to_reconnect")}";
                        break;
                }
            }
            catch (Exception ex)
            {
                Log(ex);
                trayIcon.Text = $"{AppNameSpaced}";
            }
        }

        static async void ReconnectionThread()
        {
            var attempts = 0;
            isNeedToSkipDisconnectErrorPrinting = false;
            Log("Reconnection Thread started.");

            try
            {

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
                        var res = await ConnectToOBS(Settings.Instance.ServerAddress, Utils.DecryptString(Settings.Instance.Password, EncryptionKey) ?? "");
                        if (!res)
                            await (obs?.Disconnect() ?? Task.CompletedTask);
                    }
                    catch (Exception ex)
                    {
                        if (attempts < 5)
                        {
                            Log(ex?.Message ?? string.Empty);
                        }
                        else if (attempts == 5)
                        {
                            Log(ex?.Message ?? string.Empty);
                            Log("5 Connection errors are printed. The next errors will not be displayed.");
                            isNeedToSkipDisconnectErrorPrinting = true;
                        }

                    }

                    if ((obs != null && obs.IsAuthorized) || (reconnectCancellationToken == null || reconnectCancellationToken.IsCancellationRequested))
                    {
                        isNeedToSkipDisconnectErrorPrinting = false;
                        return;
                    }

                    Thread.Sleep(4000);
                }
            }
            finally
            {
                Log("Reconnection Thread stopped.");
            }
        }

        static void StopReconnection()
        {
            if (reconnectCancellationToken != null)
            {
                reconnectCancellationToken.Cancel();
                reconnectThread?.Wait();

                reconnectCancellationToken.Dispose();
                reconnectThread?.Dispose();
                reconnectThread = null;
                reconnectCancellationToken = null;
            }
        }

        internal static Task<bool> ConnectToOBS(string adr, string pass)
        {
            var adrs = adr;
            try
            {
                if (string.IsNullOrWhiteSpace(adrs))
                    adrs = "ws://localhost:4455";
                if (!adrs.StartsWith("ws://"))
                    adrs = "ws://" + adrs;

                if (CurrentConnectionState != ConnectionState.TryingToReconnect)
                    Settings.Instance.Save();
                return obs.Connect(adrs, pass, EventSubscription.All);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex?.Message ?? string.Empty, Utils.TrErrorTitle(), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return Task.FromResult(false);
        }

        internal static void DisconnectFromOBS()
        {
            try
            {
                _ = obs.Disconnect();
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

        private void Obs_Connected(object? sender, EventArgs e)
        {
            Log($"Connected to OBS");
            ChangeConnectionState(ConnectionState.Connected);
            Settings.Instance.IsManuallyConnected = true;
        }

        private void Obs_Disconnected(object? sender, OBSDisconnectInfo e)
        {
            if (isNeedToSkipDisconnectErrorPrinting)
                return;

            if (e.Exception is WebSocketClosedException wse && wse.CloseCode.HasValue)
            {
                Log(wse);

                if ((int)wse.CloseCode < 4000)
                {
                    Log($"Disconnected from OBS: {wse.CloseCode.GetType().Name}.{wse.CloseCode} ({(int)wse.CloseCode})");

                    switch (wse.CloseCode)
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
                    var ee = (WebSocketCloseCode)wse.CloseCode;
                    Log($"Disconnected from OBS: {ee.GetType().Name}.{ee} ({(int)ee})");

                    var stop_reconnection = false;
                    string message = string.Empty;
                    switch (ee)
                    {
                        case WebSocketCloseCode.UnknownReason:
                        case WebSocketCloseCode.MessageDecodeError:
                        case WebSocketCloseCode.MissingDataField:
                        case WebSocketCloseCode.InvalidDataFieldType:
                        case WebSocketCloseCode.InvalidDataFieldValue:
                        case WebSocketCloseCode.UnknownOpCode:
                        case WebSocketCloseCode.NotIdentified:
                        case WebSocketCloseCode.AlreadyIdentified:
                        case WebSocketCloseCode.UnsupportedRpcVersion:
                        case WebSocketCloseCode.UnsupportedFeature:
                            // reconnect
                            break;
                        case WebSocketCloseCode.SessionInvalidated:
                            message = Utils.Tr("message_box_app_kicked_failed") + $"\n\n{wse.Message}";
                            stop_reconnection = true;
                            break;
                        case WebSocketCloseCode.AuthenticationFailed:
                            message = Utils.Tr("message_box_app_auth_failed") + $"\n\n{wse.Message}";
                            stop_reconnection = true;
                            break;
                    }

                    if (stop_reconnection)
                    {
                        StopReconnection();
                        Settings.Instance.IsManuallyConnected = false;
                        DisconnectFromOBS();

                        ShowMessageBox(message, Utils.TrErrorTitle(), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            else
            {
                if (e.Exception != null)
                    Log(e.Exception);
                else
                    Log("Disconnected from OBS");
            }

            if (Settings.Instance.IsManuallyConnected)
                ChangeConnectionState(ConnectionState.TryingToReconnect);
            else
                ChangeConnectionState(ConnectionState.Disconnected);
        }

        private void Obs_ExitStarted(object? sender, EventArgs e)
        {
            Log("OBS is about to close");

            if (Settings.Instance.IsCloseOnOBSClosing && settingsWindow == null)
            {
                StopReconnection();
                Dispatcher.BeginInvoke(() => Shutdown());
            }
        }
    }
}
