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
            return _map.Remove(id);
        }

        public void StoreUIElement(string id, object element)
        {
            if (_map.ContainsKey(id))
                throw new UIElementIdAlreadyInUseException(id);
            _map.Add(id, element);
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

    public abstract class UIManagerException : System.Exception
    {
        public UIManagerException(string message) : base(message)
        {
        }

        public UIManagerException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public UIManagerException()
        {
        }
    }

    public class UIElementNotFoundException : UIManagerException
    {
        public UIElementNotFoundException(string elementId) : base(elementId)
        {
        }
    }

    public class UIElementTypeException : UIManagerException
    {
        private const string ERROR_MESSAGE = "Expected type {0} but was {1}";

        public UIElementTypeException(System.Type expected, System.Type actual)
            : base(String.Format(ERROR_MESSAGE, expected.FullName, actual.FullName))
        {
        }
    }

    public class UIElementIdAlreadyInUseException : UIManagerException
    {
        public UIElementIdAlreadyInUseException(String id) : base(id)
        {
        }
    }
}