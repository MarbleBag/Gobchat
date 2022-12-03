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

var Gobchat = (function (Gobchat) {
    // Start - Constants
    const ChannelEnum = Gobchat.ChannelEnum
    const MessageSegmentEnum = Gobchat.MessageSegmentEnum
    const FFGroupUnicodes = Object.freeze([
        Gobchat.FFUnicode.GROUP_1, Gobchat.FFUnicode.GROUP_2, Gobchat.FFUnicode.GROUP_3, Gobchat.FFUnicode.GROUP_4,
        Gobchat.FFUnicode.GROUP_5, Gobchat.FFUnicode.GROUP_6, Gobchat.FFUnicode.GROUP_7
    ])

    // End - Constants

    function getEntryVisibilityCssClass(message) {
        if (!message.source)
            return null

        const visibility = message.source.visibility
        if (visibility >= 100)
            return null

        const ignoreDistance = window.gobconfig.get("behaviour.rangefilter.ignoreMention", false)
        if (ignoreDistance && message.containsMentions)
            return null

        const fadeOutStepSize = (100 / Gobchat.RangeFilterFadeOutLevels)
        const visibilityLevel = ((visibility + fadeOutStepSize - 1) / fadeOutStepSize) >> 0 //truncat decimals, makes the LSV an integer
        return `gob-chat-entry--fadeout-${visibilityLevel}`
    }

    function getEntryTriggerGroupCssClass(message) {
        if (message.source.triggerGroupId)
            return `gob-chat-entry--trigger-group-${message.source.triggerGroupId}`
        return null
    }

    function getEntryChannelCssClass(message) {
        const channelName = Gobchat.ChannelEnumToKey[message.channel]
        const data = Gobchat.Channels[channelName]
        return `gob-chat-entry--channel-${data.internalName}`
    }

    function getMessageSegmentCssClass(segmentType) {
        switch (segmentType) {
            case MessageSegmentEnum.SAY: return "gob-chat-entry__text__segment--say"
            case MessageSegmentEnum.EMOTE: return "gob-chat-entry__text__segment--emote"
            case MessageSegmentEnum.OOC: return "gob-chat-entry__text__segment--ooc"
            case MessageSegmentEnum.MENTION: return "gob-chat-entry__text__segment--mention"
            case MessageSegmentEnum.LINK: return "gob-chat-entry__text__segment--link"
            default: return null
        }
    }

    function formatSender(builder, message) {
        let sender = getSenderFromSource(message.source)
        sender = packSenderAccordingToChannel(builder, message.channel, sender)
        if (sender === null)
            return null
        return Gobchat.encodeHtmlEntities(sender)
    }

    function getSenderFromSource(source) {
        if (source === null || source.original === null) return null;
        if (source.characterName !== null && source.characterName != undefined) {
            let prefix = ""
            if (source.party >= 0) prefix = prefix + `[${source.party + 1}]`
            if (source.alliance >= 0) prefix = prefix + `[${String.fromCharCode('A' + source.alliance)}]`
            if (source.ffGroup >= 0) prefix = prefix + FFGroupUnicodes[source.ffGroup].char
            return `${prefix}${source.characterName}`
        } else {
            return source.original
        }
    }

    function packSenderAccordingToChannel(builder, channel, sender) {
        switch (channel) {
            case ChannelEnum.GOBCHATINFO:
            case ChannelEnum.GOBCHATERROR: return `[${sender}]`
            case ChannelEnum.ECHO: return "Echo:"
            case ChannelEnum.EMOTE: return sender
            case ChannelEnum.TELLSEND: return `>> ${sender}:`
            case ChannelEnum.TELLRECIEVE: return `${sender} >>`
            case ChannelEnum.ERROR: return null
            case ChannelEnum.ANIMATEDEMOTE: return null //source is set, but the animation message already contains the source name
            case ChannelEnum.PARTY: return `(${sender})`
            case ChannelEnum.ALLIANCE: return `<${sender}>`
            case ChannelEnum.GUILD:
            case ChannelEnum.LINKSHELL_1:
            case ChannelEnum.LINKSHELL_2:
            case ChannelEnum.LINKSHELL_3:
            case ChannelEnum.LINKSHELL_4:
            case ChannelEnum.LINKSHELL_5:
            case ChannelEnum.LINKSHELL_6:
            case ChannelEnum.LINKSHELL_7:
            case ChannelEnum.LINKSHELL_8:
            case ChannelEnum.CROSSWORLDLINKSHELL_1:
            case ChannelEnum.CROSSWORLDLINKSHELL_2:
            case ChannelEnum.CROSSWORLDLINKSHELL_3:
            case ChannelEnum.CROSSWORLDLINKSHELL_4:
            case ChannelEnum.CROSSWORLDLINKSHELL_5:
            case ChannelEnum.CROSSWORLDLINKSHELL_6:
            case ChannelEnum.CROSSWORLDLINKSHELL_7:
            case ChannelEnum.CROSSWORLDLINKSHELL_8:
                return `[${getChannelAbbreviation(builder, channel)}]<${sender}>`
            default:
                if (sender !== null && sender !== undefined) {
                    return sender + ":"
                }
                return null
        }
    }

    function getChannelAbbreviation(builder, channel) {
        if (channel in builder.abbreviationCache)
            return builder.abbreviationCache[channel]
        return channel
    }

    function formatTime(message) {
        const timestamp = message.timestamp
        const date = new Date(timestamp)
        return formateDate(date);
    }

    function formateDate(date) {
        function twoDigits(t) {
            return t < 10 ? '0' + t : t;
        }

        const hours = twoDigits(date.getHours())
        const minutes = twoDigits(date.getMinutes())
        return `${hours}:${minutes}`
    }

    class MessageHtmlBuilder {
        constructor() {
            this.abbreviationCache = {}
        }

        buildHtmlElement(message) {
            const $body = $("<div/>")
                .addClass("gob-chat-entry")
                .addClass(getEntryChannelCssClass(message))           
                .addClass(getEntryTriggerGroupCssClass(message))
                .addClass(getEntryVisibilityCssClass(message))

            $(`<span>[${formatTime(message)}] </span>`)
                .appendTo($body)
                .addClass("gob-chat-entry__time")

            const $content = $("<span/>")
                .appendTo($body)
                .addClass("gob-chat-entry__text")

            const sender = formatSender(this, message)
            if (sender !== null) {
                $(`<span>${sender} </span>`)
                    .appendTo($content)
                    .addClass("gob-chat-entry__sender")
            }

            message.content.forEach((messageSegment) => {
                const innerHtml = Gobchat.encodeHtmlEntities(messageSegment.text)

                $(`<span>${innerHtml}</span>`)
                    .appendTo($content)
                    .addClass("gob-chat-entry__text__segment")
                    .addClass(getMessageSegmentCssClass(messageSegment.type))
            })

            return $body[0]
        }
    }

    Gobchat.MessageHtmlBuilder = MessageHtmlBuilder

    return Gobchat
}(Gobchat || {}));