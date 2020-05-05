/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

using Gobchat.Core.Runtime;
using Gobchat.Core.UI;

namespace Gobchat.Core.Runtime
{
    public interface IUIManager
    {
        IUISynchronizer UISynchronizer { get; }

        T CreateUIElement<T>(string id, System.Func<T> generator);

        string CreateUIElement<T>(System.Func<T> generator);

        void DisposeUIElement(string id);

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
}