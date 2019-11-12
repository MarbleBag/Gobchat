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

    function buildCssClass(className, classAttributes, classAttributesFallback) {
        const attributes = getAttributes(classAttributes, classAttributesFallback)
            .map((attribute) => { return `${attribute[0]}: ${attribute[1]};` })

        const style = `
				.${className}{
					${attributes.join("")}
				}
			`
        return style
    }

    function generateCssClass(cssResults, cssClassName, styleData, styleFallbackData) {
        const css = buildCssClass(cssClassName, styleData, styleFallbackData)
        cssResults.push(css)
    }

    function generateCssClasses(cssResults, cssClassNameTemplate, styleData, styleFallbackData) {
        for (let styleKey in styleData) {
            const cssAttributes = styleData[styleKey]
            const fallbackAttributes = (styleFallbackData && styleKey in styleFallbackData) ? styleFallbackData[styleKey] : undefined
            const cssClassName = Gobchat.formatString(cssClassNameTemplate, { styleKey: styleKey })
            const css = buildCssClass(cssClassName, cssAttributes, fallbackAttributes)
            cssResults.push(css)
        }
    }

    function buildGeneralStyles(cssResults, styles) {
        generateCssClass(cssResults, "chatbox-gen", styles.chatbox)
        generateCssClasses(cssResults, "message-body-{styleKey}", styles.channel)
        generateCssClasses(cssResults, "message-segment-{styleKey}", styles.segment, styles.channel)
    }

    function buildGroupStyles(cssResults, styles) {
        {
            const data = styles.data
            for (let dataKey in data) {
                const group = data[dataKey]
                const groupStyle = group.style

                generateCssClass(cssResults, `message-group-body-${group.id}`, groupStyle.body)
                generateCssClass(cssResults, `message-group-sender-${group.id}`, groupStyle.header)
            }
        }
    }

    function buildStyle(gobchatConfig) {
        const cssResults = []

        buildGeneralStyles(cssResults, gobchatConfig.get("style"))
        buildGroupStyles(cssResults, gobchatConfig.get("behaviour.groups"))

        return cssResults.join("\n")
    }

    function setStyle(styleId, styleCssText) {
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
            setStyle(styleId, css)
        }
    }

    return Gobchat
}(Gobchat || {}));