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

using System;
using System.Collections.Generic;

namespace Gobchat.Core.Resource
{
    public sealed class CachedResourceBundle2 : IResourceBundle
    {
        private readonly IDictionary<string, object> _cache;
        private readonly IResourceBundle bundle;

        public CachedResourceBundle2(IResourceBundle bundle)
        {
            this.bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        }

        public string this[string key]
        {
            get
            {
                key = key.ToUpperInvariant();
                object result;
                if (_cache.TryGetValue(key, out result))
                {
                    return result?.ToString();
                }
                else
                {
                    result = this.bundle[key];
                    _cache.Add(key, result);
                    return result?.ToString();
                }
            }
        }

        public string CurrentLocale => this.bundle.CurrentLocale;

        public void Reload()
        {
            _cache.Clear();
            this.bundle.Reload();
        }

        public void SetLocale(string locale)
        {
            _cache.Clear();
            this.bundle.SetLocale(locale);
        }
    }

    public sealed class CachedResourceBundle : IResourceBundle
    {
        private readonly IResourceLocator _resourceResolver;
        private readonly IResourceLoader _resourceLoader;

        private readonly string _baseName;
        private readonly string _fallbackLocale;

        private readonly IDictionary<string, object> _mapping;
        private readonly List<string> _loadedExtensions;
        private string _locale;

        public CachedResourceBundle(IResourceLocator resourceResolver, IResourceLoader resourceLoader, string baseName, string fallbackLocale)
        {
            _resourceResolver = resourceResolver ?? throw new ArgumentNullException(nameof(resourceResolver));
            _resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));

            _baseName = baseName ?? throw new ArgumentNullException(nameof(baseName));
            _fallbackLocale = fallbackLocale;

            _mapping = new Dictionary<string, object>();
            _loadedExtensions = new List<string>();

            _locale = null;

            Reload();
        }

        public CachedResourceBundle(IResourceLocator resourceResolver, IResourceLoader resourceLoader, string baseName) : this(resourceResolver, resourceLoader, baseName, null)
        {
        }

        public string this[string key]
        {
            get
            {
                if (_mapping.TryGetValue(key.ToLowerInvariant(), out object result))
                    return result != null ? result.ToString() : null;
                return null;
            }
        }

        public string CurrentLocale { get => _locale; }

        public void Clear()
        {
            _mapping.Clear();
            _loadedExtensions.Clear();
            _locale = null;
        }

        public void Reload()
        {
            SetLocale(_locale);
        }

        public void SetLocale(string locale)
        {
            Clear();
            LoadAllLocales(_fallbackLocale);
            LoadAllLocales(locale);
        }

        private void LoadAllLocales(string locale)
        {
            if (locale == null)
                return;

            var elements = locale.Split(new char[] { '-', '_' });
            LoadExtension("");

            var extension = "";
            foreach (var part in elements)
            {
                extension += $"_{part}";
                LoadExtension(extension);
            }

            _locale = locale;
        }

        private void LoadExtension(string extension)
        {
            if (_loadedExtensions.Contains(extension))
                return;

            var fileName = $"{_baseName}{extension}";
            LoadFile(fileName);

            _loadedExtensions.Add(extension);
        }

        private void LoadFile(string fileName)
        {
            var data = _resourceLoader.LoadResource(_resourceResolver, fileName);
            foreach (var entry in data)
                _mapping[entry.Key.ToLowerInvariant()] = entry.Value;
        }
    }
}