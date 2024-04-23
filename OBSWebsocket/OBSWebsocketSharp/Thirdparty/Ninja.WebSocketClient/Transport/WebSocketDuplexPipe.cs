using System.IO.Pipelines;
using System.Net.WebSockets;

namespace Ninja.WebSocketClient
{
    public class WebSocketClosedException : Exception
    {
        public WebSocketCloseStatus? CloseCode { get; private set; }
        public WebSocketClosedException(string message, WebSocketCloseStatus? code) : base(message)
        {
            CloseCode = code;
        }
    }

    internal class WebSocketDuplexPipe : IDuplexPipe
    {
        private ClientWebSocket? _webSocket;
        private IDuplexPipe? _transport;
        private IDuplexPipe? _application;
        private volatile bool _aborted;
        private const int DefaultBufferSize = 1 * 1024 * 1024;
        private const int DefaultSocketBufferSize = 1024;

        internal Task Running { get; private set; } = Task.CompletedTask;

        public PipeReader Input => _transport!.Input;

        public PipeWriter Output => _transport!.Output;

        public WebSocketDuplexPipe()
        {

        }

        public async Task StartAsync(string url, Action<ClientWebSocketOptions>? setOptions = null, CancellationToken ct = default)
        {
            _webSocket = new ClientWebSocket();

            setOptions?.Invoke(_webSocket.Options);

            try
            {
                await _webSocket.ConnectAsync(new Uri(url), ct);
            }
            catch
            {
                _webSocket.Dispose();
                throw;
            }

            var pipeOptions = new PipeOptions(
                pauseWriterThreshold: DefaultBufferSize,
                resumeWriterThreshold: DefaultBufferSize / 2,
                readerScheduler: PipeScheduler.ThreadPool,
                useSynchronizationContext: false);

            var input = new Pipe(pipeOptions);
            var output = new Pipe(pipeOptions);

            // The transport duplex pipe is used by the caller to
            // - subscribe to incoming websocket messages
            // - push messages to the websocket
            _transport = new DuplexPipe(output.Reader, input.Writer);
            // The application duplex pipe is used here to
            // - subscribe to incoming messages from the caller
            // - proxy incoming data from the websocket back to the subscriber
            _application = new DuplexPipe(input.Reader, output.Writer);

            Running = ProcessSocketAsync(_webSocket);
        }

        private async Task ProcessSocketAsync(WebSocket socket)
        {
            using (socket)
            {
                // LongRunning used to avoid deadlocks
                var receiving = Task.Factory.StartNew(() => StartReceiving(socket).Wait(), TaskCreationOptions.LongRunning);
                var sending = Task.Factory.StartNew(() => StartSending(socket).Wait(), TaskCreationOptions.LongRunning);

                var trigger = await Task.WhenAny(receiving, sending);

                if (trigger == receiving)
                {
                    _application!.Input.CancelPendingRead();

                    using var delayCts = new CancellationTokenSource();

                    var resultTask = await Task.WhenAny(sending, Task.Delay(TimeSpan.FromSeconds(5), delayCts.Token));

                    if (resultTask != sending)
                    {
                        _aborted = true;

                        socket.Abort();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
                else
                {
                    _aborted = true;

                    socket.Abort();

                    _application!.Output.CancelPendingFlush();
                }
            }
        }

        private async Task StartReceiving(WebSocket socket)
        {
            try
            {
                while (true)
                {
                    ValueWebSocketReceiveResult receiveResult;

                    do
                    {
                        var memory = _application!.Output.GetMemory(DefaultSocketBufferSize);

                        receiveResult = await socket.ReceiveAsync(memory, CancellationToken.None);

                        _application.Output.Advance(receiveResult.Count);
                    }
                    while (receiveResult.MessageType != WebSocketMessageType.Close && !receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        if (socket.CloseStatus != WebSocketCloseStatus.NormalClosure)
                        {
                            throw new WebSocketClosedException($"Websocket closed with error: {socket.CloseStatus}.", socket.CloseStatus);
                        }

                        return;
                    }

                    MessageFormatter.WriteRecordSeparator(_application.Output);

                    var flushResult = await _application.Output.FlushAsync();

                    if (flushResult.IsCanceled || flushResult.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                if (!_aborted)
                {
                    await _application!.Output.CompleteAsync(ex);
                }
            }
            catch (Exception ex)
            {
                if (!_aborted)
                {
                    await _application!.Output.CompleteAsync(ex);
                }
            }
            finally
            {
                await _application!.Output.CompleteAsync();
            }
        }

        private async Task StartSending(WebSocket socket)
        {
            Exception? error = null;

            try
            {
                while (true)
                {
                    var result = await _application!.Input.ReadAsync();
                    var buffer = result.Buffer;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            break;
                        }

                        if (!buffer.IsEmpty)
                        {
                            try
                            {
                                if (WebSocketCanSend(socket))
                                {
                                    await socket.SendAsync(buffer, WebSocketMessageType.Text);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                        else if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        _application.Input.AdvanceTo(buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                if (WebSocketCanSend(socket))
                {
                    try
                    {
                        await socket.CloseOutputAsync(
                            error != null ? WebSocketCloseStatus.InternalServerError : WebSocketCloseStatus.NormalClosure,
                            "",
                            CancellationToken.None);
                    }
                    catch (Exception)
                    {
                    }
                }

                _application!.Input.Complete();
            }
        }

        private static bool WebSocketCanSend(WebSocket ws)
        {
            return !(ws.State == WebSocketState.Aborted ||
                   ws.State == WebSocketState.Closed ||
                   ws.State == WebSocketState.CloseSent);
        }

        public async Task StopAsync()
        {
            if (_application == null)
            {
                return;
            }

            _transport!.Output.Complete();
            _transport!.Input.Complete();

            _application.Input.CancelPendingRead();

            try
            {
                await Running;
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                _webSocket?.Dispose();
            }
        }
    }
}
