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
using System.Threading.Tasks;

namespace Gobchat
{
    internal class GobchatBackgroundWorker : IDisposable
    {
        public delegate void Job(CancellationToken cancellationToken);

        private CancellationTokenSource _cancellationTokenSource;
        private Task _activeTask;

        public Task ActiveTask { get { return _activeTask; } }

        public GobchatBackgroundWorker()
        {
        }

        public void Start(Job job)
        {
            Stop(false);
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _activeTask = Task.Run(() =>
            {
                var tokenSource = _cancellationTokenSource;
                try
                {
                    job(token);
                }
                finally
                {
                    tokenSource?.Dispose();
                }
            }, token);
        }

        public void Stop(bool waitForIt)
        {
            _cancellationTokenSource?.Cancel();
            if (waitForIt)
            { //TODO may not work
                _activeTask?.Wait();
            }
        }

        public void Dispose()
        {
            Stop(true);
            _cancellationTokenSource = null;
            _activeTask = null;
        }
    }
}