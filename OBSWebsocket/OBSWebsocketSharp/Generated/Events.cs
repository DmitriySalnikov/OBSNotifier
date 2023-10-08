using System.Text.Json;
using System.Text.Json.Serialization;
using OBSWebsocketSharp.Extensions;

namespace OBSWebsocketSharp
{
    public class CurrentSceneCollectionChangingData
    {
        /// <summary>Name of the current scene collection</summary>
        public string sceneCollectionName = default!;
    }

    public class CurrentSceneCollectionChangedData
    {
        /// <summary>Name of the new scene collection</summary>
        public string sceneCollectionName = default!;
    }

    public class SceneCollectionListChangedData
    {
        /// <summary>Updated list of scene collections</summary>
        public string[] sceneCollections = default!;
    }

    public class CurrentProfileChangingData
    {
        /// <summary>Name of the current profile</summary>
        public string profileName = default!;
    }

    public class CurrentProfileChangedData
    {
        /// <summary>Name of the new profile</summary>
        public string profileName = default!;
    }

    public class ProfileListChangedData
    {
        /// <summary>Updated list of profiles</summary>
        public string[] profiles = default!;
    }

    public class SourceFilterListReindexedData
    {
        /// <summary>Name of the source</summary>
        public string sourceName = default!;
        /// <summary>Array of filter objects</summary>
        public JsonElement filters = default!;
    }

    public class SourceFilterCreatedData
    {
        /// <summary>Name of the source the filter was added to</summary>
        public string sourceName = default!;
        /// <summary>Name of the filter</summary>
        public string filterName = default!;
        /// <summary>The kind of the filter</summary>
        public string filterKind = default!;
        /// <summary>Index position of the filter</summary>
        public double filterIndex = default!;
        /// <summary>The settings configured to the filter when it was created</summary>
        public JsonElement filterSettings = default!;
        /// <summary>The default settings for the filter</summary>
        public JsonElement defaultFilterSettings = default!;
    }

    public class SourceFilterRemovedData
    {
        /// <summary>Name of the source the filter was on</summary>
        public string sourceName = default!;
        /// <summary>Name of the filter</summary>
        public string filterName = default!;
    }

    public class SourceFilterNameChangedData
    {
        /// <summary>The source the filter is on</summary>
        public string sourceName = default!;
        /// <summary>Old name of the filter</summary>
        public string oldFilterName = default!;
        /// <summary>New name of the filter</summary>
        public string filterName = default!;
    }

    public class SourceFilterSettingsChangedData
    {
        /// <summary>Name of the source the filter is on</summary>
        public string sourceName = default!;
        /// <summary>Name of the filter</summary>
        public string filterName = default!;
        /// <summary>New settings object of the filter</summary>
        public JsonElement filterSettings = default!;
    }

    public class SourceFilterEnableStateChangedData
    {
        /// <summary>Name of the source the filter is on</summary>
        public string sourceName = default!;
        /// <summary>Name of the filter</summary>
        public string filterName = default!;
        /// <summary>Whether the filter is enabled</summary>
        public bool filterEnabled = default!;
    }


