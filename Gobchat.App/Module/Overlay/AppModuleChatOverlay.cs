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

using Gobchat.UI.Forms;
using Gobchat.UI.Web;
using Gobchat.Core.Runtime;
using Gobchat.Core.Config;
using System.Windows.Forms;
using Gobchat.Core.UI;
using System;

namespace Gobchat.Core.Module
{
    public sealed class AppModuleChatOverlay : IApplicationModule, System.IDisposable
    {
        public const string OverlayUIId = "Gobchat.ChatOverlayForm";

        private IGobchatConfigManager _configManager;
        private IUIManager _manager;

        private CefOverlayForm _overlay;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _manager = container.Resolve<IUIManager>();
            _configManager = container.Resolve<IGobchatConfigManager>();

            var synchronizer = _manager.UISynchronizer;
            synchronizer.RunSync(() => InitializeUI());
        }

        private void InitializeUI()
        {
            _overlay = _manager.CreateUIElement(OverlayUIId, () => new CefOverlayForm());
            _overlay.Show(); //initializes all properties
            _overlay.Visible = false;

            _configManager.OnActiveProfileChange += (s, e) => UpdateFormPosition();
            UpdateFormPosition();

            _overlay.Move += (s, e) =>
            {
                var chatLocation = _overlay.Location;
                _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.position.x", chatLocation.X);
                _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.position.y", chatLocation.Y);
            };

            _overlay.SizeChanged += (s, e) =>
            {
                var chatSize = _overlay.Size;
                _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.size.width", chatSize.Width);
                _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.size.height", chatSize.Height);
            };

            if (_manager.TryGetUIElement<INotifyIconManager>(AppModuleNotifyIcon.NotifyIconManagerId, out var trayIcon))
            {
                trayIcon.Icon = Gobchat.Resource.GobTrayIconOff;

                trayIcon.OnIconClick += (s, e) => _overlay.Visible = !_overlay.Visible;

                var menuItemHideShow = new ToolStripMenuItem();
                menuItemHideShow.Text = _overlay.Visible ? "Hide" : "Show";
                menuItemHideShow.Click += (s, e) => _overlay.Visible = !_overlay.Visible;
                _overlay.VisibleChanged += (s, e) => menuItemHideShow.Text = _overlay.Visible ? "Hide" : "Show";
                trayIcon.AddMenu("overlay.showhide", menuItemHideShow);

                var menuItemReload = new ToolStripMenuItem("Reload");
                menuItemReload.Click += (s, e) => _overlay.Reload();
                trayIcon.AddMenu("overlay.reload", menuItemReload);
            }
        }

        private void UpdateFormPosition()
        {
            if (_configManager.ActiveProfile.HasProperty("behaviour.frame.chat.position.x") &&
                _configManager.ActiveProfile.HasProperty("behaviour.frame.chat.position.y"))
            {
                var posX = _configManager.ActiveProfile.GetProperty<long>("behaviour.frame.chat.position.x");
                var posY = _configManager.ActiveProfile.GetProperty<long>("behaviour.frame.chat.position.y");
                _overlay.Location = new System.Drawing.Point((int)posX, (int)posY);
            }

            if (_configManager.ActiveProfile.HasProperty("behaviour.frame.chat.size.width") &&
                _configManager.ActiveProfile.HasProperty("behaviour.frame.chat.size.height"))
            {
                var width = _configManager.ActiveProfile.GetProperty<long>("behaviour.frame.chat.size.width");
                var height = _configManager.ActiveProfile.GetProperty<long>("behaviour.frame.chat.size.height");
                _overlay.Size = new System.Drawing.Size((int)width, (int)height);
            }

            //TODO make sure chat is not outside of display
            //TODO make sure chat is not too small
            //TODO make sure chat is not too big
        }

        private void DisposeUI()
        {
            var chatLocation = _overlay.Location;
            _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.position.x", chatLocation.X);
            _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.position.y", chatLocation.Y);

            var chatSize = _overlay.Size;
            _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.size.width", chatSize.Width);
            _configManager.ActiveProfile.SetProperty("behaviour.frame.chat.size.height", chatSize.Height);
        }

        public void Dispose(IDIContext container)
        {
            Dispose();
        }

        public void Dispose()
        {
            DisposeUI();

            var synchronizer = _manager.UISynchronizer;
            synchronizer.RunSync(() => _overlay.Close());
            _manager.DisposeUIElement(OverlayUIId);

            _manager = null;
            _overlay = null;
            _configManager = null;
        }
    }
}