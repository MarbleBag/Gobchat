using System;
using System.Windows.Forms;



namespace Gobchat.UI.Forms.Extension
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void AsyncInvoke(this Control control, Action code)
        {
            if (code == null) return;
            if (control.InvokeRequired)
                control.BeginInvoke(code);
            else
                code.Invoke();
            
        }

        /// <summary>
        /// Executes the Action synchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void SyncInvoke(this Control control, Action code)
        {
            if (code == null) return;
            if (control.InvokeRequired)
                control.Invoke(code);
            else
                code.Invoke();
            
        }
    }

    public static class FormExtension
    {
        public static void InvokeAsyncOnUIThread(this Form form, Action action)
        {
            if (action == null) return;
            if (form.InvokeRequired) form.BeginInvoke(action);
            else action.Invoke();
        }

        public static void InvokeSyncOnUIThread(this Form form, Action action)
        {
            if (action == null) return;
            if (form.InvokeRequired) form.Invoke(action);
            else action.Invoke();
        }
    }
}
