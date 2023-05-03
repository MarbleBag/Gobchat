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

namespace Gobchat.Core.Chat
{
    public static class ChatUtil
    {
        public static (string name, string server) SplitCharacterName(string name)
        {
            var sIdx = name.IndexOf("[", StringComparison.InvariantCultureIgnoreCase);
            if (sIdx >= 0)
            {
                var eIdx = name.LastIndexOf("]", StringComparison.InvariantCultureIgnoreCase);
                if (eIdx > sIdx)
                    return (name.Substring(0, sIdx).Trim(), name.Substring(sIdx + 1, eIdx).Trim());
            }
            return (name, null);
        }

        public static string StripServerName(string name)
        {
            var sIdx = name.IndexOf("[", StringComparison.InvariantCultureIgnoreCase);
            if (sIdx >= 0)
                return name.Substring(0, sIdx).Trim();
            return name;
        }
    }
}