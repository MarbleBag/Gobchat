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
using System.Globalization;
using System.Linq;

namespace Gobchat.Core.Resource
{
    internal sealed class ResourceBundle
    {
        private readonly IList<IResourceResolver> _resourceResolver;
        private readonly string _baseName;
        private readonly CultureInfo _fallbackCulture;

        private IDictionary<string, string> _mapping;
        private IList<string> _loadedLanguages;

        public ResourceBundle(IList<IResourceResolver> resourceResolver, string baseName, CultureInfo fallbackCulture)
        {
            _resourceResolver = new List<IResourceResolver>(resourceResolver);
            _baseName = baseName ?? throw new ArgumentNullException(nameof(baseName));
            _fallbackCulture = fallbackCulture;
            _mapping = new Dictionary<string, string>();
            _loadedLanguages = new List<string>();
        }

        public ResourceBundle(IList<IResourceResolver> resourceResolver, string baseName) : this(resourceResolver, baseName, null)
        {
        }

        public string this[string key]
        {
            get
            {
                if (_mapping.TryGetValue(key, out string result))
                    return result;
                return null;
            }
        }

        public void Clear()
        {
            _loadedLanguages.Clear();
            _mapping.Clear();
        }

        public void Reload()
        {
            var copy = new List<string>(_loadedLanguages);
            Clear();
            copy.ForEach(extension => LoadExtension(extension));
        }

        public void LoadCulture(CultureInfo cultureInfo)
        {
            if (_fallbackCulture != null)
                LoadCulture(_fallbackCulture.Name);
            LoadCulture(cultureInfo.Name);
        }

        private void LoadCulture(string cultureName)
        {
            var cultureNameSplit = cultureName.Split(new char[] { '-', '_' });
            LoadExtension("");
            var extension = "";
            foreach (var part in cultureNameSplit)
            {
                extension += $"_{part}";
                LoadExtension(extension);
            }
        }

        private void LoadExtension(string extension)
        {
            if (_loadedLanguages.Contains(extension))
                return;
            var fileName = $"{_baseName}{extension}";
            LoadFile(fileName);
            _loadedLanguages.Add(extension);
        }

        private void LoadFile(string fileName)
        {
            foreach (var resolver in _resourceResolver)
            {
                var resourceProvider = resolver.FindResourcesByName(fileName + ".hjson").FirstOrDefault();
                if (resourceProvider == null)
                    continue;

                using (var stream = resourceProvider.OpenStream())
                {
                    var json = Hjson.HjsonValue.Load(stream);
                    var data = json as Hjson.JsonObject;
                    //TODO
                    foreach (var key in data.Keys)
                    {
                        _mapping.Add(key, data[key].ToString());
                    }
                }
            }
        }
    }
}