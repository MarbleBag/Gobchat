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
        private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hwnd, ref Windowplacement lpwndpl);

        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWMINIMIZED = 2;

        private struct Windowplacement
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rect rcNormalPosition;
            public Rect rcDevice;
        }

        private struct Point
        {
            public int X;
            public int Y;
        }

        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public enum EventTypeEnum
        {
            Unknown,
            Minimizeed,
            Maximizeed,
            Foreground
        }

        public event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChangedEvent;

        private WinEventDelegate _wineventDelegate = null;

        private IntPtr _foregroundHook;
        private IntPtr _minimizeHook;
        private IntPtr _locationChangeHook;

        private int _initializedThread;

        public bool Enabled { get; private set; }

        public WindowObserver()
        {
            // set & unset need to be called by the same thread, prefareable any thread with a running message pump
            _initializedThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public void StartObserving()
        {
            if (Enabled)
                return;

            if (_initializedThread != System.Threading.Thread.CurrentThread.ManagedThreadId)
                throw new InvalidOperationException($"Invalid thread access. Expected thread {_initializedThread} but was {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            _wineventDelegate = new WinEventDelegate(WinEventProc);

            //  _foregroundHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _wineventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            //   if (IntPtr.Zero == _foregroundHook)
            //      logger.Error("Unable to register window foreground hook!");

            _minimizeHook = SetWinEventHook(EVENT_SYSTEM_MINIMIZEEND, EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, _wineventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            if (IntPtr.Zero == _minimizeHook)
                logger.Error("Unable to register window minimizeend hook!");

            _locationChangeHook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, _wineventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            if (IntPtr.Zero == _locationChangeHook)
                logger.Error("Unable to register window object locationchange hook!");

            Enabled = true;
        }

        public void StopObserving()
        {
            if (!Enabled)
                return;

            if (_initializedThread != System.Threading.Thread.CurrentThread.ManagedThreadId)
                throw new InvalidOperationException($"Invalid thread access. Expected thread {_initializedThread} but was {System.Threading.Thread.CurrentThread.ManagedThreadId}");

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

            if (IntPtr.Zero != _locationChangeHook)
            {
                UnhookWinEvent(_locationChangeHook);
                _locationChangeHook = IntPtr.Zero;
            }

            Enabled = false;
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero)
                return;

            EventTypeEnum evtType = EventTypeEnum.Unknown;

            switch (eventType)
            {
                case EVENT_SYSTEM_MINIMIZEEND: //for some reasons this triggers on maximize???
                    //  evtType = EventTypeEnum.Minimizeed;
                    break;

                case EVENT_SYSTEM_FOREGROUND:
                    evtType = EventTypeEnum.Foreground;
                    break;

                case EVENT_OBJECT_LOCATIONCHANGE:
                    Windowplacement placement = new Windowplacement();
                    placement.length = Marshal.SizeOf(placement);
                    GetWindowPlacement(hwnd, ref placement);

                    switch (placement.showCmd)
                    {
                        case 7: //SW_SHOWMINNOACTIVE
                        case 4: //SW_SHOWNOACTIVATE
                        case (int)SW_SHOWMINIMIZED:
                            evtType = EventTypeEnum.Minimizeed;
                            break;

                        case 1: //SW_SHOWNORMAL
                        case 5: //SW_SHOW
                        case 8: //SW_SHOWNA
                        case (int)SW_SHOWMAXIMIZED:
                            evtType = EventTypeEnum.Maximizeed;
                            break;
                    }

                    break;
            }

            if (evtType == EventTypeEnum.Unknown)
                return;

            var windowName = GetWindowTitle(hwnd);
            GetWindowThreadProcessId(hwnd, out var processId);
            ActiveWindowChangedEvent?.Invoke(this, new ActiveWindowChangedEventArgs(processId, windowName, evtType));
        }

        public string GetActiveWindowTitle()
        {
            return GetWindowTitle(GetForegroundWindow());
        }

        private string GetWindowTitle(IntPtr hwnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(hwnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private string GetProcessName(int pId)
        {
            var processName = "";
            try
            {
                using (var process = System.Diagnostics.Process.GetProcessById(pId))
                {
                    processName = process.ProcessName;
                }
            }
            catch (Exception e)
            {
                //already dead
            }
            return processName;
        }

        public void Dispose()
        {
            StopObserving();
        }

        public sealed class ActiveWindowChangedEventArgs : EventArgs
        {
            public string WindowName { get; }

            public uint ProcessId { get; }

            public EventTypeEnum EventType { get; }

            public ActiveWindowChangedEventArgs(uint processId, EventTypeEnum eventType)
            {
                ProcessId = processId;
                EventType = eventType;
            }

            public ActiveWindowChangedEventArgs(uint processId, string windowName, EventTypeEnum eventType)
            {
                ProcessId = processId;
                WindowName = windowName;
                EventType = eventType;
            }

            public override string ToString()
            {
                return $"ActiveWindowChangedEventArgs[ProcessId={ProcessId}, WindowName={WindowName}, EventType={EventType}]";
            }
        }
    }
}