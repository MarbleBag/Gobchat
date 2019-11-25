using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Gobchat.Core.Runtime
{
    // https://docs.microsoft.com/de-de/windows/win32/api/winuser/nf-winuser-registerhotkey?redirectedfrom=MSDN
    public sealed class HotkeyManager : IHotkeyManager, IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #region Helper classes

        [Flags]
        private enum ModifierKeys : uint
        {
            NONE = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        private sealed class KeyboardHookWindow : NativeWindow
        {
            private static int WM_HOTKEY = 0x0312;

            private Action<int> _onHotkey;

            public KeyboardHookWindow(Action<int> OnHotkey)
            {
                CreateHandle(new CreateParams());
                _onHotkey = OnHotkey;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == WM_HOTKEY)
                    _onHotkey.Invoke((int)m.LParam);
            }
        }

        private sealed class HotkeyCallback
        {
            public int Id { get; }

            private readonly IList<Action> _callbacks = new List<Action>();

            public HotkeyCallback(int id)
            {
                Id = id;
            }

            public void Add(Action callback)
            {
                _callbacks.Add(callback);
            }

            public void Remove(Action callback)
            {
                _callbacks.Remove(callback);
            }

            public void Invoke()
            {
                foreach (var callback in _callbacks)
                    callback.Invoke();
            }

            public bool IsEmpty { get { return _callbacks.Count == 0; } }
        }

        #endregion Helper classes

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static uint MOD_NOREPEAT = 0x4000;

        private Dictionary<uint, HotkeyCallback> _hotkeys = new Dictionary<uint, HotkeyCallback>();
        private KeyboardHookWindow _hiddenWindow;
        private IUISynchronizer _synchronizer;
        private int _hotkeyIdCounter;

        public HotkeyManager(IUISynchronizer synchronizer)
        {
            _synchronizer = synchronizer;
            _synchronizer.RunSync(() =>
                _hiddenWindow = new KeyboardHookWindow(OnHotkey)
            );
        }

        private void OnHotkey(int lookup)
        {
            if (_hotkeys.TryGetValue((uint)lookup, out var callbacks))
                callbacks.Invoke();
        }

        public void RegisterHotKey(Keys keys, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            // Keys stores modifiers in the upper 16 bits.
            // Instead of overwritting with 0 and shifting our modifiers, shift them away and put our modifier in the lower 16 bit
            var modifier = GetModifierKeys(keys);
            keys = RemoveModifierKeys(keys);
            uint lookup = ((uint)keys << 16) | ((uint)modifier);
            if (!_hotkeys.ContainsKey(lookup))
            {
                var id = Interlocked.Increment(ref _hotkeyIdCounter);
                RegisterHotKey(id, modifier, keys);
                _hotkeys[lookup] = new HotkeyCallback(id);
            }

            _hotkeys[lookup].Add(callback);
        }

        private ModifierKeys GetModifierKeys(Keys keys)
        {
            var modifiers = new ModifierKeys();

            if ((keys & Keys.Control) == Keys.Control)
                modifiers |= ModifierKeys.Control;

            if ((keys & Keys.Shift) == Keys.Shift)
                modifiers |= ModifierKeys.Shift;

            if ((keys & Keys.Alt) == Keys.Alt)
                modifiers |= ModifierKeys.Alt;

            if ((keys & Keys.LWin) == Keys.LWin || (keys & Keys.RWin) == Keys.RWin)
                modifiers |= ModifierKeys.Win;

            return modifiers;
        }

        private Keys RemoveModifierKeys(Keys keys)
        {
            return keys & ~Keys.Modifiers;
        }

        private void RegisterHotKey(int id, ModifierKeys modifier, Keys key)
        {
            int errorCode = 0;
            _synchronizer.RunSync(() =>
            {
                if (!RegisterHotKey(_hiddenWindow.Handle, id, fsModifiers: ((uint)modifier) | MOD_NOREPEAT, vk: (uint)key))
                    errorCode = Marshal.GetLastWin32Error();
            });

            if (errorCode != 0)
            {
                var exception = new System.ComponentModel.Win32Exception(errorCode);
                throw new InvalidHotkeyException(errorCode, $"{modifier}-{key}", exception);
            }
        }

        private bool UnregisterHotKey(int id)
        {
            var success = false;
            _synchronizer.RunSync(() => success = UnregisterHotKey(_hiddenWindow.Handle, id));
            return success;
        }

        public void UnregisterHotKey(Keys keys, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var modifier = GetModifierKeys(keys);
            keys = RemoveModifierKeys(keys);
            uint lookup = ((uint)keys << 16) | ((uint)modifier);
            if (_hotkeys.TryGetValue(lookup, out var hotkey))
            {
                hotkey.Remove(callback);
                if (hotkey.IsEmpty)
                    if (UnregisterHotKey(hotkey.Id))
                        _hotkeys.Remove(lookup);
            }
        }

        public void UnregisterHotKey(Action callback)
        {
            foreach (var hotkeyEntry in _hotkeys.ToList())
            {
                hotkeyEntry.Value.Remove(callback);
                if (hotkeyEntry.Value.IsEmpty)
                    if (UnregisterHotKey(hotkeyEntry.Value.Id))
                        _hotkeys.Remove(hotkeyEntry.Key);
            }
        }

        public void Dispose()
        {
            logger.Debug($"Dispose {nameof(HotkeyManager)}");
            foreach (var hotkey in _hotkeys.Values)
                UnregisterHotKey(hotkey.Id);
            _hotkeys.Clear();
            _hiddenWindow.DestroyHandle();
        }
    }
}