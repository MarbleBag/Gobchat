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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gobchat.Core.Resource
{
    internal class LocalFolderResourceResolver : IResourceResolver
    {
        internal sealed class LocalResourceProvider : IResourceProvider
        {
            public string ResourceName { get; }

            public LocalResourceProvider(string filepath)
            {
                ResourceName = filepath;
            }

            Stream IResourceProvider.OpenStream()
            {
                return System.IO.File.OpenRead(ResourceName);
            }
        }

        private readonly string _directory;

        public LocalFolderResourceResolver(string directory)
        {
            _directory = directory;
        }

        public IEnumerable<IResourceProvider> FindResourcesByName(string searchPattern)
        {
            var enumerable = System.IO.Directory.EnumerateFiles(_directory, searchPattern, System.IO.SearchOption.AllDirectories);
            return enumerable.Select(file => new LocalResourceProvider(file));
        }
    }
}