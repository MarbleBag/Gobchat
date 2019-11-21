﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Core
{

    //Code taken from ngld's OverlayPlugin
    //TODO Add link / Rework to a general hotkey manager
    public sealed class KeyboardHook : NativeWindow, IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int WM_HOTKEY = 0x0312;

        private Dictionary<int, HotKeyInfo> _hotkeys = new Dictionary<int, HotKeyInfo>();

        /// <summary>
        /// Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // check if we got a hot key pressed.
            if (m.Msg == WM_HOTKEY && _hotkeys.TryGetValue((int)m.LParam, out HotKeyInfo info))
            {
                foreach (var cb in info.Callbacks)
                {
                    cb();
                }
            }
        }

        public KeyboardHook()
        {
            // create the handle for the window.
            this.CreateHandle(new CreateParams());
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key, Action callback)
        {
            var lookupKey = (int)modifier | ((int)key << 16);
            if (!_hotkeys.ContainsKey(lookupKey))
            {
                _hotkeys[lookupKey] = new HotKeyInfo();

                // register the hot key.
                if (!RegisterHotKey(Handle, _hotkeys[lookupKey].Id, (uint)modifier, (uint)key))
                {
                    _hotkeys.Remove(lookupKey);
                    throw new InvalidOperationException($"Unable to register Hotkey {modifier}-{key}");
                }
            }

            _hotkeys[lookupKey].Callbacks.Add(callback);
        }

        public void UnregisterHotKey(ModifierKeys modifier, Keys key, Action callback)
        {
            var lookupKey = (int)modifier | ((int)key << 16);
            if (_hotkeys.TryGetValue(lookupKey, out HotKeyInfo info))
            {
                info.Callbacks.Remove(callback);

                if (info.Callbacks.Count < 1)
                {
                    if (UnregisterHotKey(Handle, info.Id))
                    {
                        _hotkeys.Remove(lookupKey);
                    }
                }
            }
        }

        public void UnregisterHotKey(Action callback)
        {
            var toRemove = new List<int>();

            foreach (var pair in _hotkeys)
            {
                if (pair.Value.Callbacks.Contains(callback))
                {
                    pair.Value.Callbacks.Remove(callback);
                    if (pair.Value.Callbacks.Count < 1)
                    {
                        if (UnregisterHotKey(Handle, pair.Value.Id))
                        {
                            toRemove.Add(pair.Key);
                        }
                    }
                }
            }

            foreach (var key in toRemove)
            {
                _hotkeys.Remove(key);
            }
        }

        public void DisableHotKeys()
        {
            foreach (var pair in _hotkeys)
            {
                if (!UnregisterHotKey(Handle, pair.Value.Id))
                {
                    //Registry.Resolve<ILogger>().Log(LogLevel.Error, $"Failed to unregister hotkey {pair.Key} in DisableHotKeys().");
                    //TODO logging
                }
            }
        }

        public void EnableHotKeys()
        {
            foreach (var pair in _hotkeys)
            {
                uint modifier = (uint)pair.Key & 0xFFFF;
                uint key = (uint)(pair.Key >> 16) & 0xFFFF;

                if (!RegisterHotKey(Handle, pair.Value.Id, modifier, key))
                {
                    //Registry.Resolve<ILogger>().Log(LogLevel.Error, $"Failed to register hotkey {modifier}, {key} in EnableHotKeys().");
                    //TODO logging
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            foreach (var info in _hotkeys)
            {
                UnregisterHotKey(Handle, info.Value.Id);
            }

            // dispose the native window.
            this.DestroyHandle();
        }

        #endregion

        private class HotKeyInfo
        {
            public List<Action> Callbacks { get; private set; }
            public int Id { get; private set; }

            private static int _idCounter = 0;

            public HotKeyInfo()
            {
                Callbacks = new List<Action>();
                Id = _idCounter++;
            }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
