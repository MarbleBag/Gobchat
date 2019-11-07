/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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
using System;

namespace Gobchat.UI.Forms.Helper
{
    internal class MouseEventHelper
    {
        private int _lastMouseClickPositionX;
        private int _lastMouseClickPositionY;
        private MouseButtonType _lastMouseClickButton;
        private DateTime _lastMouseClickTime;
        public int ClickCount { get; private set; }

        public void ProcessClick(int x, int y, MouseButtonType mouseButton, bool isKeyDown)
        {
            if (isKeyDown)
            {
                if (IsMultipleClick(x, y, mouseButton))
                    ClickCount += 1;
                else
                    ClickCount = 1;
            }

            _lastMouseClickPositionX = x;
            _lastMouseClickPositionY = x;
            _lastMouseClickButton = mouseButton;
            _lastMouseClickTime = DateTime.UtcNow;
        }

        private bool IsMultipleClick(int x, int y, MouseButtonType mouseButton)
        {
            var delta = (DateTime.UtcNow - _lastMouseClickTime).TotalMilliseconds;
            if (delta > System.Windows.Forms.SystemInformation.DoubleClickTime)
                return false;
            if (_lastMouseClickPositionX != x || _lastMouseClickPositionY != y || _lastMouseClickButton != mouseButton)
                return false;
            return true;
        }

        public MouseEvent GetMouseEvent(int x, int y, MouseButtonType button)
        {
            var modifiers = CefEventFlags.None;
            if (button == MouseButtonType.Left)
                modifiers = CefEventFlags.LeftMouseButton;
            else if (button == MouseButtonType.Right)
                modifiers = CefEventFlags.RightMouseButton;
            else if (button == MouseButtonType.Middle)
                modifiers = CefEventFlags.MiddleMouseButton;

            return new MouseEvent(x, y, modifiers);
        }

        public MouseEvent GetMouseEvent(int x, int y)
        {
            return new MouseEvent(x, y, CefEventFlags.None);
        }
    }
}
