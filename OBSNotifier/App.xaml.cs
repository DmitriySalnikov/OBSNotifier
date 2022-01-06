using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace OBSNotifier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        public static OBSWebsocket obs;
        public static PluginManager plugins;

        System.Windows.Forms.NotifyIcon trayIcon;
        SettingsWindow settingsWindow;
        DeferredAction gc_collect = new DeferredAction(() => GC.Collect(), 1000);

        private void Application_Startup(object sender, StartupEventArgs ee)
        {
            obs = new OBSWebsocket();
            obs.Connected += Obs_Connected;
            obs.Disconnected += Obs_Disconnected;
            obs.ReplayBufferStateChanged += Obs_ReplayBufferStateChanged;

            Settings.Load();
            plugins = new PluginManager();

            // Select current plugin
            if (!plugins.SelectCurrent(Settings.Instance.NotificationStyle))
                Settings.Instance.NotificationStyle = string.Empty;

            if (Settings.Instance.IsConnected && !obs.IsConnected)
            {
                ConnectToOBS(Settings.Instance.ServerAddress, Utils.DecryptString(Settings.Instance.Password));
            }

            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = OBSNotifier.Properties.Resources.icon;
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
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            obs.Disconnect();
            Settings.Instance.Save();

            plugins?.Dispose();
            plugins = null;

            trayIcon?.Dispose();
            trayIcon = null;
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
                return true;
            }
            catch (AuthFailureException)
            {
                MessageBox.Show("Authentication failed.", "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            catch (ErrorResponseException ex)
            {
                MessageBox.Show("Connect failed : " + ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "OBS Notifier Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
        }

        private void Obs_Connected(object sender, EventArgs e)
        {
            // TODO change icon
        }

        private void Obs_Disconnected(object sender, EventArgs e)
        {
            // TODO change icon
        }

        private void Obs_ReplayBufferStateChanged(OBSWebsocket sender, OBSWebsocketDotNet.Types.OutputState type)
        {
            var plugin = plugins.CurrentPlugin.plugin;
            if (Settings.Instance.IsPreviewShowing || plugin == null)
                return;

            this.InvokeAction(() =>
            {
                switch (type)
                {
                    case OBSWebsocketDotNet.Types.OutputState.Starting:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Started:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Stopping:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Stopped:
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Saved:
                        plugin.ShowNotification(NotificationType.ReplaySaved, "Replay Saved", "");
                        break;
                }
            });
        }

    }
}
