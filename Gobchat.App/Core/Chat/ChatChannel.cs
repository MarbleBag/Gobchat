/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

namespace Gobchat.Core.Chat
{
    //Do never ever change any name. The names are used to resolve values read from any config file.

    public enum ChatChannel : int
    {
        None,
        Say,
        Emote,
        Yell,
        Shout,
        TellSend,
        TellRecieve,
        Party,
        Guild,
        Alliance,

        NPC_Dialog,
        AnimatedEmote,
        PartyFinder,
        Echo,
        Error,

        Random,

        Teleport,
        System,

        CrossWorldLinkShell_1,
        CrossWorldLinkShell_2,
        CrossWorldLinkShell_3,
        CrossWorldLinkShell_4,
        CrossWorldLinkShell_5,
        CrossWorldLinkShell_6,
        CrossWorldLinkShell_7,
        CrossWorldLinkShell_8,

        LinkShell_1,
        LinkShell_2,
        LinkShell_3,
        LinkShell_4,
        LinkShell_5,
        LinkShell_6,
        LinkShell_7,
        LinkShell_8,

        GobchatInfo,
        GobchatError,
    }
}