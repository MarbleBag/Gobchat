'use strict'

var Gobchat = (function (Gobchat) {
    const ChannelEnum = Gobchat.ChannelEnum
    const MessageSegmentEnum = Gobchat.MessageSegmentEnum

    function applyClass(element, cssClass) {
        if (cssClass) element.classList.add(cssClass)
    }

    //TODO automate this
    function getChannelCssClassForChannel(channelEnum) {
        switch (channelEnum) {
            case ChannelEnum.SAY: return "message-body-say"
            case ChannelEnum.GOBCHAT_INFO: return "message-body-gobchatinfo"
            case ChannelEnum.GOBCHAT_ERROR: return "message-body-gobchaterror"
            case ChannelEnum.EMOTE: return "message-body-emote"
            case ChannelEnum.TELL_SEND: return "message-body-tellsend"
            case ChannelEnum.TELL_RECIEVE: return "message-body-tellrecieve"
            case ChannelEnum.GUILD: return "message-body-guild"
            case ChannelEnum.YELL: return "message-body-yell"
            case ChannelEnum.SHOUT: return "message-body-shout"
            case ChannelEnum.PARTY: return "message-body-party"
            case ChannelEnum.ALLIANCE: return "message-body-alliance"
            case ChannelEnum.LINKSHELL_1: return "message-body-linkshell-1"
            case ChannelEnum.LINKSHELL_2: return "message-body-linkshell-2"
            case ChannelEnum.LINKSHELL_3: return "message-body-linkshell-3"
            case ChannelEnum.LINKSHELL_4: return "message-body-linkshell-4"
            case ChannelEnum.LINKSHELL_5: return "message-body-linkshell-5"
            case ChannelEnum.LINKSHELL_6: return "message-body-linkshell-6"
            case ChannelEnum.LINKSHELL_7: return "message-body-linkshell-7"
            case ChannelEnum.LINKSHELL_8: return "message-body-linkshell-8"
            case ChannelEnum.WORLD_LINKSHELL_1: return "message-body-worldlinkshell-1"
            case ChannelEnum.WORLD_LINKSHELL_2: return "message-body-worldlinkshell-2"
            case ChannelEnum.WORLD_LINKSHELL_3: return "message-body-worldlinkshell-3"
            case ChannelEnum.WORLD_LINKSHELL_4: return "message-body-worldlinkshell-4"
            case ChannelEnum.WORLD_LINKSHELL_5: return "message-body-worldlinkshell-5"
            case ChannelEnum.WORLD_LINKSHELL_6: return "message-body-worldlinkshell-6"
            case ChannelEnum.WORLD_LINKSHELL_7: return "message-body-worldlinkshell-7"
            case ChannelEnum.WORLD_LINKSHELL_8: return "message-body-worldlinkshell-8"
            case ChannelEnum.ERROR: return "message-body-error"
            default: return null
        }
    }

    function findFirstMatchingGroup(config, message) {
        if (message.source === null || message.source.sourceId === null) return null
        switch (message.channel) {
            case ChannelEnum.TELL_SEND: return null
            case ChannelEnum.TELL_RECIEVE: return null
        }

        const groups = config.get("behaviour.groups")
        let resultId = null

        function createSearchTerm() {
            const source = message.source
            let result = source.sourceId
            if (source.playerName !== null) {
                result = source.playerName
            }
            return result.toLowerCase()
        }

        const searchTerm = createSearchTerm()

        $.each(groups.sorting, function (idx, groupId) {
            const group = groups.data[groupId]
            if (!group.active)
                return
            if ("ffgroup" in group) {
                if (message.source.ffGroupId === group.ffgroup) {
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

    function getChannelCssClassForPlayerGroup(groupId) {
        if (groupId != null && groupId != undefined)
            return `message-group-body-${groupId}`
        return null
    }

    function getSenderCssClassForPlayerGroup(groupId) {
        if (groupId != null && groupId != undefined)
            return `message-group-sender-${groupId}`
        return null
    }

    function getCssClassForMessageSegmentType(messageSegmentEnum) {
        switch (messageSegmentEnum) {
            case MessageSegmentEnum.SAY: return "message-segment-say"
            case MessageSegmentEnum.EMOTE: return "message-segment-emote"
            case MessageSegmentEnum.OOC: return "message-segment-ooc"
            case MessageSegmentEnum.MENTION: return "message-segment-mention"
            default: return null
        }
    }

    function getMessageSenderName(messageSource) {
        if (messageSource === null || messageSource.sourceId === null) return null;
        if (messageSource.playerName !== null && messageSource.playerName != undefined) {
            const prefix = messageSource.prefix || ""
            return `${prefix}${messageSource.playerName}`
        } else {
            return messageSource.sourceId
        }
    }

    function getMessageSenderInnerHtml(messageSource, channelEnum) {
        const sourceName = getMessageSenderName(messageSource)

        switch (channelEnum) {
            case ChannelEnum.GOBCHAT_INFO:
            case ChannelEnum.GOBCHAT_ERROR: return "[" + sourceName + "]"
            case ChannelEnum.ECHO: return "Echo:"
            case ChannelEnum.EMOTE: return sourceName
            case ChannelEnum.TELL_SEND: return "&gt;&gt; " + sourceName + ":"
            case ChannelEnum.TELL_RECIEVE: return sourceName + " &gt;&gt;"
            case ChannelEnum.ANIMATED_EMOTE: return null //source is set, but the animation message already contains the source name
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
            case ChannelEnum.WORLD_LINKSHELL_1: return "[CWLS1]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_2: return "[CWLS2]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_3: return "[CWLS3]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_4: return "[CWLS4]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_5: return "[CWLS5]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_6: return "[CWLS6]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_7: return "[CWLS7]&lt;" + sourceName + "&gt;"
            case ChannelEnum.WORLD_LINKSHELL_8: return "[CWLS8]&lt;" + sourceName + "&gt;"
            default:
                if (sourceName != null) {
                    return sourceName + ":"
                }
        }
        return null
    }

    class MessageHtmlBuilder {
        constructor(config) {
            this._config = config
        }

        buildHtmlElement(message) {
            const groupId = findFirstMatchingGroup(this._config, message)

            const chatEntry = document.createElement("div")
            applyClass(chatEntry, "message-body-base")
            applyClass(chatEntry, getChannelCssClassForPlayerGroup(groupId))

            const timeElement = document.createElement("span")
            timeElement.innerHTML = `[${message.timestamp}] `
            applyClass(timeElement, "message-timestamp")
            chatEntry.appendChild(timeElement)

            const messageContainer = document.createElement("span")
            applyClass(messageContainer, getChannelCssClassForChannel(message.channel))
            //applyClass(messageContainer, getChannelCssClassForFFGroup(message))
            chatEntry.appendChild(messageContainer)

            const senderInnerHtml = getMessageSenderInnerHtml(message.source, message.channel)
            if (senderInnerHtml != null) {
                const senderElement = document.createElement("span")
                senderElement.innerHTML = senderInnerHtml
                applyClass(senderElement, getSenderCssClassForPlayerGroup(groupId))
                messageContainer.appendChild(senderElement)

                const spacerElement = document.createElement("span")
                spacerElement.innerHTML = " "
                messageContainer.appendChild(spacerElement)
            }

            message.segments.forEach((segment) => {
                const segmentType = segment.segmentType
                const segmentText = segment.messageText
                const htmlEncoded = Gobchat.encodeHtmlEntities(segmentText)

                const segmentElement = document.createElement("span")
                segmentElement.innerHTML = htmlEncoded
                applyClass(segmentElement, "message-segment-base")
                applyClass(segmentElement, getCssClassForMessageSegmentType(segmentType))
                messageContainer.appendChild(segmentElement)
            })

            return chatEntry
        }
    }
    Gobchat.MessageHtmlBuilder = MessageHtmlBuilder

    return Gobchat
}(Gobchat || {}));