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

namespace Gobchat.LogConverter
{
    public enum FFXIVChatChannel : int
    {
        NONE = 0x0000,
        WORLD = 0x0003,
        SAY = 0x000a,
        EMOTE = 0x001c,
        YELL = 0x001e,
        SHOUT = 0x000b,
        TELL_SEND = 0x000c,
        TELL_RECIEVE = 0x000d,
        PARTY = 0x000e,
        GUILD = 0x0018,
        ALLIANCE = 0x000f,

        NPC_TALK = 0x0044,
        NPC_DIALOGUE = 0x003d,
        ANIMATED_EMOTE = 0x001d,
        PARTYFINDER = 0x0048,
        ECHO = 0x0038,
        ERROR = 0x003c,

        RANDOM_SELF = 0x084A,
        RANDOM_PARTY = 0x104A,
        RANDOM_OTHER = 0x204A,

        TELEPORT = 0x001f,
        SYSTEM = 0x0039,

        WORLD_LINKSHELL_1 = 0x0025,
        WORLD_LINKSHELL_2 = 0x0065,
        WORLD_LINKSHELL_3 = 0x0066,
        WORLD_LINKSHELL_4 = 0x0067,
        WORLD_LINKSHELL_5 = 0x0068,
        WORLD_LINKSHELL_6 = 0x0069,
        WORLD_LINKSHELL_7 = 0x006A,
        WORLD_LINKSHELL_8 = 0x006B,

        LINKSHELL_1 = 0x0010,
        LINKSHELL_2 = 0x0011,
        LINKSHELL_3 = 0x0012,
        LINKSHELL_4 = 0x0013,
        LINKSHELL_5 = 0x0014,
        LINKSHELL_6 = 0x0015,
        LINKSHELL_7 = 0x0016,
        LINKSHELL_8 = 0x0017,

        LOGOUT = 0x2246
    }
}