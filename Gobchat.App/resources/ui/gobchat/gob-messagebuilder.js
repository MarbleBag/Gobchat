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

var Gobchat = (function (Gobchat) {
    // Start - Constants
    const ChannelEnum = Gobchat.ChannelEnum
    const MessageSegmentEnum = Gobchat.MessageSegmentEnum
    const FadeOutLevels = 10
    const FFGroupUnicodes = Object.freeze([
        Gobchat.FFUnicode.GROUP_1, Gobchat.FFUnicode.GROUP_2, Gobchat.FFUnicode.GROUP_3, Gobchat.FFUnicode.GROUP_4,
        Gobchat.FFUnicode.GROUP_5, Gobchat.FFUnicode.GROUP_6, Gobchat.FFUnicode.GROUP_7
    ])
    // End - Constants

    function findFirstTriggerGroup(config, message) {
        if (message.source === null || message.source.original === null) return null
        switch (message.channel) {
            case ChannelEnum.TELLSEND:
            case ChannelEnum.TELLRECIEVE:
                return null
        }

        const groups = config.get("behaviour.groups")
        let resultId = null

        function createSearchTerm() {
            const source = message.source
            let result = source.original
            if (source.characterName !== null) {
                result = source.characterName
            }
            return result.toLowerCase()
        }

        const searchTerm = createSearchTerm()

        $.each(groups.sorting, function (idx, groupId) {
            const group = groups.data[groupId]
            if (!group.active)
                return

            if (message.source.ffGroup >= 0 && "ffgroup" in group) {
                if (message.source.ffGroup === group.ffgroup) {
                    resultId = groupId
                    return false
                }
                return
            }

            if (_.includes(group.trigger, searchTerm)) {
                resultId = groupId
                return false
            }
        })

        return resultId
    }

    function setTriggerGroupId(message) {
        message.groupId = findFirstTriggerGroup(window.gobconfig, message)
    }

    function getVisibilityCssClass(message) {
        if (!message.source)
            return null

        const visibility = message.source.visibility
        if (visibility >= 100)
            return null

        const ignoreDistance = window.gobconfig.get("behaviour.rangefilter.ignoreMention", false)
        if (ignoreDistance && message.containsMentions)
            return null

        const fadeOutStepSize = (100 / FadeOutLevels)
        const visibilityLevel = (visibility + (fadeOutStepSize - 1)) / fadeOutStepSize >> 0 //truncat decimals, makes the LSV an integer
        return `gc-msg-b-fadeout-${visibilityLevel}`
    }

    function getTriggerGroupBodyCssClass(message) {
    }

    function getTriggerGroupSenderCssClass(message) {
    }

    function getChannelCssClass(message) {
    }

    function getMessageSegmentCssClass(segmentType) {
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

    function packSenderAccordingToChannel(channel, sender) {
        switch (channel) {
            case ChannelEnum.GOBCHATINFO:
            case ChannelEnum.GOBCHATERROR: return "[" + sender + "]"
            case ChannelEnum.ECHO: return "Echo:"
            case ChannelEnum.EMOTE: return sender
            case ChannelEnum.TELLSEND: return "&gt;&gt; " + sender + ":"
            case ChannelEnum.TELLRECIEVE: return sender + " &gt;&gt;"
            case ChannelEnum.ANIMATEDEMOTE: return null //source is set, but the animation message already contains the source name
            case ChannelEnum.GUILD: return "[FC]&lt;" + sender + "&gt;"
            case ChannelEnum.PARTY: return "(" + sender + ")"
            case ChannelEnum.ALLIANCE: return "&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_1: return "[LS1]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_2: return "[LS2]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_3: return "[LS3]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_4: return "[LS4]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_5: return "[LS5]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_6: return "[LS6]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_7: return "[LS7]&lt;" + sender + "&gt;"
            case ChannelEnum.LINKSHELL_8: return "[LS8]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_1: return "[CWLS1]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_2: return "[CWLS2]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_3: return "[CWLS3]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_4: return "[CWLS4]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_5: return "[CWLS5]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_6: return "[CWLS6]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_7: return "[CWLS7]&lt;" + sender + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_8: return "[CWLS8]&lt;" + v + "&gt;"
            default:
                if (sourceName !== null && sourceName !== undefined) {
                    return sourceName + ":"
                }
        }
    }

    function formatSender(message) {
        let sender = getSenderFromSource(message.source)
        sender = packSenderAccordingToChannel(message.channel, sender)
        if (sender === null)
            return null
        return Gobchat.encodeHtmlEntities(sender)
    }

    class MessageHtmlBuilder {
        constructor(tabId) {
        }

        buildHtmlElement(message) {
            setTriggerGroupId(message)

            const $body = $("<div/>")

            $(`<span>[${formatTime(message)}]</span>`)
                .appendTo($body)
                .addClass("gc-msg-time")
                .addClass(getTriggerGroupBodyCssClass(message))
                .addClass(getVisibilityCssClass(message))

            const $content = $("<span/>")
                .appendTo($body)
                .addClass("gc-msg-b")
                .addClass(getChannelCssClass(message))

            const sender = formatSender(message)
            if (sender !== null) {
                $(`<span>${sender}<span> </span></span>`)
                    .appendTo($content)
                    .addClass(getTriggerGroupSenderCssClass(message))
            }

            message.content.forEach((messageSegment) => {
                const innerHtml = Gobchat.encodeHtmlEntities(messageSegment.text)

                $(`<span>${innerHtml}</span>`)
                    .appendTo($content)
                    .addClass("gc-msg-seg-b")
                    .addClass(getMessageSegmentCssClass(messageSegment.type))
            })

            return $body[0]
        }
    }
})