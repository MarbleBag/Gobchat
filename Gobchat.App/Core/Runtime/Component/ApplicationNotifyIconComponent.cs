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

using Gobchat.Core.UI;
using Gobchat.UI.Forms;
using System.Windows.Forms;

namespace Gobchat.Core.Runtime
{
    public sealed class EventQueue
    {
        private System.Collections.Concurrent.ConcurrentQueue<int> s = new System.Collections.Concurrent.ConcurrentQueue<int>();
    }

    public sealed class ApplicationNotifyIconComponent : IApplicationComponent, System.IDisposable
    {
        private const string NotifyIconManagerId = "Gobchat.NotifyIconManager";

        private NotifyIconManager _notifyIconManager;
        private IUISynchronizer _synchronizer;
        private IUIManager _manager;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _synchronizer = container.Resolve<IUISynchronizer>();
            _manager = container.Resolve<IUIManager>();

            _synchronizer.RunSync(() =>
            {
                _notifyIconManager = new NotifyIconManager
                {
                    TrayIconVisible = true
                };

                var overlayForm = _manager.GetUIElement<CefOverlayForm>(ApplicationCefOverlayComponent.OverlayUIId);
                overlayForm.VisibleChanged += OnEvent_Form_VisibleChanged;
                _notifyIconManager.SetHideShowText(GetFormHideShowState(overlayForm));

                _notifyIconManager.OnMenuClick += OnEvent_NotifyIconManager_MenuClick;

                _manager.StoreUIElement(NotifyIconManagerId, _notifyIconManager);
            });
        }

        private NotifyIconManager.HideShowState GetFormHideShowState(Form form)
        {
            return form.Visible ? NotifyIconManager.HideShowState.Hide : NotifyIconManager.HideShowState.Show;
        }

        private void OnEvent_Form_VisibleChanged(object sender, System.EventArgs e)
        {
            if (sender is Form form)
                _notifyIconManager.SetHideShowText(GetFormHideShowState(form));
        }

        private void OnEvent_NotifyIconManager_MenuClick(object sender, NotifyIconManager.NotifyIconEventArgs e)
        {
            switch (e.NotifyMenuItem)
            {
                case NotifyIconManager.NotifyMenuItem.CloseApplication:
                    Application.Exit();
                    break;

                case NotifyIconManager.NotifyMenuItem.HideShow:
                    if (_manager.TryGetUIElement<Form>(ApplicationCefOverlayComponent.OverlayUIId, out var form1))
                        form1.Visible = !form1.Visible;
                    break;

                case NotifyIconManager.NotifyMenuItem.ReloadUI:
                    if (_manager.TryGetUIElement<CefOverlayForm>(ApplicationCefOverlayComponent.OverlayUIId, out var form2)) //how to fuck with scope, gj!
                        form2.Reload();
                    break;
            }
        }

        public void Dispose(IDIContext container)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_manager == null)
                return;

            if (_manager.TryGetUIElement<Form>(ApplicationCefOverlayComponent.OverlayUIId, out var form))
                if (!form.IsDisposed)
                    form.VisibleChanged -= OnEvent_Form_VisibleChanged;

            _manager.RemoveUIElement(NotifyIconManagerId);

            _synchronizer.RunSync(() =>
            {
                _notifyIconManager.Dispose();
                _notifyIconManager = null;
            });

            _synchronizer = null;
            _manager = null;
        }
    }
}