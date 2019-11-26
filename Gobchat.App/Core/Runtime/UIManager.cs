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
using System.Collections.Generic;

namespace Gobchat.Core.Runtime
{
    public sealed class UIManager : IUIManager
    {
        private readonly Dictionary<string, object> _map = new Dictionary<string, object>();
        private readonly object _lock = new object();

        public IUISynchronizer UISynchronizer { get; }

        public UIManager(IUISynchronizer synchronizer)
        {
            UISynchronizer = synchronizer ?? throw new ArgumentNullException(nameof(synchronizer));
        }

        private void RegisterUIElement(string id, object element)
        {
            _map.Add(id, element);
            //TODO
        }

        private bool UnregisterUIElement(string id)
        {
            //TODO
            return _map.Remove(id);
        }

        public T CreateUIElement<T>(string id, Func<T> generator)
        {
            if (HasUIElement(id))
                throw new UIElementIdAlreadyInUseException(id);
            T result = default;
            UISynchronizer.RunSync(() => result = generator());
            if (result == null)
                return result;

            lock (_lock)
            {
                if (HasUIElement(id))
                {
                    if (result is IDisposable disposable)
                        UISynchronizer.RunSync(() => disposable.Dispose());
                    throw new UIElementIdAlreadyInUseException(id);
                }

                RegisterUIElement(id, result);
                return result;
            }
        }

        public void DisposeUIElement(string id)
        {
            UISynchronizer.RunSync(() =>
            {
                if (TryGetUIElement<object>(id, out var element))
                {
                    RemoveUIElement(id);
                    if (element is IDisposable disposable)
                        disposable.Dispose();
                }
            });
        }

        public T GetUIElement<T>(string id)
        {
            if (_map.TryGetValue(id, out object value))
            {
                var type = typeof(T);
                if (type.IsAssignableFrom(value.GetType()))
                    return (T)value;
                else
                    throw new UIElementTypeException(type, value.GetType());
            }
            else
            {
                throw new UIElementNotFoundException(id);
            }
        }

        public bool HasUIElement(string id)
        {
            return _map.ContainsKey(id);
        }

        public bool RemoveUIElement(string id)
        {
            return UnregisterUIElement(id);
        }

        public void StoreUIElement(string id, object element)
        {
            lock (_lock)
            {
                if (HasUIElement(id))
                    throw new UIElementIdAlreadyInUseException(id);
                RegisterUIElement(id, element);
            }
        }

        public bool TryGetUIElement<T>(string id, out T element)
        {
            element = default;

            if (_map.TryGetValue(id, out object value))
            {
                var type = typeof(T);
                if (!type.IsAssignableFrom(value.GetType()))
                    return false;

                element = (T)value;
                return true;
            }

            return false;
        }
    }
}