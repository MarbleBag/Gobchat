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

using Gobchat.Core.Resource;
using System;
using System.Globalization;

namespace Gobchat.Module.Language
{
    public interface ILocaleManager
    {
        event EventHandler<LocaleEventArgs> OnLocaleChange;

        CultureInfo ActiveCulture { get; }

        CultureInfo DefaultCulture { get; }

        string GetLocale(CultureInfo cultureInfo);

        IResourceBundle GetResourceBundle(string filename);
    }

    public sealed class LocaleEventArgs : EventArgs
    {
        public CultureInfo Locale { get; }

        public LocaleEventArgs(CultureInfo locale)
        {
            Locale = locale ?? throw new ArgumentNullException(nameof(locale));
        }
    }
}