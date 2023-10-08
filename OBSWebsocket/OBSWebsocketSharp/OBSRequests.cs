using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OBSWebsocketSharp
{
    public partial class OBSRequests(OBSWebsocket obs)
    {
#if DEBUG
        static readonly int maxDelayMs = 100 * 1000;
#else
        static readonly int maxDelayMs = 5 * 1000;
#endif

        readonly OBSWebsocket obs = obs;
        readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> requestsQueue = [];
        static readonly JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields = true,
        };

        internal void RequestResponse(JsonElement responseData)
        {
            OBSRequestResponse response = JsonSerializer.Deserialize<OBSRequestResponse>(responseData, serializerOptions) ?? throw new NullReferenceException(nameof(responseData));

            if (response.requestStatus.result)
            {
                if (requestsQueue.TryRemove(response.requestId, out var tcs))
                {
                    tcs.SetResult(response.responseData);
                }
                else
                {
                    Console.WriteLine("Response not found: " + response.requestId);
                }
            }
            else
            {
                Console.WriteLine("Response failed: " + response.requestStatus.code);
            }
        }

        async Task<JsonElement> Request(JsonObject data, [CallerMemberName] string requestType = "")
        {
            if (!obs.IsConnected)
                throw new UnauthorizedAccessException();

            var guid = Guid.NewGuid().ToString();

            var reqData = new JsonObject
            {
                {"requestId", guid},
                {"requestType", requestType},
            };

            if (data.Count > 0)
            {
                reqData.Add("requestData", data);
            }

            var tcs = new TaskCompletionSource<JsonElement>();
            do
            {
                if (requestsQueue.TryAdd(guid, tcs))
                {
                    break;
                }
            } while (true);

            await obs.SendMessageAsync(OBSWebsocket.BuildMessage(WebSocketOpCode.Request, reqData));

            if (!tcs.Task.Wait(maxDelayMs))
            {
                tcs.SetCanceled();
            }

            if (tcs.Task.IsCanceled)
            {
                requestsQueue.TryRemove(guid, out var _);
                throw new TimeoutException("Request timeout.");
            }

            var res = tcs.Task.Result;

            return res;
        }
    }

    class OBSRequestResponseStatus
    {
        public int code { get; set; } = default!;
        public bool result { get; set; } = default!;
    }


    class OBSRequestResponse
    {
        public string requestId { get; set; } = default!;
        public string requestType { get; set; } = default!;
        public OBSRequestResponseStatus requestStatus { get; set; } = default!;
        public JsonElement responseData { get; set; }
    }
}
