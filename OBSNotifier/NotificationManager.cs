using OBSNotifier.Plugins;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
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

        Minimal = Connected | Disconnected | LostConnection |
            ReplaySaved |
            RecordingStarted | RecordingStopped |
            StreamingStarted | StreamingStopped |
            SceneSwitched |
            AudioSourceMuted | AudioSourceUnmuted,

        // TODO add ability to open folder with saved files
        WithFilePaths = RecordingStopped | ReplaySaved,

        All = Connected | Disconnected | LostConnection |
            ReplayStarted | ReplayStopped | ReplaySaved |
            RecordingPaused | RecordingResumed | RecordingStarted | RecordingStopped |
            StreamingStarted | StreamingStopped |
            VirtualCameraStarted | VirtualCameraStopped |
            SceneSwitched | SceneCollectionSwitched |
            ProfileSwitched |
            AudioSourceMuted | AudioSourceUnmuted,
    }

    internal class NotificationManager
    {
        readonly OBSWebsocket obs;
        readonly App app;

        string prev_record_name;

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

            obs.RecordStateChanged += Obs_RecordingStateChanged;

            obs.StreamStateChanged += Obs_StreamingStateChanged;

            obs.ReplayBufferStateChanged += Obs_ReplayBufferStateChanged;
            obs.ReplayBufferSaved += Obs_ReplayBufferSaved;

            obs.VirtualcamStateChanged += Obs_VirtualcamStateChanged; ;

            obs.CurrentProgramSceneChanged += Obs_SceneChanged;
            obs.CurrentSceneCollectionChanged += Obs_SceneCollectionChanged;

            obs.CurrentProfileChanged += Obs_ProfileChanged;

            obs.InputMuteStateChanged += Obs_SourceMuteStateChanged;
        }

        #region Utils
        void ResetVals()
        {
            prev_record_name = "";
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
            if (IsActive(type))
            {
                string fmt = formatter(NotificationsData[type].Description, origData);
                App.Log($"New notification: {type}, Formatted Data: '{fmt}', Data Size: {origData.Length}");
                CurrentPlugin.plugin.ShowNotification(type, NotificationsData[type].Name, fmt, origData.Length == 0 ? null : origData);
            }
        }

        void ShowNotif(NotificationType type)
        {
            if (IsActive(type))
            {
                App.Log($"New notification: {type}");
                CurrentPlugin.plugin.ShowNotification(type, NotificationsData[type].Name, NotificationsData[type].Description);
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
        private void Obs_RecordingStateChanged(OBSWebsocket sender, RecordStateChanged outputState)
        {
            InvokeNotif(() =>
            {
                switch (outputState.State)
                {
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STARTED:
                        ShowNotif(NotificationType.RecordingStarted);
                        break;
                    case OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED:
                        ShowNotif(NotificationType.RecordingStopped, FormatterOneArg, outputState.OutputPath);
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
        private void Obs_StreamingStateChanged(OBSWebsocket sender, OutputStateChanged outputState)
        {
            InvokeNotif(() =>
            {
                switch (outputState.State)
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
        private void Obs_ReplayBufferStateChanged(OBSWebsocket sender, OutputStateChanged outputState)
        {
            InvokeNotif(() =>
            {
                switch (outputState.State)
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

        private void Obs_ReplayBufferSaved(OBSWebsocket sender, string replayFilename)
        {
            InvokeNotif(() => ShowNotif(NotificationType.ReplaySaved, FormatterOneArg, replayFilename));
        }
        #endregion

        #region Virtual Camera
        private void Obs_VirtualcamStateChanged(OBSWebsocket sender, OutputStateChanged outputState)
        {
            InvokeNotif(() =>
            {
                switch (outputState.State)
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
        private void Obs_SceneChanged(OBSWebsocket sender, string newSceneName)
        {
            InvokeNotif(() => ShowNotif(NotificationType.SceneSwitched, FormatterOneArg, newSceneName));
        }

        private void Obs_SceneCollectionChanged(OBSWebsocket sender, string sceneCollectionName)
        {
            InvokeNotif(() =>
            {
                try
                {
                    ShowNotif(NotificationType.SceneCollectionSwitched, FormatterOneArg, sceneCollectionName);
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
        private void Obs_ProfileChanged(OBSWebsocket sender, string profileName)
        {
            InvokeNotif(() =>
            {
                try
                {
                    ShowNotif(NotificationType.ProfileSwitched, FormatterOneArg, profileName);
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
        private void Obs_SourceMuteStateChanged(OBSWebsocket sender, string inputName, bool inputMuted)
        {
            try
            {
                var src = obs.GetInputList().Find((s) => s.InputName == inputName);
                if (src == null || !obs_audio_types.Contains(src.InputName)) // TODO need testing
                    return;
            }
            catch (Exception ex)
            {
                App.Log(ex);
                return;
            }

            if (inputMuted)
                InvokeNotif(() => ShowNotif(NotificationType.AudioSourceMuted, FormatterOneArg, inputName));
            else
                InvokeNotif(() => ShowNotif(NotificationType.AudioSourceUnmuted, FormatterOneArg, inputName));
        }
        #endregion
    }
}
