using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSNotifier
{
    [Flags]
    public enum NotificationType : long
    {
        None = 0,

        [EnumName("Connected to OBS")]
        Connected = 1 << 1,
        [EnumName("Disconnected from OBS")]
        Disconnected = 1 << 2,

        [EnumName("Replay Started")]
        ReplayStarted = 1 << 3,
        [EnumName("Replay Stopped")]
        ReplayStopped = 1 << 4,
        [EnumName("Replay Saved")]
        ReplaySaved = 1 << 5,

        [EnumName("Record Started")]
        RecordStarted = 1 << 6,
        [EnumName("Record Stopped")]
        RecordStopped = 1 << 7,

        [EnumName("Stream Started")]
        StreamStarted = 1 << 8,
        [EnumName("Stream Stopped")]
        StreamStopped = 1 << 9,

        Minimal = ReplaySaved |
            RecordStarted | RecordStopped |
            StreamStarted | StreamStopped,

        All = Connected | Disconnected |
            ReplayStarted | ReplayStopped | ReplaySaved |
            RecordStarted | RecordStopped |
            StreamStarted | StreamStopped,
    }

    internal class NotificationManager
    {
        readonly OBSWebsocket obs;
        readonly App app;
        PluginManager.PluginData CurrentPlugin { get => App.plugins.CurrentPlugin; }

        public NotificationManager(App app, OBSWebsocket obs)
        {
            this.obs = obs;
            this.app = app;

            obs.ReplayBufferStateChanged += Obs_ReplayBufferStateChanged;
        }

        bool IsActive(NotificationType type)
        {
            if (CurrentPlugin.plugin != null)
            {
                var notifs = Settings.Instance.CurrentPluginSettings.ActiveNotificationTypes??App.plugins.CurrentPlugin.plugin.DefaultActiveNotifications;
                return notifs.HasFlag(type);
            }
            return false;
        }

        private void Obs_ReplayBufferStateChanged(OBSWebsocket sender, OBSWebsocketDotNet.Types.OutputState type)
        {
            var plugin = CurrentPlugin.plugin;
            if (Settings.Instance.IsPreviewShowing || plugin == null)
                return;

            app.InvokeAction(() =>
            {
                switch (type)
                {
                    case OBSWebsocketDotNet.Types.OutputState.Started:
                        if (IsActive(NotificationType.ReplayStarted))
                            plugin.ShowNotification(NotificationType.ReplaySaved, "Replay started");
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Stopped:
                        if (IsActive(NotificationType.ReplayStopped))
                            plugin.ShowNotification(NotificationType.ReplaySaved, "Replay stopped");
                        break;
                    case OBSWebsocketDotNet.Types.OutputState.Saved:
                        if (IsActive(NotificationType.ReplaySaved))
                            plugin.ShowNotification(NotificationType.ReplaySaved, "Replay saved");
                        break;
                }
            });
        }
    }
}
