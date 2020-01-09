using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Memory.Window
{
    // based on https://stackoverflow.com/questions/4372055/detect-active-window-changed-using-c-sharp-without-polling
    public sealed class WindowObserver : IDisposable
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private const uint WINEVENT_OUTOFCONTEXT = 0x00;
        private const uint EVENT_SYSTEM_FOREGROUND = 0x03;
        private const uint EVENT_SYSTEM_MINIMIZEEND = 0x17;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

        public event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChangedEvent;

        private WinEventDelegate _wineventDelegate = null;
        private IntPtr _foregroundHook;
        private IntPtr _minimizeHook;

        public WindowObserver()
        {
            _wineventDelegate = new WinEventDelegate(WinEventProc);
            _foregroundHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _wineventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            if (IntPtr.Zero == _foregroundHook)
                logger.Error("Unable to register window foreground hook!");

            _minimizeHook = SetWinEventHook(EVENT_SYSTEM_MINIMIZEEND, EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, _wineventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            if (IntPtr.Zero == _minimizeHook)
                logger.Error("Unable to register window minimizeend hook!");
        }

        public void Dispose()
        {
            if (IntPtr.Zero != _foregroundHook)
            {
                UnhookWinEvent(_foregroundHook);
                _foregroundHook = IntPtr.Zero;
            }

            if (IntPtr.Zero != _minimizeHook)
            {
                UnhookWinEvent(_minimizeHook);
                _minimizeHook = IntPtr.Zero;
            }
        }

        public string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            //var windowTitle = GetActiveWindowTitle();
            GetWindowThreadProcessId(hwnd, out var processId);
            ActiveWindowChangedEvent?.Invoke(this, new ActiveWindowChangedEventArgs(processId));
        }

        public sealed class ActiveWindowChangedEventArgs : EventArgs
        {
            public uint ProcessId { get; }

            public ActiveWindowChangedEventArgs(uint processId)
            {
                ProcessId = processId;
            }
        }
    }
}