/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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
    public interface IMenuCollection<T> : IEnumerable<IMenuGroup<T>>
    {
        IList<IMenuGroup<T>> Groups { get; }

        IMenuGroup<T> GetGroup(string id);

        bool TryGetGroup(string id, out IMenuGroup<T> group);

        bool HasGroup(string id);

        //  IMenuGroup<T> MakeGroup(string id);

        //  IMenuGroup<T> GetOrMakeGroup(string id);

        IMenuGroup<T> RemoveGroup(string id);

        void Clear();
    }
}