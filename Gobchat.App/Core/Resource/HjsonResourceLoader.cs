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
using System.Linq;

namespace Gobchat.Core.Resource
{
    public sealed class HjsonResourceLoader : IResourceLoader
    {
        public IEnumerable<KeyValuePair<string, string>> LoadResource(IEnumerable<IResourceLocator> locators, string fileName)
        {
            var loadedData = new List<KeyValuePair<string, string>>();

            foreach (var locator in locators)
            {
                var resourceProvider = locator.FindResourcesByName(fileName + ".hjson").FirstOrDefault();
                if (resourceProvider == null)
                    continue;

                using (var stream = resourceProvider.OpenStream())
                {
                    var json = Hjson.HjsonValue.Load(stream);
                    var data = json as Hjson.JsonObject;

                    foreach (var key in data.Keys)
                        loadedData.Add(new KeyValuePair<string, string>(key.ToUpperInvariant(), data[key].ToString()));
                }
            }

            return loadedData;
        }
    }
}