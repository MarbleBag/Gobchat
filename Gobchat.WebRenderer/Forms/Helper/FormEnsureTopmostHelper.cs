using System;
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Helper
{
    internal class FormEnsureTopmostHelper : IDisposable
    {
        public Form TargetForm;
        public bool Active = false;
        public int UpdateInterval;


        private System.Threading.Timer timer;

        public FormEnsureTopmostHelper(Form target, int updateInterval)
        {
            TargetForm = target;
            UpdateInterval = updateInterval;
        }

        public void Start()
        {
            Stop();
            Active = true;
            timer = new System.Threading.Timer((state) => RunTask(), null, 0, UpdateInterval);
        }

        public void Stop()
        {
            timer?.Dispose();
            timer = null;
            Active = false;
        }

        private void RunTask()
        {
            Action action = () => EnsureTopMost();
            TargetForm.Invoke(action);
        }

        private void EnsureTopMost()
        {
            NativeMethods.SetWindowPos(TargetForm.Handle, NativeMethods.HWND_TOPMOST,
               0, 0, 0, 0,
               NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE);
        }

        public void Dispose()
        {
            Stop();
        }
    }

}
