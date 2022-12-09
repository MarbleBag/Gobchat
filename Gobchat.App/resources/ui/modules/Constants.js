/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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

export const FFUnicode = Object.freeze({
    GROUP_1: { char: '\u2605', value: 0x2605 },
    GROUP_2: { char: '\u25CF', value: 0x25CF },
    GROUP_3: { char: '\u25B2', value: 0x25B2 },
    GROUP_4: { char: '\u2666', value: 0x2666 },
    GROUP_5: { char: '\u2665', value: 0x2665 },
    GROUP_6: { char: '\u2660', value: 0x2660 },
    GROUP_7: { char: '\u2663', value: 0x2663 },
    PARTY_1: { char: '\uE090', value: 0xE090 },
    PARTY_2: { char: '\uE091', value: 0xE091 },
    PARTY_3: { char: '\uE092', value: 0xE092 },
    PARTY_4: { char: '\uE093', value: 0xE093 },
    PARTY_5: { char: '\uE094', value: 0xE094 },
    PARTY_6: { char: '\uE095', value: 0xE095 },
    PARTY_7: { char: '\uE096', value: 0xE096 },
    PARTY_8: { char: '\uE097', value: 0xE097 },
    RAID_A: { char: '\uE071', value: 0xE073 },
    RAID_B: { char: '\uE072', value: 0xE072 },
    RAID_C: { char: '\uE073', value: 0xE073 },

    ITEM_LINK: { char: '\uE0BB', value: 0xE0BB }, // replace that with \u2326
})

Object.keys(FFUnicode).forEach((e) => {
    const tuple = FFUnicode[e]
    tuple.value = tuple.char.codePointAt(0)
})

export const PlayerChannel = Object.freeze([
    Gobchat.ChannelEnum.SAY, Gobchat.ChannelEnum.EMOTE, Gobchat.ChannelEnum.YELL, Gobchat.ChannelEnum.SHOUT, Gobchat.ChannelEnum.TELL_SEND, Gobchat.ChannelEnum.TELL_RECIEVE, Gobchat.ChannelEnum.PARTY, Gobchat.ChannelEnum.GUILD, Gobchat.ChannelEnum.ALLIANCE,
    Gobchat.ChannelEnum.ANIMATED_EMOTE,
    Gobchat.ChannelEnum.WORLD_LINKSHELL_1, Gobchat.ChannelEnum.WORLD_LINKSHELL_2, Gobchat.ChannelEnum.WORLD_LINKSHELL_3, Gobchat.ChannelEnum.WORLD_LINKSHELL_4,
    Gobchat.ChannelEnum.WORLD_LINKSHELL_5, Gobchat.ChannelEnum.WORLD_LINKSHELL_6, Gobchat.ChannelEnum.WORLD_LINKSHELL_7, Gobchat.ChannelEnum.WORLD_LINKSHELL_8,
    Gobchat.ChannelEnum.LINKSHELL_1, Gobchat.ChannelEnum.LINKSHELL_2, Gobchat.ChannelEnum.LINKSHELL_3, Gobchat.ChannelEnum.LINKSHELL_4,
    Gobchat.ChannelEnum.LINKSHELL_5, Gobchat.ChannelEnum.LINKSHELL_6, Gobchat.ChannelEnum.LINKSHELL_7, Gobchat.ChannelEnum.LINKSHELL_8,
])

export const ChannelEnumValues = Object.freeze(
    Object.entries(Gobchat.ChannelEnum).map(e => e[1])
)

export const ChannelEnumToKey = (() => {
    const result = {}
    Object.entries(Gobchat.Channels).forEach(e => {
        result[e[1].chatChannel] = e[0]
    })
    return Object.freeze(result)
}
)()

export const RangeFilterFadeOutLevels = 10