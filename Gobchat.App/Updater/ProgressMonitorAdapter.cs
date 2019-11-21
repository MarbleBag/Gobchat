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
using System.Threading;
using Gobchat.Updater;

namespace Gobchat.Updater
{
    public sealed class ProgressMonitorAdapter : global::Gobchat.Core.Runtime.IProgressMonitor, IDisposable
    {
        private CancellationTokenSource _source;
        private ProgressDisplayForm _progressDisplay;

        public ProgressMonitorAdapter(ProgressDisplayForm progressDisplay)
        {
            _source = new CancellationTokenSource();
            _progressDisplay = progressDisplay;
            _progressDisplay.Cancel += (s, e) => { try { _source.Cancel(); } catch (Exception) { /*ignore*/ } };
        }

        public string StatusText
        {
            get
            {
                return global::Gobchat.UI.Forms.Extension.UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.StatusText);
            }
            set
            {
                global::Gobchat.UI.Forms.Extension.UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.StatusText = value);
            }
        }

        public double Progress
        {
            get
            {
                return global::Gobchat.UI.Forms.Extension.UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.Progress);
            }
            set
            {
                global::Gobchat.UI.Forms.Extension.UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.Progress = value);
            }
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        public CancellationToken GetCancellationToken()
        {
            return _source.Token;
        }

        public void Log(string log)
        {
            global::Gobchat.UI.Forms.Extension.UIExtensions.InvokeAsyncOnUI(_progressDisplay, c => c.AppendLog(log));
        }
    }
}