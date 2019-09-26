using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Gobchat
{

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
