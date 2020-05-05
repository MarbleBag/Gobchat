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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gobchat.Core.Resource
{
    public sealed class ResourceBundle
    {
        private readonly IList<IResourceLocator> _resourceResolver;
        private readonly string _baseName;
        private readonly CultureInfo _fallbackCulture;
        private readonly IResourceLoader _resourceLoader;
        private readonly IDictionary<string, string> _mapping;
        private readonly IList<string> _loadedLanguages;

        public ResourceBundle(IList<IResourceLocator> resourceResolver, string baseName, CultureInfo fallbackCulture)
        {
            _resourceResolver = new List<IResourceLocator>(resourceResolver);
            _baseName = baseName ?? throw new ArgumentNullException(nameof(baseName));
            _fallbackCulture = fallbackCulture;

            _resourceLoader = new HjsonResourceLoader();
            _mapping = new Dictionary<string, string>();
            _loadedLanguages = new List<string>();
        }

        public ResourceBundle(IList<IResourceLocator> resourceResolver, string baseName) : this(resourceResolver, baseName, null)
        {
        }

        public string this[string key]
        {
            get
            {
                if (_mapping.TryGetValue(key.ToUpperInvariant(), out string result))
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
            var data = _resourceLoader.LoadResource(_resourceResolver, fileName);
            foreach (var entry in data)
                _mapping.Add(entry);
        }
    }
}