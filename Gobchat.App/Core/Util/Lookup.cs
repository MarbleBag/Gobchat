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

using System.Collections.Generic;

namespace Gobchat.Core.Util
{
    internal class Lookup<K, V>
    {
        private readonly Lookup<K, V> _parent;
        private readonly IDictionary<K, V> _mapping;

        public Lookup(IDictionary<K, V> mapping)
        {
            _mapping = mapping;
        }

        public Lookup(IDictionary<K, V> mapping, Lookup<K, V> fallback)
        {
            _mapping = mapping;
            _parent = fallback;
        }

        public V this[K key]
        {
            get
            {
                if (_mapping.TryGetValue(key, out V result))
                    return result;
                if (_parent != null)
                    return _parent[key];
                return default;
            }
        }
    }
}