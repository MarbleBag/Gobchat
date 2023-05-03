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

using System.Collections.Generic;

namespace Gobchat.Core.UI
{
    public interface IMenuGroup<T>
    {
        string GroupId { get; }

        IList<IMenu<T>> Menues { get; }

        void AddMenu(IMenu<T> menu);

        IMenu<T> GetMenu(string id);

        bool HasMenu(string id);

        bool TryGetMenu(string id, out IMenu<T> menu);

        IMenu<T> RemoveMenu(string id);

        void Clear();
    }
}