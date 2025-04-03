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

'use strict'

var Gobchat = (function (Gobchat) {
    //Channel in which a player can write something

    Gobchat.PlayerChannel = Object.freeze([
        Gobchat.ChannelEnum.SAY, Gobchat.ChannelEnum.EMOTE, Gobchat.ChannelEnum.YELL, Gobchat.ChannelEnum.SHOUT, Gobchat.ChannelEnum.TELL_SEND, Gobchat.ChannelEnum.TELL_RECIEVE, Gobchat.ChannelEnum.PARTY, Gobchat.ChannelEnum.GUILD, Gobchat.ChannelEnum.ALLIANCE,
        Gobchat.ChannelEnum.ANIMATED_EMOTE,
        Gobchat.ChannelEnum.WORLD_LINKSHELL_1, Gobchat.ChannelEnum.WORLD_LINKSHELL_2, Gobchat.ChannelEnum.WORLD_LINKSHELL_3, Gobchat.ChannelEnum.WORLD_LINKSHELL_4,
        Gobchat.ChannelEnum.WORLD_LINKSHELL_5, Gobchat.ChannelEnum.WORLD_LINKSHELL_6, Gobchat.ChannelEnum.WORLD_LINKSHELL_7, Gobchat.ChannelEnum.WORLD_LINKSHELL_8,
        Gobchat.ChannelEnum.LINKSHELL_1, Gobchat.ChannelEnum.LINKSHELL_2, Gobchat.ChannelEnum.LINKSHELL_3, Gobchat.ChannelEnum.LINKSHELL_4,
        Gobchat.ChannelEnum.LINKSHELL_5, Gobchat.ChannelEnum.LINKSHELL_6, Gobchat.ChannelEnum.LINKSHELL_7, Gobchat.ChannelEnum.LINKSHELL_8,
    ])

    Gobchat.ChannelEnumValues = Object.freeze(
        Object.entries(Gobchat.ChannelEnum).map(e => e[1])
    )

    function buildChannelEnumToKey() {
        const result = {}
        Object.entries(Gobchat.Channels).forEach(e => {
            result[e[1].chatChannel] = e[0]
        })
        return result
    }

    Gobchat.ChannelEnumToKey = Object.freeze(buildChannelEnumToKey())

    Gobchat.RangeFilterFadeOutLevels = 10

    return Gobchat
}(Gobchat || {}));