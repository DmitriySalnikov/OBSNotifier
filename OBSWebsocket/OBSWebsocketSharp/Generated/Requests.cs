using System.Text.Json;
using System.Text.Json.Nodes;
using OBSWebsocketSharp.Extensions;

namespace OBSWebsocketSharp
{
    public partial class OBSRequests
    {
        /// <summary>Gets the value of a "slot" from the selected persistent data realm.</summary>
        /// <param name="realm">The data realm to select. `OBS_WEBSOCKET_DATA_REALM_GLOBAL` or `OBS_WEBSOCKET_DATA_REALM_PROFILE`</param>
        /// <param name="slotName">The name of the slot to retrieve data from</param>
        /// <returns>Value associated with the slot. `null` if not set</returns>
        public async Task<JsonElement> GetPersistentData(string realm, string slotName)
        {
            var data = new JsonObject()
            {
                {nameof(realm), realm},
                {nameof(slotName), slotName},
            };
            var response = await Request(data);
            return response;
        }

        /// <summary>Sets the value of a "slot" from the selected persistent data realm.</summary>
        /// <param name="realm">The data realm to select. `OBS_WEBSOCKET_DATA_REALM_GLOBAL` or `OBS_WEBSOCKET_DATA_REALM_PROFILE`</param>
        /// <param name="slotName">The name of the slot to retrieve data from</param>
        /// <param name="slotValue">The value to apply to the slot</param>
        public async Task SetPersistentData(string realm, string slotName, JsonObject slotValue)
        {
            var data = new JsonObject()
            {
                {nameof(realm), realm},
                {nameof(slotName), slotName},
                {nameof(slotValue), slotValue},
            };
            await Request(data);
        }

        public class GetSceneCollectionListReturn
        {
            /// <summary>The name of the current scene collection</summary>
            public string currentSceneCollectionName = default!;
            /// <summary>Array of all available scene collections</summary>
            public string[] sceneCollections = default!;
        }

