using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat
{
    namespace JavascriptEvents
    {
        public abstract class JSEvent
        {
            public string EventName { get; }
            public JSEvent(string name)
            {
                this.EventName = name ?? throw new ArgumentNullException(nameof(name));
            }
        }

        public class ChatMessageEvent : JSEvent
        {
            public string timestamp;
            public int type;
            public string source;
            public string message;

            public ChatMessageEvent(ChatMessage message) : base("ChatMessageEvent")
            {
                this.timestamp = message.Timestamp.ToString("HH:mm");
                this.type = message.MessageType;
                this.source = message.Source;
                this.message = message.Message;
            }
        }

        public class PlayerPositionEvent : JSEvent
        {
            public string playerId;
            public float x;
            public float y;
            public float z;

            public PlayerPositionEvent(PlayerPosition position) : base("PlayerPositionEvent")
            {
                this.playerId = position.Target;
                this.x = position.X;
                this.y = position.Y;
                this.z = position.Z;
            }
        }

        public class PlayerNameEvent : JSEvent
        {
            public string playername;

            public PlayerNameEvent(string playername) : base("PlayerNameEvent")
            {
                this.playername = playername;
            }
        }

        public class MentionsEvent : JSEvent
        {
            public string[] mentions;

            public MentionsEvent(string[] mentions) : base("MentionsEvent")
            {
                this.mentions = mentions;
            }
        }
    }
}
