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
    function getAttributes(source, fallback) {
        function getAttributeValue(key, source, fallback) {
            if (!fallback) return key in source ? source[key] : null
            if (key in source) {
                var val = source[key]
                if (val == null && key in fallback) {
                    val = fallback[key]
                }
                return val
            } else if (key in fallback) {
                return fallback[key]
            }
            return null
        }

        function buildAttributeList(keys, source, fallback) {
            const result = []
            for (let key of keys) {
                const data = getAttributeValue(key, source, fallback)
                if (data != null) {
                    result.push([key, data])
                }
            }
            return result
        }

        return buildAttributeList(Object.keys(source), source, fallback)
    }

    function buildCssStyleElement(cssSelector, classAttributes, classAttributesFallback) {
        const attributes = getAttributes(classAttributes, classAttributesFallback)
            .map((attribute) => { return `${attribute[0]}: ${attribute[1]};` })

        const style = `
				${cssSelector}{
					${attributes.join("")}
				}
			`
        return style
    }

    function generateCssStyleElement(cssResults, styleSelector, styleData, styleFallbackData) {
        const css = buildCssStyleElement(styleSelector, styleData, styleFallbackData)
        cssResults.push(css)
    }

    function generateCssStyleElementsFromMap(cssResults, cssStyleSelectorTemplate, styleData, styleFallbackData) {
        for (let styleKey in styleData) {
            const cssAttributes = styleData[styleKey]
            const fallbackAttributes = (styleFallbackData && styleKey in styleFallbackData) ? styleFallbackData[styleKey] : undefined
            const cssStyleSelector = Gobchat.formatString(cssStyleSelectorTemplate, { styleKey: styleKey })
            generateCssStyleElement(cssResults, cssStyleSelector, cssAttributes, fallbackAttributes)
        }
    }

    function generateStyleSheet(cssResults, styles) {
        generateCssStyleElement(cssResults, ".chatbox-gen", styles.chatbox)

        generateCssStyleElementsFromMap(cssResults, ".message-body-{styleKey}", styles.channel)
        generateCssStyleElementsFromMap(cssResults, ".message-segment-{styleKey}", styles.segment, styles.channel)

        generateCssStyleElement(cssResults, ".chatbox-search-msg-marked:not(.chatbox-search-msg-selected)", styles.chatsearch.marked)
        generateCssStyleElement(cssResults, ".chatbox-search-msg-selected", styles.chatsearch.selected)
    }

    function generateTimestampStyle(cssResults, showTimestamp) {
        const attributes = {}
        if (!showTimestamp) {
            attributes["display"] = "none"
        }
        generateCssStyleElement(cssResults, ".message-timestamp", attributes)
    }

    function generateGroupStyles(cssResults, styles) {
        const data = styles.data
        for (let dataKey in data) {
            const group = data[dataKey]
            const groupStyle = group.style

            generateCssStyleElement(cssResults, `.message-group-body-${group.id}`, groupStyle.body)
            generateCssStyleElement(cssResults, `.message-group-sender-${group.id}`, groupStyle.header)
        }
    }

    function generateFadeOutStyle(cssResults, minOpacity) {
        generateCssStyleElement(cssResults, ".message-body-fadeout-0", { "display": "none" })

        minOpacity = minOpacity / 100.0
        const steps = 10
        const stepSize = (1.0 - minOpacity) / steps
        for (let i = 1; i <= steps; ++i) {
            generateCssStyleElement(cssResults, `.message-body-fadeout-${i}`, { "opacity": `${minOpacity + (i - 1) * stepSize}` })
        }
    }

    function buildStyle(gobchatConfig) {
        const cssResults = []

        generateStyleSheet(cssResults, gobchatConfig.get("style"))
        generateTimestampStyle(cssResults, gobchatConfig.get("behaviour.showTimestamp"))
        generateGroupStyles(cssResults, gobchatConfig.get("behaviour.groups"))
        generateFadeOutStyle(cssResults, gobchatConfig.get("behaviour.fadeout.minopacity"))

        return cssResults.join("\n")
    }

    function setDocumentStyle(styleId, styleCssText) {
        let styleElement = document.getElementById(styleId)
        if (!styleElement) {
            styleElement = document.createElement("style")
            styleElement.id = styleId
            document.head.appendChild(styleElement)
            //document.getElementsByTagName('head')[0].appendChild(styleElement)
        }

        styleElement.type = 'text/css';
        styleElement.innerHTML = styleCssText
    }

    Gobchat.StyleBuilder = {
        updateStyle: function (styleConfig, styleId) {
            const css = buildStyle(styleConfig)
            setDocumentStyle(styleId, css)
        }
    }

    return Gobchat
}(Gobchat || {}));