using System.Buffers;
using System.Net.WebSockets;

namespace Ninja.WebSocketClient
{
    public class NinjaWebSocket
    {
        private readonly WebSocketDuplexPipe _webSocketPipe;
        private readonly string _url;
        private Action<ClientWebSocketOptions>? _setOptions = null;
        private int _keepAliveInterval;
        private int _reconnectInterval;
        private Timer? _keepAliveTimer;
        private Timer? _reconnectTimer;
        private Func<ArraySegment<byte>?>? _keepAlivePayloadFunc;
        private Exception? ConnectException { get; set; }
        private Exception? CloseException { get; set; }
        private Task? ReceiveTask { get; set; }

        public ConnectionState ConnectionState { get; private set; }
        public event Func<Task>? OnConnected;
        public event Func<Exception?, Task>? OnReconnecting;
        public event Func<Task>? OnKeepAlive;
        public event Func<Exception?, Task>? OnClosed;
        public event Func<ReadOnlySequence<byte>?, Task>? OnReceived;

        public NinjaWebSocket(string url)
        {
            _url = url;
            _webSocketPipe = new WebSocketDuplexPipe();
        }

        public async Task<bool> StartAsync(CancellationToken ct = default)
        {
            try
            {
                await _webSocketPipe.StartAsync(_url, _setOptions, ct);

                ConnectionState = ConnectionState.Connected;

                ReceiveTask = ReceiveLoop();

                await (OnConnected?.Invoke() ?? Task.CompletedTask);
                return true;
            }
            catch (Exception ex)
            {
                ConnectException = ex;

                if (ConnectionState == ConnectionState.Reconnecting)
                {
                    _ = OnReconnecting?.Invoke(ConnectException);
                }
            }
            return false;
        }

        public async Task SendAsync(ArraySegment<byte> data, CancellationToken ct = default)
        {
            await _webSocketPipe.Output.WriteAsync(data, ct);
        }

        public async Task StopAsync()
        {
            _keepAliveTimer?.Dispose();
            _reconnectTimer?.Dispose();

            _webSocketPipe.Input.CancelPendingRead();

            await (ReceiveTask ?? Task.CompletedTask);

            await _webSocketPipe.StopAsync();
        }

        public NinjaWebSocket SetOptions(Action<ClientWebSocketOptions> setOptions)
        {
            _setOptions = setOptions;

            return this;
        }

        public NinjaWebSocket SetKeepAlive(Func<ArraySegment<byte>?>? payloadFunc = null, int intervalMilliseconds = 30000)
        {
            _keepAliveInterval = intervalMilliseconds;
            _keepAlivePayloadFunc = payloadFunc;
            _keepAliveTimer = new Timer((objState) =>
            {
                if (ConnectionState != ConnectionState.Connected)
                {
                    return;
                }

                _ = KeepAlive();

            }, null, TimeSpan.FromMilliseconds(_keepAliveInterval), TimeSpan.FromMilliseconds(_keepAliveInterval));

            return this;
        }

        public NinjaWebSocket SetAutomaticReconnect(int intervalMilliseconds = 10000)
        {
            _reconnectInterval = intervalMilliseconds;

            _reconnectTimer = new Timer((objState) =>
            {
                if (ConnectionState == ConnectionState.Connected)
                {
                    return;
                }

                ConnectionState = ConnectionState.Reconnecting;

                _ = StartAsync();

            }, null, TimeSpan.FromMilliseconds(_reconnectInterval), TimeSpan.FromMilliseconds(_reconnectInterval));

            return this;
        }

        private async Task ReceiveLoop()
        {
            var input = _webSocketPipe.Input;

            try
            {
                while (true)
                {
                    var result = await input.ReadAsync();
                    var buffer = result.Buffer;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            break;
                        }

                        if (!buffer.IsEmpty)
                        {
                            while (MessageParser.TryParse(ref buffer, out var payload))
                            {
                                await (OnReceived?.Invoke(payload) ?? Task.CompletedTask);
                            }
                        }

                        if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        input.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {

                CloseException = ex;
            }
            finally
            {
                ConnectionState = ConnectionState.Disconnected;

                _ = OnClosed?.Invoke(CloseException);
            }
        }

        private async Task KeepAlive(CancellationToken ct = default)
        {
            var payload = _keepAlivePayloadFunc?.Invoke() ?? Memory<byte>.Empty;

            await _webSocketPipe.Output.WriteAsync(payload, ct);

            _ = OnKeepAlive?.Invoke();
        }
    }
}