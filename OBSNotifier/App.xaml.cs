using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public enum ConnectionState
        {
            Connected,
            Disconnected,
            TryingToReconnect,
        }

        public static event EventHandler<ConnectionState> ConnectionStateChanged;
        public static OBSWebsocket obs;
        public static PluginManager plugins;
        public static NotificationManager notifications;
        public static ConnectionState CurrentConnectionState { get; private set; }
        public static bool IsNeedToSkipNextConnectionNotifications = false;

        static System.Windows.Forms.NotifyIcon trayIcon;
        SettingsWindow settingsWindow;
        public DeferredAction gc_collect = new DeferredAction(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }, 1000);
        static DeferredAction close_reconnect = new DeferredAction(() => StopReconnection(), 500);
        static Task reconnectThread;
        static CancellationTokenSource reconnectCancellationToken;

        private void Application_Startup(object sender, StartupEventArgs ee)
        {
            CurrentConnectionState = ConnectionState.Disconnected;

            obs = new OBSWebsocket();
            obs.Connected += Obs_Connected;
            obs.Disconnected += Obs_Disconnected;

            Settings.Load();
            plugins = new PluginManager();
            notifications = new NotificationManager(this, obs);

            // Clear unused
            if (Settings.Instance.ClearUnusedPluginSettings())
                Settings.Instance.Save();

            // Select current plugin
            if (!plugins.SelectCurrent(Settings.Instance.NotificationStyle))
                Settings.Instance.NotificationStyle = string.Empty;

            // Create tray icon
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = OBSNotifier.Properties.Resources.obs_notifier_64px;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, ev) =>
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
                };
            trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] {
                new System.Windows.Forms.MenuItem("Exit", (s,e) => Shutdown()),
            });

            // Connect to obs if previously connected
            if (Settings.Instance.IsConnected && !obs.IsConnected)
            {
                IsNeedToSkipNextConnectionNotifications = true;
                ChangeConnectionState(ConnectionState.TryingToReconnect);
            }

            UpdateTrayStatus();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            StopReconnection();

            settingsWindow?.Close();
            settingsWindow = null;

            obs.Disconnect();
            Settings.Instance.Save(true);

            plugins?.Dispose();
            plugins = null;

            trayIcon?.Dispose();
            trayIcon = null;

            gc_collect.Dispose();
            gc_collect = null;

            close_reconnect.Dispose();
            close_reconnect = null;
        }

        static void ChangeConnectionState(ConnectionState newState)
        {
            if (CurrentConnectionState != newState)
            {
                if (newState == ConnectionState.TryingToReconnect)
                {
                    close_reconnect.Cancel();
                    StopReconnection();
                    reconnectCancellationToken = new CancellationTokenSource();
                    reconnectThread = Task.Run(ReconnectionThread, reconnectCancellationToken.Token);
                }
                else
                {
                    close_reconnect.CallDeferred();
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
                Thread.Sleep(6000);
                if (reconnectCancellationToken.IsCancellationRequested) return;
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
                MessageBox.Show("Authentication failed.", "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (ErrorResponseException ex)
            {
                MessageBox.Show("Connect failed : " + ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
            // TODO change icon
        }

        private void Obs_Disconnected(object sender, EventArgs e)
        {
            if (Settings.Instance.IsConnected)
                ChangeConnectionState(ConnectionState.TryingToReconnect);
            else
                ChangeConnectionState(ConnectionState.Disconnected);
            // TODO change icon
        }
    }
}
