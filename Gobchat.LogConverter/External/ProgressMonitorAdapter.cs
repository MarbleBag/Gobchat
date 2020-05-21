/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

namespace Gobchat.LogConverter
{
    public sealed class ProgressMonitorAdapter : IProgressMonitor, IDisposable
    {
        private readonly CancellationTokenSource _source;
        private readonly LogConverterForm _progressDisplay;

        public ProgressMonitorAdapter(LogConverterForm progressDisplay)
        {
            _source = new CancellationTokenSource();
            _progressDisplay = progressDisplay;
            _progressDisplay.OnCancel += (s, e) => { try { _source.Cancel(); } catch (Exception) { /*ignore*/ } };
        }

        public string StatusText
        {
            get
            {
                return "";// UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.StatusText);
            }
            set
            {
                // UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.StatusText = value);
            }
        }

        public double Progress
        {
            get
            {
                return UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.Progress);
            }
            set
            {
                UIExtensions.InvokeSyncOnUI(_progressDisplay, c => c.Progress = value);
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
            UIExtensions.InvokeAsyncOnUI(_progressDisplay, c => c.AppendLog(log));
        }
    }
}