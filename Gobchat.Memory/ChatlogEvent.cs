using System;
using System.Collections.Generic;

namespace Gobchat.Memory.Chat
{
    public class ChatlogEvent : System.EventArgs
    {
        public List<ChatlogItem> ChatlogItems { get; }

        public ChatlogEvent(List<ChatlogItem> chatlogItems)
        {
            ChatlogItems = chatlogItems ?? throw new ArgumentNullException(nameof(chatlogItems));
        }
    }
}