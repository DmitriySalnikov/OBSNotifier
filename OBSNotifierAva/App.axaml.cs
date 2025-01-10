using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Ninja.WebSocketClient;
using OBSNotifier.Forms;
using OBSNotifier.Modules.Event;
using OBSWebsocketSharp;
using ReactiveUI;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
        public static readonly bool IsPortable = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".portable"));
        public static readonly string AppDataFolder = IsPortable ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        public static event EventHandler<ConnectionState>? ConnectionStateChanged;
        public static OBSWebsocket OBS = null!;
        public static ModuleManager Modules = null!;
        public static NotificationManager? Notifications;
        public static ConnectionState CurrentConnectionState { get; private set; }
        public static bool IsNeedToSkipNextConnectionNotifications = false;
        public DeferredActionWPF gc_collect = new(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);

        static DispatcherOperation? close_reconnect;
        static Task? reconnectThread;
        static CancellationTokenSource? reconnectCancellationToken;
        static bool isNeedToSkipDisconnectErrorPrinting = false;
        static List<NotifierWindow> openedWindows = [];

        VersionCheckerGitHub versionCheckerGitHub = new("DmitriySalnikov", "OBSNotifier", AppName, ShowMessageBoxForVersionCheck);
        SettingsWindow? settingsWindow;
        AboutWindow? aboutBox;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (Design.IsDesignMode)
            {
                OBS = new(OBSEventInvoke);
                Modules = new ModuleManager();
                logger ??= new Logger("");
                Thread.CurrentThread.CurrentUICulture = DefaultLanguage;
                Settings.Load();
                base.OnFrameworkInitializationCompleted();
                return;
            }

            IClassicDesktopStyleApplicationLifetime? desktop = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktop != null)
            {
                desktop.Exit += DesktopApp_Exit;
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                // Mobile?
                // singleViewPlatform.MainView = new MainView
                // {
                //     DataContext = new MainViewModel()
                // };
            }

#if DEBUG
            UtilsWinApi.CreateUnicodeConsole();
