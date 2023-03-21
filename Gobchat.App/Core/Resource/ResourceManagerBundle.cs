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

namespace Gobchat.Core.Resource
{
    public sealed class ResourceManagerBundle : IResourceBundle
    {
        private global::System.Globalization.CultureInfo _resourceCulture;
        private readonly global::System.Resources.ResourceManager _resourceManager;

        public ResourceManagerBundle(global::System.Resources.ResourceManager resourceManager)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        public string this[string key] =>
            _resourceManager.GetString(key, _resourceCulture);

        public string CurrentLocale => _resourceCulture.Name;

        public void Clear()
        {
        }

        public void Reload()
        {
        }

        public void SetLocale(string locale)
        {
            _resourceCulture = new System.Globalization.CultureInfo(locale);
        }
    }
}