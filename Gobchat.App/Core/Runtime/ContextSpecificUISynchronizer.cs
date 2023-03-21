/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

namespace Gobchat.Core.Runtime
{
    internal sealed class ContextSpecificUISynchronizer : IUISynchronizer
    {
        private readonly SynchronizationContext _context;

        public ContextSpecificUISynchronizer(SynchronizationContext uiContext)
        {
            this._context = uiContext;
        }

        public void RunAsync(Action action) => RunAsync(action, null);

        public void RunAsync(Action action, IUnhandledExceptionHandler handler)
        {
            if (handler != null)
                _context.Post((s) => { try { action.Invoke(); } catch (Exception e) { handler.Handle(e); } }, null);
            else
                _context.Post((s) => action.Invoke(), null);
        }

        public void RunSync(Action action)
        {
            _context.Send((s) =>
            {
                action.Invoke();
            }, null);
        }

        public TOut RunSync<TOut>(Func<TOut> action)
        {
            TOut result = default;
            _context.Send((s) =>
            {
                result = action.Invoke();
            }, null);
            return result;
        }
    }
}