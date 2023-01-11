using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;
using System;
using System.Collections.Generic;

namespace OBSNotifier
{
    internal class NotificationDescriptionAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public NotificationDescriptionAttribute(string name, string desc = null)
        {
            Name = name;
            Description = desc;
        }
    }

    [Flags]
    public enum NotificationType : ulong
    {
        None = 0,

        [NotificationDescription("Connected to OBS")]
        Connected = 1L << 0,
        [NotificationDescription("Disconnected from OBS")]
        Disconnected = 1L << 1,
        [NotificationDescription("Lost Connection to OBS", "Trying to reconnect")]
        LostConnection = 1L << 2,

        [NotificationDescription("Replay Started")]
        ReplayStarted = 1L << 5,
        [NotificationDescription("Replay Stopped")]
        ReplayStopped = 1L << 6,
        [NotificationDescription("Replay Saved", "{0}")]
        ReplaySaved = 1L << 7,

        [NotificationDescription("Recording Started")]
        RecordingStarted = 1L << 8,
        [NotificationDescription("Recording Stopped", "{0}")]
        RecordingStopped = 1L << 9,
        [NotificationDescription("Recording Paused")]
        RecordingPaused = 1L << 10,
        [NotificationDescription("Recording Resumed")]
        RecordingResumed = 1L << 11,

        [NotificationDescription("Streaming Started")]
        StreamingStarted = 1L << 12,
        [NotificationDescription("Streaming Stopped")]
        StreamingStopped = 1L << 13,

        [NotificationDescription("Virtual Camera Started")]
        VirtualCameraStarted = 1L << 14,
        [NotificationDescription("Virtual Camera Stopped")]
        VirtualCameraStopped = 1L << 15,

        [NotificationDescription("Scene Switched", "Current: {0}")]
        SceneSwitched = 1L << 24,
        [NotificationDescription("Scene Collection Switched", "Current: {0}")]
        SceneCollectionSwitched = 1L << 25,

        [NotificationDescription("Profile Switched", "Current: {0}")]
        ProfileSwitched = 1L << 32,

        [NotificationDescription("Audio is Muted", "Source: {0}")]
        AudioSourceMuted = 1L << 34,
        [NotificationDescription("Audio is Turned On", "Source: {0}")]
        AudioSourceUnmuted = 1L << 35,

        [NotificationDescription("Screenshot saved", "{0}")]
        ScreenshotSaved = 1L << 38,

        Minimal = Connected | Disconnected | LostConnection |
            ReplaySaved |
            RecordingStarted | RecordingStopped |
            StreamingStarted | StreamingStopped |
            SceneSwitched |
            AudioSourceMuted | AudioSourceUnmuted,

        // TODO add ability to open folder with saved files
        WithFilePaths = RecordingStopped | ReplaySaved | ScreenshotSaved,

        All = Connected | Disconnected | LostConnection |
            ReplayStarted | ReplayStopped | ReplaySaved |
            RecordingPaused | RecordingResumed | RecordingStarted | RecordingStopped |
            StreamingStarted | StreamingStopped |
            VirtualCameraStarted | VirtualCameraStopped |
            SceneSwitched | SceneCollectionSwitched |
            ProfileSwitched |
            AudioSourceMuted | AudioSourceUnmuted | ScreenshotSaved,
    }

    internal class NotificationManager
    {
        readonly OBSWebsocket obs;
        readonly App app;

        // https://github.com/obsproject/obs-studio/blob/fab293a6862dbe6aca9eb1bde0b00fad2d2cd785/UI/window-basic-main.cpp#L3098
        readonly List<string> obs_audio_types = new List<string>
        {
            "wasapi_input_capture",
            "wasapi_output_capture",
            "coreaudio_input_capture",
            "coreaudio_output_capture",
            "pulse_input_capture",
            "pulse_output_capture",
            "alsa_input_capture",

            // idk if this is really used somewhere
            // https://github.com/obsproject/obs-studio/blob/19ced32c584ac8788953b68d28293ae65d59f0a4/UI/importers/xsplit.cpp#L268
            "dshow_input",
            "dshow_output",
        };

        PluginManager.PluginData CurrentPlugin { get => App.plugins.CurrentPlugin; }

        #region NotifData
        static List<NotificationType> SkipNotifTypes = new List<NotificationType> {
            NotificationType.None,
            NotificationType.Minimal,
            NotificationType.WithFilePaths,
            NotificationType.All,
        };

        static Dictionary<NotificationType, NotificationDescriptionAttribute> notificationsData;
        public static Dictionary<NotificationType, NotificationDescriptionAttribute> NotificationsData
        {
            get
            {
                if (notificationsData == null)
                {
                    notificationsData = new Dictionary<NotificationType, NotificationDescriptionAttribute>();
                    var type = typeof(NotificationType);
                    foreach (NotificationType e in Enum.GetValues(type))
                    {
                        if (SkipNotifTypes.Contains(e))
                            continue;

                        var member = type.GetMember(e.ToString())[0];
                        notificationsData.Add(e, Attribute.GetCustomAttribute(member, typeof(NotificationDescriptionAttribute)) as NotificationDescriptionAttribute);
                    }
                }

                return notificationsData;
            }
        }
        #endregion

        public NotificationManager(App app, OBSWebsocket obs)
        {
            this.obs = obs;
            this.app = app;

            App.ConnectionStateChanged += App_ConnectionStateChanged;

            obs.RecordStateChanged += Obs_RecordStateChanged;
            obs.StreamStateChanged += Obs_StreamStateChanged;
            obs.VirtualcamStateChanged += Obs_VirtualcamStateChanged;
            obs.ReplayBufferStateChanged += Obs_ReplayBufferStateChanged;

            obs.ReplayBufferSaved += Obs_ReplayBufferSaved;

            obs.CurrentProgramSceneChanged += Obs_CurrentProgramSceneChanged;
            obs.CurrentSceneCollectionChanged += Obs_CurrentSceneCollectionChanged;

            obs.CurrentProfileChanged += Obs_CurrentProfileChanged;

            obs.InputMuteStateChanged += Obs_InputMuteStateChanged;

            obs.ScreenshotSaved += Obs_ScreenshotSaved;
        }

        #region Utils
        void ResetVals()
        {
            // Nothing to reset in this version
        }

        void InvokeNotif(Action act)
        {
            if (IsDisabled()) return;

            try
            {
                app.InvokeAction(act);
            }
            catch (Exception ex)
            {
                App.Log(ex);
                return;
            }
        }

        bool IsDisabled()
        {
            return Settings.Instance.IsPreviewShowing || CurrentPlugin.plugin == null;
        }

        bool IsActive(NotificationType type)
        {
            if (CurrentPlugin.plugin != null)
            {
                var notifs = Settings.Instance.CurrentPluginSettings.ActiveNotificationTypes ?? App.plugins.CurrentPlugin.plugin.DefaultActiveNotifications;
                return notifs.HasFlag(type);
            }
            return false;
        }

        void ShowNotif(NotificationType type, Func<string, object[], string> formatter, params object[] origData)
        {
            try
            {
                if (IsActive(type))
                {
                    string fmt = formatter(NotificationsData[type].Description, origData);
                    App.Log($"New notification: {type}, Formatted Data: '{fmt}', Data Size: {origData.Length}");
                    CurrentPlugin.plugin.ShowNotification(type, NotificationsData[type].Name, fmt, origData.Length == 0 ? null : origData);
                }
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }
        }

        void ShowNotif(NotificationType type)
        {
            try
            {
                if (IsActive(type))
                {
                    App.Log($"New notification: {type}");
                    CurrentPlugin.plugin.ShowNotification(type, NotificationsData[type].Name, NotificationsData[type].Description);
                }
            }
            catch (Exception ex)
            {
                App.Log(ex);
            }
        }

        string FormatterOneArg(string format, object[] data)
        {
            return string.Format(format, data[0]);
        }
        #endregion

        #region OBS Connection
        private void App_ConnectionStateChanged(object sender, App.ConnectionState e)
        {
            if (App.IsNeedToSkipNextConnectionNotifications)
            {
                App.IsNeedToSkipNextConnectionNotifications = false;
                return;
            }

            InvokeNotif(() =>
            {
                switch (e)
                {
                    case App.ConnectionState.Connected:
                        ResetVals();
                        ShowNotif(NotificationType.Connected);
                        break;
                    case App.ConnectionState.Disconnected:
                        ResetVals();
                        ShowNotif(NotificationType.Disconnected);
                        break;
                    case App.ConnectionState.TryingToReconnect:
                        ShowNotif(NotificationType.LostConnection);
                        break;
                }
            });
        }
        #endregion

        #region Recording
        private void Obs_RecordStateChanged(object sender, RecordStateChangedEventArgs e)
        {
            InvokeNotif(() =>
            {
                switch (e.OutputState.State)
                {
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                        ShowNotif(NotificationType.RecordingStarted);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                        ShowNotif(NotificationType.RecordingStopped, FormatterOneArg, e.OutputState.OutputPath);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STARTING:
                        // Nothing to do
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING:
                        // Nothing to do
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED:
                        ShowNotif(NotificationType.RecordingPaused);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED:
                        ShowNotif(NotificationType.RecordingResumed);
                        break;
                }
            });
        }
        #endregion

        #region Streaming
        private void Obs_StreamStateChanged(object sender, StreamStateChangedEventArgs e)
        {
            InvokeNotif(() =>
            {
                switch (e.OutputState.State)
                {
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                        ShowNotif(NotificationType.StreamingStarted);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                        ShowNotif(NotificationType.StreamingStopped);
                        break;
                }
            });
        }
        #endregion

        #region Replays
        private void Obs_ReplayBufferStateChanged(object sender, ReplayBufferStateChangedEventArgs e)
        {
            InvokeNotif(() =>
            {
                switch (e.OutputState.State)
                {
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                        ShowNotif(NotificationType.ReplayStarted);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                        ShowNotif(NotificationType.ReplayStopped);
                        break;
                }
            });
        }

        private void Obs_ReplayBufferSaved(object sender, ReplayBufferSavedEventArgs e)
        {
            InvokeNotif(() => ShowNotif(NotificationType.ReplaySaved, FormatterOneArg, e.SavedReplayPath));
        }
        #endregion

        #region Virtual Camera
        private void Obs_VirtualcamStateChanged(object sender, VirtualcamStateChangedEventArgs e)
        {
            InvokeNotif(() =>
            {
                switch (e.OutputState.State)
                {
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                        ShowNotif(NotificationType.VirtualCameraStarted);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                        ShowNotif(NotificationType.VirtualCameraStopped);
                        break;
                }
            });
        }
        #endregion

        #region Scenes
        private void Obs_CurrentProgramSceneChanged(object sender, ProgramSceneChangedEventArgs e)
        {
            InvokeNotif(() => ShowNotif(NotificationType.SceneSwitched, FormatterOneArg, e.SceneName));
        }

        private void Obs_CurrentSceneCollectionChanged(object sender, CurrentSceneCollectionChangedEventArgs e)
        {
            InvokeNotif(() =>
            {
                try
                {
                    ShowNotif(NotificationType.SceneCollectionSwitched, FormatterOneArg, e.SceneCollectionName);
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                    return;
                }
            });
        }
        #endregion

        #region Profiles
        private void Obs_CurrentProfileChanged(object sender, CurrentProfileChangedEventArgs e)
        {
            InvokeNotif(() =>
            {
                try
                {
                    ShowNotif(NotificationType.ProfileSwitched, FormatterOneArg, e.ProfileName);
                }
                catch (Exception ex)
                {
                    App.Log(ex);
                    return;
                }
            }
            );
        }
        #endregion

        #region Audio
        private void Obs_InputMuteStateChanged(object sender, InputMuteStateChangedEventArgs e)
        {
            try
            {
                var src = obs.GetInputList().Find((s) => s.InputName == e.InputName);
                if (src == null || !obs_audio_types.Contains(src.InputKind))
                    return;
            }
            catch (Exception ex)
            {
                App.Log(ex);
                return;
            }

            InvokeNotif(() =>
            {
                if (e.InputMuted)
                    ShowNotif(NotificationType.AudioSourceMuted, FormatterOneArg, e.InputName);
                else
                    ShowNotif(NotificationType.AudioSourceUnmuted, FormatterOneArg, e.InputName);
            });
        }
        #endregion

        #region Screenshots
        private void Obs_ScreenshotSaved(object sender, ScreenshotSavedEventArgs e)
        {
            InvokeNotif(() => ShowNotif(NotificationType.ScreenshotSaved, FormatterOneArg, e.SavedScreenshotPath));
        }
        #endregion
    }
}