#endif
            logger ??= new Logger(Path.Combine(AppDataFolder, "logs/log.txt"));

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
                // TODO wait to close
                if (!Environment.CommandLine.Contains("--force_close"))
                    ShowMessageBox(Tr.MessageBoxApp.AlreadyRunning(AppNameSpaced), Tr.MessageBoxApp.AlreadyRunningTitle);
                else
                    LogError(Tr.MessageBoxApp.AlreadyRunning(AppNameSpaced));

                mutex.Dispose();

                Environment.ExitCode = -1;
                desktop?.Shutdown(-1);
                return;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (!Directory.Exists(AppDataFolder))
                Directory.CreateDirectory(AppDataFolder);

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

            OBS = new(OBSEventInvoke);
            OBS.Authorized += Obs_Connected;
            OBS.Disconnected += Obs_Disconnected;
            OBS.Events.ExitStarted += Obs_ExitStarted;

            Notifications = new NotificationManager(this, OBS);

            // Clear unused
            if (Settings.Instance.ClearUnusedModuleSettings())
                Settings.Instance.Save();

            // Select active modules
            Modules.UpdateActiveModules();

            // Update old settings
            Settings.Instance.PatchSavedSettings();

            // TODO
            // // Create tray icon
            // trayIcon = new()
            // {
            //     Icon = AppImages.obs_notifier_64px,
            //     Visible = true
            // };
            // trayIcon.DoubleClick += Menu_OpenSettingsWindow;

            ReconstructTrayMenu();

            if (Settings.Instance.FirstRun)
            {
                Settings.Instance.FirstRun = false;
                Settings.Instance.DisplayID = 0;
                Settings.Instance.Save();

                // TODO add locale
                MessageBoxManager
                    .GetMessageBoxStandard(
                        $"Welcome to {AppNameSpaced}!",
                        $"The {AppNameSpaced} will always be in the tray while it's running",
                        icon: MsBox.Avalonia.Enums.Icon.Info)
                    .ShowAsync()
                   ;// TODO .Wait();
                // TODO trayIcon.ShowBalloonTip(3000, $"{AppNameSpaced} Info", $"The {AppNameSpaced} will always be in the tray while it's running", System.Windows.Forms.ToolTipIcon.Info);
                Menu_OpenSettingsWindow();
            }
            else
            {
                // Connect to obs if previously connected
                if (Settings.Instance.IsManuallyConnected && !OBS.IsAuthorized)
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
            base.OnFrameworkInitializationCompleted();
        }

        private void DesktopApp_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log("App exiting");
            StopReconnection();
            Log("Force Saving");
            Settings.Instance?.Save(true);

            Log("Disconnecting");
            if (OBS != null)
            {
                OBS.Connected -= Obs_Connected;
                OBS.Disconnected -= Obs_Disconnected;
                OBS.Events.ExitStarted -= Obs_ExitStarted;
                _ = OBS.Disconnect();
            }

            Log("Clearing variables");
            gc_collect.Dispose();

            settingsWindow?.Close();
            settingsWindow = null;

            aboutBox?.Close();
            aboutBox = null;

            Modules?.Dispose();

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

        void OBSEventInvoke(Action action)
        {
            Dispatcher.UIThread.Invoke(action);
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

        static async Task<VersionCheckerGitHub.MSGDialogResult> ShowMessageBoxForVersionCheck(VersionCheckerGitHub.MSGType type, Dictionary<string, string> customData, Exception? ex)
        {
            string text = "";
            string caption = "";
            Icon icon = Icon.None;
            ButtonEnum buttons = ButtonEnum.Ok;

            switch (type)
            {
                case VersionCheckerGitHub.MSGType.InfoUpdateAvailable:
                    text = Tr.MessageBoxVersionCheck.NewVersionAvailable(customData["current_version"], customData["new_version"]);
                    caption = Tr.MessageBoxVersionCheck.NewVersionAvailableTitle(AppNameSpaced);
                    icon = Icon.Info;
                    buttons = ButtonEnum.YesNoCancel;
                    break;
                case VersionCheckerGitHub.MSGType.InfoUsingLatestVersion:
                    text = Tr.MessageBoxVersionCheck.LatestVersion(customData["current_version"]);
                    caption = Utils.TrErrorTitle();
                    icon = Icon.Info;
                    break;
                case VersionCheckerGitHub.MSGType.FailedToRequestInfo:
                    text = $"{Tr.MessageBoxVersionCheck.FailedRequest}\n\n{ex?.Message ?? string.Empty}";
                    caption = Utils.TrErrorTitle();
                    icon = Icon.Error;
                    break;
                case VersionCheckerGitHub.MSGType.FailedToGetInfo:
                    text = $"{Tr.MessageBoxVersionCheck.FailedParseInfo}\n\n{ex?.Message ?? string.Empty}";
                    caption = Utils.TrErrorTitle();
                    icon = Icon.Error;
                    break;
                case VersionCheckerGitHub.MSGType.FailedToProcessData:
                    text = $"{Tr.MessageBoxVersionCheck.FailedToCheck}\n\n{ex?.Message ?? string.Empty}";
                    caption = Utils.TrErrorTitle();
                    icon = Icon.Error;
                    break;
            }

            var res = await MessageBoxManager.GetMessageBoxStandard(caption, text, buttons, icon).ShowAsync();

            return res switch
            {
                ButtonResult.Ok => VersionCheckerGitHub.MSGDialogResult.OK,
                ButtonResult.Cancel => VersionCheckerGitHub.MSGDialogResult.Cancel,
                ButtonResult.Yes => VersionCheckerGitHub.MSGDialogResult.Yes,
                ButtonResult.No => VersionCheckerGitHub.MSGDialogResult.No,
                // TODO test. Ok or Cancel??
                ButtonResult.None => VersionCheckerGitHub.MSGDialogResult.Cancel,
                _ => VersionCheckerGitHub.MSGDialogResult.Cancel,
            };
        }

        void Menu_ShowAboutWindow()
        {
            if (aboutBox != null)
                return;

            aboutBox = new AboutWindow();
            aboutBox.Closed += (s, ev) => aboutBox = null;
            aboutBox.Show();
        }

        void Menu_OpenSettingsWindow()
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

        void Menu_CheckForUpdates()
        {
            versionCheckerGitHub.CheckForUpdates();
        }

        void Menu_OpenLogsFolder()
        {
            Utils.ProcessStartShell(Path.Combine(AppDataFolder, "logs"));
        }

        public static void RegisterWindow(NotifierWindow wnd)
        {
            if (!openedWindows.Contains(wnd))
            {
                openedWindows.Add(wnd);

                EventHandler remove = null!;
                remove = (object? s, EventArgs e) => { openedWindows.Remove(wnd); wnd.Closed -= remove; };
                wnd.Closed += remove;
            }
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

        public static async Task<ButtonResult> ShowMessageBox(string messageBoxText, string caption = "", ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.None, ButtonResult defaultResult = ButtonResult.None)
        {
            logger.Write($"MessageBox shown. Text: '{messageBoxText}', Caption: '{caption}', Button: '{button}', Icon: '{icon}', DefaultResult: '{defaultResult}'");

            var res = await MessageBoxManager.GetMessageBoxStandard(caption, messageBoxText, button, icon).ShowAsync();
            if (res == ButtonResult.None)
            {
                res = defaultResult;
            }

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
            /*
            // Create lang menu
            var lang_menu = new Forms.ToolStripMenuItem( Tr.TrayMenu.Languages);

            // Sort all except default one, so skip(1)
            var sorted_langs = Languages.Skip(1).ToArray();
            // Sort by codes
            Array.Sort(sorted_langs, (CultureInfo x, CultureInfo y) => x.Name.CompareTo(y.Name));

            var lang_hint = lang_menu.DropDownItems.Add(Tr.TrayMenu.LanguagesCompletionHint);
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

                var lang_item_str = l.Name == DefaultLanguage.Name ? l.NativeName : Tr.TrayMenu.LanguagesCompletionTemplate(l.NativeName, progress_tr, progress_apr);
                var item = lang_menu.DropDownItems.Add(lang_item_str, null, (s, e) => Language = l);

                // Highlight the currently selected language
                if (Language.Equals(l))
                {
                    // TODO Utils.UpdateContextItemStyle(item, System.Drawing.FontStyle.Bold);
                }
            }

            // Link to the translations page
            lang_menu.DropDownItems.Add("Help with translations!", null, (s, e) => Utils.ProcessStartShell("https://crowdin.com/project/obs-notifier"));

             */

            var lang_item = new NativeMenuItem(Tr.TrayMenu.Languages);
            var lang_menu = new NativeMenu();
            // Sort all except default one, so skip(1)
            var sorted_langs = Languages.Skip(1).ToArray();
            // Sort by codes
            Array.Sort(sorted_langs, (CultureInfo x, CultureInfo y) => x.Name.CompareTo(y.Name));

            lang_menu.Add(new NativeMenuItem(Tr.TrayMenu.LanguagesCompletionHint));

            // Don't forget to prepend(default)
            foreach (var l in sorted_langs.Prepend(Languages.First()))
            {
                var progress_tr = "~";
                var progress_apr = "~";

                if (TranslationProgress.TryGetValue(l.Name, out TranslationProgressData value))
                {
                    progress_tr = value.WholeProgress.ToString();
                    progress_apr = value.ApprovedProgress.ToString();
                }

                var lang_item_str = l.Name == DefaultLanguage.Name ? l.NativeName : Tr.TrayMenu.LanguagesCompletionTemplate(l.NativeName, progress_tr, progress_apr);
                lang_menu.Add(new NativeMenuItem(lang_item_str) { Command = ReactiveCommand.Create(() => Language = l) });

                // Highlight the currently selected language
                if (Language.Equals(l))
                {
                    // TODO change font to Bold
                    // Utils.UpdateContextItemStyle(item, System.Drawing.FontStyle.Bold);
                }
            }

            lang_menu.Add(new NativeMenuItem("Help with translations!") { Command = ReactiveCommand.Create(() => Utils.ProcessStartShell("https://crowdin.com/project/obs-notifier")) });
            lang_item.Menu = lang_menu;

            var icons = new TrayIcons();
            var icon = new TrayIcon() { Command = ReactiveCommand.Create(() => Menu_OpenSettingsWindow()) };
            var menu = new NativeMenu
            {
                new NativeMenuItem(Tr.TrayMenu.OpenSettings) { Command = ReactiveCommand.Create(()=>Menu_OpenSettingsWindow())},
                new NativeMenuItemSeparator(),
                lang_item,
                new NativeMenuItem(Tr.TrayMenu.CheckUpdates) { Command = ReactiveCommand.Create(()=>Menu_CheckForUpdates())},
                new NativeMenuItem(Tr.TrayMenu.OpenLogsFolder) { Command = ReactiveCommand.Create(()=>Menu_OpenLogsFolder())},
                new NativeMenuItem(Tr.TrayMenu.About(AppNameSpaced)) { Command = ReactiveCommand.Create(()=>Menu_ShowAboutWindow())},
                new NativeMenuItemSeparator(),
                new NativeMenuItem(Tr.TrayMenu.Exit)
                {
                    Command = ReactiveCommand.Create(()=>{
                        IClassicDesktopStyleApplicationLifetime? desktop = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                        if (desktop != null)
                        {
                            desktop.Shutdown();
                        }
                    })
                }
            };

            // TODO change font to Bold
            // Utils.UpdateContextItemStyle(trayIcon.ContextMenuStrip.Items[0], System.Drawing.FontStyle.Bold)

            icon.Menu = menu;
            icons.Add(icon);
            TrayIcon.SetIcons(this, icons);
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
                    Dispatcher.UIThread.Invoke(() => StopReconnection());
                }

                Dispatcher.UIThread.Invoke(() => ConnectionStateChanged?.Invoke(Current, newState));
                UpdateTrayStatus();
            }
        }

        static void UpdateTrayStatus()
        {
            //TrayIcon.GetIcons(Current)[0].
            /* TODO
            try
            {
                // max 128 chars
                switch (CurrentConnectionState)
                {
                    case ConnectionState.Connected:
                        trayIcon.Icon = AppImages.obs_notifier_connected_64px;
                        trayIcon.Text = $"{AppNameSpaced}:\n{Tr.TrayMenu.StatusConnected}";
                        break;
                    case ConnectionState.Disconnected:
                        trayIcon.Icon = AppImages.obs_notifier_64px;
                        trayIcon.Text = $"{AppNameSpaced}:\n{Tr.TrayMenu.StatusNotConnected}";
                        break;
                    case ConnectionState.TryingToReconnect:
                        trayIcon.Icon = AppImages.obs_notifier_reconnect_64px;
                        trayIcon.Text = $"{AppNameSpaced}:\n{Tr.TrayMenu.StatusTryingToReconnect}";
                        break;
                }
            }
            catch (Exception ex)
            {
                Log(ex);
                trayIcon.Text = $"{AppNameSpaced}";
            }
            */
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
                            await (OBS?.Disconnect() ?? Task.CompletedTask);
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

                    if ((OBS != null && OBS.IsAuthorized) || (reconnectCancellationToken == null || reconnectCancellationToken.IsCancellationRequested))
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
                return OBS.Connect(adrs, pass, EventSubscription.All);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex?.Message ?? string.Empty, Utils.TrErrorTitle(), ButtonEnum.Ok, Icon.Warning);
            }

            return Task.FromResult(false);
        }

        internal static void DisconnectFromOBS()
        {
            try
            {
                _ = OBS.Disconnect();
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
                            message = Tr.MessageBoxApp.KickedFailed + $"\n\n{wse.Message}";
                            stop_reconnection = true;
                            break;
                        case WebSocketCloseCode.AuthenticationFailed:
                            message = Tr.MessageBoxApp.AuthFailed + $"\n\n{wse.Message}";
                            stop_reconnection = true;
                            break;
                    }

                    if (stop_reconnection)
                    {
                        StopReconnection();
                        Settings.Instance.IsManuallyConnected = false;
                        DisconnectFromOBS();

                        // TODO test Warning icon (Exclamation)
                        ShowMessageBox(message, Utils.TrErrorTitle(), ButtonEnum.Ok, Icon.Warning);
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
                Dispatcher.UIThread.Invoke(() => { if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.Shutdown(0); });
            }
        }
    }
}
