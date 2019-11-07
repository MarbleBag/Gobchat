using System;

namespace Gobchat.WebRenderer
{
    internal class WebRenderFactory
    {
        internal void Initialize()
        {
            
        }

        internal void Dispose()
        {
            
        }

        internal IWebRenderer Make()
        {
            return new WebRenderer();
        }
    }

}
