using System;

namespace Gobchat.UI.Web
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

        public class LoadGobchatConfigEvent : JSEvent
        {
            public string data;
            public LoadGobchatConfigEvent(string data) : base("LoadGobchatConfig")
            {
                this.data = data;
            }
        }

        public class OverlayStateUpdate : JSEvent
        {
            public bool isLocked;
            public OverlayStateUpdate(bool isLocked) : base("OverlayStateUpdate")
            {
                this.isLocked = isLocked;
            }
        }
    }
}
