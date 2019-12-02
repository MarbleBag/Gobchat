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
using Gobchat.UI.Forms;
using Gobchat.Core.Module;
using Gobchat.Core.Runtime;
using Gobchat.Core.Module.Chat;

namespace Gobchat.Module.Chat
{
    public sealed class AppModuleChat : IApplicationModule, IDisposable
    {
        private IndependendBackgroundWorker _backgroundWorker;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var synchronizer = container.Resolve<IUISynchronizer>();
            var manager = container.Resolve<IUIManager>();

            var work = new SomeoneWhoDoesSomeWork();

            void job(System.Threading.CancellationToken token)
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        work.Update();
                        if (token.IsCancellationRequested)
                            break;
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                //TODO may need some logging
                finally
                {
                    work.Dispose();
                }
            }

            var overlayForm = manager.GetUIElement<CefOverlayForm>(AppModuleChatOverlay.OverlayUIId);
            synchronizer.RunSync(() => work.Initialize(container, overlayForm));

            _backgroundWorker = new IndependendBackgroundWorker();
            _backgroundWorker.Start(job);
        }

        public void Dispose(IDIContext container)
        {
            Dispose();
        }

        public void Dispose()
        {
            _backgroundWorker?.Dispose();
            _backgroundWorker = null;
        }
    }
}