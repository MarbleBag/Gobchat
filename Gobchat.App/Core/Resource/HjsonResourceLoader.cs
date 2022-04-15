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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hjson;

namespace Gobchat.Core.Resource
{
    public sealed class HjsonResourceLoader : IResourceLoader
    {
        IResourceCollection IResourceLoader.LoadResource(IResourceLocator locator, string uri)
        {
            var resourceProvider = locator.FindResourcesById(uri + ".hjson").FirstOrDefault();
            var lookup = new Dictionary<string, object>();

            if (resourceProvider != null)
            {
                using (var stream = resourceProvider.OpenStream())
                {
                    var json = Hjson.HjsonValue.Load(stream);
                    var data = json as Hjson.JsonObject;
                    BuildLookup(lookup, data);
                }
            }

            return new StringResourceCollection(lookup);
        }

        private void BuildLookup(Dictionary<string, object> lookup, JsonObject data)
        {
            BuildLookup(lookup, null, data);
        }

        private void BuildLookup(Dictionary<string, object> lookup, string path, JsonObject data)
        {
            foreach (var key in data.Keys)
            {
                var lookupKey = (path == null ? key : $"{path}.{key}").ToUpperInvariant();
                var value = data[key];

                if (value == null)
                {
                    lookup.Add(lookupKey, null);
                    continue;
                }

                switch (value.JsonType)
                {
                    case JsonType.Array:
                        BuildLookup(lookup, lookupKey, value as JsonArray);
                        break;

                    case JsonType.Object:
                        BuildLookup(lookup, lookupKey, value as JsonObject);
                        break;

                    default:
                        lookup.Add(lookupKey, value.ToString());
                        break;
                }
            }
        }

        private void BuildLookup(Dictionary<string, object> lookup, string path, JsonArray data)
        {
            var count = data.Count;
            for (var i = 0; i < count; ++i)
            {
                var lookupKey = (path == null ? i.ToString() : $"{path}.{i}").ToUpperInvariant();
                var value = data[i];

                if (value == null)
                {
                    lookup.Add(lookupKey, null);
                    continue;
                }

                switch (value.JsonType)
                {
                    case JsonType.Array:
                        BuildLookup(lookup, lookupKey, value as JsonArray);
                        break;

                    case JsonType.Object:
                        BuildLookup(lookup, lookupKey, value as JsonObject);
                        break;

                    default:
                        lookup.Add(lookupKey, value.ToString());
                        break;
                }
            }
        }

        private sealed class StringResourceCollection : IResourceCollection
        {
            private readonly Dictionary<string, object> _lookup;

            public StringResourceCollection(Dictionary<string, object> lookup)
            {
                _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return _lookup.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public string[] GetKeys()
            {
                return _lookup.Keys.ToArray();
            }

            public object GetObject(string key)
            {
                if (_lookup.TryGetValue(key, out var value))
                    return value;
                return null;
            }
        }
    }
}