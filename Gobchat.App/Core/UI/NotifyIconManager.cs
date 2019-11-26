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
    public sealed class NotifyIconManager2 : IDisposable
    {
        public sealed class NotifyIconEventArgs : EventArgs
        {
            public string Id { get; }
            public MenuItem MenuItem { get; }

            public NotifyIconEventArgs(string id, MenuItem menuItem)
            {
                Id = id;
                MenuItem = menuItem;
            }
        }

        private sealed class ManagedMenuItem
        {
            public string Id { get; }
            public MenuItem MenuItem { get; }

            public event EventHandler OnDispose;

            public event EventHandler<NotifyIconEventArgs> OnClick;

            public ManagedMenuItem(string id, MenuItem menuItem)
            {
                Id = id;
                MenuItem = menuItem;
                MenuItem.Disposed += MenuItem_Disposed;
                MenuItem.Click += MenuItem_Click;
            }

            private void MenuItem_Click(object sender, EventArgs e)
            {
                OnClick?.Invoke(this, new NotifyIconEventArgs(Id, MenuItem));
            }

            private void MenuItem_Disposed(object sender, EventArgs e)
            {
                MenuItem.Disposed -= MenuItem_Disposed;
                MenuItem.Click += MenuItem_Click;
                OnDispose?.Invoke(this, new EventArgs());
            }
        }

        private readonly NotifyIcon _icon;
        private bool _isDisposed;

        public string DefaultGroup { get; private set; }

        public event EventHandler<EventArgs> OnIconClick;

        public NotifyIconManager2(string name)
        {
            _icon = new NotifyIcon();
            _icon.Text = name;
            _icon.Click += OnEvent_NotifyIcon_Click;
        }

        public void SetMenuItem(string id, MenuItem item, string group)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (group == null) throw new ArgumentNullException(nameof(group));

            ManagedMenuItem managedMenuItem = GetManagedMenuItem(id);

            if (managedMenuItem == null)
            {
                managedMenuItem = new ManagedMenuItem(id, item);
                managedMenuItem.OnDispose += OnEvent_ManagedMenuItem_Dispose;
            }
            else if (managedMenuItem.MenuItem != item)
                throw new ArgumentException(); //TODO

            AddToGroup(group, managedMenuItem);
        }

        public void SetMenuItem(string id, MenuItem item)
        {
            SetMenuItem(id, item, DefaultGroup);
        }

        public MenuItem GetMenuItem(string id)
        {
            ManagedMenuItem managedMenuItem = GetManagedMenuItem(id);
            return managedMenuItem?.MenuItem;
        }

        private void OnEvent_ManagedMenuItem_Dispose(object sender, EventArgs e)
        {
            RemoveManagedMenuItem((sender as ManagedMenuItem).Id);
        }

        private void RemoveManagedMenuItem(string id)
        {
            //TODO
        }

        private void AddToGroup(string group, ManagedMenuItem managedMenuItem)
        {
            throw new NotImplementedException();
        }

        private ManagedMenuItem GetManagedMenuItem(string id)
        {
            throw new NotImplementedException();
        }

        private bool HasMenuItem(string id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;

            _icon.Dispose();
        }

        private void OnEvent_NotifyIcon_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseEventArgs)
                if (mouseEventArgs.Button == MouseButtons.Left)
                    OnIconClick?.Invoke(this, new EventArgs());
        }
    }

    public sealed class NotifyIconManager : IDisposable
    {
        public enum HideShowState
        {
            Show,
            Hide
        }

        public enum IconState
        {
            ClientFound,
            ClientNotFound
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

        private const string IconOn = @"resources/GobTrayIconOn.ico";
        private const string IconOff = @"resources/GobTrayIconOff.ico";

        private readonly System.Drawing.Icon _iconImage_StateOn;
        private readonly System.Drawing.Icon _iconImage_StateOff;

        private readonly NotifyIcon _icon;
        private MenuItem _menuHideShow;
        private MenuItem _menuUnPause;
        private MenuItem _menuReloadUI;
        private MenuItem _menuCloseApplication;

        public bool TrayIconVisible { get { return _icon.Visible; } set { _icon.Visible = value; } }

        public event EventHandler<NotifyIconEventArgs> OnMenuClick;

        public NotifyIconManager()
        {
            _iconImage_StateOn = new System.Drawing.Icon(IconOn);
            _iconImage_StateOff = new System.Drawing.Icon(IconOff);

            _icon = new NotifyIcon();
            _icon.Icon = _iconImage_StateOff;
            _icon.Text = "Gobchat";

            _menuHideShow = new MenuItem("");
            _menuUnPause = new MenuItem("Pause");
            _menuUnPause.Enabled = false; //TODO
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

        public void SetIconState(IconState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            switch (state)
            {
                case IconState.ClientFound:
                    _icon.Icon = _iconImage_StateOn;
                    break;

                case IconState.ClientNotFound:
                    _icon.Icon = _iconImage_StateOff;
                    break;
            }
        }

        public void Dispose()
        {
            _iconImage_StateOn.Dispose();
            _iconImage_StateOff.Dispose();

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