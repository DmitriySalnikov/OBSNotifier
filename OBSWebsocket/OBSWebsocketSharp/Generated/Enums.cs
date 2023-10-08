namespace OBSWebsocketSharp
{
    public enum EventSubscription
    {
        /// <summary>Subcription value used to disable all events.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "0", false)]
        None = 0,

        /// <summary>Subscription value to receive events in the `General` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 0)", false)]
        General = (1 << 0),

        /// <summary>Subscription value to receive events in the `Config` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 1)", false)]
        Config = (1 << 1),

        /// <summary>Subscription value to receive events in the `Scenes` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 2)", false)]
        Scenes = (1 << 2),

        /// <summary>Subscription value to receive events in the `Inputs` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 3)", false)]
        Inputs = (1 << 3),

        /// <summary>Subscription value to receive events in the `Transitions` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 4)", false)]
        Transitions = (1 << 4),

        /// <summary>Subscription value to receive events in the `Filters` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 5)", false)]
        Filters = (1 << 5),

        /// <summary>Subscription value to receive events in the `Outputs` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 6)", false)]
        Outputs = (1 << 6),

        /// <summary>Subscription value to receive events in the `SceneItems` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 7)", false)]
        SceneItems = (1 << 7),

        /// <summary>Subscription value to receive events in the `MediaInputs` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 8)", false)]
        MediaInputs = (1 << 8),

        /// <summary>Subscription value to receive the `VendorEvent` event.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 9)", false)]
        Vendors = (1 << 9),

        /// <summary>Subscription value to receive events in the `Ui` category.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 10)", false)]
        Ui = (1 << 10),

        /// <summary>Helper to receive all non-high-volume events.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(General | Config | Scenes | Inputs | Transitions | Filters | Outputs | SceneItems | MediaInputs | Vendors | Ui)", false)]
        All = (General | Config | Scenes | Inputs | Transitions | Filters | Outputs | SceneItems | MediaInputs | Vendors | Ui),

        /// <summary>Subscription value to receive the `InputVolumeMeters` high-volume event.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 16)", false)]
        InputVolumeMeters = (1 << 16),

        /// <summary>Subscription value to receive the `InputActiveStateChanged` high-volume event.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 17)", false)]
        InputActiveStateChanged = (1 << 17),

        /// <summary>Subscription value to receive the `InputShowStateChanged` high-volume event.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 18)", false)]
        InputShowStateChanged = (1 << 18),

        /// <summary>Subscription value to receive the `SceneItemTransformChanged` high-volume event.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "(1 << 19)", false)]
        SceneItemTransformChanged = (1 << 19),

    }

    public enum RequestBatchExecutionType
    {
        /// <summary>Not a request batch.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "-1", false)]
        None = -1,

        /// <summary>
        /// A request batch which processes all requests serially, as fast as possible.
        /// 
        /// Note: To introduce artificial delay, use the `Sleep` request and the `sleepMillis` request field.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "0", false)]
        SerialRealtime = 0,

        /// <summary>
        /// A request batch type which processes all requests serially, in sync with the graphics thread. Designed to provide high accuracy for animations.
        /// 
        /// Note: To introduce artificial delay, use the `Sleep` request and the `sleepFrames` request field.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "1", false)]
        SerialFrame = 1,

        /// <summary>
        /// A request batch type which processes all requests using all available threads in the thread pool.
        /// 
        /// Note: This is mainly experimental, and only really shows its colors during requests which require lots of
        /// active processing, like `GetSourceScreenshot`.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "2", false)]
        Parallel = 2,

    }

    public enum RequestStatus
    {
        /// <summary>Unknown status, should never be used.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "0", false)]
        Unknown = 0,

        /// <summary>For internal use to signify a successful field check.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "10", false)]
        NoError = 10,

        /// <summary>The request has succeeded.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "100", false)]
        Success = 100,

        /// <summary>The `requestType` field is missing from the request data.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "203", false)]
        MissingRequestType = 203,

        /// <summary>The request type is invalid or does not exist.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "204", false)]
        UnknownRequestType = 204,

        /// <summary>
        /// Generic error code.
        /// 
        /// Note: A comment is required to be provided by obs-websocket.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "205", false)]
        GenericError = 205,

        /// <summary>The request batch execution type is not supported.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "206", false)]
        UnsupportedRequestBatchExecutionType = 206,

        /// <summary>
        /// The server is not ready to handle the request.
        /// 
        /// Note: This usually occurs during OBS scene collection change or exit. Requests may be tried again after a delay if this code is given.
        /// </summary>
        /// <remarks>Since version 5.3.0</remarks>
        [EnumElementMetadata(1, false, "5.3.0", "207", false)]
        NotReady = 207,

        /// <summary>A required request field is missing.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "300", false)]
        MissingRequestField = 300,

        /// <summary>The request does not have a valid requestData object.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "301", false)]
        MissingRequestData = 301,

        /// <summary>
        /// Generic invalid request field message.
        /// 
        /// Note: A comment is required to be provided by obs-websocket.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "400", false)]
        InvalidRequestField = 400,

        /// <summary>A request field has the wrong data type.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "401", false)]
        InvalidRequestFieldType = 401,

        /// <summary>A request field (number) is outside of the allowed range.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "402", false)]
        RequestFieldOutOfRange = 402,

        /// <summary>A request field (string or array) is empty and cannot be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "403", false)]
        RequestFieldEmpty = 403,

        /// <summary>There are too many request fields (eg. a request takes two optionals, where only one is allowed at a time).</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "404", false)]
        TooManyRequestFields = 404,

        /// <summary>An output is running and cannot be in order to perform the request.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "500", false)]
        OutputRunning = 500,

        /// <summary>An output is not running and should be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "501", false)]
        OutputNotRunning = 501,

        /// <summary>An output is paused and should not be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "502", false)]
        OutputPaused = 502,

        /// <summary>An output is not paused and should be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "503", false)]
        OutputNotPaused = 503,

        /// <summary>An output is disabled and should not be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "504", false)]
        OutputDisabled = 504,

        /// <summary>Studio mode is active and cannot be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "505", false)]
        StudioModeActive = 505,

        /// <summary>Studio mode is not active and should be.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "506", false)]
        StudioModeNotActive = 506,

        /// <summary>
        /// The resource was not found.
        /// 
        /// Note: Resources are any kind of object in obs-websocket, like inputs, profiles, outputs, etc.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "600", false)]
        ResourceNotFound = 600,

        /// <summary>The resource already exists.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "601", false)]
        ResourceAlreadyExists = 601,

        /// <summary>The type of resource found is invalid.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "602", false)]
        InvalidResourceType = 602,

        /// <summary>There are not enough instances of the resource in order to perform the request.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "603", false)]
        NotEnoughResources = 603,

        /// <summary>The state of the resource is invalid. For example, if the resource is blocked from being accessed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "604", false)]
        InvalidResourceState = 604,

        /// <summary>The specified input (obs_source_t-OBS_SOURCE_TYPE_INPUT) had the wrong kind.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "605", false)]
        InvalidInputKind = 605,

        /// <summary>
        /// The resource does not support being configured.
        /// 
        /// This is particularly relevant to transitions, where they do not always have changeable settings.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "606", false)]
        ResourceNotConfigurable = 606,

        /// <summary>The specified filter (obs_source_t-OBS_SOURCE_TYPE_FILTER) had the wrong kind.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "607", false)]
        InvalidFilterKind = 607,

        /// <summary>Creating the resource failed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "700", false)]
        ResourceCreationFailed = 700,

        /// <summary>Performing an action on the resource failed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "701", false)]
        ResourceActionFailed = 701,

        /// <summary>
        /// Processing the request failed unexpectedly.
        /// 
        /// Note: A comment is required to be provided by obs-websocket.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "702", false)]
        RequestProcessingFailed = 702,

        /// <summary>The combination of request fields cannot be used to perform an action.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "703", false)]
        CannotAct = 703,

    }

    public enum ObsOutputState
    {
        /// <summary>Unknown state.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_UNKNOWN", true)]
        OBS_WEBSOCKET_OUTPUT_UNKNOWN,

        /// <summary>The output is starting.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_STARTING", true)]
        OBS_WEBSOCKET_OUTPUT_STARTING,

        /// <summary>The input has started.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_STARTED", true)]
        OBS_WEBSOCKET_OUTPUT_STARTED,

        /// <summary>The output is stopping.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_STOPPING", true)]
        OBS_WEBSOCKET_OUTPUT_STOPPING,

        /// <summary>The output has stopped.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_STOPPED", true)]
        OBS_WEBSOCKET_OUTPUT_STOPPED,

        /// <summary>The output has disconnected and is reconnecting.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_RECONNECTING", true)]
        OBS_WEBSOCKET_OUTPUT_RECONNECTING,

        /// <summary>The output has reconnected successfully.</summary>
        /// <remarks>Since version 5.1.0</remarks>
        [EnumElementMetadata(1, true, "5.1.0", "OBS_WEBSOCKET_OUTPUT_RECONNECTED", true)]
        OBS_WEBSOCKET_OUTPUT_RECONNECTED,

        /// <summary>The output is now paused.</summary>
        /// <remarks>Since version 5.1.0</remarks>
        [EnumElementMetadata(1, true, "5.1.0", "OBS_WEBSOCKET_OUTPUT_PAUSED", true)]
        OBS_WEBSOCKET_OUTPUT_PAUSED,

        /// <summary>The output has been resumed (unpaused).</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_OUTPUT_RESUMED", true)]
        OBS_WEBSOCKET_OUTPUT_RESUMED,

    }

    public enum ObsMediaInputAction
    {
        /// <summary>No action.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NONE", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NONE,

        /// <summary>Play the media input.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY,

        /// <summary>Pause the media input.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE,

        /// <summary>Stop the media input.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP,

        /// <summary>Restart the media input.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART,

        /// <summary>Go to the next playlist item.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT,

        /// <summary>Go to the previous playlist item.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, true, "5.0.0", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS", true)]
        OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS,

    }

    public enum WebSocketCloseCode
    {
        /// <summary>For internal use only to tell the request handler not to perform any close action.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "0", false)]
        DontClose = 0,

        /// <summary>Unknown reason, should never be used.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4000", false)]
        UnknownReason = 4000,

        /// <summary>The server was unable to decode the incoming websocket message.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4002", false)]
        MessageDecodeError = 4002,

        /// <summary>A data field is required but missing from the payload.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4003", false)]
        MissingDataField = 4003,

        /// <summary>A data field's value type is invalid.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4004", false)]
        InvalidDataFieldType = 4004,

        /// <summary>A data field's value is invalid.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4005", false)]
        InvalidDataFieldValue = 4005,

        /// <summary>The specified `op` was invalid or missing.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4006", false)]
        UnknownOpCode = 4006,

        /// <summary>The client sent a websocket message without first sending `Identify` message.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4007", false)]
        NotIdentified = 4007,

        /// <summary>
        /// The client sent an `Identify` message while already identified.
        /// 
        /// Note: Once a client has identified, only `Reidentify` may be used to change session parameters.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4008", false)]
        AlreadyIdentified = 4008,

        /// <summary>The authentication attempt (via `Identify`) failed.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4009", false)]
        AuthenticationFailed = 4009,

        /// <summary>The server detected the usage of an old version of the obs-websocket RPC protocol.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4010", false)]
        UnsupportedRpcVersion = 4010,

        /// <summary>
        /// The websocket session has been invalidated by the obs-websocket server.
        /// 
        /// Note: This is the code used by the `Kick` button in the UI Session List. If you receive this code, you must not automatically reconnect.
        /// </summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4011", false)]
        SessionInvalidated = 4011,

        /// <summary>A requested feature is not supported due to hardware/software limitations.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "4012", false)]
        UnsupportedFeature = 4012,

    }

    public enum WebSocketOpCode
    {
        /// <summary>The initial message sent by obs-websocket to newly connected clients.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "0", false)]
        Hello = 0,

        /// <summary>The message sent by a newly connected client to obs-websocket in response to a `Hello`.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "1", false)]
        Identify = 1,

        /// <summary>The response sent by obs-websocket to a client after it has successfully identified with obs-websocket.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "2", false)]
        Identified = 2,

        /// <summary>The message sent by an already-identified client to update identification parameters.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "3", false)]
        Reidentify = 3,

        /// <summary>The message sent by obs-websocket containing an event payload.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "5", false)]
        Event = 5,

        /// <summary>The message sent by a client to obs-websocket to perform a request.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "6", false)]
        Request = 6,

        /// <summary>The message sent by obs-websocket in response to a particular request from a client.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "7", false)]
        RequestResponse = 7,

        /// <summary>The message sent by a client to obs-websocket to perform a batch of requests.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "8", false)]
        RequestBatch = 8,

        /// <summary>The message sent by obs-websocket in response to a particular batch of requests from a client.</summary>
        /// <remarks>Since version 5.0.0</remarks>
        [EnumElementMetadata(1, false, "5.0.0", "9", false)]
        RequestBatchResponse = 9,

    }

}
