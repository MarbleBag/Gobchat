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
using Gobchat.Core.Config;

namespace Gobchat.Module.Chat
{
    public sealed class AppModuleChat : IApplicationModule, IDisposable
    {
        private IndependendBackgroundWorker _backgroundWorker;

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var synchronizer = container.Resolve<IUISynchronizer>();
            var manager = container.Resolve<IUIManager>();
            var config = container.Resolve<IGobchatConfigManager>();

            var work = new SomeoneWhoDoesSomeWork();

            void job(System.Threading.CancellationToken token)
            {
                try
                {
                    var timer = new System.Diagnostics.Stopwatch();

                    while (!token.IsCancellationRequested)
                    {
                        timer.Restart();
                        work.Update();

                        var updateTimer = config.ActiveProfile.GetProperty<long>("behaviour.chatUpdateInterval");

                        timer.Stop();
                        var timeSpend = timer.Elapsed;

                        if (token.IsCancellationRequested)
                            break;

                        int waitTime = (int)Math.Max(0, updateTimer - timeSpend.Milliseconds);
                        if (waitTime > 0)
                            System.Threading.Thread.Sleep(waitTime);
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