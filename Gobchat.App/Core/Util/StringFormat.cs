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
using System.Globalization;

namespace Gobchat.Core.Util
{
    public static class StringFormat
    {
        public static String Format(String format, object arg0, object arg1, object arg2)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
        }

        public static String Format(String format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static String Format(String format, object arg0, object arg1)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
        }

        public static String Format(String format, object arg0)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0);
        }
    }
}