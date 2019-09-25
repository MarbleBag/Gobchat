using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Gobchat
{

    public class JavascriptBuilder
    {
        private readonly System.Text.StringBuilder stringbuilder;
        private readonly Newtonsoft.Json.JsonSerializer jsonSerializer;
        private readonly Newtonsoft.Json.JsonTextWriter jsonWriter;

        public JavascriptBuilder(){
            stringbuilder = new System.Text.StringBuilder(1000);
            jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            jsonWriter = new Newtonsoft.Json.JsonTextWriter(new System.IO.StringWriter(stringbuilder));
        }

        public string BuildCustomEventDispatcher(JavascriptEvents.JSEvent evt)
        {
            stringbuilder.Append("document.dispatchEvent(new CustomEvent('");
            stringbuilder.Append(evt.EventName);
            stringbuilder.Append("', { detail: ");
            jsonSerializer.Serialize(jsonWriter, evt);
            stringbuilder.Append(" }));");
            string result = stringbuilder.ToString();
            stringbuilder.Clear();
            return result;
        }
    }

    public class JavascriptDispatcher
    {
        private JavascriptBuilder jsBuilder = new JavascriptBuilder();
        private GobchatOverlay overlay;

        public JavascriptDispatcher(GobchatOverlay overlay)
        {
            this.overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
        }

        public void DispatchEventToOverlay(JavascriptEvents.JSEvent evt)
        {
            if (overlay.Overlay.Renderer == null) return; //overlay not active
            string eventScript = jsBuilder.BuildCustomEventDispatcher(evt);

            //string quoted =  eventScript.Replace("{", "{{").Replace("}", "}}");
            //overlay.getLogger().LogInfo($"Script: {quoted}");

            overlay.Overlay.Renderer.ExecuteScript(eventScript);
        }
    }

    
}
