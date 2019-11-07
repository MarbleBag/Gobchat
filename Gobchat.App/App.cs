using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Gobchat.Core
{

    public sealed class App : IDisposable
    {

        private bool _isInitialized;
        private volatile bool _running;
        private SomeoneWhoDoesSomeWork _work;

        private Gobchat.UI.Forms.CefOverlayForm _overlay;
        private Task<bool> _workerThread;

        private Thread thread;
        private NotifyIcon trayIcon;

        [STAThread]
        internal void Run()
        {
            if (!_isInitialized)
                InitializApp();

            Gobchat.UI.Web.WebRenderManager.Initialize();
            _overlay = new Gobchat.UI.Forms.CefOverlayForm();

            _workerThread = new System.Threading.Tasks.Task<bool>(() =>
             {
                 _running = true;
                 _work.Initialize(_overlay);
                 while (_running)
                 {
                     _work.Update();

                     Thread.Sleep(1000);
                 }
                 _work.Dispose();
                 return true;
             });
            _workerThread.Start();

            Application.EnableVisualStyles();
            Application.Run(_overlay);
        }

        private void InitializApp()
        {
            _isInitialized = true;

            Application.ApplicationExit += (sender, e) => Dispose();

            _work = new SomeoneWhoDoesSomeWork();
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            _running = false;

            _workerThread?.Wait(30000);
            _workerThread = null;

            // thread?.Join();
            // thread = null;

            trayIcon?.Dispose();
            trayIcon = null;
        }
    }
}
