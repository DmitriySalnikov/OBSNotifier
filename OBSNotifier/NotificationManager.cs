using OBSNotifier.Modules;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;
using System;
using System.Collections.Generic;

namespace OBSNotifier
{
    internal class NotificationDescriptionAttribute : Attribute
    {
        string name = "";
        public string Name
        {
            get
            {
                return Utils.Tr(name);
            }
            set
            {
                name = value;
            }
        }

        string desc = "";
        public string Description
        {
            get
            {
                return translate_description && !string.IsNullOrWhiteSpace(desc) ? Utils.Tr(desc) : desc;
            }
            set
            {
                desc = value;
            }
        }

        bool translate_description = false;

        public NotificationDescriptionAttribute(string name, string desc = null, bool translateDesc = true)
        {
            this.name = name;
            this.desc = desc;
            translate_description = translateDesc;
        }
    }

    [Flags]
    public enum NotificationType : ulong
    {
        None = 0,

        [NotificationDescription("notification_events_connected")]
        Connected = 1L << 0,
        [NotificationDescription("notification_events_disconnected")]
        Disconnected = 1L << 1,
        [NotificationDescription("notification_events_lost_connection", "notification_events_lost_connection_2nd_line")]
        LostConnection = 1L << 2,

        [NotificationDescription("notification_events_replay_started")]
        ReplayStarted = 1L << 5,
        [NotificationDescription("notification_events_replay_stopped")]
        ReplayStopped = 1L << 6,
        [NotificationDescription("notification_events_replay_saved", "{0}", false)]
        ReplaySaved = 1L << 7,

        [NotificationDescription("notification_events_recording_started")]
        RecordingStarted = 1L << 8,
        [NotificationDescription("notification_events_recording_stopped", "{0}", false)]
        RecordingStopped = 1L << 9,
        [NotificationDescription("notification_events_recording_paused")]
        RecordingPaused = 1L << 10,
        [NotificationDescription("notification_events_recording_resumed")]
        RecordingResumed = 1L << 11,

        [NotificationDescription("notification_events_streaming_started")]
        StreamingStarted = 1L << 12,
        [NotificationDescription("notification_events_streaming_stopped")]
        StreamingStopped = 1L << 13,

        [NotificationDescription("notification_events_virtual_camera_started")]
        VirtualCameraStarted = 1L << 14,
        [NotificationDescription("notification_events_virtual_camera_stopped")]
        VirtualCameraStopped = 1L << 15,

        [NotificationDescription("notification_events_scene_switched", "notification_events_scene_switched_2nd_line")]
        SceneSwitched = 1L << 24,
        [NotificationDescription("notification_events_scene_collection_switched", "notification_events_scene_collection_switched_2nd_line")]
        SceneCollectionSwitched = 1L << 25,

        [NotificationDescription("notification_events_profile_switched", "notification_events_profile_switched_2nd_line")]
        ProfileSwitched = 1L << 32,

        [NotificationDescription("notification_events_audio_muted", "notification_events_audio_muted_2nd_line")]
        AudioSourceMuted = 1L << 34,
        [NotificationDescription("notification_events_audio_turned_on", "notification_events_audio_turned_on_2nd_line")]
        AudioSourceUnmuted = 1L << 35,

        [NotificationDescription("notification_events_screenshot_saved", "{0}", false)]
        ScreenshotSaved = 1L << 38,

        Minimal = Connected | Disconnected | LostConnection |
            ReplaySaved |
            RecordingStarted | RecordingStopped |
            StreamingStarted | StreamingStopped |
            SceneSwitched |
            AudioSourceMuted | AudioSourceUnmuted,

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

        ModuleManager.ModuleData CurrentModule { get => App.modules.CurrentModule; }

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
            return Settings.Instance.IsPreviewShowing || CurrentModule.instance == null;
        }

        bool IsActive(NotificationType type)
        {
            if (CurrentModule.instance != null)
            {
                var notifs = Settings.Instance.CurrentModuleSettings.ActiveNotificationTypes ?? App.modules.CurrentModule.instance.DefaultActiveNotifications;
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
                    CurrentModule.instance.ShowNotification(type, NotificationsData[type].Name, fmt, origData.Length == 0 ? null : origData);
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
                    CurrentModule.instance.ShowNotification(type, NotificationsData[type].Name, NotificationsData[type].Description);
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
