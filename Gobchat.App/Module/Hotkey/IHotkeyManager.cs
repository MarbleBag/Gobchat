using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;

namespace Gobchat.Core.Module.Hotkey
{
    public interface IHotkeyManager
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <exception cref="InvalidHotkeyException"></exception>
        void RegisterHotKey(Keys key, Action callback);

        void UnregisterHotKey(Keys key, Action callback);

        void UnregisterHotKey(Action callback);
    }
}