using System.Buffers;

namespace Ninja.WebSocketClient
{
    internal static class MessageParser
    {
        public const byte RecordSeparator = 0x1e;

        public static void WriteRecordSeparator(IBufferWriter<byte> output)
        {
            var buffer = output.GetSpan(1);
            buffer[0] = RecordSeparator;
            output.Advance(1);
        }

        public static bool TryParse(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            if (buffer.IsSingleSegment)
            {
                var span = buffer.First.Span;
                var index = span.IndexOf(RecordSeparator);

                if (index == -1)
                {
                    payload = default;
                    return false;
                }

                payload = buffer.Slice(0, index);

                buffer = buffer.Slice(index + 1);

                return true;
            }
            else
            {
                return TryParseMultiSegment(ref buffer, out payload);
            }
        }

        private static bool TryParseMultiSegment(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            var position = buffer.PositionOf(RecordSeparator);
            if (position == null)
            {
                payload = default;
                return false;
            }

            payload = buffer.Slice(0, position.Value);

            // Skip record separator
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

            return true;
        }
    }
}
