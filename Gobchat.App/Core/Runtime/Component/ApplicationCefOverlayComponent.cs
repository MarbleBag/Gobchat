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

namespace Gobchat.Core.Runtime
{
    public sealed class ApplicationCefOverlayComponent : IApplicationComponent, System.IDisposable
    {
        public const string OverlayUIId = "Gobchat.ChatOverlayForm";

        private CefOverlayForm _overlayForm;
        private IUISynchronizer _synchronizer;
        private IUIManager _manager;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _synchronizer = container.Resolve<IUISynchronizer>();
            _manager = container.Resolve<IUIManager>();

#if DEBUG
            var cefFolder = GobchatApplicationContext.ApplicationLocation;
#else
            var cefFolder = System.IO.Path.Combine(GobchatApplicationContext.ApplicationLocation, @"libs\cef");
#endif

            System.IO.Directory.CreateDirectory(cefFolder);
            CEFManager.CefAssemblyLocation = cefFolder;

            _synchronizer.RunSync(() =>
            {
                global::Gobchat.UI.Web.CEFManager.Initialize();
                _overlayForm = new CefOverlayForm();
                _overlayForm.Show();
                _overlayForm.Visible = false;
            });

            _manager.StoreUIElement(OverlayUIId, _overlayForm);
        }

        public void Dispose(IDIContext container)
        {
        }

        public void Dispose()
        {
            _manager.RemoveUIElement(OverlayUIId);
            _synchronizer.RunSync(() =>
            {
                _overlayForm?.Close();
                _overlayForm?.Dispose();
                _overlayForm = null;

                global::Gobchat.UI.Web.CEFManager.Dispose();
            });

            _synchronizer = null;
            _manager = null;
        }
    }
}