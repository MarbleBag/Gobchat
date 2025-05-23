﻿/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

namespace Gobchat.Core.Resource
{
    public interface IResourceLocator
    {
        IEnumerable<IResourceProvider> FindResourcesById(string id);
    }

    public interface IResourceProvider
    {
        string ResourceName { get; }

        System.IO.Stream OpenStream();
    }

    public interface IResourceLoader
    {
        IResourceCollection LoadResource(IResourceLocator locator, string id);
    }

    public interface IResourceCollection : IEnumerable<KeyValuePair<string, object>>
    {
        object GetObject(string key);

        string[] GetKeys();
    }
}