using System;
using System.Globalization;

namespace Gobchat
{
    public class ChatLogLineParser : ACTLogLineHandler
    {
        public delegate void OnMessage(ChatMessage message);

        private readonly Logger logger;
        private readonly OnMessage onMessage;

        public ChatLogLineParser(Logger logger, OnMessage onMessage)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.onMessage = onMessage ?? throw new ArgumentNullException(nameof(onMessage));
        }

        public void Handle(ReaderIndex index, ACTLogLine entry)
        {
            int? channel = ExtractChannelInfo(index, entry.Message);
            if (!channel.HasValue)
                return;

            int vChannel = channel.GetValueOrDefault(0);
            string source = ExtractSource(vChannel, index, entry.Message);
            string message = ExtractMessage(index, entry.Message);

            var result = new ChatMessage(entry.TimeStamp, source, vChannel, message);

            onMessage(result);
        }

        private string ExtractSource(int channel, ReaderIndex index, string message)
        {
            // var channelType = EnumHelper.ToEnum<Type0ChannelEnum>(channel.GetValueOrDefault(0));
            switch ((ChatChannelEnum)channel)
            {
                case ChatChannelEnum.SAY:
                case ChatChannelEnum.EMOTE:
                case ChatChannelEnum.YELL:
                case ChatChannelEnum.PARTY:
                case ChatChannelEnum.ALLIANCE:
                case ChatChannelEnum.GUILD:
                case ChatChannelEnum.TELL_SEND:
                case ChatChannelEnum.TELL_RECIEVE:
                case ChatChannelEnum.WORLD_LINKSHELL_1:
                case ChatChannelEnum.WORLD_LINKSHELL_2:
                case ChatChannelEnum.WORLD_LINKSHELL_3:
                case ChatChannelEnum.WORLD_LINKSHELL_4:
                case ChatChannelEnum.WORLD_LINKSHELL_5:
                case ChatChannelEnum.WORLD_LINKSHELL_6:
                case ChatChannelEnum.WORLD_LINKSHELL_7:
                case ChatChannelEnum.WORLD_LINKSHELL_8:
                case ChatChannelEnum.LINKSHELL_1:
                case ChatChannelEnum.LINKSHELL_2:
                case ChatChannelEnum.LINKSHELL_3:
                case ChatChannelEnum.LINKSHELL_4:
                case ChatChannelEnum.LINKSHELL_5:
                case ChatChannelEnum.LINKSHELL_6:
                case ChatChannelEnum.LINKSHELL_7:
                case ChatChannelEnum.LINKSHELL_8:
                case ChatChannelEnum.NPC_TALK:
                case ChatChannelEnum.ANIMATED_EMOTE:
                    return ExtractSource(index, message);
                default:
                    return null;
            }
        }

        private int? ExtractChannelInfo(ReaderIndex index, string message)
        {
            var fragment = ACTLogLineUtil.ExtractNextFragmentFromLog(":", index, message);
            if (fragment == null)
                return null;
            if (Int32.TryParse(fragment, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int result))
                return result;
            return null;
        }

        private string ExtractSource(ReaderIndex index, string message)
        {
            var fragment = ACTLogLineUtil.ExtractNextFragmentFromLog(":", index, message);
            return fragment ?? "";
        }
        
        private string ExtractMessage(ReaderIndex index, string message)
        {
            int length = message.Length - index.Value;
            return length <= 0 ? "" : message.Substring(index.Value, length);
        }
    }
}
