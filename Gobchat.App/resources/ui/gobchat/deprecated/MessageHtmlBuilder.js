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
    const ChannelEnum = Gobchat.ChannelEnum
    const MessageSegmentEnum = Gobchat.MessageSegmentEnum
    const FadeOutLevels = 10

    const FFGroupUnicodes = Object.freeze([
        Gobchat.FFUnicode.GROUP_1, Gobchat.FFUnicode.GROUP_2, Gobchat.FFUnicode.GROUP_3, Gobchat.FFUnicode.GROUP_4,
        Gobchat.FFUnicode.GROUP_5, Gobchat.FFUnicode.GROUP_6, Gobchat.FFUnicode.GROUP_7
    ])

    function applyClass(element, cssClass) {
        if (cssClass) element.classList.add(cssClass)
    }

    function findFirstMatchingGroup(config, message) {
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

    function setGroupId(config, message) {
        message.groupId = findFirstMatchingGroup(config, message)
    }

    function getBodyGroupCSSClass(message) {
        if (message.triggerGroupId)
            return `message-group-body-${message.triggerGroupId}`
        return null
    }

    function getSenderGroupCSSClass(message) {
        if (message.triggerGroupId)
            return `message-group-sender-${message.triggerGroupId}`
        return null
    }

    function getBodyVisibilityCSSClass(config, message) {
        if (!message.source)
            return null

        const visibility = message.source.visibility
        if (visibility >= 100)
            return null

        const ignoreDistance = config.get("behaviour.rangefilter.ignoreMention", false)
        if (ignoreDistance && message.containsMentions)
            return null

        const fadeOutStepSize = (100 / FadeOutLevels)
        const visibilityLevel = (visibility + (fadeOutStepSize - 1)) / fadeOutStepSize >> 0
        return `message-body-fadeout-${visibilityLevel}`
    }

    function getChannelCSSClass(message) {
        switch (message.channel) {
            case ChannelEnum.SAY: return "message-body-say"
            case ChannelEnum.GOBCHATINFO: return "message-body-gobchatinfo"
            case ChannelEnum.GOBCHATERROR: return "message-body-gobchaterror"
            case ChannelEnum.EMOTE: return "message-body-emote"
            case ChannelEnum.ANIMATEDEMOTE: return "message-body-animatedemote"
            case ChannelEnum.TELLSEND: return "message-body-tellsend"
            case ChannelEnum.TELLRECIEVE: return "message-body-tellrecieve"
            case ChannelEnum.GUILD: return "message-body-guild"
            case ChannelEnum.YELL: return "message-body-yell"
            case ChannelEnum.SHOUT: return "message-body-shout"
            case ChannelEnum.PARTY: return "message-body-party"
            case ChannelEnum.ECHO: return "message-body-echo"
            case ChannelEnum.ALLIANCE: return "message-body-alliance"
            case ChannelEnum.LINKSHELL_1: return "message-body-linkshell-1"
            case ChannelEnum.LINKSHELL_2: return "message-body-linkshell-2"
            case ChannelEnum.LINKSHELL_3: return "message-body-linkshell-3"
            case ChannelEnum.LINKSHELL_4: return "message-body-linkshell-4"
            case ChannelEnum.LINKSHELL_5: return "message-body-linkshell-5"
            case ChannelEnum.LINKSHELL_6: return "message-body-linkshell-6"
            case ChannelEnum.LINKSHELL_7: return "message-body-linkshell-7"
            case ChannelEnum.LINKSHELL_8: return "message-body-linkshell-8"
            case ChannelEnum.CROSSWORLDLINKSHELL_1: return "message-body-crossworldlinkshell-1"
            case ChannelEnum.CROSSWORLDLINKSHELL_2: return "message-body-crossworldlinkshell-2"
            case ChannelEnum.CROSSWORLDLINKSHELL_3: return "message-body-crossworldlinkshell-3"
            case ChannelEnum.CROSSWORLDLINKSHELL_4: return "message-body-crossworldlinkshell-4"
            case ChannelEnum.CROSSWORLDLINKSHELL_5: return "message-body-crossworldlinkshell-5"
            case ChannelEnum.CROSSWORLDLINKSHELL_6: return "message-body-crossworldlinkshell-6"
            case ChannelEnum.CROSSWORLDLINKSHELL_7: return "message-body-crossworldlinkshell-7"
            case ChannelEnum.CROSSWORLDLINKSHELL_8: return "message-body-crossworldlinkshell-8"
            case ChannelEnum.ERROR: return "message-body-error"
            case ChannelEnum.NPC_DIALOG: return "message-body-npc-dialog"
            case ChannelEnum.RANDOM: return "message-body-random"
            default: return null
        }
    }

    function getMessageSegmentCSSClass(messageSegmentEnum) {
        switch (messageSegmentEnum) {
            case MessageSegmentEnum.SAY: return "message-segment-say"
            case MessageSegmentEnum.EMOTE: return "message-segment-emote"
            case MessageSegmentEnum.OOC: return "message-segment-ooc"
            case MessageSegmentEnum.MENTION: return "message-segment-mention"
            case MessageSegmentEnum.LINK: return "message-segment-link"
            default: return null
        }
    }

    function formatSource(source) {
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

    function formatSender(message) {
        const sourceName = formatSource(message.source)

        switch (message.channel) {
            case ChannelEnum.GOBCHATINFO:
            case ChannelEnum.GOBCHATERROR: return "[" + sourceName + "]"
            case ChannelEnum.ECHO: return "Echo:"
            case ChannelEnum.EMOTE: return sourceName
            case ChannelEnum.TELLSEND: return "&gt;&gt; " + sourceName + ":"
            case ChannelEnum.TELLRECIEVE: return sourceName + " &gt;&gt;"
            case ChannelEnum.ANIMATEDEMOTE: return null //source is set, but the animation message already contains the source name
            case ChannelEnum.GUILD: return "[FC]&lt;" + sourceName + "&gt;"
            case ChannelEnum.PARTY: return "(" + sourceName + ")"
            case ChannelEnum.ALLIANCE: return "&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_1: return "[LS1]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_2: return "[LS2]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_3: return "[LS3]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_4: return "[LS4]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_5: return "[LS5]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_6: return "[LS6]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_7: return "[LS7]&lt;" + sourceName + "&gt;"
            case ChannelEnum.LINKSHELL_8: return "[LS8]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_1: return "[CWLS1]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_2: return "[CWLS2]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_3: return "[CWLS3]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_4: return "[CWLS4]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_5: return "[CWLS5]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_6: return "[CWLS6]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_7: return "[CWLS7]&lt;" + sourceName + "&gt;"
            case ChannelEnum.CROSSWORLDLINKSHELL_8: return "[CWLS8]&lt;" + sourceName + "&gt;"
            default:
                if (sourceName !== null && sourceName !== undefined) {
                    return sourceName + ":"
                }
        }
        return null
    }

    function formateDate(date) {
        function twoDigits(t) {
            return t < 10 ? '0' + t : t;
        }

        const hours = twoDigits(date.getHours())
        const minutes = twoDigits(date.getMinutes())
        return `${hours}:${minutes}`
    }

    function formatTime(message) {
        const timestamp = message.timestamp
        const date = new Date(timestamp)
        return formateDate(date);
    }

    class MessageHtmlBuilder {
        constructor(config) {
            this._config = config
        }

        buildHtmlElement(message) {
            //setGroupId(this._config, message)

            const bodyElement = document.createElement("div")
            applyClass(bodyElement, "message-body-base")
            applyClass(bodyElement, getBodyGroupCSSClass(message))
            applyClass(bodyElement, getBodyVisibilityCSSClass(this._config, message))

            const timeElement = document.createElement("span")
            bodyElement.appendChild(timeElement)
            applyClass(timeElement, "message-timestamp")
            timeElement.innerHTML = `[${formatTime(message)}] `

            const contentElement = document.createElement("span")
            bodyElement.appendChild(contentElement)
            applyClass(contentElement, getChannelCSSClass(message))

            const senderText = formatSender(message)
            if (senderText !== null) {
                const senderElement = document.createElement("span")
                contentElement.appendChild(senderElement)
                applyClass(senderElement, getSenderGroupCSSClass(message))
                senderElement.innerHTML = senderText

                const spacerElement = document.createElement("span")
                contentElement.appendChild(spacerElement)
                spacerElement.innerHTML = " "
            }

            message.content.forEach((messageSegment) => {
                const segmentElement = document.createElement("span")
                contentElement.appendChild(segmentElement)

                segmentElement.innerHTML = Gobchat.encodeHtmlEntities(messageSegment.text)
                applyClass(segmentElement, "message-segment-base")
                applyClass(segmentElement, getMessageSegmentCSSClass(messageSegment.type))
            })

            return bodyElement
        }
    }
    Gobchat.MessageHtmlBuilder = MessageHtmlBuilder

    return Gobchat
}(Gobchat || {}));