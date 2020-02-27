/*******************************************************************************
 * Copyright (C) 2020 MarbleBag
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

namespace Gobchat.Core.Util
{
    public sealed class DelayedCallback : IDisposable
    {
        private System.Threading.Timer _timer;
        private int _counter;
        private TimeSpan _time;
        private Action _callback;

        public DelayedCallback(TimeSpan time, Action callback)
        {
            _timer = new System.Threading.Timer(OnEvent_Timer, null, -1, -1);
            _callback = callback;
            _time = time;
        }

        private void OnEvent_Timer(object state)
        {
            System.Threading.Interlocked.Decrement(ref _counter);
            _callback();
        }

        public void Call()
        {
            System.Threading.Interlocked.Increment(ref _counter);
            _timer.Change(_time, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            _timer.Change(-1, -1);
            _timer.Dispose();
            if (_counter > 0)
            {
                _counter = 0;
                _callback();
            }
        }
    }
}