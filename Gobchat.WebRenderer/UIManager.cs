using System;
using System.Threading;
using System.Windows.Forms;
using Gobchat.UI.Forms.Extension;

namespace Gobchat.UI
{
    public class UIManager
    {
        private Form _form;
        private NotifyIcon trayIcon;

        private Thread uiThread;

        public void Execute(bool executeInNewThread)
        {
            if (executeInNewThread)
            {
                uiThread = new Thread(() => SpinUp());
                uiThread.Start();
            }
            else
            {
                SpinUp();
            }
        }

        [STAThread]
        private void SpinUp()
        {
            Initialize();
            Run();
        }

        public void InvokeOnUIThread(Action action, bool asyncInvoke = true)
        {
            if (asyncInvoke)
                _form.InvokeAsyncOnUIThread(action);
            else
                _form.InvokeSyncOnUIThread(action);
        }

        private void Initialize()
        {
            Gobchat.UI.Web.WebRenderManager.Initialize();
            Application.ApplicationExit += (sender, e) => Dispose();

            _form = new Gobchat.UI.Forms.CefOverlayForm();
        }

        private void Run()
        {
            Application.EnableVisualStyles();
            Application.Run(_form);
        }

        private void Dispose()
        {
            Gobchat.UI.Web.WebRenderManager.Dispose();
        }

        private class MultiFormContext : ApplicationContext
        {
            private int openForms;
            public MultiFormContext(params Form[] forms)
            {
                openForms = forms.Length;

                foreach (var form in forms)
                {
                    form.FormClosed += (s, args) =>
                    {
                        //When we have closed the last of the "starting" forms, 
                        //end the program.
                        if (Interlocked.Decrement(ref openForms) == 0)
                            ExitThread();
                    };

                    form.Show();
                }
            }
        }
    }
}
