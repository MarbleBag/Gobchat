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
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Extension
{
    public static class UIExtensions
    {
        private class AsyncInvokeHandler
        {
            private readonly object objLock = new object();
            private readonly Action _action;
            private bool isCompleted;

            private event EventHandler<EventArgs> _onCompletion;

            public event EventHandler<EventArgs> OnCompletion
            {
                add
                {
                    lock (objLock)
                    {
                        if (isCompleted)
                        {
                            value.Invoke(this, new EventArgs());
                        }
                        else
                        {
                            _onCompletion += value;
                        }
                    }
                }
                remove
                {
                    _onCompletion -= value;
                }
            }

            public AsyncInvokeHandler(Action action)
            {
                this._action = action;
            }

            public void Invoke()
            {
                _action.Invoke();
                EndInvoke();
            }

            private void EndInvoke()
            {
                lock (objLock)
                {
                    isCompleted = true;
                    _onCompletion?.Invoke(this, new EventArgs());
                    _onCompletion = null;
                }
            }
        }

        public static TOut InvokeSyncOnUI<TIn, TOut>(this TIn control, Func<TIn, TOut> action) where TIn : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (control.InvokeRequired)
            {
                return (TOut)control.Invoke(action, new object[] { control });
            }
            else if (!control.IsHandleCreated)
            {
                throw new ArgumentException("Handle not created");
            }
            else if (control.IsDisposed)
            {
                throw new ObjectDisposedException(control.Name);
            }
            else
            {
                return action.Invoke(control);
            }
        }

        public static void InvokeAsyncOnUI<T>(this T control, Action<T> action) where T : Control
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (control.InvokeRequired)
            {
                AsyncInvokeHandler handle = new AsyncInvokeHandler(() => action.Invoke(control));
                var asyncResult = control.BeginInvoke((Action)(() => handle.Invoke()));
                handle.OnCompletion += (s, e) => control.EndInvoke(asyncResult);
            }
            else if (!control.IsHandleCreated)
            {
                throw new ArgumentException("Handle not created");
            }
            else if (control.IsDisposed)
            {
                throw new ObjectDisposedException(control.Name);
            }
            else //already on UI thread
            {
                action.Invoke(control);
            }
        }
    }
}