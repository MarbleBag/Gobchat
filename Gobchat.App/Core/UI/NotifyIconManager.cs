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

using System;
using System.Windows.Forms;

namespace Gobchat.Core.UI
{
    public sealed class NotifyIconManager : IDisposable
    {
        public enum HideShowState
        {
            Show,
            Hide
        }

        public enum NotifyMenuItem
        {
            HideShow,
            UnPause,
            ReloadUI,
            CloseApplication
        }

        public sealed class NotifyIconEventArgs : EventArgs
        {
            public NotifyMenuItem NotifyMenuItem { get; }

            public NotifyIconEventArgs(NotifyMenuItem notifyMenuItem)
            {
                NotifyMenuItem = notifyMenuItem;
            }
        }

        private readonly NotifyIcon _icon;
        private MenuItem _menuHideShow;
        private MenuItem _menuUnPause;
        private MenuItem _menuReloadUI;
        private MenuItem _menuCloseApplication;

        public bool TrayIconVisible { get { return _icon.Visible; } set { _icon.Visible = value; } }

        public event EventHandler<NotifyIconEventArgs> OnMenuClick;

        public NotifyIconManager()
        {
            _icon = new NotifyIcon();
            _icon.Icon = new System.Drawing.Icon(@"resources/gobtray.ico");
            _icon.Text = "Gobchat";

            _menuHideShow = new MenuItem("");
            _menuUnPause = new MenuItem("Pause");
            _menuReloadUI = new MenuItem("Reload UI");
            _menuCloseApplication = new MenuItem("Close");

            _icon.Click += OnEvent_HideShow;
            _menuHideShow.Click += OnEvent_HideShow;

            _menuUnPause.Click += OnEvent_UnPause;
            _menuReloadUI.Click += OnEvent_ReloadUI;
            _menuCloseApplication.Click += OnEvent_CloseApplication;

            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(_menuHideShow);
            contextMenu.MenuItems.Add(_menuUnPause);
            contextMenu.MenuItems.Add(_menuReloadUI);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(_menuCloseApplication);

            _icon.ContextMenu = contextMenu;

            SetHideShowText(HideShowState.Hide);
        }

        public void SetHideShowText(HideShowState state)
        {
            switch (state)
            {
                case HideShowState.Show:
                    _menuHideShow.Text = "Show";
                    break;

                case HideShowState.Hide:
                    _menuHideShow.Text = "Hide";
                    break;
            }
        }

        public void Dispose()
        {
            _icon.Dispose();
            _menuHideShow = null;
            _menuUnPause = null;
            _menuReloadUI = null;
            _menuCloseApplication = null;
        }

        private void OnEvent_UnPause(object sender, EventArgs e)
        {
            OnMenuClick?.Invoke(this, new NotifyIconEventArgs(NotifyMenuItem.UnPause));
        }

        private void OnEvent_HideShow(object sender, EventArgs e)
        {
            if (sender == _icon)
            {
                // notifyicon.click raises an event for left and right click. Overall we only care for a left-click
                if (e is MouseEventArgs mouseEventArgs)
                    if (mouseEventArgs.Button == MouseButtons.Left)
                        OnMenuClick?.Invoke(this, new NotifyIconEventArgs(NotifyMenuItem.HideShow));
            }
            else
            {
                OnMenuClick?.Invoke(this, new NotifyIconEventArgs(NotifyMenuItem.HideShow));
            }
        }

        private void OnEvent_CloseApplication(object sender, EventArgs e)
        {
            OnMenuClick?.Invoke(this, new NotifyIconEventArgs(NotifyMenuItem.CloseApplication));
        }

        private void OnEvent_ReloadUI(object sender, EventArgs e)
        {
            OnMenuClick?.Invoke(this, new NotifyIconEventArgs(NotifyMenuItem.ReloadUI));
        }
    }
}