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
using System.Collections.Generic;

namespace Gobchat.Core.Util
{
    public class EventHandlerManager<T> : IDisposable where T : Delegate
    {
        public static EventHandlerManager<T> operator +(EventHandlerManager<T> manager, T right)
        {
            manager.AddHandler(right);
            return manager;
        }

        public static EventHandlerManager<T> operator -(EventHandlerManager<T> manager, T right)
        {
            manager.RemoveHandler(right);
            return manager;
        }

        public static EventHandlerManager<T> Add(EventHandlerManager<T> left, T right)
        {
            return left += right;
        }

        public static EventHandlerManager<T> Subtract(EventHandlerManager<T> left, T right)
        {
            return left -= right;
        }

        protected readonly List<T> _handler = new List<T>();
        public Func<object[]> Generator { get; set; }

        public EventHandlerManager()
        {
        }

        public virtual void AddHandler(T handler)
        {
            lock (_handler)
            {
                if (!_handler.Contains(handler))
                    _handler.Add(handler);
            }
        }

        public virtual void RemoveHandler(T handler)
        {
            lock (_handler)
            {
                _handler.Remove(handler);
            }
        }

        protected virtual T[] HandlerCopy()
        {
            lock (_handler)
            {
                return _handler.ToArray();
            }
        }

        protected void Invoke(T[] delegates, Func<object[]> generator)
        {
            var args = generator();
            foreach (var del in delegates)
                del.DynamicInvoke(args);
        }

        public void Invoke(Func<object[]> generator)
        {
            Invoke(HandlerCopy(), generator);
        }

        public void Invoke()
        {
            Invoke(HandlerCopy(), Generator ?? throw new ArgumentNullException(nameof(Generator)));
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _handler.Clear();
                    Generator = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventHandlerManager()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}