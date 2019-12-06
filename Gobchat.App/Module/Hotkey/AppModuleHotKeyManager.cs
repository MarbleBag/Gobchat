using Gobchat.Core.Runtime;
using NLog;

namespace Gobchat.Core.Module.Hotkey
{
    public sealed class AppModuleHotKeyManager : IApplicationModule, System.IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private HotkeyManager _hotkeyManager;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var synchronizer = container.Resolve<IUISynchronizer>();

            //Needs to run on the UI thread, because this hotkey manager uses a hidden window for its hotkeys
            _hotkeyManager = new HotkeyManager(synchronizer);
            container.Register<IHotkeyManager>((c, _) => _hotkeyManager);
        }

        public void Dispose(IDIContext container)
        {
            Dispose();
        }

        public void Dispose()
        {
            _hotkeyManager.Dispose();
            _hotkeyManager = null;
        }
    }
}