    public class InputCreatedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>The kind of the input</summary>
        public string inputKind = default!;
        /// <summary>The unversioned kind of input (aka no `_v2` stuff)</summary>
        public string unversionedInputKind = default!;
        /// <summary>The settings configured to the input when it was created</summary>
        public JsonElement inputSettings = default!;
        /// <summary>The default settings for the input</summary>
        public JsonElement defaultInputSettings = default!;
    }

    public class InputRemovedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
    }

    public class InputNameChangedData
    {
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>Old name of the input</summary>
        public string oldInputName = default!;
        /// <summary>New name of the input</summary>
        public string inputName = default!;
    }

    public class InputSettingsChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>New settings object of the input</summary>
        public JsonElement inputSettings = default!;
    }

    public class InputActiveStateChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>Whether the input is active</summary>
        public bool videoActive = default!;
    }

    public class InputShowStateChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>Whether the input is showing</summary>
        public bool videoShowing = default!;
    }

    public class InputMuteStateChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>Whether the input is muted</summary>
        public bool inputMuted = default!;
    }

    public class InputVolumeChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>New volume level multiplier</summary>
        public double inputVolumeMul = default!;
        /// <summary>New volume level in dB</summary>
        public double inputVolumeDb = default!;
    }

    public class InputAudioBalanceChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>New audio balance value of the input</summary>
        public double inputAudioBalance = default!;
    }

    public class InputAudioSyncOffsetChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>New sync offset in milliseconds</summary>
        public double inputAudioSyncOffset = default!;
    }

    public class InputAudioTracksChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>Object of audio tracks along with their associated enable states</summary>
        public JsonElement inputAudioTracks = default!;
    }

    public class InputAudioMonitorTypeChangedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>New monitor type of the input</summary>
        public string monitorType = default!;
    }

    public class InputVolumeMetersData
    {
        /// <summary>Array of active inputs with their associated volume levels</summary>
        public JsonElement inputs = default!;
    }

    public class MediaInputPlaybackStartedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
    }

    public class MediaInputPlaybackEndedData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
    }

    public class MediaInputActionTriggeredData
    {
        /// <summary>Name of the input</summary>
        public string inputName = default!;
        /// <summary>UUID of the input</summary>
        public string inputUuid = default!;
        /// <summary>Action performed on the input. See `ObsMediaInputAction` enum</summary>
        public string mediaAction = default!;
        /// <summary>
        /// Action performed on the input. See `ObsMediaInputAction` enum
        /// 
        /// Same as <see cref="mediaAction"/>, but converted to enum.
        /// </summary>
        [JsonIgnore]
        public ObsMediaInputAction mediaActionEnum { get => JsonExtensions.EnumFromString<ObsMediaInputAction>(mediaAction); }
    }

    public class StreamStateChangedData
    {
        /// <summary>Whether the output is active</summary>
        public bool outputActive = default!;
        /// <summary>The specific state of the output</summary>
        public string outputState = default!;
        /// <summary>
        /// The specific state of the output
        /// 
        /// Same as <see cref="outputState"/>, but converted to enum.
        /// </summary>
        [JsonIgnore]
        public ObsOutputState outputStateEnum { get => JsonExtensions.EnumFromString<ObsOutputState>(outputState); }
    }

    public class RecordStateChangedData
    {
        /// <summary>Whether the output is active</summary>
        public bool outputActive = default!;
        /// <summary>The specific state of the output</summary>
        public string outputState = default!;
        /// <summary>
        /// The specific state of the output
        /// 
        /// Same as <see cref="outputState"/>, but converted to enum.
        /// </summary>
        [JsonIgnore]
        public ObsOutputState outputStateEnum { get => JsonExtensions.EnumFromString<ObsOutputState>(outputState); }
        /// <summary>File name for the saved recording, if record stopped. `null` otherwise</summary>
        public string outputPath = default!;
    }

    public class ReplayBufferStateChangedData
    {
        /// <summary>Whether the output is active</summary>
        public bool outputActive = default!;
        /// <summary>The specific state of the output</summary>
        public string outputState = default!;
        /// <summary>
        /// The specific state of the output
        /// 
        /// Same as <see cref="outputState"/>, but converted to enum.
        /// </summary>
        [JsonIgnore]
        public ObsOutputState outputStateEnum { get => JsonExtensions.EnumFromString<ObsOutputState>(outputState); }
    }

    public class VirtualcamStateChangedData
    {
        /// <summary>Whether the output is active</summary>
        public bool outputActive = default!;
        /// <summary>The specific state of the output</summary>
        public string outputState = default!;
        /// <summary>
        /// The specific state of the output
        /// 
        /// Same as <see cref="outputState"/>, but converted to enum.
        /// </summary>
        [JsonIgnore]
        public ObsOutputState outputStateEnum { get => JsonExtensions.EnumFromString<ObsOutputState>(outputState); }
    }

    public class ReplayBufferSavedData
    {
        /// <summary>Path of the saved replay file</summary>
        public string savedReplayPath = default!;
    }

    public class SceneItemCreatedData
    {
        /// <summary>Name of the scene the item was added to</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene the item was added to</summary>
        public string sceneUuid = default!;
        /// <summary>Name of the underlying source (input/scene)</summary>
        public string sourceName = default!;
        /// <summary>UUID of the underlying source (input/scene)</summary>
        public string sourceUuid = default!;
        /// <summary>Numeric ID of the scene item</summary>
        public double sceneItemId = default!;
        /// <summary>Index position of the item</summary>
        public double sceneItemIndex = default!;
    }

    public class SceneItemRemovedData
    {
        /// <summary>Name of the scene the item was removed from</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene the item was removed from</summary>
        public string sceneUuid = default!;
        /// <summary>Name of the underlying source (input/scene)</summary>
        public string sourceName = default!;
        /// <summary>UUID of the underlying source (input/scene)</summary>
        public string sourceUuid = default!;
        /// <summary>Numeric ID of the scene item</summary>
        public double sceneItemId = default!;
    }

    public class SceneItemListReindexedData
    {
        /// <summary>Name of the scene</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene</summary>
        public string sceneUuid = default!;
        /// <summary>Array of scene item objects</summary>
        public JsonElement sceneItems = default!;
    }

    public class SceneItemEnableStateChangedData
    {
        /// <summary>Name of the scene the item is in</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene the item is in</summary>
        public string sceneUuid = default!;
        /// <summary>Numeric ID of the scene item</summary>
        public double sceneItemId = default!;
        /// <summary>Whether the scene item is enabled (visible)</summary>
        public bool sceneItemEnabled = default!;
    }

    public class SceneItemLockStateChangedData
    {
        /// <summary>Name of the scene the item is in</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene the item is in</summary>
        public string sceneUuid = default!;
        /// <summary>Numeric ID of the scene item</summary>
        public double sceneItemId = default!;
        /// <summary>Whether the scene item is locked</summary>
        public bool sceneItemLocked = default!;
    }

    public class SceneItemSelectedData
    {
        /// <summary>Name of the scene the item is in</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene the item is in</summary>
        public string sceneUuid = default!;
        /// <summary>Numeric ID of the scene item</summary>
        public double sceneItemId = default!;
    }

    public class SceneItemTransformChangedData
    {
        /// <summary>The name of the scene the item is in</summary>
        public string sceneName = default!;
        /// <summary>The UUID of the scene the item is in</summary>
        public string sceneUuid = default!;
        /// <summary>Numeric ID of the scene item</summary>
        public double sceneItemId = default!;
        /// <summary>New transform/crop info of the scene item</summary>
        public JsonElement sceneItemTransform = default!;
    }

    public class SceneCreatedData
    {
        /// <summary>Name of the new scene</summary>
        public string sceneName = default!;
        /// <summary>UUID of the new scene</summary>
        public string sceneUuid = default!;
        /// <summary>Whether the new scene is a group</summary>
        public bool isGroup = default!;
    }

    public class SceneRemovedData
    {
        /// <summary>Name of the removed scene</summary>
        public string sceneName = default!;
        /// <summary>UUID of the removed scene</summary>
        public string sceneUuid = default!;
        /// <summary>Whether the scene was a group</summary>
        public bool isGroup = default!;
    }

    public class SceneNameChangedData
    {
        /// <summary>UUID of the scene</summary>
        public string sceneUuid = default!;
        /// <summary>Old name of the scene</summary>
        public string oldSceneName = default!;
        /// <summary>New name of the scene</summary>
        public string sceneName = default!;
    }

    public class CurrentProgramSceneChangedData
    {
        /// <summary>Name of the scene that was switched to</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene that was switched to</summary>
        public string sceneUuid = default!;
    }

    public class CurrentPreviewSceneChangedData
    {
        /// <summary>Name of the scene that was switched to</summary>
        public string sceneName = default!;
        /// <summary>UUID of the scene that was switched to</summary>
        public string sceneUuid = default!;
    }

    public class SceneListChangedData
    {
        /// <summary>Updated array of scenes</summary>
        public JsonElement scenes = default!;
    }

    public class CurrentSceneTransitionChangedData
    {
        /// <summary>Name of the new transition</summary>
        public string transitionName = default!;
        /// <summary>UUID of the new transition</summary>
        public string transitionUuid = default!;
    }

    public class CurrentSceneTransitionDurationChangedData
    {
        /// <summary>Transition duration in milliseconds</summary>
        public double transitionDuration = default!;
    }

    public class SceneTransitionStartedData
    {
        /// <summary>Scene transition name</summary>
        public string transitionName = default!;
        /// <summary>Scene transition UUID</summary>
        public string transitionUuid = default!;
    }

    public class SceneTransitionEndedData
    {
        /// <summary>Scene transition name</summary>
        public string transitionName = default!;
        /// <summary>Scene transition UUID</summary>
        public string transitionUuid = default!;
    }

    public class SceneTransitionVideoEndedData
    {
        /// <summary>Scene transition name</summary>
        public string transitionName = default!;
        /// <summary>Scene transition UUID</summary>
        public string transitionUuid = default!;
    }

    public class StudioModeStateChangedData
    {
        /// <summary>True == Enabled, False == Disabled</summary>
        public bool studioModeEnabled = default!;
    }

    public class ScreenshotSavedData
    {
        /// <summary>Path of the saved image file</summary>
        public string savedScreenshotPath = default!;
    }

    public class VendorEventData
    {
        /// <summary>Name of the vendor emitting the event</summary>
        public string vendorName = default!;
        /// <summary>Vendor-provided event typedef</summary>
        public string eventType = default!;
        /// <summary>Vendor-provided event data. {} if event does not provide any data</summary>
        public JsonElement eventData = default!;
    }

    public class CustomEventData
    {
        /// <summary>Custom event data</summary>
        public JsonElement eventData = default!;
    }

    public class OBSEvents
    {
        /// <summary>
        /// The current scene collection has begun changing.
        /// 
        /// Note: We recommend using this event to trigger a pause of all polling requests, as performing any requests during a
        /// scene collection change is considered undefined behavior and can cause crashes!
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Config, 1, 1, false, "5.0.0", "config")]
        public event EventHandler<CurrentSceneCollectionChangingData>? CurrentSceneCollectionChanging;

        /// <summary>
        /// The current scene collection has changed.
        /// 
        /// Note: If polling has been paused during `CurrentSceneCollectionChanging`, this is the que to restart polling.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Config, 1, 1, false, "5.0.0", "config")]
        public event EventHandler<CurrentSceneCollectionChangedData>? CurrentSceneCollectionChanged;

        /// <summary>The scene collection list has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Config, 1, 1, false, "5.0.0", "config")]
        public event EventHandler<SceneCollectionListChangedData>? SceneCollectionListChanged;

        /// <summary>The current profile has begun changing.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Config, 1, 1, false, "5.0.0", "config")]
        public event EventHandler<CurrentProfileChangingData>? CurrentProfileChanging;

        /// <summary>The current profile has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Config, 1, 1, false, "5.0.0", "config")]
        public event EventHandler<CurrentProfileChangedData>? CurrentProfileChanged;

        /// <summary>The profile list has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Config, 1, 1, false, "5.0.0", "config")]
        public event EventHandler<ProfileListChangedData>? ProfileListChanged;

        /// <summary>A source's filter list has been reindexed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Filters, 3, 1, false, "5.0.0", "filters")]
        public event EventHandler<SourceFilterListReindexedData>? SourceFilterListReindexed;

        /// <summary>A filter has been added to a source.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Filters, 2, 1, false, "5.0.0", "filters")]
        public event EventHandler<SourceFilterCreatedData>? SourceFilterCreated;

        /// <summary>A filter has been removed from a source.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Filters, 2, 1, false, "5.0.0", "filters")]
        public event EventHandler<SourceFilterRemovedData>? SourceFilterRemoved;

        /// <summary>The name of a source filter has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Filters, 2, 1, false, "5.0.0", "filters")]
        public event EventHandler<SourceFilterNameChangedData>? SourceFilterNameChanged;

        /// <summary>An source filter's settings have changed (been updated).</summary>
        /// <remarks>Since version 5.4.0</remarks>
        [EventDataMetadata(EventSubscription.Filters, 3, 1, false, "5.4.0", "filters")]
        public event EventHandler<SourceFilterSettingsChangedData>? SourceFilterSettingsChanged;

        /// <summary>A source filter's enable state has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Filters, 3, 1, false, "5.0.0", "filters")]
        public event EventHandler<SourceFilterEnableStateChangedData>? SourceFilterEnableStateChanged;

        /// <summary>OBS has begun the shutdown process.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.General, 1, 1, false, "5.0.0", "general")]
        public event EventHandler? ExitStarted;

        /// <summary>An input has been created.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 2, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputCreatedData>? InputCreated;

        /// <summary>An input has been removed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 2, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputRemovedData>? InputRemoved;

        /// <summary>The name of an input has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 2, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputNameChangedData>? InputNameChanged;

        /// <summary>
        /// An input's settings have changed (been updated).
        /// 
        /// Note: On some inputs, changing values in the properties dialog will cause an immediate update. Pressing the "Cancel" button will revert the settings, resulting in another event being fired.
        /// </summary>
        /// <remarks>Since version 5.4.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 3, 1, false, "5.4.0", "inputs")]
        public event EventHandler<InputSettingsChangedData>? InputSettingsChanged;

        /// <summary>
        /// An input's active state has changed.
        /// 
        /// When an input is active, it means it's being shown by the program feed.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.InputActiveStateChanged, 3, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputActiveStateChangedData>? InputActiveStateChanged;

        /// <summary>
        /// An input's show state has changed.
        /// 
        /// When an input is showing, it means it's being shown by the preview or a dialog.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.InputShowStateChanged, 3, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputShowStateChangedData>? InputShowStateChanged;

        /// <summary>An input's mute state has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 2, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputMuteStateChangedData>? InputMuteStateChanged;

        /// <summary>An input's volume level has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 3, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputVolumeChangedData>? InputVolumeChanged;

        /// <summary>The audio balance value of an input has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 2, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputAudioBalanceChangedData>? InputAudioBalanceChanged;

        /// <summary>The sync offset of an input has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 3, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputAudioSyncOffsetChangedData>? InputAudioSyncOffsetChanged;

        /// <summary>The audio tracks of an input have changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 3, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputAudioTracksChangedData>? InputAudioTracksChanged;

        /// <summary>
        /// The monitor type of an input has changed.
        /// 
        /// Available types are:
        /// 
        /// - `OBS_MONITORING_TYPE_NONE`
        /// - `OBS_MONITORING_TYPE_MONITOR_ONLY`
        /// - `OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT`
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Inputs, 2, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputAudioMonitorTypeChangedData>? InputAudioMonitorTypeChanged;

        /// <summary>A high-volume event providing volume levels of all active inputs every 50 milliseconds.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.InputVolumeMeters, 4, 1, false, "5.0.0", "inputs")]
        public event EventHandler<InputVolumeMetersData>? InputVolumeMeters;

        /// <summary>A media input has started playing.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.MediaInputs, 2, 1, false, "5.0.0", "media inputs")]
        public event EventHandler<MediaInputPlaybackStartedData>? MediaInputPlaybackStarted;

        /// <summary>A media input has finished playing.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.MediaInputs, 2, 1, false, "5.0.0", "media inputs")]
        public event EventHandler<MediaInputPlaybackEndedData>? MediaInputPlaybackEnded;

        /// <summary>An action has been performed on an input.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.MediaInputs, 2, 1, false, "5.0.0", "media inputs")]
        public event EventHandler<MediaInputActionTriggeredData>? MediaInputActionTriggered;

        /// <summary>The state of the stream output has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Outputs, 2, 1, false, "5.0.0", "outputs")]
        public event EventHandler<StreamStateChangedData>? StreamStateChanged;

        /// <summary>The state of the record output has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Outputs, 2, 1, false, "5.0.0", "outputs")]
        public event EventHandler<RecordStateChangedData>? RecordStateChanged;

        /// <summary>The state of the replay buffer output has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Outputs, 2, 1, false, "5.0.0", "outputs")]
        public event EventHandler<ReplayBufferStateChangedData>? ReplayBufferStateChanged;

        /// <summary>The state of the virtualcam output has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Outputs, 2, 1, false, "5.0.0", "outputs")]
        public event EventHandler<VirtualcamStateChangedData>? VirtualcamStateChanged;

        /// <summary>The replay buffer has been saved.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Outputs, 2, 1, false, "5.0.0", "outputs")]
        public event EventHandler<ReplayBufferSavedData>? ReplayBufferSaved;

        /// <summary>A scene item has been created.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItems, 3, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemCreatedData>? SceneItemCreated;

        /// <summary>
        /// A scene item has been removed.
        /// 
        /// This event is not emitted when the scene the item is in is removed.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItems, 3, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemRemovedData>? SceneItemRemoved;

        /// <summary>A scene's item list has been reindexed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItems, 3, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemListReindexedData>? SceneItemListReindexed;

        /// <summary>A scene item's enable state has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItems, 3, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemEnableStateChangedData>? SceneItemEnableStateChanged;

        /// <summary>A scene item's lock state has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItems, 3, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemLockStateChangedData>? SceneItemLockStateChanged;

        /// <summary>A scene item has been selected in the Ui.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItems, 2, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemSelectedData>? SceneItemSelected;

        /// <summary>The transform/crop of a scene item has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.SceneItemTransformChanged, 4, 1, false, "5.0.0", "scene items")]
        public event EventHandler<SceneItemTransformChangedData>? SceneItemTransformChanged;

        /// <summary>A new scene has been created.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Scenes, 2, 1, false, "5.0.0", "scenes")]
        public event EventHandler<SceneCreatedData>? SceneCreated;

        /// <summary>A scene has been removed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Scenes, 2, 1, false, "5.0.0", "scenes")]
        public event EventHandler<SceneRemovedData>? SceneRemoved;

        /// <summary>The name of a scene has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Scenes, 2, 1, false, "5.0.0", "scenes")]
        public event EventHandler<SceneNameChangedData>? SceneNameChanged;

        /// <summary>The current program scene has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Scenes, 1, 1, false, "5.0.0", "scenes")]
        public event EventHandler<CurrentProgramSceneChangedData>? CurrentProgramSceneChanged;

        /// <summary>The current preview scene has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Scenes, 1, 1, false, "5.0.0", "scenes")]
        public event EventHandler<CurrentPreviewSceneChangedData>? CurrentPreviewSceneChanged;

        /// <summary>
        /// The list of scenes has changed.
        /// 
        /// TODO: Make OBS fire this event when scenes are reordered.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Scenes, 2, 1, false, "5.0.0", "scenes")]
        public event EventHandler<SceneListChangedData>? SceneListChanged;

        /// <summary>The current scene transition has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Transitions, 2, 1, false, "5.0.0", "transitions")]
        public event EventHandler<CurrentSceneTransitionChangedData>? CurrentSceneTransitionChanged;

        /// <summary>The current scene transition duration has changed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Transitions, 2, 1, false, "5.0.0", "transitions")]
        public event EventHandler<CurrentSceneTransitionDurationChangedData>? CurrentSceneTransitionDurationChanged;

        /// <summary>A scene transition has started.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Transitions, 2, 1, false, "5.0.0", "transitions")]
        public event EventHandler<SceneTransitionStartedData>? SceneTransitionStarted;

        /// <summary>
        /// A scene transition has completed fully.
        /// 
        /// Note: Does not appear to trigger when the transition is interrupted by the user.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Transitions, 2, 1, false, "5.0.0", "transitions")]
        public event EventHandler<SceneTransitionEndedData>? SceneTransitionEnded;

        /// <summary>
        /// A scene transition's video has completed fully.
        /// 
        /// Useful for stinger transitions to tell when the video *actually* ends.
        /// `SceneTransitionEnded` only signifies the cut point, not the completion of transition playback.
        /// 
        /// Note: Appears to be called by every transition, regardless of relevance.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Transitions, 2, 1, false, "5.0.0", "transitions")]
        public event EventHandler<SceneTransitionVideoEndedData>? SceneTransitionVideoEnded;

        /// <summary>Studio mode has been enabled or disabled.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Ui, 1, 1, false, "5.0.0", "ui")]
        public event EventHandler<StudioModeStateChangedData>? StudioModeStateChanged;

        /// <summary>
        /// A screenshot has been saved.
        /// 
        /// Note: Triggered for the screenshot feature available in `Settings -> Hotkeys -> Screenshot Output` ONLY.
        /// Applications using `Get/SaveSourceScreenshot` should implement a `CustomEvent` if this kind of inter-client
        /// communication is desired.
        /// </summary>
        /// <remarks>Since version 5.1.0</remarks>
        [EventDataMetadata(EventSubscription.Ui, 2, 1, false, "5.1.0", "ui")]
        public event EventHandler<ScreenshotSavedData>? ScreenshotSaved;

        /// <summary>
        /// An event has been emitted from a vendor.
        /// 
        /// A vendor is a unique name registered by a third-party plugin or script, which allows for custom requests and events to be added to obs-websocket.
        /// If a plugin or script implements vendor requests or events, documentation is expected to be provided with them.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.Vendors, 3, 1, false, "5.0.0", "general")]
        public event EventHandler<VendorEventData>? VendorEvent;

        /// <summary>Custom event emitted by `BroadcastCustomEvent`.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EventDataMetadata(EventSubscription.General, 1, 1, false, "5.0.0", "general")]
        public event EventHandler<CustomEventData>? CustomEvent;

        internal void ProcessEventData(JsonElement json)
        {
            string eventType = json.ReadString("eventType") ?? "";
            JsonElement data = default;
            if (json.TryGetProperty("eventData", out var val))
            {
                data = val;
            }
            string failedToDeserialize = "Failed to deserialize data for the {0} event.";
            JsonSerializerOptions serializerOptions = new()
            {
                IncludeFields = true,
            };

            switch (eventType)
            {
                case nameof(CurrentSceneCollectionChanging):
                    {
                        CurrentSceneCollectionChanging?.Invoke(this, JsonSerializer.Deserialize<CurrentSceneCollectionChangingData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentSceneCollectionChanging))));
                        break;
                    }
                case nameof(CurrentSceneCollectionChanged):
                    {
                        CurrentSceneCollectionChanged?.Invoke(this, JsonSerializer.Deserialize<CurrentSceneCollectionChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentSceneCollectionChanged))));
                        break;
                    }
                case nameof(SceneCollectionListChanged):
                    {
                        SceneCollectionListChanged?.Invoke(this, JsonSerializer.Deserialize<SceneCollectionListChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneCollectionListChanged))));
                        break;
                    }
                case nameof(CurrentProfileChanging):
                    {
                        CurrentProfileChanging?.Invoke(this, JsonSerializer.Deserialize<CurrentProfileChangingData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentProfileChanging))));
                        break;
                    }
                case nameof(CurrentProfileChanged):
                    {
                        CurrentProfileChanged?.Invoke(this, JsonSerializer.Deserialize<CurrentProfileChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentProfileChanged))));
                        break;
                    }
                case nameof(ProfileListChanged):
                    {
                        ProfileListChanged?.Invoke(this, JsonSerializer.Deserialize<ProfileListChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(ProfileListChanged))));
                        break;
                    }
                case nameof(SourceFilterListReindexed):
                    {
                        SourceFilterListReindexed?.Invoke(this, JsonSerializer.Deserialize<SourceFilterListReindexedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SourceFilterListReindexed))));
                        break;
                    }
                case nameof(SourceFilterCreated):
                    {
                        SourceFilterCreated?.Invoke(this, JsonSerializer.Deserialize<SourceFilterCreatedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SourceFilterCreated))));
                        break;
                    }
                case nameof(SourceFilterRemoved):
                    {
                        SourceFilterRemoved?.Invoke(this, JsonSerializer.Deserialize<SourceFilterRemovedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SourceFilterRemoved))));
                        break;
                    }
                case nameof(SourceFilterNameChanged):
                    {
                        SourceFilterNameChanged?.Invoke(this, JsonSerializer.Deserialize<SourceFilterNameChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SourceFilterNameChanged))));
                        break;
                    }
                case nameof(SourceFilterSettingsChanged):
                    {
                        SourceFilterSettingsChanged?.Invoke(this, JsonSerializer.Deserialize<SourceFilterSettingsChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SourceFilterSettingsChanged))));
                        break;
                    }
                case nameof(SourceFilterEnableStateChanged):
                    {
                        SourceFilterEnableStateChanged?.Invoke(this, JsonSerializer.Deserialize<SourceFilterEnableStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SourceFilterEnableStateChanged))));
                        break;
                    }
                case nameof(ExitStarted):
                    {
                        ExitStarted?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                case nameof(InputCreated):
                    {
                        InputCreated?.Invoke(this, JsonSerializer.Deserialize<InputCreatedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputCreated))));
                        break;
                    }
                case nameof(InputRemoved):
                    {
                        InputRemoved?.Invoke(this, JsonSerializer.Deserialize<InputRemovedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputRemoved))));
                        break;
                    }
                case nameof(InputNameChanged):
                    {
                        InputNameChanged?.Invoke(this, JsonSerializer.Deserialize<InputNameChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputNameChanged))));
                        break;
                    }
                case nameof(InputSettingsChanged):
                    {
                        InputSettingsChanged?.Invoke(this, JsonSerializer.Deserialize<InputSettingsChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputSettingsChanged))));
                        break;
                    }
                case nameof(InputActiveStateChanged):
                    {
                        InputActiveStateChanged?.Invoke(this, JsonSerializer.Deserialize<InputActiveStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputActiveStateChanged))));
                        break;
                    }
                case nameof(InputShowStateChanged):
                    {
                        InputShowStateChanged?.Invoke(this, JsonSerializer.Deserialize<InputShowStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputShowStateChanged))));
                        break;
                    }
                case nameof(InputMuteStateChanged):
                    {
                        InputMuteStateChanged?.Invoke(this, JsonSerializer.Deserialize<InputMuteStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputMuteStateChanged))));
                        break;
                    }
                case nameof(InputVolumeChanged):
                    {
                        InputVolumeChanged?.Invoke(this, JsonSerializer.Deserialize<InputVolumeChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputVolumeChanged))));
                        break;
                    }
                case nameof(InputAudioBalanceChanged):
                    {
                        InputAudioBalanceChanged?.Invoke(this, JsonSerializer.Deserialize<InputAudioBalanceChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputAudioBalanceChanged))));
                        break;
                    }
                case nameof(InputAudioSyncOffsetChanged):
                    {
                        InputAudioSyncOffsetChanged?.Invoke(this, JsonSerializer.Deserialize<InputAudioSyncOffsetChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputAudioSyncOffsetChanged))));
                        break;
                    }
                case nameof(InputAudioTracksChanged):
                    {
                        InputAudioTracksChanged?.Invoke(this, JsonSerializer.Deserialize<InputAudioTracksChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputAudioTracksChanged))));
                        break;
                    }
                case nameof(InputAudioMonitorTypeChanged):
                    {
                        InputAudioMonitorTypeChanged?.Invoke(this, JsonSerializer.Deserialize<InputAudioMonitorTypeChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputAudioMonitorTypeChanged))));
                        break;
                    }
                case nameof(InputVolumeMeters):
                    {
                        InputVolumeMeters?.Invoke(this, JsonSerializer.Deserialize<InputVolumeMetersData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(InputVolumeMeters))));
                        break;
                    }
                case nameof(MediaInputPlaybackStarted):
                    {
                        MediaInputPlaybackStarted?.Invoke(this, JsonSerializer.Deserialize<MediaInputPlaybackStartedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(MediaInputPlaybackStarted))));
                        break;
                    }
                case nameof(MediaInputPlaybackEnded):
                    {
                        MediaInputPlaybackEnded?.Invoke(this, JsonSerializer.Deserialize<MediaInputPlaybackEndedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(MediaInputPlaybackEnded))));
                        break;
                    }
                case nameof(MediaInputActionTriggered):
                    {
                        MediaInputActionTriggered?.Invoke(this, JsonSerializer.Deserialize<MediaInputActionTriggeredData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(MediaInputActionTriggered))));
                        break;
                    }
                case nameof(StreamStateChanged):
                    {
                        StreamStateChanged?.Invoke(this, JsonSerializer.Deserialize<StreamStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(StreamStateChanged))));
                        break;
                    }
                case nameof(RecordStateChanged):
                    {
                        RecordStateChanged?.Invoke(this, JsonSerializer.Deserialize<RecordStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(RecordStateChanged))));
                        break;
                    }
                case nameof(ReplayBufferStateChanged):
                    {
                        ReplayBufferStateChanged?.Invoke(this, JsonSerializer.Deserialize<ReplayBufferStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(ReplayBufferStateChanged))));
                        break;
                    }
                case nameof(VirtualcamStateChanged):
                    {
                        VirtualcamStateChanged?.Invoke(this, JsonSerializer.Deserialize<VirtualcamStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(VirtualcamStateChanged))));
                        break;
                    }
                case nameof(ReplayBufferSaved):
                    {
                        ReplayBufferSaved?.Invoke(this, JsonSerializer.Deserialize<ReplayBufferSavedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(ReplayBufferSaved))));
                        break;
                    }
                case nameof(SceneItemCreated):
                    {
                        SceneItemCreated?.Invoke(this, JsonSerializer.Deserialize<SceneItemCreatedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemCreated))));
                        break;
                    }
                case nameof(SceneItemRemoved):
                    {
                        SceneItemRemoved?.Invoke(this, JsonSerializer.Deserialize<SceneItemRemovedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemRemoved))));
                        break;
                    }
                case nameof(SceneItemListReindexed):
                    {
                        SceneItemListReindexed?.Invoke(this, JsonSerializer.Deserialize<SceneItemListReindexedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemListReindexed))));
                        break;
                    }
                case nameof(SceneItemEnableStateChanged):
                    {
                        SceneItemEnableStateChanged?.Invoke(this, JsonSerializer.Deserialize<SceneItemEnableStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemEnableStateChanged))));
                        break;
                    }
                case nameof(SceneItemLockStateChanged):
                    {
                        SceneItemLockStateChanged?.Invoke(this, JsonSerializer.Deserialize<SceneItemLockStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemLockStateChanged))));
                        break;
                    }
                case nameof(SceneItemSelected):
                    {
                        SceneItemSelected?.Invoke(this, JsonSerializer.Deserialize<SceneItemSelectedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemSelected))));
                        break;
                    }
                case nameof(SceneItemTransformChanged):
                    {
                        SceneItemTransformChanged?.Invoke(this, JsonSerializer.Deserialize<SceneItemTransformChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneItemTransformChanged))));
                        break;
                    }
                case nameof(SceneCreated):
                    {
                        SceneCreated?.Invoke(this, JsonSerializer.Deserialize<SceneCreatedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneCreated))));
                        break;
                    }
                case nameof(SceneRemoved):
                    {
                        SceneRemoved?.Invoke(this, JsonSerializer.Deserialize<SceneRemovedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneRemoved))));
                        break;
                    }
                case nameof(SceneNameChanged):
                    {
                        SceneNameChanged?.Invoke(this, JsonSerializer.Deserialize<SceneNameChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneNameChanged))));
                        break;
                    }
                case nameof(CurrentProgramSceneChanged):
                    {
                        CurrentProgramSceneChanged?.Invoke(this, JsonSerializer.Deserialize<CurrentProgramSceneChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentProgramSceneChanged))));
                        break;
                    }
                case nameof(CurrentPreviewSceneChanged):
                    {
                        CurrentPreviewSceneChanged?.Invoke(this, JsonSerializer.Deserialize<CurrentPreviewSceneChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentPreviewSceneChanged))));
                        break;
                    }
                case nameof(SceneListChanged):
                    {
                        SceneListChanged?.Invoke(this, JsonSerializer.Deserialize<SceneListChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneListChanged))));
                        break;
                    }
                case nameof(CurrentSceneTransitionChanged):
                    {
                        CurrentSceneTransitionChanged?.Invoke(this, JsonSerializer.Deserialize<CurrentSceneTransitionChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentSceneTransitionChanged))));
                        break;
                    }
                case nameof(CurrentSceneTransitionDurationChanged):
                    {
                        CurrentSceneTransitionDurationChanged?.Invoke(this, JsonSerializer.Deserialize<CurrentSceneTransitionDurationChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CurrentSceneTransitionDurationChanged))));
                        break;
                    }
                case nameof(SceneTransitionStarted):
                    {
                        SceneTransitionStarted?.Invoke(this, JsonSerializer.Deserialize<SceneTransitionStartedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneTransitionStarted))));
                        break;
                    }
                case nameof(SceneTransitionEnded):
                    {
                        SceneTransitionEnded?.Invoke(this, JsonSerializer.Deserialize<SceneTransitionEndedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneTransitionEnded))));
                        break;
                    }
                case nameof(SceneTransitionVideoEnded):
                    {
                        SceneTransitionVideoEnded?.Invoke(this, JsonSerializer.Deserialize<SceneTransitionVideoEndedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(SceneTransitionVideoEnded))));
                        break;
                    }
                case nameof(StudioModeStateChanged):
                    {
                        StudioModeStateChanged?.Invoke(this, JsonSerializer.Deserialize<StudioModeStateChangedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(StudioModeStateChanged))));
                        break;
                    }
                case nameof(ScreenshotSaved):
                    {
                        ScreenshotSaved?.Invoke(this, JsonSerializer.Deserialize<ScreenshotSavedData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(ScreenshotSaved))));
                        break;
                    }
                case nameof(VendorEvent):
                    {
                        VendorEvent?.Invoke(this, JsonSerializer.Deserialize<VendorEventData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(VendorEvent))));
                        break;
                    }
                case nameof(CustomEvent):
                    {
                        CustomEvent?.Invoke(this, JsonSerializer.Deserialize<CustomEventData>(data, serializerOptions) ?? throw new ArgumentException(string.Format(failedToDeserialize, nameof(CustomEvent))));
                        break;
                    }
                default:
                {
                    Console.WriteLine($"Unsupported Event: {eventType}\n{data.GetRawText()}");
                    break;
                }
            }
        }
    }
}
