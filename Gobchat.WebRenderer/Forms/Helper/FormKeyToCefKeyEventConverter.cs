/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using CefSharp;
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Helper
{
    internal sealed class FormMessageToCefKeyEventConverter
    {
        public bool IsProcessableKeyEvent(ref Message m)
        {
            return IsRelevantKey(m.Msg);
        }

        public KeyEvent ProcessKeyEvent(ref Message m)
        {
            var keyEvent = new CefSharp.KeyEvent();
            keyEvent.WindowsKeyCode = m.WParam.ToInt32();
            keyEvent.NativeKeyCode = (int)m.LParam.ToInt64();
            keyEvent.IsSystemKey = m.Msg == NativeMethods.WM_SYSCHAR ||
                                   m.Msg == NativeMethods.WM_SYSKEYDOWN ||
                                   m.Msg == NativeMethods.WM_SYSKEYUP;

            if (m.Msg == NativeMethods.WM_KEYDOWN || m.Msg == NativeMethods.WM_SYSKEYDOWN)
                keyEvent.Type = KeyEventType.RawKeyDown;
            else if (m.Msg == NativeMethods.WM_KEYUP || m.Msg == NativeMethods.WM_SYSKEYUP)
                keyEvent.Type = KeyEventType.KeyUp;
            else
                keyEvent.Type = KeyEventType.Char;

            keyEvent.Modifiers = GetKeyboardModifiers(ref m);

            return keyEvent;
        }

        private bool IsRelevantKey(int msg)
        {
            switch (msg)
            {
                case NativeMethods.WM_KEYDOWN:
                case NativeMethods.WM_KEYUP:
                case NativeMethods.WM_CHAR:
                case NativeMethods.WM_SYSKEYDOWN:
                case NativeMethods.WM_SYSKEYUP:
                case NativeMethods.WM_SYSCHAR:
                    return true;

                default:
                    return false;
            }
        }

        private CefEventFlags GetKeyboardModifiers(ref Message m)
        {
            var modifiers = CefEventFlags.None;

            if (IsKeyDown(Keys.Shift)) modifiers |= CefEventFlags.ShiftDown;
            if (IsKeyDown(Keys.ShiftKey)) modifiers |= CefEventFlags.ShiftDown;
            if (IsKeyDown(Keys.Control)) modifiers |= CefEventFlags.ControlDown;
            if (IsKeyDown(Keys.ControlKey)) modifiers |= CefEventFlags.ControlDown;
            if (IsKeyDown(Keys.Menu)) modifiers |= CefEventFlags.AltDown;

            if (IsKeyToggled(Keys.NumLock)) modifiers |= CefEventFlags.NumLockOn;
            if (IsKeyToggled(Keys.Capital)) modifiers |= CefEventFlags.CapsLockOn;

            switch ((Keys)m.WParam)
            {
                case Keys.Return:
                    if (((m.LParam.ToInt64() >> 48) & 0x0100) != 0)
                        modifiers |= CefEventFlags.IsKeyPad;
                    break;

                case Keys.Insert:
                case Keys.Delete:
                case Keys.Home:
                case Keys.End:
                case Keys.Prior:
                case Keys.Next:
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    if (!(((m.LParam.ToInt64() >> 48) & 0x0100) != 0))
                        modifiers |= CefEventFlags.IsKeyPad;
                    break;

                case Keys.NumLock:
                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.Divide:
                case Keys.Multiply:
                case Keys.Subtract:
                case Keys.Add:
                case Keys.Decimal:
                case Keys.Clear:
                    modifiers |= CefEventFlags.IsKeyPad;
                    break;

                case Keys.Shift:
                    if (IsKeyDown(Keys.LShiftKey)) modifiers |= CefEventFlags.IsLeft;
                    else modifiers |= CefEventFlags.IsRight;
                    break;

                case Keys.Control:
                    if (IsKeyDown(Keys.LControlKey)) modifiers |= CefEventFlags.IsLeft;
                    else modifiers |= CefEventFlags.IsRight;
                    break;

                case Keys.Alt:
                    if (IsKeyDown(Keys.LMenu)) modifiers |= CefEventFlags.IsLeft;
                    else modifiers |= CefEventFlags.IsRight;
                    break;

                case Keys.LWin:
                    modifiers |= CefEventFlags.IsLeft;
                    break;

                case Keys.RWin:
                    modifiers |= CefEventFlags.IsRight;
                    break;
            }

            return modifiers;
        }

        private bool IsKeyDown(Keys key)
        {
            return (NativeMethods.GetKeyState((int)key) & NativeMethods.KEY_PRESSED) != 0;
        }

        private bool IsKeyToggled(Keys key)
        {
            return (NativeMethods.GetKeyState((int)key) & NativeMethods.KEY_TOGGLED) != 0;
        }
    }
}