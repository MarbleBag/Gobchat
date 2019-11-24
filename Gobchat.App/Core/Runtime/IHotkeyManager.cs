using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;

namespace Gobchat.Core.Runtime
{
    public interface IHotkeyManager : IDisposable
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

    public sealed class InvalidHotkeyException : System.Exception
    {
        public int ErrorCode { get; }

        public InvalidHotkeyException(string message) : base(message)
        {
        }

        public InvalidHotkeyException(int errorCode, string hotkey, Exception innerException)
            : base($"Error [{errorCode}] - {hotkey}", innerException)
        {
            ErrorCode = errorCode;
        }

        public InvalidHotkeyException()
        {
        }
    }
}