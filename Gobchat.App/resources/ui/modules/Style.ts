/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

import * as Utility from './CommonUtility.js'
import * as Constants from './Constants.js'
import * as Chat from './Chat.js'

export class StyleLoader {
    #styles: { [key: string]: { label: string, files: string[] } } = {}
    #activeStyles: string[] = []
    #activeStyleSheetIds: string[] = []
    readonly #filePrefix: string | null

    constructor(filePrefix: string) {
        this.#filePrefix = filePrefix ? filePrefix : null
    }

    async initialize(): Promise<void> {
        this.#styles = {}

        const json = await GobchatAPI.readTextFromFile("ui/styles/styles.json")
        const styles = JSON.parse(json);

        for (const style of styles) {
            const styleKey = style.label.toLowerCase().trim()
            if (styleKey.length == 0)
                continue

            const files: string[] = [].concat(style.files || []).filter(file => Utility.isString(file))

            this.#styles[styleKey] = {
                label: style.label,
                files: files.map(file => file.trim()).filter(file => file.length > 0)
            }
        }
    }

    get styles(): { id: string, label: string }[] {
        return Object.keys(this.#styles)
            .map(key => {
                return {
                    id: key,
                    label: this.#styles[key].label
                }
            })
    }

    get activeStyles(): string[] {
        return ([] as string[]).concat(this.#activeStyles || [])
    }

    async activateStyles(): Promise<void>
    async activateStyles(styleIds: string | string[], target: HTMLElement | JQuery): Promise<void>
    async activateStyles(styleIds: string | string[], target: HTMLElement | JQuery, insertMod: "in" | "after" | "before"): Promise<void>
    async activateStyles(styleIds?: string | string[], target?: HTMLElement | JQuery, insertMod: "in"|"after"|"before" = "in"): Promise<void> {
        styleIds = ([] as string[]).concat(styleIds || []).filter(e => Utility.isString(e)).map(e => e.toLowerCase())

        for (const styleId of styleIds)
            if (!this.#styles[styleId])
                throw new Error(`Style with id '${styleId}' not available`)

        const _target = !target || $(target).length === 0 ? $("head") : $(target).first()
        const body = $("body")

        // use hide / show to trigger a reflow, so the new loaded style gets applied everywhere.
        // Sometimes, without this, styles aren't applied to scrollbars. Still no idea why.
        body.hide()

        for (const id of this.#activeStyleSheetIds)
            $(`#${id}`).remove()

        this.#activeStyleSheetIds = []
        this.#activeStyles = []

        const awaitPromises: Promise<void>[] = []

        for (const styleId of styleIds) {
            this.#activeStyles.push(styleId)

            const style = this.#styles[styleId]
            const randomIdPrefix = Utility.generateId(8)

            for (const file of style.files) {
                const id = `gobstyle-${randomIdPrefix}-${file.replace(/[\s\\/]+/g, '_').replace(/[^-\.\w]+/g, '')}`

                const $link = $(`<link rel="stylesheet" href="">`).attr('id', id)
                this.#activeStyleSheetIds.push(id)

                awaitPromises.push(new Promise(function (resolve, reject) {
                    $link.one("load", () => resolve())
                    $link.one("error", () => reject())
                }))

                const path = this.#filePrefix ? `${this.#filePrefix}/${file}` : file
                $link.attr("href", path)

                switch(insertMod){
                    case 'in':
                        $link.appendTo(_target)
                        break
                    case 'after':
                        $link.insertAfter(_target)
                        break
                    case 'before':
                        $link.insertBefore(_target)
                }                
            }
        }

        awaitPromises.push(new Promise((resolve, reject) => {
            window.requestAnimationFrame(() => resolve())
        }))

        const results = await Promise.allSettled(awaitPromises)

        body.show()

        let errorMsg = ""
        for (const result of results) {
            if (result.status === "rejected")
                errorMsg += result.reason + '\n'
        }
        if (errorMsg.length > 0)
            throw new Error(errorMsg)
    }
}

type CssRuleGenerator = () => string

function hasValueNoUnit(value: string) {
    return /\d$/.test(value)
}

function addUnitToValueIfMissing(value: string, fallbackUnit: string) {
    return hasValueNoUnit(value) ? value + fallbackUnit : value
}

export class StyleBuilder {

    private static RuleGenerators: CssRuleGenerator[] = []

    public static generateAndSetCssRules(htmlStyleSheetId: string) {
        const rules = StyleBuilder.generateCssRules()
        StyleBuilder.setStyleOnCurrentDocument(htmlStyleSheetId, rules)
    }

    /*
    static { // template
        StyleBuilder.RuleGenerators.push(() => {
            return ""
        })
    }
    */

    static { // font size
        StyleBuilder.RuleGenerators.push(() => {
            //const baseFontSize = gobConfig.get("style.base-font-size")

            const configFontSize = gobConfig.get("style.config.font-size") as string
            const uiFontSize = gobConfig.get("style.chatui.font-size") as string

            //configFontSize = addUnitToValueIfMissing(configFontSize, "px")
            //uiFontSize = addUnitToValueIfMissing(uiFontSize, "px")

            return StyleBuilder.toCss(":root", {
                "--gob-font-size-config": `max(8px, ${configFontSize})`,
                "--gob-font-size-chat-ui": `max(8px, ${uiFontSize})`,
            })
        })
    }

    static { // general rules
        StyleBuilder.RuleGenerators.push(() => {
            const configStyle = gobConfig.get("style")

            const results: string[] = []

            results.push(StyleBuilder.toCss(`.${Chat.CssClass.Chat_History}`, configStyle["chat-history"]))

            results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry}`, configStyle.channel.base))

            for (const channel of Object.values(Gobchat.Channels)) {
                if (channel.internalName in configStyle.channel) {
                    const channelClass = Utility.formatString(Chat.CssClass.ChatEntry_Channel_Partial, channel.internalName)
                    const selector = `.${channelClass} .${Chat.CssClass.ChatEntry_Text}`
                    results.push(StyleBuilder.toCss(selector, configStyle.channel[channel.internalName]))
                }
            }

            return results.join("")
        })
    }

    static { // chat entry formatting
        StyleBuilder.RuleGenerators.push(() => {
            const configStyle = gobConfig.get("style")
            const tabClassesWithMentions = StyleBuilder.filterTabs(tab => tab.formatting.mentions)
            const tabClassesWithRoleplay = StyleBuilder.filterTabs(tab => tab.formatting.roleplay)

            const results: string[] = []

            results.push(StyleBuilder.toCss(
                tabClassesWithMentions.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Mention}`),
                configStyle.segment.mention
            ))

            results.push(StyleBuilder.toCss(
                tabClassesWithRoleplay.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Say}`),
                configStyle.segment.say, configStyle.channel.say
            ))

            results.push(StyleBuilder.toCss(
                tabClassesWithRoleplay.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Emote}`),
                configStyle.segment.emote, configStyle.channel.emote
            ))

            results.push(StyleBuilder.toCss(
                tabClassesWithRoleplay.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Ooc}`),
                configStyle.segment.ooc
            ))

            results.push(StyleBuilder.toCss(
                `.${Chat.CssClass.ChatEntry_Segment_Link}`,
                configStyle.segment.link
            ))

            return results.join("")
        })
    }

    static { // time stamp
        StyleBuilder.RuleGenerators.push(() => {
            const tabClasses = StyleBuilder.filterTabs(tab => !tab.formatting.timestamps)
            const selectors = tabClasses.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Time}`)
            return StyleBuilder.toCss(selectors, { "display": "none" })
        })
    }

    static { // range filter
        StyleBuilder.RuleGenerators.push(() => {
            const tabClasses = StyleBuilder.filterTabs(tab => tab.formatting.rangefilter)
            if (tabClasses.length === 0)
                return ""

            const configRangeFilter = gobConfig.get("behaviour.rangefilter")
            const startopacity = configRangeFilter.startopacity / (configRangeFilter.maxopacity + 0.0)
            const endopacity = configRangeFilter.endopacity / (configRangeFilter.maxopacity + 0.0)
            const opacityByLevel = (startopacity - endopacity) / (configRangeFilter.opacitysteps - 1)

            const results: string[] = []

            for (let i = 0; i <= configRangeFilter.opacitysteps; ++i) {
                const rangeFilterClass = Utility.formatString(Chat.CssClass.ChatEntry_FadeOut_Partial, i)
                const selectors = tabClasses.map(tabClass => `.${tabClass} .${rangeFilterClass}`)

                if(i === 0){
                    results.push(StyleBuilder.toCss(selectors, { "display": "none" }))
                }else{
                    results.push(StyleBuilder.toCss(selectors, { "opacity": `${(i - 1) * opacityByLevel + endopacity}` }))
                }                
            }

            return results.join("")
        })
    }

    static { // visible chat entries by channel by tab
        StyleBuilder.RuleGenerators.push(() => {
            const configTabs = gobConfig.get("behaviour.chattabs.data")
            const configGroups = gobConfig.get("behaviour.groups.data")

            const tabs = Object.values(configTabs) as any[]

            const results: string[] = []

            for (const tab of tabs) {
                const tabClass = Utility.formatString(Chat.CssClass.Chat_History_Tab_Partial, tab.id)

                {
                    const invisibleChannels = _.difference(Constants.ChannelEnumValues, tab.channel.visible as number[])
                        .map(id => Constants.ChannelEnumToKey[id])
                        .map(key => Gobchat.Channels[key])
                        .map(channel => channel.internalName)

                    const selectors = invisibleChannels.map(channelName => `.${tabClass} .${Utility.formatString(Chat.CssClass.ChatEntry_Channel_Partial, channelName)}`)
                    results.push(StyleBuilder.toCss(selectors, { "display": "none" }))
                }

                if (tab.groups.type === "only") {
                    const notSelector = tab.groups.filter.map(groupId => `.${Utility.formatString(Chat.CssClass.ChatEntry_TriggerGroup_Partial, groupId)}`).join(",")
                    const selector = `.${tabClass} .${Chat.CssClass.ChatEntry}:not(${notSelector})`
                    results.push(StyleBuilder.toCss(selector, { "display": "None" }))
                } else if (tab.groups.type === "hide") {
                    const selectors = tab.groups.filter.map(groupId => `.${tabClass} .${Utility.formatString(Chat.CssClass.ChatEntry_TriggerGroup_Partial, groupId)}`)
                    results.push(StyleBuilder.toCss(selectors, { "display": "none" }))
                }
            }

            return results.join("")
        })
    }

    static { // trigger groups
        StyleBuilder.RuleGenerators.push(() => {
            const configTriggerGroups = gobConfig.get("behaviour.groups.data")
            const results: string[] = []

            for (const key of Object.keys(configTriggerGroups)) {
                const triggerGroup = configTriggerGroups[key]
                const cssClass = Utility.formatString(Chat.CssClass.ChatEntry_TriggerGroup_Partial, triggerGroup.id)
                results.push(StyleBuilder.toCss(`.${cssClass}`, triggerGroup.style.body))
                results.push(StyleBuilder.toCss(`.${cssClass} .${Chat.CssClass.ChatEntry_Sender}`, triggerGroup.style.header))
            }

            return results.join("")
        })
    }

    static { // search
        StyleBuilder.RuleGenerators.push(() => {
            const configStyle = gobConfig.get("style")
            const results: string[] = []

            results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry_MarkedbySearch}:not(.${Chat.CssClass.ChatEntry_SelectedBySearch})`, configStyle.chatsearch.marked))
            results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry_SelectedBySearch}`, configStyle.chatsearch.selected))

            return results.join("")
        })
    }

    private static copy<T>(object: T): T {
        return JSON.parse(JSON.stringify(object))
    }

    private static toCss(selectors: string | string[], ...properties: { [property: string]: string }[]): string {
        selectors = ([] as string[]).concat(selectors)
        if (selectors.length === 0 || !properties || properties.length === 0)
            return ""

        let baseProperties = properties[0]

        if (properties.length > 1) {
            baseProperties = StyleBuilder.copy(baseProperties)
            for (let i = 1; i < properties.length; ++i) {
                Object.keys(baseProperties).forEach(key => {
                    const currentValue = baseProperties[key]
                    if (currentValue === null || currentValue === undefined) {
                        if (key in properties[i])
                            baseProperties[key] = properties[i][key]
                    }
                })
            }
        }

        const allSelectors = selectors.map(selector => `${selector}`).join(",")
        return `${allSelectors}${StyleBuilder.objectToCss(baseProperties)}\n`
    }

    private static objectToCss(object: { [property: string]: string }): string {
        const content = Object.entries(object)
        .filter(e => e[1] !== null && !e[0].startsWith("$"))
        .map(e => `\t${e[0]}: ${e[1]};`)
        .join("\n")
        return `{\n${content}\n}`
    }

    private static filterTabs(filter?: (tabConfig: any) => boolean): string[] {
        const configTabs = gobConfig.get("behaviour.chattabs.data")
        const filteredTabs = filter ? Object.keys(configTabs).filter(key => filter(configTabs[key])) : Object.keys(configTabs)
        const tabClasses = filteredTabs.map(key => configTabs[key].id)
            .map(id => Utility.formatString(Chat.CssClass.Chat_History_Tab_Partial, id))
        return tabClasses
    }

    private static generateCssRules(): string {
        const results: string[] = []
        for (const ruleGenerator of StyleBuilder.RuleGenerators)
            results.push(ruleGenerator())
        return results.join("")
    }

    private static setStyleOnCurrentDocument(htmlStyleSheetId: string, cssRules: string): void {
        let styleElement = document.getElementById(htmlStyleSheetId) as HTMLStyleElement
        if (!styleElement) {
            styleElement = document.createElement("style")
            styleElement.id = htmlStyleSheetId
            document.head.appendChild(styleElement)
        }
        styleElement.innerHTML = cssRules
    }
}
