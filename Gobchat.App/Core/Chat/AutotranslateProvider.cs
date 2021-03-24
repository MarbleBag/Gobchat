/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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

namespace Gobchat.Core.Chat
{
    public sealed class AutotranslateProvider : IAutotranslateProvider
    {
        private readonly Resource.IResourceBundle _resourceBundle;

        public AutotranslateProvider(Resource.IResourceBundle resourceBundle)
        {
            _resourceBundle = resourceBundle ?? throw new ArgumentNullException(nameof(resourceBundle));
        }

        public string GetTranslationFor(string key)
        {
            return _resourceBundle[key];
        }

        public void SetLocale(string locale)
        {
            _resourceBundle.SetLocale(locale);
        }
    }
}