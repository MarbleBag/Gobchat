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
using System.Collections.ObjectModel;

namespace Gobchat.Core.Chat
{
    public static class FFXIVUnicodes
    {
        public static readonly FFXIVUnicode Group_1 = new FFXIVUnicode("\u2605");
        public static readonly FFXIVUnicode Group_2 = new FFXIVUnicode("\u25CF");
        public static readonly FFXIVUnicode Group_3 = new FFXIVUnicode("\u25B2");
        public static readonly FFXIVUnicode Group_4 = new FFXIVUnicode("\u2666");
        public static readonly FFXIVUnicode Group_5 = new FFXIVUnicode("\u2665");
        public static readonly FFXIVUnicode Group_6 = new FFXIVUnicode("\u2660");
        public static readonly FFXIVUnicode Group_7 = new FFXIVUnicode("\u2663");

        public static readonly FFXIVUnicode Party_1 = new FFXIVUnicode("\uE090");
        public static readonly FFXIVUnicode Party_2 = new FFXIVUnicode("\uE091");
        public static readonly FFXIVUnicode Party_3 = new FFXIVUnicode("\uE092");
        public static readonly FFXIVUnicode Party_4 = new FFXIVUnicode("\uE093");
        public static readonly FFXIVUnicode Party_5 = new FFXIVUnicode("\uE094");
        public static readonly FFXIVUnicode Party_6 = new FFXIVUnicode("\uE095");
        public static readonly FFXIVUnicode Party_7 = new FFXIVUnicode("\uE096");
        public static readonly FFXIVUnicode Party_8 = new FFXIVUnicode("\uE097");

        public static readonly FFXIVUnicode Raid_A = new FFXIVUnicode("\uE071");
        public static readonly FFXIVUnicode Raid_B = new FFXIVUnicode("\uE072");
        public static readonly FFXIVUnicode Raid_C = new FFXIVUnicode("\uE073");

        public static readonly FFXIVUnicode ItemLink = new FFXIVUnicode("\uE0BB"); // replace that with \u2326

        public static readonly ReadOnlyCollection<FFXIVUnicode> GroupUnicodes = Array.AsReadOnly<FFXIVUnicode>(
            new[] { Group_1, Group_2, Group_3, Group_4, Group_5, Group_6, Group_7 });

        public static readonly ReadOnlyCollection<FFXIVUnicode> PartyUnicodes = Array.AsReadOnly<FFXIVUnicode>(
            new[] { Party_1, Party_2, Party_3, Party_4, Party_5, Party_6, Party_7, Party_8 });

        public static readonly ReadOnlyCollection<FFXIVUnicode> RaidUnicodes = Array.AsReadOnly<FFXIVUnicode>(
            new[] { Raid_A, Raid_B, Raid_C });
    }
}