using System.Buffers;
using System.Net.WebSockets;

namespace Ninja.WebSocketClient
{
    internal static class WebSocketExtensions
    {
        public static async ValueTask SendAsync(this WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            if (buffer.IsSingleSegment)
            {
                await webSocket.SendAsync(buffer.First, webSocketMessageType, endOfMessage: true, cancellationToken);
            }
            else
            {
                var position = buffer.Start;

                buffer.TryGet(ref position, out var prevSegment);

                while (buffer.TryGet(ref position, out var segment))
                {
                    await webSocket.SendAsync(prevSegment, webSocketMessageType, endOfMessage: false, cancellationToken);

                    prevSegment = segment;
                }

                await webSocket.SendAsync(prevSegment, webSocketMessageType, endOfMessage: true, cancellationToken);
            }
        }
    }
}