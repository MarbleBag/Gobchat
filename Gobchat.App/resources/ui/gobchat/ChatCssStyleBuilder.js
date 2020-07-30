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

'use strict';

var Gobchat = (function (module) {
    module.ChatStyleBuilder = {
        updateStyle: function (styleId) {
            const style = buildStyle(window.gobconfig)
            setDocumentStyle(styleId, style)
        }
    }

    function buildTabClassSelector(id) {
        return `chat-tab-${id}`
    }

    function buildStyle() {
        let result = ""
        result += buildGeneralStyles()
        result += buildTabBasedStyles()
        return result
    }

    function buildGeneralStyles() {
        const style = gobconfig.get("style")

        let result = ""
        result += toCss([".chat-panel-bg"], style.chatbox)

        result += toCss([`.chat-msg-c-base`], style.channel.base)
        Object.entries(style.channel).forEach(e => {
            result += toCss([`.chat-msg-text.chat-msg-c-${e[0]}`], e[1])
        })

        result += toCss([`.chat-msg-seg-link`], style.segment.link)

        result += toCss([".search-msg-marked:not(.search-msg-selected)"], style.chatsearch.marked)
        result += toCss([".search-msg-selected"], style.chatsearch.selected)

        return result
    }

    function buildTabBasedStyles() {
        const tabModels = gobconfig.get("behaviour.chattabs.data")
        const style = gobconfig.get("style")

        let result = ""
        const tabsWithoutTime = getTabsWithoutTime(tabModels).map(selector => `.${selector} .chat-msg-time`)
        result += toCss(tabsWithoutTime, { "display": "none" })

        const tabsWithMentions = getTabsWithMention(tabModels).map(selector => `.${selector} .chat-msg-seg-mention`)
        result += toCss(tabsWithMentions, style.segment.mention)

        const tabsWithRoleplay = getTabsWithRoleplay(tabModels)
        result += toCss(tabsWithRoleplay.map(selector => `.${selector} .chat-msg-seg-say`), style.segment.say, style.channel.say)
        result += toCss(tabsWithRoleplay.map(selector => `.${selector} .chat-msg-seg-emote`), style.segment.emote, style.channel.emote)
        result += toCss(tabsWithRoleplay.map(selector => `.${selector} .chat-msg-seg-ooc`), style.segment.ooc)

        Object.entries(tabModels).forEach(entry => {
            const model = entry[1]
            const tabSelector = buildTabClassSelector(model.id)
            const invisibleChannels = _.difference(Gobchat.ChannelEnumValues, model.channel.visible)

            const selectors = invisibleChannels.map(c => Gobchat.ChannelEnumToKey[c]).map(c => `.${tabSelector} .chat-msg-c-base.chat-msg-c-${c}`)
            result += toCss(selectors, { "display": "none" })
        })

        return result
    }

    function getTabsWithoutTime(tabModels) {
        return Object.entries(tabModels)
            .filter(e => !e[1].formatting.timestamps)
            .map(e => buildTabClassSelector(e[1].id))
    }

    function getTabsWithMention(tabModels) {
        return Object.entries(tabModels)
            .filter(e => e[1].formatting.mentions)
            .map(e => buildTabClassSelector(e[1].id))
    }

    function getTabsWithRangefilter(tabModels) {
        return Object.entries(tabModels)
            .filter(e => e[1].formatting.rangefilter)
            .map(e => buildTabClassSelector(e[1].id))
    }

    function getTabsWithRoleplay(tabModels) {
        return Object.entries(tabModels)
            .filter(e => e[1].formatting.roleplay)
            .map(e => buildTabClassSelector(e[1].id))
    }

    function copy(object) {
        return JSON.parse(JSON.stringify(object))
    }

    function toCss(selectors, object) {
        if (selectors.length === 0)
            return ""

        if (arguments.length > 2) {
            object = copy(object)

            for (let i = 2, done = false; i < arguments.length && !done; ++i) {
                done = true

                const nextObject = arguments[i]
                Object.keys(object).forEach(property => {
                    const currentValue = object[property]
                    if (currentValue === null || currentValue === undefined) {
                        if (property in nextObject)
                            object[property] = nextObject[property]
                        done = false
                    }
                })
            }
        }

        return `${selectors.map(e => `.gob-chatbox ${e}`).join(",")}${objectToCss(object)}\n`
    }

    function objectToCss(object) {
        return `{
            ${
            Object.entries(object)
                .filter(e => e[1] !== null && !e[0].startsWith("$"))
                .map(e => `${e[0]}: ${e[1]};`)
                .join("")
            }
            }`
    }

    function setDocumentStyle(styleId, innerHtml) {
        let styleElement = document.getElementById(styleId)
        if (!styleElement) {
            styleElement = document.createElement("style")
            styleElement.id = styleId
            document.head.appendChild(styleElement)
        }

        styleElement.type = 'text/css';
        styleElement.innerHTML = innerHtml
    }

    return Gobchat
}(Gobchat || {}));