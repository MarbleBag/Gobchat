using NLog;

namespace Gobchat.Core.Runtime
{
    public sealed class ApplicationHotkeyComponent : IApplicationComponent
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IHotkeyManager _hotkeyManager;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var synchronizer = container.Resolve<IUISynchronizer>();
            //Needs to run on the UI thread, because this hotkey manager uses a hidden window for its hotkeys
            _hotkeyManager = new HotkeyManager(synchronizer);
            container.Register<IHotkeyManager>((c, _) => _hotkeyManager);
        }

        public void Dispose(IDIContext container)
        {
            _hotkeyManager.Dispose();
            _hotkeyManager = null;
        }
    }
}