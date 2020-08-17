/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

jQuery(function ($) {
    function runTest() {
        function fireMessageEvent(e) {
            const msg = { channel: e.channel || Gobchat.ChannelEnum.PARTY, source: e.source || "Browser", message: e.message || "Empty" }
            GobchatAPI.sendChatMessage(msg.channel, msg.source, msg.message)
        }

        const textMessages = [
            { channel: Gobchat.ChannelEnum.PARTY, message: '"Say" ((ooc)) *emote* mention' }, //base test
            { channel: Gobchat.ChannelEnum.PARTY, message: '"Mention contained in say"' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '((Mention contained in ooc))' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '*Mention contained in emote*' },
            { channel: Gobchat.ChannelEnum.SAY, message: '"Some speech", followed by an emote, "and some more speech."' },
            { channel: Gobchat.ChannelEnum.SAY, message: 'Display of some special html characters: < > | <<  ≫ « »' },
            { channel: Gobchat.ChannelEnum.EMOTE, message: 'Display of some special ffxiv characters: \uE0BB' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '"Say that\'s divided by some *emote characters* but should still be displayed correctly."' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '*An emote mixed with "for some important stuff" but contained in an emote*' },
            { channel: Gobchat.ChannelEnum.PARTY, message: 'Ooc mixed inbetween "some ((ooc)) say" *or ((ooc)) emote*' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '"Mixed stuff *inbetween but the scope enclosing character* is missing here' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '»Say mixed with different types«, of symbols, »to see if they work«' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '«Say mixed with different types», of symbols, «to see if they work»' },
            { channel: Gobchat.ChannelEnum.PARTY, message: 'Incomplete tokens(' },
            { channel: Gobchat.ChannelEnum.PARTY, message: 'Incomplete tokens)' },
            { channel: Gobchat.ChannelEnum.PARTY, message: '((Incomplete tokens)' },
        ]

        textMessages.forEach(e => fireMessageEvent(e))

        Object.entries(Gobchat.Channels)
            .filter(e => e[1].relevant)
            .map(e => { return { message: `Channel test: ${e[0]}`, channel: e[1].chatChannel, source: "Test Code" } })
            .forEach(e => fireMessageEvent(e))

        const fillMe = Array.from({ length: 20 }, (v, k) => `"Just some text to fill the chat, this is line <${k + 1}>"`).map(e => { return { message: e, source: "Di'e I'of-oOdin", channel: Gobchat.ChannelEnum.SAY } })
        fillMe.forEach(e => fireMessageEvent(e))
    }

    window.setTimeout(runTest, 10000);
})
