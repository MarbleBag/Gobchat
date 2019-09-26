using System;

namespace Gobchat
{
    public class ChatMessage
    {
        public class ChatMessageSegment
        {
            public int Format { get; }
            public string Message { get; }
        }

        public class ChatMessageSource
        {
            public int Data { get; }
            public string Name { get; }
        }

        public ChatMessage(DateTime timestamp, string source, int messageType, string message)
        {
            Timestamp = timestamp;
            Source = source;
            MessageType = messageType;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public DateTime Timestamp { get; }
        public string Source { get; }
        public int MessageType { get; }
        public string Message { get; }

        public override string ToString()
        {
            var className = nameof(ChatMessage);
            return $"{className} => time:{Timestamp} | source:{Source} | type:{MessageType} | msg:{Message}";
        }
    }
}
