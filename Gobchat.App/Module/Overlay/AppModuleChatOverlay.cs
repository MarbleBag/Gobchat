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
using Gobchat.Core.Runtime;
using Gobchat.Core.Config;
using System.Windows.Forms;
using Gobchat.Core.UI;
using System;
using NLog;
using Gobchat.Module.NotifyIcon;
using Gobchat.Core.Util;

namespace Gobchat.Module.Overlay
{
    public sealed class AppModuleChatOverlay : IApplicationModule, System.IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string OverlayUIId = "Gobchat.ChatOverlayForm";

        private IGobchatConfigManager _configManager;
        private IUIManager _manager;

        private CefOverlayForm _overlay;

        private DelayedCallback _moveCallback;
        private DelayedCallback _resizeCallback;

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

            _configManager.OnActiveProfileChange += (s, e) => _manager.UISynchronizer.RunSync(UpdateFormPosition);
            _configManager.AddPropertyChangeListener("behaviour.frame.chat", (s, e) => _manager.UISynchronizer.RunSync(UpdateFormPosition));
            UpdateFormPosition();

            _moveCallback = new DelayedCallback(TimeSpan.FromSeconds(1), () =>
            {
                var chatLocation = _overlay.Location;
                _configManager.SetProperty("behaviour.frame.chat.position.x", chatLocation.X);
                _configManager.SetProperty("behaviour.frame.chat.position.y", chatLocation.Y);
                _configManager.DispatchChangeEvents();
            });

            _resizeCallback = new DelayedCallback(TimeSpan.FromSeconds(1), () =>
            {
                var chatSize = _overlay.Size;
                _configManager.SetProperty("behaviour.frame.chat.size.width", chatSize.Width);
                _configManager.SetProperty("behaviour.frame.chat.size.height", chatSize.Height);
                _configManager.DispatchChangeEvents();
            });

            _overlay.Move += (s, e) => _moveCallback.Call();
            _overlay.SizeChanged += (s, e) => _resizeCallback.Call();

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

#if DEBUG
                var menuItemDevTool = new ToolStripMenuItem("DevTool");
                menuItemDevTool.Click += (s, e) => _overlay.Browser.ShowDevTools();
                trayIcon.AddMenuToGroup("debug", "overlay.devtool", menuItemDevTool);
#endif
            }
        }

        private void UpdateFormPosition()
        {
            try
            {
                if (_configManager.HasProperty("behaviour.frame.chat.position.x") &&
                    _configManager.HasProperty("behaviour.frame.chat.position.y"))
                {
                    var posX = _configManager.GetProperty<long>("behaviour.frame.chat.position.x");
                    var posY = _configManager.GetProperty<long>("behaviour.frame.chat.position.y");
                    var newLocation = new System.Drawing.Point((int)posX, (int)posY);
                    if (!newLocation.Equals(_overlay.Location))
                        _overlay.Location = newLocation;
                }

                if (_configManager.HasProperty("behaviour.frame.chat.size.width") &&
                _configManager.HasProperty("behaviour.frame.chat.size.height"))
                {
                    var width = _configManager.GetProperty<long>("behaviour.frame.chat.size.width");
                    var height = _configManager.GetProperty<long>("behaviour.frame.chat.size.height");
                    var newSize = new System.Drawing.Size((int)width, (int)height);
                    if (!newSize.Equals(_overlay.Size))
                        _overlay.Size = newSize;
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
            }

            //TODO make sure chat is not outside of display
            //TODO make sure chat is not too small
            //TODO make sure chat is not too big
        }

        private void DisposeUI()
        {
            var chatLocation = _overlay.Location;
            _configManager.SetProperty("behaviour.frame.chat.position.x", chatLocation.X);
            _configManager.SetProperty("behaviour.frame.chat.position.y", chatLocation.Y);

            var chatSize = _overlay.Size;
            _configManager.SetProperty("behaviour.frame.chat.size.width", chatSize.Width);
            _configManager.SetProperty("behaviour.frame.chat.size.height", chatSize.Height);
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
            _moveCallback.Dispose();
            _resizeCallback.Dispose();

            _manager = null;
            _overlay = null;
            _configManager = null;
            _moveCallback = null;
            _resizeCallback = null;
        }
    }
}