using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string appGUID = "EAC71402-ACC2-40F1-A75A-4060C19E1F9F";
        Mutex mutex = new Mutex(false, "Global\\" + appGUID);

        public enum ConnectionState
        {
            Connected,
            Disconnected,
            TryingToReconnect,
        }

        static Logger logger;
        public static event EventHandler<ConnectionState> ConnectionStateChanged;
        public static OBSWebsocket obs;
        public static PluginManager plugins;
        public static NotificationManager notifications;
        public static ConnectionState CurrentConnectionState { get; private set; }
        public static bool IsNeedToSkipNextConnectionNotifications = false;
        public DeferredAction gc_collect = new DeferredAction(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);

        static System.Windows.Forms.NotifyIcon trayIcon;
        static DispatcherOperation close_reconnect;
        static Task reconnectThread;
        static CancellationTokenSource reconnectCancellationToken;
        static bool isNeedToSkipDisconnectErrorPrinting = false;

        VersionCheckerGitHub versionCheckerGitHub = new VersionCheckerGitHub("DmitriySalnikov", "OBSNotifier");
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

            if (!mutex.WaitOne(0, false))
            {
                mutex.Dispose();
                mutex = null;

                Environment.ExitCode = -1;
                ShowMessageBox("An instance of this application is already running. The application will be closed.", "Instance already running");
                Shutdown();
                return;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            logger = new Logger("log.txt");

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

            // Initialize Settings
            Settings.Load();
            plugins = new PluginManager();
            notifications = new NotificationManager(this, obs);

            // Clear unused
            if (Settings.Instance.ClearUnusedPluginSettings())
                Settings.Instance.Save();

            // Select current plugin
            if (!plugins.SelectCurrent(Settings.Instance.NotificationStyle))
            {
                // Select the default plugin if the previously used plugin is not found
                Settings.Instance.NotificationStyle = "Default";
                Settings.Instance.Save();
                plugins.SelectCurrent(Settings.Instance.NotificationStyle);
            }

            // Create tray icon
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_64px;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += Menu_OpenSettingsWindow;
            trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] {
                new System.Windows.Forms.MenuItem("Open Settings", Menu_OpenSettingsWindow),
                new System.Windows.Forms.MenuItem("Check for updates", Menu_CheckForUpdates),
                new System.Windows.Forms.MenuItem("About", Menu_ShowAboutWindow),
                new System.Windows.Forms.MenuItem("Exit", (s,e) => Shutdown()),
            });

            if (Settings.Instance.FirstRun)
            {
                Settings.Instance.FirstRun = false;
                Settings.Instance.DisplayID = WPFScreens.Primary.DeviceName;
                Settings.Instance.Save();

                trayIcon.ShowBalloonTip(3000, "OBS Notifier Info", "The OBS notifier will always be in the tray while it's running", System.Windows.Forms.ToolTipIcon.Info);
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

            plugins?.Dispose();
            plugins = null;

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
                    trayIcon.Text = "OBS Notifier:\nConnected";
                    break;
                case ConnectionState.Disconnected:
                    trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_64px;
                    trayIcon.Text = "OBS Notifier:\nNot connected";
                    break;
                case ConnectionState.TryingToReconnect:
                    trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_reconnect_64px;
                    trayIcon.Text = "OBS Notifier:\nTrying to reconnect";
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
                if (reconnectCancellationToken.IsCancellationRequested)
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

                if (obs.IsConnected || reconnectCancellationToken.IsCancellationRequested)
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

                Settings.Instance.Save();
                return obs.ConnectAsync(adrs, pass);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                            ShowMessageBox("Authentication failed.", "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
