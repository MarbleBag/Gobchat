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

using Gobchat.UI.Forms;
using Gobchat.Core.Runtime;
using Gobchat.Core.Config;
using System.Windows.Forms;
using Gobchat.Core.UI;
using System;
using Gobchat.Module.NotifyIcon;
using Gobchat.Core.Util;

namespace Gobchat.Module.Overlay
{
    public sealed class AppModuleChatOverlay : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public const string OverlayUIId = "Gobchat.ChatOverlayForm";

        private IConfigManager _configManager;
        private IUIManager _manager;

        private CefOverlayForm _overlay;

        private DelayedCallback _moveCallback;
        private DelayedCallback _resizeCallback;

        /// <summary>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// Requires: <see cref="IConfigManager"/> <br></br>
        /// <br></br>
        /// Adds to UI element: <see cref="INotifyIconManager"/> <br></br>
        /// Installs UI element: <see cref="CefOverlayForm"/> <br></br>
        /// </summary>
        public AppModuleChatOverlay()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _manager = container.Resolve<IUIManager>();
            _configManager = container.Resolve<IConfigManager>();

            var synchronizer = _manager.UISynchronizer;
            synchronizer.RunSync(() => InitializeUI());
        }

        private void InitializeUI()
        {
            _overlay = _manager.CreateUIElement(OverlayUIId, () => new CefOverlayForm());
            _overlay.Show(); //initializes all properties
            _overlay.Visible = false;

            _configManager.AddPropertyChangeListener("behaviour.frame.chat", true, true, OnEvent_ConfigManager_PositionChange);

            _moveCallback = new DelayedCallback(TimeSpan.FromSeconds(1), () =>
            {
                var location = _overlay.Location;
                if (IsFrameOnScreens(_overlay.DesktopBounds))
                {
                    _configManager.SetProperty("behaviour.frame.chat.position.x", location.X);
                    _configManager.SetProperty("behaviour.frame.chat.position.y", location.Y);
                    _configManager.DispatchChangeEvents();
                }
                else // restore last location and size from config
                {
                    UpdateFormPosition();
                }
            });

            _resizeCallback = new DelayedCallback(TimeSpan.FromSeconds(1), () =>
            {
                var size = _overlay.Size;
                if (IsFrameOnScreens(_overlay.DesktopBounds))
                {
                    _configManager.SetProperty("behaviour.frame.chat.size.width", size.Width);
                    _configManager.SetProperty("behaviour.frame.chat.size.height", size.Height);
                    _configManager.DispatchChangeEvents();
                }
                else // restore last location and size from config
                {
                    UpdateFormPosition();
                }
            });

            _overlay.Move += (s, e) => _moveCallback.Call();
            _overlay.SizeChanged += (s, e) => _resizeCallback.Call();

            _overlay.Browser.OnBrowserLoadPageDone += (s, e) =>
            {
                if (!_overlay.Visible)
                    _manager.UISynchronizer.RunSync(() => _overlay.Visible = true);
            };

            if (_manager.TryGetUIElement<INotifyIconManager>(AppModuleNotifyIcon.NotifyIconManagerId, out var trayIcon))
            {
                //trayIcon.Icon = Gobchat.Resource.GobTrayIconOff;

                trayIcon.OnIconClick += (s, e) => _overlay.Visible = !_overlay.Visible;

                var menuItemHideShow = new ToolStripMenuItem();
                menuItemHideShow.Text = _overlay.Visible ? Resources.Module_NotifyIcon_UI_Hide : Resources.Module_NotifyIcon_UI_Show;
                menuItemHideShow.Click += (s, e) => _overlay.Visible = !_overlay.Visible;
                _overlay.VisibleChanged += (s, e) => menuItemHideShow.Text = _overlay.Visible ? Resources.Module_NotifyIcon_UI_Hide : Resources.Module_NotifyIcon_UI_Show;
                trayIcon.AddMenu("overlay.showhide", menuItemHideShow);

                var menuItemReload = new ToolStripMenuItem(Resources.Module_NotifyIcon_UI_Reload);
                menuItemReload.Click += (s, e) => _overlay.Reload();
                trayIcon.AddMenu("overlay.reload", menuItemReload);

                var menuItemFrameReset = new ToolStripMenuItem(Resources.Module_NotifyIcon_UI_Reset);
                menuItemFrameReset.Click += (s, e) => ResetFrameToDefaultLocation();
                trayIcon.AddMenu("overlay.reset", menuItemFrameReset);

#if DEBUG
                var menuItemDevTool = new ToolStripMenuItem("DevTool");
                menuItemDevTool.Click += (s, e) => _overlay.Browser.ShowDevTools();
                trayIcon.AddMenuToGroup("debug", "overlay.devtool", menuItemDevTool);
#endif
            }

            _overlay.Visible = false;
        }

        private void OnEvent_ConfigManager_PositionChange(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            UpdateFormPosition();
        }

        private void UpdateFormPosition()
        {
            _manager.UISynchronizer.RunSync(UpdateFormPositionOnUIThread);
        }

        private void UpdateFormPositionOnUIThread()
        {
            try
            {
                var posX = _configManager.GetProperty<long>("behaviour.frame.chat.position.x");
                var posY = _configManager.GetProperty<long>("behaviour.frame.chat.position.y");
                var width = _configManager.GetProperty<long>("behaviour.frame.chat.size.width");
                var height = _configManager.GetProperty<long>("behaviour.frame.chat.size.height");

                var location = new System.Drawing.Point((int)posX, (int)posY);
                var size = new System.Drawing.Size((int)width, (int)height);

                if (!IsFrameOnScreens(new System.Drawing.Rectangle(location, size)))
                { // location and size invalid, fallback to default location
                    logger.Info("Overlay off screen, reseting position and size");
                    ResetFrameToDefaultLocation();
                    return;
                }

                if (!location.Equals(_overlay.Location))
                    _overlay.Location = location;

                if (!size.Equals(_overlay.Size))
                    _overlay.Size = size;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
            }
        }

        private bool IsFrameOnScreens(System.Drawing.Rectangle frameArea, float minCoverage = 0.2f)
        {
            var coveredPixels = 0;
            foreach (var screen in Screen.AllScreens)
            {
                var screenArea = screen.WorkingArea;
                if (screenArea.IntersectsWith(frameArea))
                {
                    var intersection = new System.Drawing.Rectangle(frameArea.Location, frameArea.Size);
                    intersection.Intersect(screenArea);
                    coveredPixels += intersection.Width * intersection.Height;
                }
            }
            var coverage = coveredPixels / (frameArea.Width * frameArea.Height * 1f);
            return coverage >= minCoverage;
        }

        private void ResetFrameToDefaultLocation()
        {
            _configManager.DeleteProperty("behaviour.frame.chat.position");
            _configManager.DeleteProperty("behaviour.frame.chat.size");
            _configManager.DispatchChangeEvents();
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(OnEvent_ConfigManager_PositionChange);

            var chatLocation = _overlay.Location;
            _configManager.SetProperty("behaviour.frame.chat.position.x", chatLocation.X);
            _configManager.SetProperty("behaviour.frame.chat.position.y", chatLocation.Y);

            var chatSize = _overlay.Size;
            _configManager.SetProperty("behaviour.frame.chat.size.width", chatSize.Width);
            _configManager.SetProperty("behaviour.frame.chat.size.height", chatSize.Height);

            _manager.UISynchronizer.RunSync(() => _overlay.Close());

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