        /// <summary>Gets an array of all scene collections</summary>
        /// <returns><see cref="GetSceneCollectionListReturn"/></returns>
        public async Task<GetSceneCollectionListReturn> GetSceneCollectionList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSceneCollectionListReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSceneCollectionListReturn));
        }

        /// <summary>
        /// Switches to a scene collection.
        /// 
        /// Note: This will block until the collection has finished changing.
        /// </summary>
        /// <param name="sceneCollectionName">Name of the scene collection to switch to</param>
        public async Task SetCurrentSceneCollection(string sceneCollectionName)
        {
            var data = new JsonObject()
            {
                {nameof(sceneCollectionName), sceneCollectionName},
            };
            await Request(data);
        }

        /// <summary>
        /// Creates a new scene collection, switching to it in the process.
        /// 
        /// Note: This will block until the collection has finished changing.
        /// </summary>
        /// <param name="sceneCollectionName">Name for the new scene collection</param>
        public async Task CreateSceneCollection(string sceneCollectionName)
        {
            var data = new JsonObject()
            {
                {nameof(sceneCollectionName), sceneCollectionName},
            };
            await Request(data);
        }

        public class GetProfileListReturn
        {
            /// <summary>The name of the current profile</summary>
            public string currentProfileName = default!;
            /// <summary>Array of all available profiles</summary>
            public string[] profiles = default!;
        }

        /// <summary>Gets an array of all profiles</summary>
        /// <returns><see cref="GetProfileListReturn"/></returns>
        public async Task<GetProfileListReturn> GetProfileList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetProfileListReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetProfileListReturn));
        }

        /// <summary>Switches to a profile.</summary>
        /// <param name="profileName">Name of the profile to switch to</param>
        public async Task SetCurrentProfile(string profileName)
        {
            var data = new JsonObject()
            {
                {nameof(profileName), profileName},
            };
            await Request(data);
        }

        /// <summary>Creates a new profile, switching to it in the process</summary>
        /// <param name="profileName">Name for the new profile</param>
        public async Task CreateProfile(string profileName)
        {
            var data = new JsonObject()
            {
                {nameof(profileName), profileName},
            };
            await Request(data);
        }

        /// <summary>Removes a profile. If the current profile is chosen, it will change to a different profile first.</summary>
        /// <param name="profileName">Name of the profile to remove</param>
        public async Task RemoveProfile(string profileName)
        {
            var data = new JsonObject()
            {
                {nameof(profileName), profileName},
            };
            await Request(data);
        }

        public class GetProfileParameterReturn
        {
            /// <summary>Value associated with the parameter. `null` if not set and no default</summary>
            public string parameterValue = default!;
            /// <summary>Default value associated with the parameter. `null` if no default</summary>
            public string defaultParameterValue = default!;
        }

        /// <summary>Gets a parameter from the current profile's configuration.</summary>
        /// <param name="parameterCategory">Category of the parameter to get</param>
        /// <param name="parameterName">Name of the parameter to get</param>
        /// <returns><see cref="GetProfileParameterReturn"/></returns>
        public async Task<GetProfileParameterReturn> GetProfileParameter(string parameterCategory, string parameterName)
        {
            var data = new JsonObject()
            {
                {nameof(parameterCategory), parameterCategory},
                {nameof(parameterName), parameterName},
            };
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetProfileParameterReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetProfileParameterReturn));
        }

        /// <summary>Sets the value of a parameter in the current profile's configuration.</summary>
        /// <param name="parameterCategory">Category of the parameter to set</param>
        /// <param name="parameterName">Name of the parameter to set</param>
        /// <param name="parameterValue">Value of the parameter to set. Use `null` to delete</param>
        public async Task SetProfileParameter(string parameterCategory, string parameterName, string parameterValue)
        {
            var data = new JsonObject()
            {
                {nameof(parameterCategory), parameterCategory},
                {nameof(parameterName), parameterName},
                {nameof(parameterValue), parameterValue},
            };
            await Request(data);
        }

        public class GetVideoSettingsReturn
        {
            /// <summary>Numerator of the fractional FPS value</summary>
            public double fpsNumerator = default!;
            /// <summary>Denominator of the fractional FPS value</summary>
            public double fpsDenominator = default!;
            /// <summary>Width of the base (canvas) resolution in pixels</summary>
            public double baseWidth = default!;
            /// <summary>Height of the base (canvas) resolution in pixels</summary>
            public double baseHeight = default!;
            /// <summary>Width of the output resolution in pixels</summary>
            public double outputWidth = default!;
            /// <summary>Height of the output resolution in pixels</summary>
            public double outputHeight = default!;
        }

        /// <summary>
        /// Gets the current video settings.
        /// 
        /// Note: To get the true FPS value, divide the FPS numerator by the FPS denominator. Example: `60000/1001`
        /// </summary>
        /// <returns><see cref="GetVideoSettingsReturn"/></returns>
        public async Task<GetVideoSettingsReturn> GetVideoSettings()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetVideoSettingsReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetVideoSettingsReturn));
        }

        /// <summary>
        /// Sets the current video settings.
        /// 
        /// Note: Fields must be specified in pairs. For example, you cannot set only `baseWidth` without needing to specify `baseHeight`.
        /// </summary>
        /// <param name="fpsNumerator">
        /// Numerator of the fractional FPS value
        /// <code>If omitted: Not changed</code>
        /// <code>Restrictions: &gt;= 1</code>
        /// </param>
        /// <param name="fpsDenominator">
        /// Denominator of the fractional FPS value
        /// <code>If omitted: Not changed</code>
        /// <code>Restrictions: &gt;= 1</code>
        /// </param>
        /// <param name="baseWidth">
        /// Width of the base (canvas) resolution in pixels
        /// <code>If omitted: Not changed</code>
        /// <code>Restrictions: &gt;= 1, &lt;= 4096</code>
        /// </param>
        /// <param name="baseHeight">
        /// Height of the base (canvas) resolution in pixels
        /// <code>If omitted: Not changed</code>
        /// <code>Restrictions: &gt;= 1, &lt;= 4096</code>
        /// </param>
        /// <param name="outputWidth">
        /// Width of the output resolution in pixels
        /// <code>If omitted: Not changed</code>
        /// <code>Restrictions: &gt;= 1, &lt;= 4096</code>
        /// </param>
        /// <param name="outputHeight">
        /// Height of the output resolution in pixels
        /// <code>If omitted: Not changed</code>
        /// <code>Restrictions: &gt;= 1, &lt;= 4096</code>
        /// </param>
        public async Task SetVideoSettings(double? fpsNumerator = null, double? fpsDenominator = null, double? baseWidth = null, double? baseHeight = null, double? outputWidth = null, double? outputHeight = null)
        {
            if (!(fpsNumerator >= 1))
            {
                throw new ArgumentOutOfRangeException(nameof(fpsNumerator), $"{nameof(fpsNumerator)} outside of \">= 1\"");
            }
            if (!(fpsDenominator >= 1))
            {
                throw new ArgumentOutOfRangeException(nameof(fpsDenominator), $"{nameof(fpsDenominator)} outside of \">= 1\"");
            }
            if (!(baseWidth >= 1) || !(baseWidth <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(baseWidth), $"{nameof(baseWidth)} outside of \">= 1, <= 4096\"");
            }
            if (!(baseHeight >= 1) || !(baseHeight <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(baseHeight), $"{nameof(baseHeight)} outside of \">= 1, <= 4096\"");
            }
            if (!(outputWidth >= 1) || !(outputWidth <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(outputWidth), $"{nameof(outputWidth)} outside of \">= 1, <= 4096\"");
            }
            if (!(outputHeight >= 1) || !(outputHeight <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(outputHeight), $"{nameof(outputHeight)} outside of \">= 1, <= 4096\"");
            }
            var data = new JsonObject();
            if (fpsNumerator != null)
            {
                data.Add(nameof(fpsNumerator), fpsNumerator);
            }
            if (fpsDenominator != null)
            {
                data.Add(nameof(fpsDenominator), fpsDenominator);
            }
            if (baseWidth != null)
            {
                data.Add(nameof(baseWidth), baseWidth);
            }
            if (baseHeight != null)
            {
                data.Add(nameof(baseHeight), baseHeight);
            }
            if (outputWidth != null)
            {
                data.Add(nameof(outputWidth), outputWidth);
            }
            if (outputHeight != null)
            {
                data.Add(nameof(outputHeight), outputHeight);
            }
            await Request(data);
        }

        public class GetStreamServiceSettingsReturn
        {
            /// <summary>Stream service type, like `rtmp_custom` or `rtmp_common`</summary>
            public string streamServiceType = default!;
            /// <summary>Stream service settings</summary>
            public JsonElement streamServiceSettings = default!;
        }

        /// <summary>Gets the current stream service settings (stream destination).</summary>
        /// <returns><see cref="GetStreamServiceSettingsReturn"/></returns>
        public async Task<GetStreamServiceSettingsReturn> GetStreamServiceSettings()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetStreamServiceSettingsReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetStreamServiceSettingsReturn));
        }

        /// <summary>
        /// Sets the current stream service settings (stream destination).
        /// 
        /// Note: Simple RTMP settings can be set with type `rtmp_custom` and the settings fields `server` and `key`.
        /// </summary>
        /// <param name="streamServiceType">Type of stream service to apply. Example: `rtmp_common` or `rtmp_custom`</param>
        /// <param name="streamServiceSettings">Settings to apply to the service</param>
        public async Task SetStreamServiceSettings(string streamServiceType, JsonObject streamServiceSettings)
        {
            var data = new JsonObject()
            {
                {nameof(streamServiceType), streamServiceType},
                {nameof(streamServiceSettings), streamServiceSettings},
            };
            await Request(data);
        }

        /// <summary>Gets the current directory that the record output is set to.</summary>
        /// <returns>Output directory</returns>
        public async Task<string> GetRecordDirectory()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadString("recordDirectory") ?? throw new NullReferenceException("recordDirectory");
        }

        /// <summary>Sets the current directory that the record output writes files to.</summary>
        /// <param name="recordDirectory">Output directory</param>
        public async Task SetRecordDirectory(string recordDirectory)
        {
            var data = new JsonObject()
            {
                {nameof(recordDirectory), recordDirectory},
            };
            await Request(data);
        }

        /// <summary>
        /// Gets an array of all available source filter kinds.
        /// 
        /// Similar to `GetInputKindList`
        /// </summary>
        /// <returns>Array of source filter kinds</returns>
        public async Task<string[]> GetSourceFilterKindList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadStringArray("sourceFilterKinds") ?? throw new NullReferenceException("sourceFilterKinds");
        }

        /// <summary>Gets an array of all of a source's filters.</summary>
        /// <param name="sourceName">
        /// Name of the source
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Array of filters</returns>
        public async Task<JsonElement> GetSourceFilterList(string? sourceName = null, string? sourceUuid = null)
        {
            var data = new JsonObject();
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>Gets the default settings for a filter kind.</summary>
        /// <param name="filterKind">Filter kind to get the default settings for</param>
        /// <returns>Object of default settings for the filter kind</returns>
        public async Task<JsonElement> GetSourceFilterDefaultSettings(string filterKind)
        {
            var data = new JsonObject()
            {
                {nameof(filterKind), filterKind},
            };
            var response = await Request(data);
            return response;
        }

        /// <summary>Creates a new filter, adding it to the specified source.</summary>
        /// <param name="filterName">Name of the new filter to be created</param>
        /// <param name="filterKind">The kind of filter to be created</param>
        /// <param name="sourceName">
        /// Name of the source to add the filter to
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source to add the filter to
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="filterSettings">
        /// Settings object to initialize the filter with
        /// <code>If omitted: Default settings used</code>
        /// </param>
        public async Task CreateSourceFilter(string filterName, string filterKind, string? sourceName = null, string? sourceUuid = null, JsonObject? filterSettings = null)
        {
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
                {nameof(filterKind), filterKind},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (filterSettings != null)
            {
                data.Add(nameof(filterSettings), filterSettings);
            }
            await Request(data);
        }

        /// <summary>Removes a filter from a source.</summary>
        /// <param name="filterName">Name of the filter to remove</param>
        /// <param name="sourceUuid">
        /// UUID of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceName">
        /// Name of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task RemoveSourceFilter(string filterName, string? sourceUuid = null, string? sourceName = null)
        {
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
            };
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            await Request(data);
        }

        /// <summary>Sets the name of a source filter (rename).</summary>
        /// <param name="filterName">Current name of the filter</param>
        /// <param name="newFilterName">New name for the filter</param>
        /// <param name="sourceName">
        /// Name of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSourceFilterName(string filterName, string newFilterName, string? sourceName = null, string? sourceUuid = null)
        {
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
                {nameof(newFilterName), newFilterName},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            await Request(data);
        }

        public class GetSourceFilterReturn
        {
            /// <summary>Whether the filter is enabled</summary>
            public bool filterEnabled = default!;
            /// <summary>Index of the filter in the list, beginning at 0</summary>
            public double filterIndex = default!;
            /// <summary>The kind of filter</summary>
            public string filterKind = default!;
            /// <summary>Settings object associated with the filter</summary>
            public JsonElement filterSettings = default!;
        }

        /// <summary>Gets the info for a specific source filter.</summary>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="sourceUuid">
        /// UUID of the source
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceName">
        /// Name of the source
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetSourceFilterReturn"/></returns>
        public async Task<GetSourceFilterReturn> GetSourceFilter(string filterName, string? sourceUuid = null, string? sourceName = null)
        {
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
            };
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSourceFilterReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSourceFilterReturn));
        }

        /// <summary>Sets the index position of a filter on a source.</summary>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="filterIndex">
        /// New index position of the filter
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sourceName">
        /// Name of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSourceFilterIndex(string filterName, double filterIndex, string? sourceName = null, string? sourceUuid = null)
        {
            if (!(filterIndex >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(filterIndex), $"{nameof(filterIndex)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
                {nameof(filterIndex), filterIndex},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            await Request(data);
        }

        /// <summary>Sets the settings of a source filter.</summary>
        /// <param name="filterName">Name of the filter to set the settings of</param>
        /// <param name="filterSettings">Object of settings to apply</param>
        /// <param name="sourceName">
        /// Name of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="overlay">
        /// True == apply the settings on top of existing ones, False == reset the input to its defaults, then apply settings.
        /// <code>If omitted: true</code>
        /// </param>
        public async Task SetSourceFilterSettings(string filterName, JsonObject filterSettings, string? sourceName = null, string? sourceUuid = null, bool? overlay = null)
        {
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
                {nameof(filterSettings), filterSettings},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (overlay != null)
            {
                data.Add(nameof(overlay), overlay);
            }
            await Request(data);
        }

        /// <summary>Sets the enable state of a source filter.</summary>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="filterEnabled">New enable state of the filter</param>
        /// <param name="sourceName">
        /// Name of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source the filter is on
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSourceFilterEnabled(string filterName, bool filterEnabled, string? sourceName = null, string? sourceUuid = null)
        {
            var data = new JsonObject()
            {
                {nameof(filterName), filterName},
                {nameof(filterEnabled), filterEnabled},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            await Request(data);
        }

        public class GetVersionReturn
        {
            /// <summary>Current OBS Studio version</summary>
            public string obsVersion = default!;
            /// <summary>Current obs-websocket version</summary>
            public string obsWebSocketVersion = default!;
            /// <summary>Current latest obs-websocket RPC version</summary>
            public double rpcVersion = default!;
            /// <summary>Array of available RPC requests for the currently negotiated RPC version</summary>
            public string[] availableRequests = default!;
            /// <summary>Image formats available in `GetSourceScreenshot` and `SaveSourceScreenshot` requests.</summary>
            public string[] supportedImageFormats = default!;
            /// <summary>Name of the platform. Usually `windows`, `macos`, or `ubuntu` (linux flavor). Not guaranteed to be any of those</summary>
            public string platform = default!;
            /// <summary>Description of the platform, like `Windows 10 (10.0)`</summary>
            public string platformDescription = default!;
        }

        /// <summary>Gets data about the current plugin and RPC version.</summary>
        /// <returns><see cref="GetVersionReturn"/></returns>
        public async Task<GetVersionReturn> GetVersion()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetVersionReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetVersionReturn));
        }

        public class GetStatsReturn
        {
            /// <summary>Current CPU usage in percent</summary>
            public double cpuUsage = default!;
            /// <summary>Amount of memory in MB currently being used by OBS</summary>
            public double memoryUsage = default!;
            /// <summary>Available disk space on the device being used for recording storage</summary>
            public double availableDiskSpace = default!;
            /// <summary>Current FPS being rendered</summary>
            public double activeFps = default!;
            /// <summary>Average time in milliseconds that OBS is taking to render a frame</summary>
            public double averageFrameRenderTime = default!;
            /// <summary>Number of frames skipped by OBS in the render thread</summary>
            public double renderSkippedFrames = default!;
            /// <summary>Total number of frames outputted by the render thread</summary>
            public double renderTotalFrames = default!;
            /// <summary>Number of frames skipped by OBS in the output thread</summary>
            public double outputSkippedFrames = default!;
            /// <summary>Total number of frames outputted by the output thread</summary>
            public double outputTotalFrames = default!;
            /// <summary>Total number of messages received by obs-websocket from the client</summary>
            public double webSocketSessionIncomingMessages = default!;
            /// <summary>Total number of messages sent by obs-websocket to the client</summary>
            public double webSocketSessionOutgoingMessages = default!;
        }

        /// <summary>Gets statistics about OBS, obs-websocket, and the current session.</summary>
        /// <returns><see cref="GetStatsReturn"/></returns>
        public async Task<GetStatsReturn> GetStats()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetStatsReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetStatsReturn));
        }

        /// <summary>Broadcasts a `CustomEvent` to all WebSocket clients. Receivers are clients which are identified and subscribed.</summary>
        /// <param name="eventData">Data payload to emit to all receivers</param>
        public async Task BroadcastCustomEvent(JsonObject eventData)
        {
            var data = new JsonObject()
            {
                {nameof(eventData), eventData},
            };
            await Request(data);
        }

        public class CallVendorRequestReturn
        {
            /// <summary>Echoed of `vendorName`</summary>
            public string vendorName = default!;
            /// <summary>Echoed of `requestType`</summary>
            public string requestType = default!;
            /// <summary>Object containing appropriate response data. {} if request does not provide any response data</summary>
            public JsonElement responseData = default!;
        }

        /// <summary>
        /// Call a request registered to a vendor.
        /// 
        /// A vendor is a unique name registered by a third-party plugin or script, which allows for custom requests and events to be added to obs-websocket.
        /// If a plugin or script implements vendor requests or events, documentation is expected to be provided with them.
        /// </summary>
        /// <param name="vendorName">Name of the vendor to use</param>
        /// <param name="requestType">The request type to call</param>
        /// <param name="requestData">
        /// Object containing appropriate request data
        /// <code>If omitted: {}</code>
        /// </param>
        /// <returns><see cref="CallVendorRequestReturn"/></returns>
        public async Task<CallVendorRequestReturn> CallVendorRequest(string vendorName, string requestType, JsonObject? requestData = null)
        {
            var data = new JsonObject()
            {
                {nameof(vendorName), vendorName},
                {nameof(requestType), requestType},
            };
            if (requestData != null)
            {
                data.Add(nameof(requestData), requestData);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<CallVendorRequestReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(CallVendorRequestReturn));
        }

        /// <summary>
        /// Gets an array of all hotkey names in OBS.
        /// 
        /// Note: Hotkey functionality in obs-websocket comes as-is, and we do not guarantee support if things are broken. In 9/10 usages of hotkey requests, there exists a better, more reliable method via other requests.
        /// </summary>
        /// <returns>Array of hotkey names</returns>
        public async Task<string[]> GetHotkeyList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadStringArray("hotkeys") ?? throw new NullReferenceException("hotkeys");
        }

        /// <summary>
        /// Triggers a hotkey using its name. See `GetHotkeyList`.
        /// 
        /// Note: Hotkey functionality in obs-websocket comes as-is, and we do not guarantee support if things are broken. In 9/10 usages of hotkey requests, there exists a better, more reliable method via other requests.
        /// </summary>
        /// <param name="hotkeyName">Name of the hotkey to trigger</param>
        /// <param name="contextName">
        /// Name of context of the hotkey to trigger
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task TriggerHotkeyByName(string hotkeyName, string? contextName = null)
        {
            var data = new JsonObject()
            {
                {nameof(hotkeyName), hotkeyName},
            };
            if (contextName != null)
            {
                data.Add(nameof(contextName), contextName);
            }
            await Request(data);
        }

        /// <summary>
        /// Triggers a hotkey using a sequence of keys.
        /// 
        /// Note: Hotkey functionality in obs-websocket comes as-is, and we do not guarantee support if things are broken. In 9/10 usages of hotkey requests, there exists a better, more reliable method via other requests.
        /// </summary>
        /// <param name="keyId">
        /// The OBS key ID to use. See https://github.com/obsproject/obs-studio/blob/master/libobs/obs-hotkeys.h
        /// <code>If omitted: Not pressed</code>
        /// </param>
        /// <param name="keyModifiers">
        /// Object containing key modifiers to apply
        /// <code>If omitted: Ignored</code>
        /// </param>
        /// <param name="keyModifiers.shift">
        /// Press Shift
        /// <code>If omitted: Not pressed</code>
        /// </param>
        /// <param name="keyModifiers.control">
        /// Press CTRL
        /// <code>If omitted: Not pressed</code>
        /// </param>
        /// <param name="keyModifiers.alt">
        /// Press ALT
        /// <code>If omitted: Not pressed</code>
        /// </param>
        /// <param name="keyModifiers.command">
        /// Press CMD (Mac)
        /// <code>If omitted: Not pressed</code>
        /// </param>
        public async Task TriggerHotkeyByKeySequence(string? keyId = null, JsonObject? keyModifiers = null)
        {
            var data = new JsonObject();
            if (keyId != null)
            {
                data.Add(nameof(keyId), keyId);
            }
            if (keyModifiers != null)
            {
                data.Add(nameof(keyModifiers), keyModifiers);
            }
            await Request(data);
        }

        /// <summary>Sleeps for a time duration or number of frames. Only available in request batches with types `SERIAL_REALTIME` or `SERIAL_FRAME`.</summary>
        /// <param name="sleepMillis">
        /// Number of milliseconds to sleep for (if `SERIAL_REALTIME` mode)
        /// <code>If omitted: Unknown</code>
        /// <code>Restrictions: &gt;= 0, &lt;= 50000</code>
        /// </param>
        /// <param name="sleepFrames">
        /// Number of frames to sleep for (if `SERIAL_FRAME` mode)
        /// <code>If omitted: Unknown</code>
        /// <code>Restrictions: &gt;= 0, &lt;= 10000</code>
        /// </param>
        public async Task Sleep(double? sleepMillis = null, double? sleepFrames = null)
        {
            if (!(sleepMillis >= 0) || !(sleepMillis <= 50000))
            {
                throw new ArgumentOutOfRangeException(nameof(sleepMillis), $"{nameof(sleepMillis)} outside of \">= 0, <= 50000\"");
            }
            if (!(sleepFrames >= 0) || !(sleepFrames <= 10000))
            {
                throw new ArgumentOutOfRangeException(nameof(sleepFrames), $"{nameof(sleepFrames)} outside of \">= 0, <= 10000\"");
            }
            var data = new JsonObject();
            if (sleepMillis != null)
            {
                data.Add(nameof(sleepMillis), sleepMillis);
            }
            if (sleepFrames != null)
            {
                data.Add(nameof(sleepFrames), sleepFrames);
            }
            await Request(data);
        }

        /// <summary>Gets an array of all inputs in OBS.</summary>
        /// <param name="inputKind">
        /// Restrict the array to only inputs of the specified kind
        /// <code>If omitted: All kinds included</code>
        /// </param>
        /// <returns>Array of inputs</returns>
        public async Task<JsonElement> GetInputList(string? inputKind = null)
        {
            var data = new JsonObject();
            if (inputKind != null)
            {
                data.Add(nameof(inputKind), inputKind);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>Gets an array of all available input kinds in OBS.</summary>
        /// <param name="unversioned">
        /// True == Return all kinds as unversioned, False == Return with version suffixes (if available)
        /// <code>If omitted: false</code>
        /// </param>
        /// <returns>Array of input kinds</returns>
        public async Task<string[]> GetInputKindList(bool? unversioned = null)
        {
            var data = new JsonObject();
            if (unversioned != null)
            {
                data.Add(nameof(unversioned), unversioned);
            }
            var response = await Request(data);
            return response.ReadStringArray("inputKinds") ?? throw new NullReferenceException("inputKinds");
        }

        public class GetSpecialInputsReturn
        {
            /// <summary>Name of the Desktop Audio input</summary>
            public string desktop1 = default!;
            /// <summary>Name of the Desktop Audio 2 input</summary>
            public string desktop2 = default!;
            /// <summary>Name of the Mic/Auxiliary Audio input</summary>
            public string mic1 = default!;
            /// <summary>Name of the Mic/Auxiliary Audio 2 input</summary>
            public string mic2 = default!;
            /// <summary>Name of the Mic/Auxiliary Audio 3 input</summary>
            public string mic3 = default!;
            /// <summary>Name of the Mic/Auxiliary Audio 4 input</summary>
            public string mic4 = default!;
        }

        /// <summary>Gets the names of all special inputs.</summary>
        /// <returns><see cref="GetSpecialInputsReturn"/></returns>
        public async Task<GetSpecialInputsReturn> GetSpecialInputs()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSpecialInputsReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSpecialInputsReturn));
        }

        public class CreateInputReturn
        {
            /// <summary>UUID of the newly created input</summary>
            public string inputUuid = default!;
            /// <summary>ID of the newly created scene item</summary>
            public double sceneItemId = default!;
        }

        /// <summary>Creates a new input, adding it as a scene item to the specified scene.</summary>
        /// <param name="inputName">Name of the new input to created</param>
        /// <param name="inputKind">The kind of input to be created</param>
        /// <param name="sceneName">
        /// Name of the scene to add the input to as a scene item
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene to add the input to as a scene item
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputSettings">
        /// Settings object to initialize the input with
        /// <code>If omitted: Default settings used</code>
        /// </param>
        /// <param name="sceneItemEnabled">
        /// Whether to set the created scene item to enabled or disabled
        /// <code>If omitted: True</code>
        /// </param>
        /// <returns><see cref="CreateInputReturn"/></returns>
        public async Task<CreateInputReturn> CreateInput(string inputName, string inputKind, string? sceneName = null, string? sceneUuid = null, JsonObject? inputSettings = null, bool? sceneItemEnabled = null)
        {
            var data = new JsonObject()
            {
                {nameof(inputName), inputName},
                {nameof(inputKind), inputKind},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (inputSettings != null)
            {
                data.Add(nameof(inputSettings), inputSettings);
            }
            if (sceneItemEnabled != null)
            {
                data.Add(nameof(sceneItemEnabled), sceneItemEnabled);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<CreateInputReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(CreateInputReturn));
        }

        /// <summary>
        /// Removes an existing input.
        /// 
        /// Note: Will immediately remove all associated scene items.
        /// </summary>
        /// <param name="inputName">
        /// Name of the input to remove
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to remove
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task RemoveInput(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            await Request(data);
        }

        /// <summary>Sets the name of an input (rename).</summary>
        /// <param name="newInputName">New name for the input</param>
        /// <param name="inputUuid">
        /// Current input UUID
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Current input name
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetInputName(string newInputName, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(newInputName), newInputName},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>Gets the default settings for an input kind.</summary>
        /// <param name="inputKind">Input kind to get the default settings for</param>
        /// <returns>Object of default settings for the input kind</returns>
        public async Task<JsonElement> GetInputDefaultSettings(string inputKind)
        {
            var data = new JsonObject()
            {
                {nameof(inputKind), inputKind},
            };
            var response = await Request(data);
            return response;
        }

        public class GetInputSettingsReturn
        {
            /// <summary>Object of settings for the input</summary>
            public JsonElement inputSettings = default!;
            /// <summary>The kind of the input</summary>
            public string inputKind = default!;
        }

        /// <summary>
        /// Gets the settings of an input.
        /// 
        /// Note: Does not include defaults. To create the entire settings object, overlay `inputSettings` over the `defaultInputSettings` provided by `GetInputDefaultSettings`.
        /// </summary>
        /// <param name="inputName">
        /// Name of the input to get the settings of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to get the settings of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetInputSettingsReturn"/></returns>
        public async Task<GetInputSettingsReturn> GetInputSettings(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetInputSettingsReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetInputSettingsReturn));
        }

        /// <summary>Sets the settings of an input.</summary>
        /// <param name="inputSettings">Object of settings to apply</param>
        /// <param name="inputName">
        /// Name of the input to set the settings of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to set the settings of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="overlay">
        /// True == apply the settings on top of existing ones, False == reset the input to its defaults, then apply settings.
        /// <code>If omitted: true</code>
        /// </param>
        public async Task SetInputSettings(JsonObject inputSettings, string? inputName = null, string? inputUuid = null, bool? overlay = null)
        {
            var data = new JsonObject()
            {
                {nameof(inputSettings), inputSettings},
            };
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (overlay != null)
            {
                data.Add(nameof(overlay), overlay);
            }
            await Request(data);
        }

        /// <summary>Gets the audio mute state of an input.</summary>
        /// <param name="inputName">
        /// Name of input to get the mute state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of input to get the mute state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Whether the input is muted</returns>
        public async Task<bool> GetInputMute(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return response.ReadBool("inputMuted") ?? throw new NullReferenceException("inputMuted");
        }

        /// <summary>Sets the audio mute state of an input.</summary>
        /// <param name="inputMuted">Whether to mute the input or not</param>
        /// <param name="inputUuid">
        /// UUID of the input to set the mute state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input to set the mute state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetInputMute(bool inputMuted, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(inputMuted), inputMuted},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>Toggles the audio mute state of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to toggle the mute state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to toggle the mute state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Whether the input has been muted or unmuted</returns>
        public async Task<bool> ToggleInputMute(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return response.ReadBool("inputMuted") ?? throw new NullReferenceException("inputMuted");
        }

        public class GetInputVolumeReturn
        {
            /// <summary>Volume setting in mul</summary>
            public double inputVolumeMul = default!;
            /// <summary>Volume setting in dB</summary>
            public double inputVolumeDb = default!;
        }

        /// <summary>Gets the current volume setting of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to get the volume of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to get the volume of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetInputVolumeReturn"/></returns>
        public async Task<GetInputVolumeReturn> GetInputVolume(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetInputVolumeReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetInputVolumeReturn));
        }

        /// <summary>Sets the volume setting of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to set the volume of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to set the volume of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputVolumeMul">
        /// Volume setting in mul
        /// <code>If omitted: `inputVolumeDb` should be specified</code>
        /// <code>Restrictions: &gt;= 0, &lt;= 20</code>
        /// </param>
        /// <param name="inputVolumeDb">
        /// Volume setting in dB
        /// <code>If omitted: `inputVolumeMul` should be specified</code>
        /// <code>Restrictions: &gt;= -100, &lt;= 26</code>
        /// </param>
        public async Task SetInputVolume(string? inputName = null, string? inputUuid = null, double? inputVolumeMul = null, double? inputVolumeDb = null)
        {
            if (!(inputVolumeMul >= 0) || !(inputVolumeMul <= 20))
            {
                throw new ArgumentOutOfRangeException(nameof(inputVolumeMul), $"{nameof(inputVolumeMul)} outside of \">= 0, <= 20\"");
            }
            if (!(inputVolumeDb >= -100) || !(inputVolumeDb <= 26))
            {
                throw new ArgumentOutOfRangeException(nameof(inputVolumeDb), $"{nameof(inputVolumeDb)} outside of \">= -100, <= 26\"");
            }
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputVolumeMul != null)
            {
                data.Add(nameof(inputVolumeMul), inputVolumeMul);
            }
            if (inputVolumeDb != null)
            {
                data.Add(nameof(inputVolumeDb), inputVolumeDb);
            }
            await Request(data);
        }

        /// <summary>Gets the audio balance of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to get the audio balance of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to get the audio balance of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Audio balance value from 0.0-1.0</returns>
        public async Task<double> GetInputAudioBalance(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return response.ReadDouble("inputAudioBalance") ?? throw new NullReferenceException("inputAudioBalance");
        }

        /// <summary>Sets the audio balance of an input.</summary>
        /// <param name="inputAudioBalance">
        /// New audio balance value
        /// <code>Restrictions: &gt;= 0.0, &lt;= 1.0</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to set the audio balance of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input to set the audio balance of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetInputAudioBalance(double inputAudioBalance, string? inputUuid = null, string? inputName = null)
        {
            if (!(inputAudioBalance >= 0.0) || !(inputAudioBalance <= 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(inputAudioBalance), $"{nameof(inputAudioBalance)} outside of \">= 0.0, <= 1.0\"");
            }
            var data = new JsonObject()
            {
                {nameof(inputAudioBalance), inputAudioBalance},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the audio sync offset of an input.
        /// 
        /// Note: The audio sync offset can be negative too!
        /// </summary>
        /// <param name="inputName">
        /// Name of the input to get the audio sync offset of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to get the audio sync offset of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Audio sync offset in milliseconds</returns>
        public async Task<double> GetInputAudioSyncOffset(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return response.ReadDouble("inputAudioSyncOffset") ?? throw new NullReferenceException("inputAudioSyncOffset");
        }

        /// <summary>Sets the audio sync offset of an input.</summary>
        /// <param name="inputAudioSyncOffset">
        /// New audio sync offset in milliseconds
        /// <code>Restrictions: &gt;= -950, &lt;= 20000</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to set the audio sync offset of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input to set the audio sync offset of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetInputAudioSyncOffset(double inputAudioSyncOffset, string? inputUuid = null, string? inputName = null)
        {
            if (!(inputAudioSyncOffset >= -950) || !(inputAudioSyncOffset <= 20000))
            {
                throw new ArgumentOutOfRangeException(nameof(inputAudioSyncOffset), $"{nameof(inputAudioSyncOffset)} outside of \">= -950, <= 20000\"");
            }
            var data = new JsonObject()
            {
                {nameof(inputAudioSyncOffset), inputAudioSyncOffset},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the audio monitor type of an input.
        /// 
        /// The available audio monitor types are:
        /// 
        /// - `OBS_MONITORING_TYPE_NONE`
        /// - `OBS_MONITORING_TYPE_MONITOR_ONLY`
        /// - `OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT`
        /// </summary>
        /// <param name="inputName">
        /// Name of the input to get the audio monitor type of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to get the audio monitor type of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Audio monitor type</returns>
        public async Task<string> GetInputAudioMonitorType(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return response.ReadString("monitorType") ?? throw new NullReferenceException("monitorType");
        }

        /// <summary>Sets the audio monitor type of an input.</summary>
        /// <param name="monitorType">Audio monitor type</param>
        /// <param name="inputUuid">
        /// UUID of the input to set the audio monitor type of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input to set the audio monitor type of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetInputAudioMonitorType(string monitorType, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(monitorType), monitorType},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>Gets the enable state of all audio tracks of an input.</summary>
        /// <param name="inputName">
        /// Name of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Object of audio tracks and associated enable states</returns>
        public async Task<JsonElement> GetInputAudioTracks(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>Sets the enable state of audio tracks of an input.</summary>
        /// <param name="inputAudioTracks">Track settings to apply</param>
        /// <param name="inputUuid">
        /// UUID of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetInputAudioTracks(JsonObject inputAudioTracks, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(inputAudioTracks), inputAudioTracks},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the items of a list property from an input's properties.
        /// 
        /// Note: Use this in cases where an input provides a dynamic, selectable list of items. For example, display capture, where it provides a list of available displays.
        /// </summary>
        /// <param name="propertyName">Name of the list property to get the items of</param>
        /// <param name="inputUuid">
        /// UUID of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Array of items in the list property</returns>
        public async Task<JsonElement> GetInputPropertiesListPropertyItems(string propertyName, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(propertyName), propertyName},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>
        /// Presses a button in the properties of an input.
        /// 
        /// Some known `propertyName` values are:
        /// 
        /// - `refreshnocache` - Browser source reload button
        /// 
        /// Note: Use this in cases where there is a button in the properties of an input that cannot be accessed in any other way. For example, browser sources, where there is a refresh button.
        /// </summary>
        /// <param name="propertyName">Name of the button property to press</param>
        /// <param name="inputUuid">
        /// UUID of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the input
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task PressInputPropertiesButton(string propertyName, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(propertyName), propertyName},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        public class GetMediaInputStatusReturn
        {
            /// <summary>State of the media input</summary>
            public string mediaState = default!;
            /// <summary>Total duration of the playing media in milliseconds. `null` if not playing</summary>
            public double mediaDuration = default!;
            /// <summary>Position of the cursor in milliseconds. `null` if not playing</summary>
            public double mediaCursor = default!;
        }

        /// <summary>
        /// Gets the status of a media input.
        /// 
        /// Media States:
        /// 
        /// - `OBS_MEDIA_STATE_NONE`
        /// - `OBS_MEDIA_STATE_PLAYING`
        /// - `OBS_MEDIA_STATE_OPENING`
        /// - `OBS_MEDIA_STATE_BUFFERING`
        /// - `OBS_MEDIA_STATE_PAUSED`
        /// - `OBS_MEDIA_STATE_STOPPED`
        /// - `OBS_MEDIA_STATE_ENDED`
        /// - `OBS_MEDIA_STATE_ERROR`
        /// </summary>
        /// <param name="inputName">
        /// Name of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetMediaInputStatusReturn"/></returns>
        public async Task<GetMediaInputStatusReturn> GetMediaInputStatus(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetMediaInputStatusReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetMediaInputStatusReturn));
        }

        /// <summary>
        /// Sets the cursor position of a media input.
        /// 
        /// This request does not perform bounds checking of the cursor position.
        /// </summary>
        /// <param name="mediaCursor">
        /// New cursor position to set
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetMediaInputCursor(double mediaCursor, string? inputUuid = null, string? inputName = null)
        {
            if (!(mediaCursor >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(mediaCursor), $"{nameof(mediaCursor)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(mediaCursor), mediaCursor},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>
        /// Offsets the current cursor position of a media input by the specified value.
        /// 
        /// This request does not perform bounds checking of the cursor position.
        /// </summary>
        /// <param name="mediaCursorOffset">Value to offset the current cursor position by</param>
        /// <param name="inputUuid">
        /// UUID of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task OffsetMediaInputCursor(double mediaCursorOffset, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(mediaCursorOffset), mediaCursorOffset},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>Triggers an action on a media input.</summary>
        /// <param name="mediaAction">Identifier of the `ObsMediaInputAction` enum</param>
        /// <param name="inputUuid">
        /// UUID of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputName">
        /// Name of the media input
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task TriggerMediaInputAction(ObsMediaInputAction mediaAction, string? inputUuid = null, string? inputName = null)
        {
            var data = new JsonObject()
            {
                {nameof(mediaAction), Enum.GetName(typeof(ObsMediaInputAction), mediaAction)},
            };
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            await Request(data);
        }

        /// <summary>Gets the status of the virtualcam output.</summary>
        /// <returns>Whether the output is active</returns>
        public async Task<bool> GetVirtualCamStatus()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Toggles the state of the virtualcam output.</summary>
        /// <returns>Whether the output is active</returns>
        public async Task<bool> ToggleVirtualCam()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Starts the virtualcam output.</summary>
        public async Task StartVirtualCam()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Stops the virtualcam output.</summary>
        public async Task StopVirtualCam()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Gets the status of the replay buffer output.</summary>
        /// <returns>Whether the output is active</returns>
        public async Task<bool> GetReplayBufferStatus()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Toggles the state of the replay buffer output.</summary>
        /// <returns>Whether the output is active</returns>
        public async Task<bool> ToggleReplayBuffer()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Starts the replay buffer output.</summary>
        public async Task StartReplayBuffer()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Stops the replay buffer output.</summary>
        public async Task StopReplayBuffer()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Saves the contents of the replay buffer output.</summary>
        public async Task SaveReplayBuffer()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Gets the filename of the last replay buffer save file.</summary>
        /// <returns>File path</returns>
        public async Task<string> GetLastReplayBufferReplay()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadString("savedReplayPath") ?? throw new NullReferenceException("savedReplayPath");
        }

        /// <summary>Gets the list of available outputs.</summary>
        /// <returns>Array of outputs</returns>
        public async Task<JsonElement> GetOutputList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response;
        }

        public class GetOutputStatusReturn
        {
            /// <summary>Whether the output is active</summary>
            public bool outputActive = default!;
            /// <summary>Whether the output is reconnecting</summary>
            public bool outputReconnecting = default!;
            /// <summary>Current formatted timecode string for the output</summary>
            public string outputTimecode = default!;
            /// <summary>Current duration in milliseconds for the output</summary>
            public double outputDuration = default!;
            /// <summary>Congestion of the output</summary>
            public double outputCongestion = default!;
            /// <summary>Number of bytes sent by the output</summary>
            public double outputBytes = default!;
            /// <summary>Number of frames skipped by the output's process</summary>
            public double outputSkippedFrames = default!;
            /// <summary>Total number of frames delivered by the output's process</summary>
            public double outputTotalFrames = default!;
        }

        /// <summary>Gets the status of an output.</summary>
        /// <param name="outputName">Output name</param>
        /// <returns><see cref="GetOutputStatusReturn"/></returns>
        public async Task<GetOutputStatusReturn> GetOutputStatus(string outputName)
        {
            var data = new JsonObject()
            {
                {nameof(outputName), outputName},
            };
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetOutputStatusReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetOutputStatusReturn));
        }

        /// <summary>Toggles the status of an output.</summary>
        /// <param name="outputName">Output name</param>
        /// <returns>Whether the output is active</returns>
        public async Task<bool> ToggleOutput(string outputName)
        {
            var data = new JsonObject()
            {
                {nameof(outputName), outputName},
            };
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Starts an output.</summary>
        /// <param name="outputName">Output name</param>
        public async Task StartOutput(string outputName)
        {
            var data = new JsonObject()
            {
                {nameof(outputName), outputName},
            };
            await Request(data);
        }

        /// <summary>Stops an output.</summary>
        /// <param name="outputName">Output name</param>
        public async Task StopOutput(string outputName)
        {
            var data = new JsonObject()
            {
                {nameof(outputName), outputName},
            };
            await Request(data);
        }

        /// <summary>Gets the settings of an output.</summary>
        /// <param name="outputName">Output name</param>
        /// <returns>Output settings</returns>
        public async Task<JsonElement> GetOutputSettings(string outputName)
        {
            var data = new JsonObject()
            {
                {nameof(outputName), outputName},
            };
            var response = await Request(data);
            return response;
        }

        /// <summary>Sets the settings of an output.</summary>
        /// <param name="outputName">Output name</param>
        /// <param name="outputSettings">Output settings</param>
        public async Task SetOutputSettings(string outputName, JsonObject outputSettings)
        {
            var data = new JsonObject()
            {
                {nameof(outputName), outputName},
                {nameof(outputSettings), outputSettings},
            };
            await Request(data);
        }

        public class GetRecordStatusReturn
        {
            /// <summary>Whether the output is active</summary>
            public bool outputActive = default!;
            /// <summary>Whether the output is paused</summary>
            public bool outputPaused = default!;
            /// <summary>Current formatted timecode string for the output</summary>
            public string outputTimecode = default!;
            /// <summary>Current duration in milliseconds for the output</summary>
            public double outputDuration = default!;
            /// <summary>Number of bytes sent by the output</summary>
            public double outputBytes = default!;
        }

        /// <summary>Gets the status of the record output.</summary>
        /// <returns><see cref="GetRecordStatusReturn"/></returns>
        public async Task<GetRecordStatusReturn> GetRecordStatus()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetRecordStatusReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetRecordStatusReturn));
        }

        /// <summary>Toggles the status of the record output.</summary>
        /// <returns>The new active state of the output</returns>
        public async Task<bool> ToggleRecord()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Starts the record output.</summary>
        public async Task StartRecord()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Stops the record output.</summary>
        /// <returns>File name for the saved recording</returns>
        public async Task<string> StopRecord()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadString("outputPath") ?? throw new NullReferenceException("outputPath");
        }

        /// <summary>Toggles pause on the record output.</summary>
        public async Task ToggleRecordPause()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Pauses the record output.</summary>
        public async Task PauseRecord()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Resumes the record output.</summary>
        public async Task ResumeRecord()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>
        /// Gets a list of all scene items in a scene.
        /// 
        /// Scenes only
        /// </summary>
        /// <param name="sceneName">
        /// Name of the scene to get the items of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene to get the items of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Array of scene items in the scene</returns>
        public async Task<JsonElement> GetSceneItemList(string? sceneName = null, string? sceneUuid = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>
        /// Basically GetSceneItemList, but for groups.
        /// 
        /// Using groups at all in OBS is discouraged, as they are very broken under the hood. Please use nested scenes instead.
        /// 
        /// Groups only
        /// </summary>
        /// <param name="sceneName">
        /// Name of the group to get the items of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the group to get the items of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Array of scene items in the group</returns>
        public async Task<JsonElement> GetGroupSceneItemList(string? sceneName = null, string? sceneUuid = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>
        /// Searches a scene for a source, and returns its id.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sourceName">Name of the source to find</param>
        /// <param name="sceneName">
        /// Name of the scene or group to search in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene or group to search in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="searchOffset">
        /// Number of matches to skip during search. >= 0 means first forward. -1 means last (top) item
        /// <code>If omitted: 0</code>
        /// <code>Restrictions: &gt;= -1</code>
        /// </param>
        /// <returns>Numeric ID of the scene item</returns>
        public async Task<double> GetSceneItemId(string sourceName, string? sceneName = null, string? sceneUuid = null, double? searchOffset = null)
        {
            if (!(searchOffset >= -1))
            {
                throw new ArgumentOutOfRangeException(nameof(searchOffset), $"{nameof(searchOffset)} outside of \">= -1\"");
            }
            var data = new JsonObject()
            {
                {nameof(sourceName), sourceName},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (searchOffset != null)
            {
                data.Add(nameof(searchOffset), searchOffset);
            }
            var response = await Request(data);
            return response.ReadDouble("sceneItemId") ?? throw new NullReferenceException("sceneItemId");
        }

        public class GetSceneItemSourceReturn
        {
            /// <summary>Name of the source associated with the scene item</summary>
            public string sourceName = default!;
            /// <summary>UUID of the source associated with the scene item</summary>
            public string sourceUuid = default!;
        }

        /// <summary>Gets the source associated with a scene item.</summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetSceneItemSourceReturn"/></returns>
        public async Task<GetSceneItemSourceReturn> GetSceneItemSource(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSceneItemSourceReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSceneItemSourceReturn));
        }

        /// <summary>
        /// Creates a new scene item using a source.
        /// 
        /// Scenes only
        /// </summary>
        /// <param name="sceneName">
        /// Name of the scene to create the new item in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene to create the new item in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceName">
        /// Name of the source to add to the scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source to add to the scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneItemEnabled">
        /// Enable state to apply to the scene item on creation
        /// <code>If omitted: True</code>
        /// </param>
        /// <returns>Numeric ID of the scene item</returns>
        public async Task<double> CreateSceneItem(string? sceneName = null, string? sceneUuid = null, string? sourceName = null, string? sourceUuid = null, bool? sceneItemEnabled = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (sceneItemEnabled != null)
            {
                data.Add(nameof(sceneItemEnabled), sceneItemEnabled);
            }
            var response = await Request(data);
            return response.ReadDouble("sceneItemId") ?? throw new NullReferenceException("sceneItemId");
        }

        /// <summary>
        /// Removes a scene item from a scene.
        /// 
        /// Scenes only
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task RemoveSceneItem(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            await Request(data);
        }

        /// <summary>
        /// Duplicates a scene item, copying all transform and crop info.
        /// 
        /// Scenes only
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="destinationSceneName">
        /// Name of the scene to create the duplicated item in
        /// <code>If omitted: From scene is assumed</code>
        /// </param>
        /// <param name="destinationSceneUuid">
        /// UUID of the scene to create the duplicated item in
        /// <code>If omitted: From scene is assumed</code>
        /// </param>
        /// <returns>Numeric ID of the duplicated scene item</returns>
        public async Task<double> DuplicateSceneItem(double sceneItemId, string? sceneName = null, string? sceneUuid = null, string? destinationSceneName = null, string? destinationSceneUuid = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (destinationSceneName != null)
            {
                data.Add(nameof(destinationSceneName), destinationSceneName);
            }
            if (destinationSceneUuid != null)
            {
                data.Add(nameof(destinationSceneUuid), destinationSceneUuid);
            }
            var response = await Request(data);
            return response.ReadDouble("sceneItemId") ?? throw new NullReferenceException("sceneItemId");
        }

        /// <summary>
        /// Gets the transform and crop info of a scene item.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Object containing scene item transform info</returns>
        public async Task<JsonElement> GetSceneItemTransform(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            var response = await Request(data);
            return response;
        }

        /// <summary>Sets the transform and crop info of a scene item.</summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneItemTransform">Object containing scene item transform info to update</param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSceneItemTransform(double sceneItemId, JsonObject sceneItemTransform, string? sceneName = null, string? sceneUuid = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
                {nameof(sceneItemTransform), sceneItemTransform},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the enable state of a scene item.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Whether the scene item is enabled. `true` for enabled, `false` for disabled</returns>
        public async Task<bool> GetSceneItemEnabled(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            var response = await Request(data);
            return response.ReadBool("sceneItemEnabled") ?? throw new NullReferenceException("sceneItemEnabled");
        }

        /// <summary>
        /// Sets the enable state of a scene item.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneItemEnabled">New enable state of the scene item</param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSceneItemEnabled(double sceneItemId, bool sceneItemEnabled, string? sceneName = null, string? sceneUuid = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
                {nameof(sceneItemEnabled), sceneItemEnabled},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the lock state of a scene item.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Whether the scene item is locked. `true` for locked, `false` for unlocked</returns>
        public async Task<bool> GetSceneItemLocked(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            var response = await Request(data);
            return response.ReadBool("sceneItemLocked") ?? throw new NullReferenceException("sceneItemLocked");
        }

        /// <summary>
        /// Sets the lock state of a scene item.
        /// 
        /// Scenes and Group
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneItemLocked">New lock state of the scene item</param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSceneItemLocked(double sceneItemId, bool sceneItemLocked, string? sceneName = null, string? sceneUuid = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
                {nameof(sceneItemLocked), sceneItemLocked},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the index position of a scene item in a scene.
        /// 
        /// An index of 0 is at the bottom of the source list in the UI.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Index position of the scene item</returns>
        public async Task<double> GetSceneItemIndex(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            var response = await Request(data);
            return response.ReadDouble("sceneItemIndex") ?? throw new NullReferenceException("sceneItemIndex");
        }

        /// <summary>
        /// Sets the index position of a scene item in a scene.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneItemIndex">
        /// New index position of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSceneItemIndex(double sceneItemId, double sceneItemIndex, string? sceneName = null, string? sceneUuid = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            if (!(sceneItemIndex >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemIndex), $"{nameof(sceneItemIndex)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
                {nameof(sceneItemIndex), sceneItemIndex},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the blend mode of a scene item.
        /// 
        /// Blend modes:
        /// 
        /// - `OBS_BLEND_NORMAL`
        /// - `OBS_BLEND_ADDITIVE`
        /// - `OBS_BLEND_SUBTRACT`
        /// - `OBS_BLEND_SCREEN`
        /// - `OBS_BLEND_MULTIPLY`
        /// - `OBS_BLEND_LIGHTEN`
        /// - `OBS_BLEND_DARKEN`
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns>Current blend mode</returns>
        public async Task<string> GetSceneItemBlendMode(double sceneItemId, string? sceneUuid = null, string? sceneName = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            var response = await Request(data);
            return response.ReadString("sceneItemBlendMode") ?? throw new NullReferenceException("sceneItemBlendMode");
        }

        /// <summary>
        /// Sets the blend mode of a scene item.
        /// 
        /// Scenes and Groups
        /// </summary>
        /// <param name="sceneItemId">
        /// Numeric ID of the scene item
        /// <code>Restrictions: &gt;= 0</code>
        /// </param>
        /// <param name="sceneItemBlendMode">New blend mode</param>
        /// <param name="sceneName">
        /// Name of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene the item is in
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSceneItemBlendMode(double sceneItemId, string sceneItemBlendMode, string? sceneName = null, string? sceneUuid = null)
        {
            if (!(sceneItemId >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneItemId), $"{nameof(sceneItemId)} outside of \">= 0\"");
            }
            var data = new JsonObject()
            {
                {nameof(sceneItemId), sceneItemId},
                {nameof(sceneItemBlendMode), sceneItemBlendMode},
            };
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        public class GetSceneListReturn
        {
            /// <summary>Current program scene name. Can be `null` if internal state desync</summary>
            public string currentProgramSceneName = default!;
            /// <summary>Current program scene UUID. Can be `null` if internal state desync</summary>
            public string currentProgramSceneUuid = default!;
            /// <summary>Current preview scene name. `null` if not in studio mode</summary>
            public string currentPreviewSceneName = default!;
            /// <summary>Current preview scene UUID. `null` if not in studio mode</summary>
            public string currentPreviewSceneUuid = default!;
            /// <summary>Array of scenes</summary>
            public JsonElement scenes = default!;
        }

        /// <summary>Gets an array of all scenes in OBS.</summary>
        /// <returns><see cref="GetSceneListReturn"/></returns>
        public async Task<GetSceneListReturn> GetSceneList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSceneListReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSceneListReturn));
        }

        /// <summary>
        /// Gets an array of all groups in OBS.
        /// 
        /// Groups in OBS are actually scenes, but renamed and modified. In obs-websocket, we treat them as scenes where we can.
        /// </summary>
        /// <returns>Array of group names</returns>
        public async Task<string[]> GetGroupList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadStringArray("groups") ?? throw new NullReferenceException("groups");
        }

        public class GetCurrentProgramSceneReturn
        {
            /// <summary>Current program scene name</summary>
            public string sceneName = default!;
            /// <summary>Current program scene UUID</summary>
            public string sceneUuid = default!;
            /// <summary>Current program scene name (Deprecated)</summary>
            public string currentProgramSceneName = default!;
            /// <summary>Current program scene UUID (Deprecated)</summary>
            public string currentProgramSceneUuid = default!;
        }

        /// <summary>
        /// Gets the current program scene.
        /// 
        /// Note: This request is slated to have the `currentProgram`-prefixed fields removed from in an upcoming RPC version.
        /// </summary>
        /// <returns><see cref="GetCurrentProgramSceneReturn"/></returns>
        public async Task<GetCurrentProgramSceneReturn> GetCurrentProgramScene()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetCurrentProgramSceneReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetCurrentProgramSceneReturn));
        }

        /// <summary>Sets the current program scene.</summary>
        /// <param name="sceneName">
        /// Scene name to set as the current program scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// Scene UUID to set as the current program scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetCurrentProgramScene(string? sceneName = null, string? sceneUuid = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        public class GetCurrentPreviewSceneReturn
        {
            /// <summary>Current preview scene name</summary>
            public string sceneName = default!;
            /// <summary>Current preview scene UUID</summary>
            public string sceneUuid = default!;
            /// <summary>Current preview scene name</summary>
            public string currentPreviewSceneName = default!;
            /// <summary>Current preview scene UUID</summary>
            public string currentPreviewSceneUuid = default!;
        }

        /// <summary>
        /// Gets the current preview scene.
        /// 
        /// Only available when studio mode is enabled.
        /// 
        /// Note: This request is slated to have the `currentPreview`-prefixed fields removed from in an upcoming RPC version.
        /// </summary>
        /// <returns><see cref="GetCurrentPreviewSceneReturn"/></returns>
        public async Task<GetCurrentPreviewSceneReturn> GetCurrentPreviewScene()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetCurrentPreviewSceneReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetCurrentPreviewSceneReturn));
        }

        /// <summary>
        /// Sets the current preview scene.
        /// 
        /// Only available when studio mode is enabled.
        /// </summary>
        /// <param name="sceneName">
        /// Scene name to set as the current preview scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// Scene UUID to set as the current preview scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetCurrentPreviewScene(string? sceneName = null, string? sceneUuid = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        /// <summary>Creates a new scene in OBS.</summary>
        /// <param name="sceneName">Name for the new scene</param>
        /// <returns>UUID of the created scene</returns>
        public async Task<string> CreateScene(string sceneName)
        {
            var data = new JsonObject()
            {
                {nameof(sceneName), sceneName},
            };
            var response = await Request(data);
            return response.ReadString("sceneUuid") ?? throw new NullReferenceException("sceneUuid");
        }

        /// <summary>Removes a scene from OBS.</summary>
        /// <param name="sceneName">
        /// Name of the scene to remove
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene to remove
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task RemoveScene(string? sceneName = null, string? sceneUuid = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            await Request(data);
        }

        /// <summary>Sets the name of a scene (rename).</summary>
        /// <param name="newSceneName">New name for the scene</param>
        /// <param name="sceneUuid">
        /// UUID of the scene to be renamed
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneName">
        /// Name of the scene to be renamed
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task SetSceneName(string newSceneName, string? sceneUuid = null, string? sceneName = null)
        {
            var data = new JsonObject()
            {
                {nameof(newSceneName), newSceneName},
            };
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            await Request(data);
        }

        public class GetSceneSceneTransitionOverrideReturn
        {
            /// <summary>Name of the overridden scene transition, else `null`</summary>
            public string transitionName = default!;
            /// <summary>Duration of the overridden scene transition, else `null`</summary>
            public double transitionDuration = default!;
        }

        /// <summary>
        /// Gets the scene transition overridden for a scene.
        /// 
        /// Note: A transition UUID response field is not currently able to be implemented as of 2024-1-18.
        /// </summary>
        /// <param name="sceneName">
        /// Name of the scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetSceneSceneTransitionOverrideReturn"/></returns>
        public async Task<GetSceneSceneTransitionOverrideReturn> GetSceneSceneTransitionOverride(string? sceneName = null, string? sceneUuid = null)
        {
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSceneSceneTransitionOverrideReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSceneSceneTransitionOverrideReturn));
        }

        /// <summary>Sets the scene transition overridden for a scene.</summary>
        /// <param name="sceneName">
        /// Name of the scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sceneUuid">
        /// UUID of the scene
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="transitionName">
        /// Name of the scene transition to use as override. Specify `null` to remove
        /// <code>If omitted: Unchanged</code>
        /// </param>
        /// <param name="transitionDuration">
        /// Duration to use for any overridden transition. Specify `null` to remove
        /// <code>If omitted: Unchanged</code>
        /// <code>Restrictions: &gt;= 50, &lt;= 20000</code>
        /// </param>
        public async Task SetSceneSceneTransitionOverride(string? sceneName = null, string? sceneUuid = null, string? transitionName = null, double? transitionDuration = null)
        {
            if (!(transitionDuration >= 50) || !(transitionDuration <= 20000))
            {
                throw new ArgumentOutOfRangeException(nameof(transitionDuration), $"{nameof(transitionDuration)} outside of \">= 50, <= 20000\"");
            }
            var data = new JsonObject();
            if (sceneName != null)
            {
                data.Add(nameof(sceneName), sceneName);
            }
            if (sceneUuid != null)
            {
                data.Add(nameof(sceneUuid), sceneUuid);
            }
            if (transitionName != null)
            {
                data.Add(nameof(transitionName), transitionName);
            }
            if (transitionDuration != null)
            {
                data.Add(nameof(transitionDuration), transitionDuration);
            }
            await Request(data);
        }

        public class GetSourceActiveReturn
        {
            /// <summary>Whether the source is showing in Program</summary>
            public bool videoActive = default!;
            /// <summary>Whether the source is showing in the UI (Preview, Projector, Properties)</summary>
            public bool videoShowing = default!;
        }

        /// <summary>
        /// Gets the active and show state of a source.
        /// 
        /// **Compatible with inputs and scenes.**
        /// </summary>
        /// <param name="sourceName">
        /// Name of the source to get the active state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source to get the active state of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <returns><see cref="GetSourceActiveReturn"/></returns>
        public async Task<GetSourceActiveReturn> GetSourceActive(string? sourceName = null, string? sourceUuid = null)
        {
            var data = new JsonObject();
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSourceActiveReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSourceActiveReturn));
        }

        /// <summary>
        /// Gets a Base64-encoded screenshot of a source.
        /// 
        /// The `imageWidth` and `imageHeight` parameters are treated as "scale to inner", meaning the smallest ratio will be used and the aspect ratio of the original resolution is kept.
        /// If `imageWidth` and `imageHeight` are not specified, the compressed image will use the full resolution of the source.
        /// 
        /// **Compatible with inputs and scenes.**
        /// </summary>
        /// <param name="imageFormat">Image compression format to use. Use `GetVersion` to get compatible image formats</param>
        /// <param name="sourceName">
        /// Name of the source to take a screenshot of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source to take a screenshot of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="imageWidth">
        /// Width to scale the screenshot to
        /// <code>If omitted: Source value is used</code>
        /// <code>Restrictions: &gt;= 8, &lt;= 4096</code>
        /// </param>
        /// <param name="imageHeight">
        /// Height to scale the screenshot to
        /// <code>If omitted: Source value is used</code>
        /// <code>Restrictions: &gt;= 8, &lt;= 4096</code>
        /// </param>
        /// <param name="imageCompressionQuality">
        /// Compression quality to use. 0 for high compression, 100 for uncompressed. -1 to use "default" (whatever that means, idk)
        /// <code>If omitted: -1</code>
        /// <code>Restrictions: &gt;= -1, &lt;= 100</code>
        /// </param>
        /// <returns>Base64-encoded screenshot</returns>
        public async Task<string> GetSourceScreenshot(string imageFormat, string? sourceName = null, string? sourceUuid = null, double? imageWidth = null, double? imageHeight = null, double? imageCompressionQuality = null)
        {
            if (!(imageWidth >= 8) || !(imageWidth <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(imageWidth), $"{nameof(imageWidth)} outside of \">= 8, <= 4096\"");
            }
            if (!(imageHeight >= 8) || !(imageHeight <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(imageHeight), $"{nameof(imageHeight)} outside of \">= 8, <= 4096\"");
            }
            if (!(imageCompressionQuality >= -1) || !(imageCompressionQuality <= 100))
            {
                throw new ArgumentOutOfRangeException(nameof(imageCompressionQuality), $"{nameof(imageCompressionQuality)} outside of \">= -1, <= 100\"");
            }
            var data = new JsonObject()
            {
                {nameof(imageFormat), imageFormat},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (imageWidth != null)
            {
                data.Add(nameof(imageWidth), imageWidth);
            }
            if (imageHeight != null)
            {
                data.Add(nameof(imageHeight), imageHeight);
            }
            if (imageCompressionQuality != null)
            {
                data.Add(nameof(imageCompressionQuality), imageCompressionQuality);
            }
            var response = await Request(data);
            return response.ReadString("imageData") ?? throw new NullReferenceException("imageData");
        }

        /// <summary>
        /// Saves a screenshot of a source to the filesystem.
        /// 
        /// The `imageWidth` and `imageHeight` parameters are treated as "scale to inner", meaning the smallest ratio will be used and the aspect ratio of the original resolution is kept.
        /// If `imageWidth` and `imageHeight` are not specified, the compressed image will use the full resolution of the source.
        /// 
        /// **Compatible with inputs and scenes.**
        /// </summary>
        /// <param name="imageFormat">Image compression format to use. Use `GetVersion` to get compatible image formats</param>
        /// <param name="imageFilePath">Path to save the screenshot file to. Eg. `C:\Users\user\Desktop\screenshot.png`</param>
        /// <param name="sourceName">
        /// Name of the source to take a screenshot of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source to take a screenshot of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="imageWidth">
        /// Width to scale the screenshot to
        /// <code>If omitted: Source value is used</code>
        /// <code>Restrictions: &gt;= 8, &lt;= 4096</code>
        /// </param>
        /// <param name="imageHeight">
        /// Height to scale the screenshot to
        /// <code>If omitted: Source value is used</code>
        /// <code>Restrictions: &gt;= 8, &lt;= 4096</code>
        /// </param>
        /// <param name="imageCompressionQuality">
        /// Compression quality to use. 0 for high compression, 100 for uncompressed. -1 to use "default" (whatever that means, idk)
        /// <code>If omitted: -1</code>
        /// <code>Restrictions: &gt;= -1, &lt;= 100</code>
        /// </param>
        public async Task SaveSourceScreenshot(string imageFormat, string imageFilePath, string? sourceName = null, string? sourceUuid = null, double? imageWidth = null, double? imageHeight = null, double? imageCompressionQuality = null)
        {
            if (!(imageWidth >= 8) || !(imageWidth <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(imageWidth), $"{nameof(imageWidth)} outside of \">= 8, <= 4096\"");
            }
            if (!(imageHeight >= 8) || !(imageHeight <= 4096))
            {
                throw new ArgumentOutOfRangeException(nameof(imageHeight), $"{nameof(imageHeight)} outside of \">= 8, <= 4096\"");
            }
            if (!(imageCompressionQuality >= -1) || !(imageCompressionQuality <= 100))
            {
                throw new ArgumentOutOfRangeException(nameof(imageCompressionQuality), $"{nameof(imageCompressionQuality)} outside of \">= -1, <= 100\"");
            }
            var data = new JsonObject()
            {
                {nameof(imageFormat), imageFormat},
                {nameof(imageFilePath), imageFilePath},
            };
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (imageWidth != null)
            {
                data.Add(nameof(imageWidth), imageWidth);
            }
            if (imageHeight != null)
            {
                data.Add(nameof(imageHeight), imageHeight);
            }
            if (imageCompressionQuality != null)
            {
                data.Add(nameof(imageCompressionQuality), imageCompressionQuality);
            }
            await Request(data);
        }

        public class GetStreamStatusReturn
        {
            /// <summary>Whether the output is active</summary>
            public bool outputActive = default!;
            /// <summary>Whether the output is currently reconnecting</summary>
            public bool outputReconnecting = default!;
            /// <summary>Current formatted timecode string for the output</summary>
            public string outputTimecode = default!;
            /// <summary>Current duration in milliseconds for the output</summary>
            public double outputDuration = default!;
            /// <summary>Congestion of the output</summary>
            public double outputCongestion = default!;
            /// <summary>Number of bytes sent by the output</summary>
            public double outputBytes = default!;
            /// <summary>Number of frames skipped by the output's process</summary>
            public double outputSkippedFrames = default!;
            /// <summary>Total number of frames delivered by the output's process</summary>
            public double outputTotalFrames = default!;
        }

        /// <summary>Gets the status of the stream output.</summary>
        /// <returns><see cref="GetStreamStatusReturn"/></returns>
        public async Task<GetStreamStatusReturn> GetStreamStatus()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetStreamStatusReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetStreamStatusReturn));
        }

        /// <summary>Toggles the status of the stream output.</summary>
        /// <returns>New state of the stream output</returns>
        public async Task<bool> ToggleStream()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("outputActive") ?? throw new NullReferenceException("outputActive");
        }

        /// <summary>Starts the stream output.</summary>
        public async Task StartStream()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Stops the stream output.</summary>
        public async Task StopStream()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>Sends CEA-608 caption text over the stream output.</summary>
        /// <param name="captionText">Caption text</param>
        public async Task SendStreamCaption(string captionText)
        {
            var data = new JsonObject()
            {
                {nameof(captionText), captionText},
            };
            await Request(data);
        }

        /// <summary>
        /// Gets an array of all available transition kinds.
        /// 
        /// Similar to `GetInputKindList`
        /// </summary>
        /// <returns>Array of transition kinds</returns>
        public async Task<string[]> GetTransitionKindList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadStringArray("transitionKinds") ?? throw new NullReferenceException("transitionKinds");
        }

        public class GetSceneTransitionListReturn
        {
            /// <summary>Name of the current scene transition. Can be null</summary>
            public string currentSceneTransitionName = default!;
            /// <summary>UUID of the current scene transition. Can be null</summary>
            public string currentSceneTransitionUuid = default!;
            /// <summary>Kind of the current scene transition. Can be null</summary>
            public string currentSceneTransitionKind = default!;
            /// <summary>Array of transitions</summary>
            public JsonElement transitions = default!;
        }

        /// <summary>Gets an array of all scene transitions in OBS.</summary>
        /// <returns><see cref="GetSceneTransitionListReturn"/></returns>
        public async Task<GetSceneTransitionListReturn> GetSceneTransitionList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetSceneTransitionListReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetSceneTransitionListReturn));
        }

        public class GetCurrentSceneTransitionReturn
        {
            /// <summary>Name of the transition</summary>
            public string transitionName = default!;
            /// <summary>UUID of the transition</summary>
            public string transitionUuid = default!;
            /// <summary>Kind of the transition</summary>
            public string transitionKind = default!;
            /// <summary>Whether the transition uses a fixed (unconfigurable) duration</summary>
            public bool transitionFixed = default!;
            /// <summary>Configured transition duration in milliseconds. `null` if transition is fixed</summary>
            public double transitionDuration = default!;
            /// <summary>Whether the transition supports being configured</summary>
            public bool transitionConfigurable = default!;
            /// <summary>Object of settings for the transition. `null` if transition is not configurable</summary>
            public JsonElement transitionSettings = default!;
        }

        /// <summary>Gets information about the current scene transition.</summary>
        /// <returns><see cref="GetCurrentSceneTransitionReturn"/></returns>
        public async Task<GetCurrentSceneTransitionReturn> GetCurrentSceneTransition()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return JsonSerializer.Deserialize<GetCurrentSceneTransitionReturn>(response, serializerOptions) ?? throw new NullReferenceException(nameof(GetCurrentSceneTransitionReturn));
        }

        /// <summary>
        /// Sets the current scene transition.
        /// 
        /// Small note: While the namespace of scene transitions is generally unique, that uniqueness is not a guarantee as it is with other resources like inputs.
        /// </summary>
        /// <param name="transitionName">Name of the transition to make active</param>
        public async Task SetCurrentSceneTransition(string transitionName)
        {
            var data = new JsonObject()
            {
                {nameof(transitionName), transitionName},
            };
            await Request(data);
        }

        /// <summary>Sets the duration of the current scene transition, if it is not fixed.</summary>
        /// <param name="transitionDuration">
        /// Duration in milliseconds
        /// <code>Restrictions: &gt;= 50, &lt;= 20000</code>
        /// </param>
        public async Task SetCurrentSceneTransitionDuration(double transitionDuration)
        {
            if (!(transitionDuration >= 50) || !(transitionDuration <= 20000))
            {
                throw new ArgumentOutOfRangeException(nameof(transitionDuration), $"{nameof(transitionDuration)} outside of \">= 50, <= 20000\"");
            }
            var data = new JsonObject()
            {
                {nameof(transitionDuration), transitionDuration},
            };
            await Request(data);
        }

        /// <summary>Sets the settings of the current scene transition.</summary>
        /// <param name="transitionSettings">Settings object to apply to the transition. Can be `{}`</param>
        /// <param name="overlay">
        /// Whether to overlay over the current settings or replace them
        /// <code>If omitted: true</code>
        /// </param>
        public async Task SetCurrentSceneTransitionSettings(JsonObject transitionSettings, bool? overlay = null)
        {
            var data = new JsonObject()
            {
                {nameof(transitionSettings), transitionSettings},
            };
            if (overlay != null)
            {
                data.Add(nameof(overlay), overlay);
            }
            await Request(data);
        }

        /// <summary>
        /// Gets the cursor position of the current scene transition.
        /// 
        /// Note: `transitionCursor` will return 1.0 when the transition is inactive.
        /// </summary>
        /// <returns>Cursor position, between 0.0 and 1.0</returns>
        public async Task<double> GetCurrentSceneTransitionCursor()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadDouble("transitionCursor") ?? throw new NullReferenceException("transitionCursor");
        }

        /// <summary>Triggers the current scene transition. Same functionality as the `Transition` button in studio mode.</summary>
        public async Task TriggerStudioModeTransition()
        {
            var data = new JsonObject();
            await Request(data);
        }

        /// <summary>
        /// Sets the position of the TBar.
        /// 
        /// **Very important note**: This will be deprecated and replaced in a future version of obs-websocket.
        /// </summary>
        /// <param name="position">
        /// New position
        /// <code>Restrictions: &gt;= 0.0, &lt;= 1.0</code>
        /// </param>
        /// <param name="release">
        /// Whether to release the TBar. Only set `false` if you know that you will be sending another position update
        /// <code>If omitted: `true`</code>
        /// </param>
        public async Task SetTBarPosition(double position, bool? release = null)
        {
            if (!(position >= 0.0) || !(position <= 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"{nameof(position)} outside of \">= 0.0, <= 1.0\"");
            }
            var data = new JsonObject()
            {
                {nameof(position), position},
            };
            if (release != null)
            {
                data.Add(nameof(release), release);
            }
            await Request(data);
        }

        /// <summary>Gets whether studio is enabled.</summary>
        /// <returns>Whether studio mode is enabled</returns>
        public async Task<bool> GetStudioModeEnabled()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response.ReadBool("studioModeEnabled") ?? throw new NullReferenceException("studioModeEnabled");
        }

        /// <summary>Enables or disables studio mode</summary>
        /// <param name="studioModeEnabled">True == Enabled, False == Disabled</param>
        public async Task SetStudioModeEnabled(bool studioModeEnabled)
        {
            var data = new JsonObject()
            {
                {nameof(studioModeEnabled), studioModeEnabled},
            };
            await Request(data);
        }

        /// <summary>Opens the properties dialog of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to open the dialog of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to open the dialog of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task OpenInputPropertiesDialog(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            await Request(data);
        }

        /// <summary>Opens the filters dialog of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to open the dialog of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to open the dialog of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task OpenInputFiltersDialog(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            await Request(data);
        }

        /// <summary>Opens the interact dialog of an input.</summary>
        /// <param name="inputName">
        /// Name of the input to open the dialog of
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="inputUuid">
        /// UUID of the input to open the dialog of
        /// <code>If omitted: Unknown</code>
        /// </param>
        public async Task OpenInputInteractDialog(string? inputName = null, string? inputUuid = null)
        {
            var data = new JsonObject();
            if (inputName != null)
            {
                data.Add(nameof(inputName), inputName);
            }
            if (inputUuid != null)
            {
                data.Add(nameof(inputUuid), inputUuid);
            }
            await Request(data);
        }

        /// <summary>Gets a list of connected monitors and information about them.</summary>
        /// <returns>a list of detected monitors with some information</returns>
        public async Task<JsonElement> GetMonitorList()
        {
            var data = new JsonObject();
            var response = await Request(data);
            return response;
        }

        /// <summary>
        /// Opens a projector for a specific output video mix.
        /// 
        /// Mix types:
        /// 
        /// - `OBS_WEBSOCKET_VIDEO_MIX_TYPE_PREVIEW`
        /// - `OBS_WEBSOCKET_VIDEO_MIX_TYPE_PROGRAM`
        /// - `OBS_WEBSOCKET_VIDEO_MIX_TYPE_MULTIVIEW`
        /// 
        /// Note: This request serves to provide feature parity with 4.x. It is very likely to be changed/deprecated in a future release.
        /// </summary>
        /// <param name="videoMixType">Type of mix to open</param>
        /// <param name="monitorIndex">
        /// Monitor index, use `GetMonitorList` to obtain index
        /// <code>If omitted: -1: Opens projector in windowed mode</code>
        /// </param>
        /// <param name="projectorGeometry">
        /// Size/Position data for a windowed projector, in Qt Base64 encoded format. Mutually exclusive with `monitorIndex`
        /// <code>If omitted: N/A</code>
        /// </param>
        public async Task OpenVideoMixProjector(string videoMixType, double? monitorIndex = null, string? projectorGeometry = null)
        {
            var data = new JsonObject()
            {
                {nameof(videoMixType), videoMixType},
            };
            if (monitorIndex != null)
            {
                data.Add(nameof(monitorIndex), monitorIndex);
            }
            if (projectorGeometry != null)
            {
                data.Add(nameof(projectorGeometry), projectorGeometry);
            }
            await Request(data);
        }

        /// <summary>
        /// Opens a projector for a source.
        /// 
        /// Note: This request serves to provide feature parity with 4.x. It is very likely to be changed/deprecated in a future release.
        /// </summary>
        /// <param name="sourceName">
        /// Name of the source to open a projector for
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="sourceUuid">
        /// UUID of the source to open a projector for
        /// <code>If omitted: Unknown</code>
        /// </param>
        /// <param name="monitorIndex">
        /// Monitor index, use `GetMonitorList` to obtain index
        /// <code>If omitted: -1: Opens projector in windowed mode</code>
        /// </param>
        /// <param name="projectorGeometry">
        /// Size/Position data for a windowed projector, in Qt Base64 encoded format. Mutually exclusive with `monitorIndex`
        /// <code>If omitted: N/A</code>
        /// </param>
        public async Task OpenSourceProjector(string? sourceName = null, string? sourceUuid = null, double? monitorIndex = null, string? projectorGeometry = null)
        {
            var data = new JsonObject();
            if (sourceName != null)
            {
                data.Add(nameof(sourceName), sourceName);
            }
            if (sourceUuid != null)
            {
                data.Add(nameof(sourceUuid), sourceUuid);
            }
            if (monitorIndex != null)
            {
                data.Add(nameof(monitorIndex), monitorIndex);
            }
            if (projectorGeometry != null)
            {
                data.Add(nameof(projectorGeometry), projectorGeometry);
            }
            await Request(data);
        }

    }
}
