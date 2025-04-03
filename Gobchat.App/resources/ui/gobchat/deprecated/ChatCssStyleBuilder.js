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

'use strict';

var Gobchat = (function (module) {
    module.ChatStyleBuilder = {
        updateStyle: function (styleId) {
            const style = buildStyle(window.gobconfig)
            setDocumentStyle(styleId, style)
        }
    }

    function buildTabClassSelector(id) {
        return `.chat-tab-${id}`
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
        // chat bachground
        result += toCss([".gob-chatbox__panel"], style.chatbox)

        // channel
        result += toCss([`.gob-chat-entry`], style.channel.base)
        Object.entries(Gobchat.Channels).forEach(e => {
            const channelName = e[1].internalName
            if (channelName in style.channel)
                result += toCss([`.gob-chat-entry--channel-${channelName} .gob-chat-entry__text`], style.channel[channelName])
        })

        result += toCss([`.gob-chat-entry__text__segment--link`], style.segment.link)

        // trigger groups
        {
            const data = gobconfig.get("behaviour.groups.data")
            Object.entries(data).forEach(e => {
                result += toCss([`.gob-chat-entry--trigger-group-${e[1].id}`], e[1].style.body)
                result += toCss([`.gob-chat-entry--trigger-group-${e[1].id} .gob-chat-entry__sender`], e[1].style.header)
            })
        }

        // search
        result += toCss([".search-msg-marked:not(.search-msg-selected)"], style.chatsearch.marked)
        result += toCss([".search-msg-selected"], style.chatsearch.selected)

        return result
    }

    function buildTabBasedStyles() {
        const tabModels = gobconfig.get("behaviour.chattabs.data")
        const style = gobconfig.get("style")

        let result = ""

        // timestamp
        const tabsWithoutTime = getTabsWithoutTime(tabModels).map(selector => `${selector} .gob-chat-entry__time`)
        result += toCss(tabsWithoutTime, { "display": "none" })

        // roleplay
        const tabsWithMentions = getTabsWithMention(tabModels).map(selector => `${selector} .gob-chat-entry__text__segment--mention`)
        result += toCss(tabsWithMentions, style.segment.mention)

        const tabsWithRoleplay = getTabsWithRoleplay(tabModels)
        result += toCss(tabsWithRoleplay.map(selector => `${selector} .gob-chat-entry__text__segment--say`), style.segment.say, style.channel.say)
        result += toCss(tabsWithRoleplay.map(selector => `${selector} .gob-chat-entry__text__segment--emote`), style.segment.emote, style.channel.emote)
        result += toCss(tabsWithRoleplay.map(selector => `${selector} .gob-chat-entry__text__segment--ooc`), style.segment.ooc)

        // range filter
        {
            const tabsWithRangeFilter = getTabsWithRangefilter(tabModels)
            result += toCss(tabsWithRangeFilter.map(selector => `${selector} .gob-chat-entry--fadeout-0`), { "display": "none" })

            const data = gobconfig.get("behaviour.rangefilter")
            const startopacity = data.startopacity / 100.0
            const endopacity = data.endopacity / 100.0
            const opacityByLevel = (startopacity - endopacity) / (Gobchat.RangeFilterFadeOutLevels - 1)

            //map levels [1, Gobchat.RangeFilterFadeOutLevels] to opacity [endopacity, startopacity]
            //the higher the level, the higher the opacity
            for (let i = 1; i <= Gobchat.RangeFilterFadeOutLevels; ++i) {
                result += toCss(
                    tabsWithRangeFilter.map(selector => `${selector} .gob-chat-entry--fadeout-${i}`),
                    { "opacity": `${(i - 1) * opacityByLevel + endopacity}` }
                )
            }
        }

        // channel visibility
        {
            function getChannelName(channelEnum) {
                const key = Gobchat.ChannelEnumToKey[channelEnum]
                return Gobchat.Channels[key].internalName
            }

            Object.entries(tabModels).forEach(entry => {
                const model = entry[1]
                const tabSelector = buildTabClassSelector(model.id)
                const invisibleChannels = _.difference(Gobchat.ChannelEnumValues, model.channel.visible)

                const selectors = invisibleChannels
                    .map(c => getChannelName(c))
                    .map(c => `${tabSelector} .gob-chat-entry--channel-${c}`)

                result += toCss(selectors, { "display": "none" })
            })
        }

        //effects
        {
            function getCssObjectForType(type) {
                switch (type) {
                    case 0: return null
                    case 1: // highlight
                        return {
                            "background": "rgba(245, 160, 0, 0.2)"
                        }
                    case 2: // blink
                        return {
                            "animation": "subtile-blinker 2s linear infinite"
                        }
                    case 3: // green text
                        return {
                            "color": "#73cf4b"
                        }
                    default: return null
                }
            }

            const effects = gobconfig.get("behaviour.chattabs.effect")
            result += toCss(".gob-chatbox__chat-navi__button.tab-effect-mention", getCssObjectForType(effects.mention))
            result += toCss(".gob-chatbox__chat-navi__button.tab-effect-message", getCssObjectForType(effects.message))
        }

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

    // turns an object into a css string for the given selectors. Any additional parameter passed via args are localy merged into the first object
    function toCss(selectors, object) {
        selectors = [].concat(selectors)
        if (selectors.length === 0 || !object)
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

        const allSelectors = selectors.map(e => `.gob-chatbox ${e}`).join(",")
        return `${allSelectors}${objectToCss(object)}\n`
    }

    // turns an object into a css string. The object properties and their values become css attributes
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