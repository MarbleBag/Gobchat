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

using System;

namespace Gobchat.Core.Util
{
    public sealed class EventHandlerConfigManager<T> : EventHandlerManager<T> where T : Delegate
    {
        private bool _invoked;

        public bool InvokeOnce { get; set; }
        public bool DeleteAfterInvokation { get; set; }
        public bool InvokeAfterRegistration { get; set; }

        public EventHandlerConfigManager()
        {
        }

        public void ResetInvokeOnce()
        {
            _invoked = false;
        }

        public override void AddHandler(T handler)
        {
            var invokeImmediately = InvokeAfterRegistration;
            var storeHandler = false;

            lock (_handler)
            {
                if (InvokeOnce && _invoked)
                    invokeImmediately = true;
                else
                    storeHandler = true;

                if (invokeImmediately && DeleteAfterInvokation)
                    storeHandler = false;

                if (storeHandler)
                    base.AddHandler(handler);
            }

            if (invokeImmediately)
            {
                var data = Generator();
                handler.DynamicInvoke(data);
            }
        }

        protected override T[] HandlerCopy()
        {
            lock (_handler)
            {
                if (InvokeOnce && _invoked)
                    return Array.Empty<T>();

                var handler = base.HandlerCopy();
                if (DeleteAfterInvokation)
                    _handler.Clear();

                _invoked = true;
                return handler;
            }
        }
    }
}