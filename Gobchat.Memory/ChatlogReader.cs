using Sharlayan;
using Sharlayan.Models.ReadResults;
using System.Collections.Generic;

namespace Gobchat.Memory.Chat
{
    internal class ChatlogReader
    {

        private int previousArrayIndex = 0;
        private int previousOffset = 0;

        public ChatlogReader()
        {

        }

        public List<Sharlayan.Core.ChatLogItem> Query()
        {
            ChatLogResult readResult = Reader.GetChatLog(previousArrayIndex, previousOffset);
            previousArrayIndex = readResult.PreviousArrayIndex;
            previousOffset = readResult.PreviousOffset;
            return readResult.ChatLogItems;
        }

    }

}
