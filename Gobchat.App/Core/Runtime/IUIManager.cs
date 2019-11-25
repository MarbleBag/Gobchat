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

namespace Gobchat.Core.Runtime
{
    public interface IUIManager
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UIElementNotFoundException"></exception>
        /// <exception cref="UIElementTypeException"></exception>
        T GetUIElement<T>(string id);

        bool TryGetUIElement<T>(string id, out T element);

        bool HasUIElement(string id);

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="element"></param>
        /// <exception cref="UIElementIdAlreadyInUseException"></exception>
        void StoreUIElement(string id, object element);

        bool RemoveUIElement(string id);
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
            : base(string.Format(System.Globalization.CultureInfo.InvariantCulture, ERROR_MESSAGE, expected.FullName, actual.FullName))
        {
        }
    }

    public class UIElementIdAlreadyInUseException : UIManagerException
    {
        public UIElementIdAlreadyInUseException(string id) : base(id)
        {
        }
    }
}