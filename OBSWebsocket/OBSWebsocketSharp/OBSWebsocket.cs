﻿using Ninja.WebSocketClient;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OBSWebsocketSharp
{
    public class OBSWebsocket
    {
        NinjaWebSocket? ws = null;
        string? authString = null;
        int rpcVersion = 1;
        Version obsWebsocketVersion = new Version(0, 0, 0);
        Action<Action>? eventInvoke;
        AuthData authData = new(null, null);

        public event EventHandler? Connected;
        public event EventHandler? Authorized;
        public event EventHandler<OBSDisconnectInfo>? Disconnected;
        public event EventHandler<OBSReconnectInfo>? Reconnecting;

        public bool IsConnected { get; private set; }
        public bool IsAuthorized { get; private set; }

        public Version ObsWebsocketVersion { get => obsWebsocketVersion; }

        public OBSRequests Requests { get; private set; }
        public OBSEvents Events { get; private set; }

        public OBSWebsocket(Action<Action>? eventInvoke = null)
        {
            this.eventInvoke = eventInvoke;
            Requests = new(this);
            Events = new();
        }

        public async Task<bool> Connect(string url, string? password, EventSubscription? subscriptions = null)
        {
            if (ws != null)
            {
                throw new Exception("Already connected");
            }

            authData = new AuthData(password, subscriptions);

            ws = new NinjaWebSocket(url)
                .SetKeepAlive(intervalMilliseconds: 5000)
                .SetAutomaticReconnect(intervalMilliseconds: 5000);

            ws.OnConnected += OnConnected;
            ws.OnReceived += OnReceived;
            ws.OnKeepAlive += OnPing;
            ws.OnReconnecting += OnReconnecting;
            ws.OnClosed += OnClosed;

            // Connect to the ws and start listening
            await ws.StartAsync();
            return true;
        }

        public async Task OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
            IsConnected = true;

            await Task.CompletedTask;
        }

        public async Task OnReceived(ReadOnlySequence<byte>? data)
        {
            if (data != null)
            {
                string json_txt = Encoding.UTF8.GetString(data.Value.ToArray());
                Console.WriteLine(json_txt);

                if (ReadMessage(JsonDocument.Parse(json_txt), out var op, out var d))
                {
                    switch (op)
                    {
                        case WebSocketOpCode.Hello:
                            {
                                if (d == null)
                                {
                                    return;
                                }

                                var hello = JsonSerializer.Deserialize<ObsHello>(d.Value);
                                if (hello == null)
                                {
                                    if (ws != null)
                                        await ws.StopAsync();
                                    return;
                                }

                                obsWebsocketVersion = Version.Parse(hello.obsWebSocketVersion);
                                rpcVersion = hello.rpcVersion;

                                var identify = new JsonObject()
                                        {
                                            { "rpcVersion", hello.rpcVersion},
                                            { "eventSubscriptions", (long)(authData.subscriptions ?? EventSubscription.All) },
                                        };

                                if (hello.authentication != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(authData.password))
                                    {
                                        authString = Hash256Base64(Hash256Base64(authData.password + hello.authentication.salt) + hello.authentication.challenge);
                                        identify.Add("authentication", authString);
                                    }
                                    else
                                    {
                                        throw new ArgumentNullException(nameof(authData.password));
                                    }
                                }
                                else
                                {
                                    authString = "";
                                }

                                await SendMessageAsync(BuildMessage(WebSocketOpCode.Identify, identify));
                                break;
                            }
                        case WebSocketOpCode.Identified:
                            {
                                Console.WriteLine("Identified!");

                                IsAuthorized = true;
                                Authorized?.Invoke(this, EventArgs.Empty);

                                break;
                            }
                        case WebSocketOpCode.Event:
                            {
                                if (d == null)
                                {
                                    return;
                                }

                                if (eventInvoke != null)
                                {
                                    eventInvoke.Invoke(() => Events.ProcessEventData(d.Value));
                                }
                                else
                                {
                                    _ = Task.Run(() => Events.ProcessEventData(d.Value));
                                }
                                break;
                            }
                        case WebSocketOpCode.RequestResponse:
                            {
                                if (d == null)
                                {
                                    return;
                                }

                                Requests.RequestResponse(d.Value);
                                break;
                            }
                        case WebSocketOpCode.RequestBatchResponse:
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Empty data received!");
            }

            return;
        }

        public async Task OnPing()
        {
            Console.WriteLine("Ping.");

            await Task.CompletedTask;
        }

        public async Task OnReconnecting(Exception? ex)
        {
            // Notify users the connection was lost and the client is reconnecting.
            Console.WriteLine($"Reconnecting: {ex?.Message}");
            Reconnecting?.Invoke(this, new OBSReconnectInfo(ex));

            await Task.CompletedTask;
        }

        public async Task OnClosed(Exception? ex)
        {
            IsConnected = false;
            IsAuthorized = false;

            authString = null;
            rpcVersion = -1;
            obsWebsocketVersion = new Version(0, 0, 0);

            if (ws != null)
            {
                await ws.StopAsync();
                ws = null;
            }

            // Notify users the connection has been closed.
            Console.WriteLine($"Closed: {ex?.Message}");
            Disconnected?.Invoke(this, new OBSDisconnectInfo(ex));
        }

        public async Task Disconnect()
        {
            if (ws != null && ws.ConnectionState != ConnectionState.Disconnected)
            {
                await ws.StopAsync();
                ws = null;
            }
        }

        static string Hash256Base64(string str)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(str)));
        }

        static bool ReadMessage(JsonDocument doc, out WebSocketOpCode op, out JsonElement? data)
        {
            if (doc.RootElement.TryGetProperty("op", out var op_e))
            {
                if (op_e.TryGetInt32(out int code))
                {
                    op = (WebSocketOpCode)code;

                    if (doc.RootElement.TryGetProperty("d", out JsonElement d))
                    {
                        data = d;
                        return true;
                    }
                }
            }

            op = (WebSocketOpCode)(-1);
            data = null;
            return false;
        }

        internal async Task SendMessageAsync(JsonObject data)
        {
            if (ws == null)
                return;

            Console.WriteLine(JsonSerializer.Serialize(data));
            await ws.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)));
        }

        internal static JsonObject BuildMessage(WebSocketOpCode code, JsonObject data)
        {
            return new JsonObject()
            {
                { "op", (int)code },
                { "d", data },
            };
        }

        public async Task ChangeSubscriptions(EventSubscription subscriptions)
        {
            if (authString == null)
                return;

            var data = new JsonObject()
            {
                {"rpcVersion", 1},
                {"authentication", authString},
                {"eventSubscriptions", (long)subscriptions},
            };

            await SendMessageAsync(BuildMessage(WebSocketOpCode.Reidentify, data));
        }

        class AuthData(string? password, EventSubscription? subscriptions)
        {
            public string? password = password;
            public EventSubscription? subscriptions = subscriptions;
        }
    }

    public class OBSDisconnectInfo(Exception? exception)
    {
        public Exception? Exception { get; private set; } = exception;
    }

    public class OBSReconnectInfo(Exception? exception)
    {
        public Exception? Exception { get; private set; } = exception;
    }

    class ObsHello
    {
        // May not be present
        public AuthHello? authentication { get; set; }
        public string obsWebSocketVersion { get; set; } = "";
        public int rpcVersion { get; set; } = -1;
    }

    class AuthHello
    {
        public string challenge { get; set; } = "";
        public string salt { get; set; } = "";
    }
}