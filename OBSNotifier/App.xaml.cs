using Newtonsoft.Json;
using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
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

        static System.Windows.Forms.NotifyIcon trayIcon;
        SettingsWindow settingsWindow;
        public DeferredAction gc_collect = new DeferredAction(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);
        static DispatcherOperation close_reconnect;
        static Task reconnectThread;
        static CancellationTokenSource reconnectCancellationToken;
        AboutBox1 aboutBox;

        #region Version Checking

        private WebClient updateClient = null;
        private bool startupUpdateCheck = true;

        #endregion

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

            // Fix the current directory if app starts using autorun (in System32...)
            if (Environment.CurrentDirectory.ToLower() == Environment.GetFolderPath(Environment.SpecialFolder.System).ToLower())
                Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);

            CurrentConnectionState = ConnectionState.Disconnected;

            obs = new OBSWebsocket();
            obs.WSTimeout = TimeSpan.FromMilliseconds(1000);
            obs.Connected += Obs_Connected;
            obs.Disconnected += Obs_Disconnected;
            obs.OBSExit += Obs_OBSExit;

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
                if (Settings.Instance.IsConnected && !obs.IsConnected)
                {
                    IsNeedToSkipNextConnectionNotifications = true;
                    ChangeConnectionState(ConnectionState.TryingToReconnect);
                }
            }

            UpdateTrayStatus();

            CheckForUpdates();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
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

            updateClient?.Dispose();
            updateClient = null;

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
            CheckForUpdates();
        }

        public static void Log(string txt)
        {
            logger?.Write(txt);
        }

        public static void Log(Exception ex)
        {
            logger.Write(ex);
        }

        public static MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            logger?.Write($"MessageBox shown. Text: '{messageBoxText}', Caption: '{caption}', Button: '{button}', Icon: '{icon}', DefaultResult: '{defaultResult}', Options: '{options}'");
            return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
        }

        static void ChangeConnectionState(ConnectionState newState)
        {
            if (CurrentConnectionState != newState)
            {
                if (newState == ConnectionState.TryingToReconnect)
                {
                    close_reconnect?.Abort();
                    StopReconnection();

                    reconnectCancellationToken = new CancellationTokenSource();
                    reconnectThread = Task.Run(ReconnectionThread, reconnectCancellationToken.Token);
                }
                else
                {
                    close_reconnect = Current.InvokeAction(() => StopReconnection());
                }

                CurrentConnectionState = newState;
                Current.InvokeAction(() => ConnectionStateChanged?.Invoke(Current, CurrentConnectionState));

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

        static void ReconnectionThread()
        {
            while (true)
            {
                if (obs.IsConnected)
                    return;
                if (ConnectToOBS(Settings.Instance.ServerAddress, Utils.DecryptString(Settings.Instance.Password)))
                    return;

                if (reconnectCancellationToken.IsCancellationRequested) return;
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    if (reconnectCancellationToken.IsCancellationRequested) return;
                }
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

        internal static bool ConnectToOBS(string adr, string pas)
        {
            var adrs = adr;
            try
            {
                if (string.IsNullOrWhiteSpace(adrs))
                    adrs = "ws://localhost:4444";
                if (!adrs.StartsWith("ws://"))
                    adrs = "ws://" + adrs;
                var pass = pas;

                obs.Connect(adrs, pass);

                if (obs.IsConnected)
                {
                    Settings.Instance.IsConnected = true;
                    Settings.Instance.Save();
                    return true;
                }
            }
            catch (AuthFailureException)
            {
                ShowMessageBox("Authentication failed.", "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (ErrorResponseException ex)
            {
                ShowMessageBox("Connect failed : " + ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return false;
        }

        internal static void DisconnectFromOBS()
        {
            Settings.Instance.IsConnected = false;
            obs.Disconnect();

            if (CurrentConnectionState == ConnectionState.TryingToReconnect)
                IsNeedToSkipNextConnectionNotifications = true;

            ChangeConnectionState(ConnectionState.Disconnected);
            Settings.Instance.Save();
        }

        private void Obs_Connected(object sender, EventArgs e)
        {
            ChangeConnectionState(ConnectionState.Connected);
        }

        private void Obs_Disconnected(object sender, EventArgs e)
        {
            if (Settings.Instance.IsConnected)
                ChangeConnectionState(ConnectionState.TryingToReconnect);
            else
                ChangeConnectionState(ConnectionState.Disconnected);
        }

        private void Obs_OBSExit(object sender, EventArgs e)
        {
            if (Settings.Instance.IsCloseOnOBSClosing && settingsWindow == null)
            {
                StopReconnection();
                this.InvokeAction(() => Shutdown());
            }
        }

        #region Version Checking

        void CheckForUpdates()
        {
            // Skip if currently checking
            if (updateClient != null)
                return;

            updateClient = new WebClient();
            updateClient.DownloadStringCompleted += UpdateClient_DownloadStringCompleted;
            updateClient.Headers.Add("Content-Type", "application/json");
            updateClient.Headers.Add("User-Agent", "OBS Notifier");

            try
            {
                updateClient.DownloadStringAsync(new Uri("https://api.github.com/repos/DmitriySalnikov/OBSNotifier/releases/latest"));
            }
            catch (Exception ex)
            {
                if (!startupUpdateCheck)
                    ShowMessageBox($"Failed to request info about the new version.\n{ex.Message}");
                this.InvokeAction(() => ClearUpdateData());
            }
        }

        private void UpdateClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error is WebException webExp)
            {
                if (!startupUpdateCheck)
                    ShowMessageBox($"Failed to get info about the new version.\n{webExp.Message}");

                // I think it's better to do this not at the time of calling the event
                this.InvokeAction(() => ClearUpdateData());
                return;
            }

            try
            {
                dynamic resultObject = JsonConvert.DeserializeObject(e.Result);
                Version newVersion = new Version(resultObject.tag_name.Value);
                Version currentVersion = new Version(System.Windows.Forms.Application.ProductVersion);
                string updateUrl = resultObject.html_url.Value;

                // Load a previously skipped version
                Version skipVersion = null;
                try
                {
                    skipVersion = new Version(Settings.Instance.SkipVersion);
                }
                catch { }

                // Skip if the new version matches the skip version, or don't skip if checking manually
                if (newVersion != skipVersion || !startupUpdateCheck)
                {
                    // New release
                    if (newVersion > currentVersion)
                    {
                        var updateDialog = ShowMessageBox($"Current version: {System.Windows.Forms.Application.ProductVersion}\nNew version: {newVersion}\nWould you like to go to the download page?\n\nSelect \"No\" to skip this version.", "A new version of OBS Notifier is available", MessageBoxButton.YesNoCancel);
                        if (updateDialog == MessageBoxResult.Yes)
                        {
                            // Open the download page
                            Process.Start(updateUrl);
                        }
                        else if (updateDialog == MessageBoxResult.No)
                        {
                            // Set the new version to skip
                            Settings.Instance.SkipVersion = newVersion.ToString();
                            Settings.Instance.Save();
                        }
                    }
                    else
                    {
                        // Don't show this on startup
                        if (!startupUpdateCheck)
                        {
                            ShowMessageBox($"You are using the latest version: {System.Windows.Forms.Application.ProductVersion}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't show this on startup
                if (!startupUpdateCheck)
                {
                    ShowMessageBox($"Failed to check for update.\n{ex.Message}");
                }
            }

            this.InvokeAction(() => ClearUpdateData());
        }

        void ClearUpdateData()
        {
            updateClient?.Dispose();
            updateClient = null;
            startupUpdateCheck = false;
        }

        #endregion
    }
}
