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

namespace Gobchat.Core.Runtime
{
    public sealed class IndependendBackgroundWorker : System.IDisposable
    {
        public delegate void Job(System.Threading.CancellationToken cancellationToken);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0069:Disposable fields should be disposed", Justification = "Disposed on cancel")]
        private System.Threading.CancellationTokenSource _cancellationTokenSource;

        private object _startupLock = new object();

        public System.Threading.Tasks.Task ActiveTask { get; private set; }

        public bool IsRunning
        {
            get
            {
                lock (_startupLock)
                {
                    var isCompleted = (ActiveTask?.IsCompleted).GetValueOrDefault(true);
                    return !isCompleted;
                }
            }
        }

        private readonly object _innerLock = new object();

        public IndependendBackgroundWorker()
        {
        }

        public void Start(Job job)
        {
            lock (_startupLock)
            {
                Stop(false);
                _cancellationTokenSource = new System.Threading.CancellationTokenSource();
                var token = _cancellationTokenSource.Token;
                ActiveTask = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        job(token);
                    }
                    finally
                    {
                        lock (_innerLock)
                        {
                            _cancellationTokenSource?.Dispose();
                            _cancellationTokenSource = null;
                        }
                    }
                }, token);
            }
        }

        /// <summary>
        /// Signals the current task to stop
        /// </summary>
        /// <param name="waitForIt">If 'true' the method will block until the current task is finished, otherwise returns immediately</param>
        public void Stop(bool waitForIt)
        {
            if (_cancellationTokenSource != null)
                lock (_innerLock)
                    _cancellationTokenSource?.Cancel();

            if (waitForIt) //TODO may not work
                ActiveTask?.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calls <see cref="global::Gobchat.Core.Runtime.IndependendBackgroundWorker.Stop(bool)"/> with 'true' and waits for the task to finish
        /// </summary>
        public void Dispose()
        {
            Stop(true);
            ActiveTask = null;
        }
    }
}