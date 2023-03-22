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

'use strict';

import * as Databinding from "/module/Databinding"
import * as Components from "/module/Components"
import * as Utility from "/module/CommonUtility"
import * as Dialog from "/module/Dialog"
import * as Locale from "/module/Locale"
import * as Config from "/module/Config"

const MessageSegments = {
    SAY: { type: Gobchat.MessageSegmentEnum.SAY, styleId: "style.segment.say", translationId: "main.chat.segment.type.say" },
    EMOTE: { type: Gobchat.MessageSegmentEnum.EMOTE, styleId: "style.segment.emote", translationId: "main.chat.segment.type.emote" },
    MENTION: { type: Gobchat.MessageSegmentEnum.MENTION, styleId: "style.segment.mention", translationId: "main.chat.segment.type.mention" },
    OOC: { type: Gobchat.MessageSegmentEnum.OOC, styleId: "style.segment.ooc", translationId: "main.chat.segment.type.ooc" }
}

const ConfigKeyData = "behaviour.segment.data"
const ConfigKeyOrder = "behaviour.segment.order"
const JQueryDataKey = "ConfigBinding"
const DataAttributeElementId = "data-gob-entryid"

// --------------------------------------------------------------------------------------------------------

const binding = new Databinding.BindingContext(gobConfig)

// group 1
// item 1
Databinding.bindElement(binding, $("#cp-formatting_chat_font-family"))
Components.makeResetButton($("#cp-formatting_chat_font-family_reset"), $("#cp-formatting_chat_font-family"))

// item 2

function makeFontSizeSelector(id: string, minValue: number | null, maxValue: number | null, unit: string) {
    const input = $(`#${id}`)
    const slider = $(`#${id}_slider`)
    const selector = $(`#${id}_selector`)
    const btnReset = $(`#${id}_reset`)

    Databinding.bindElement<string>(binding, input, {
        elementToConfig: (element, event, storedValue) => {
            let newValue = Utility.toInt(element.val())

            if (newValue === null) {  // restore old value                    
                newValue = Utility.extractFirstNumber(storedValue)
                newValue = Utility.toInt(newValue)
            }

            if (newValue === null)
                newValue = 0

            if (minValue !== null && newValue < minValue)
                newValue = minValue

            if (maxValue !== null && newValue > maxValue)
                newValue = maxValue

            element.val(newValue)
            slider.val(newValue)
            selector.val(newValue)
            if (selector.val() === null)
                selector.val("")

            return `${newValue}${unit}`
        },

        configToElement: (element, storedValue) => {
            const value = Utility.extractFirstNumber(storedValue)
            element.val(value)
            slider.val(value)
            selector.val(value)
            if (selector.val() === null)
                selector.val("")
        }
    })

    slider.on("input", () => input.val(slider.val()).change())
    
    selector.on("change", () => {
        const selectedValue = selector.val()
        if (selectedValue.length > 0) {
            input.val(selectedValue).change()
        } else {
            selector.val(input.val())
            if (selector.val() === null)
                selector.val("")
        }
    })

    Components.makeResetButton(btnReset, input)
}

makeFontSizeSelector("cp-formatting_chat-history_font-size", 8, null, "px")
makeFontSizeSelector("cp-formatting_chat-ui_font-size", 8, null, "px")
makeFontSizeSelector("cp-formatting_chat-history_gap", 0, null, "px")

// group 2
// item 1
const colorTable = $("#cp-formatting_color-table")
const colorTableEntryTemplate = $('#cp-formatting_template_color-table_entry')
Object.values(MessageSegments).forEach(messageSegment => {
    const entry = $(colorTableEntryTemplate.html())
    colorTable.append(entry)

    const lblName = entry.find(".js-label")
    const selColor = entry.find(".js-color-selector")
    const btnResetColor = entry.find(".js-color-reset")

    lblName.attr(Locale.HtmlAttribute.TextId, `${messageSegment.translationId}`)
    lblName.attr(Locale.HtmlAttribute.TooltipId, `${messageSegment.translationId}.tooltip`)

    Databinding.setConfigKey(selColor, `${messageSegment.styleId}.color`)
    Components.makeColorSelector(selColor)
    Components.makeResetButton(btnResetColor, selColor)
    Databinding.bindColorSelector(binding, selColor)
})
gobLocale.updateElement(colorTable)

// group 3
// item 1
const btnAddSegmentDetection = $("#cp-formatting_segment-detection_add")
btnAddSegmentDetection.on("click", event => {
    const data = gobConfig.get(ConfigKeyData)
    const id = Utility.generateId(6, Object.keys(data))

    data[id] = gobConfig.getDefault("behaviour.segment.data-template")
    gobConfig.set(ConfigKeyData, data)

    const order = gobConfig.get(ConfigKeyOrder)
    order.push(id)
    gobConfig.set(ConfigKeyOrder, order)
})

const btnSegmentDetectionTableReset = $("#cp-formatting_segment-detection_reset")
btnSegmentDetectionTableReset.on("click", (event) => {
    gobConfig.reset(ConfigKeyData)
    gobConfig.reset(ConfigKeyOrder)
})

// item 2
const segmentDetectionTable = $("#cp-formatting_segment-detection-table")
const segmentDetectionTableEntryTemplate = $("#cp-formatting_template_segment-detection-table_entry")

segmentDetectionTable.accordion({
    heightStyle: "content",
    header: "> div > h4",
}).sortable({
    axis: "y",
    handle: "h4",
    stop: function (event, ui) {
        //update sort order
        let order: string[] = []
        segmentDetectionTable.find(`> [${DataAttributeElementId}]`).each(function () {
            order.push($(this).attr(DataAttributeElementId) as string)
        })

        order = order.filter(e => e !== null && e !== undefined && e.length > 0)
        gobConfig.set(ConfigKeyOrder, order)
    }
})

async function buildSegmentDetectionTable() {
    segmentDetectionTable.children().each(function () {
        $(this).data<Databinding.BindingContext>(JQueryDataKey).clearBindings()
    })
    segmentDetectionTable.empty()

    const ids = gobConfig.get(ConfigKeyOrder) as string[]
    ids.forEach((id, idx) => buildSegmentDetectionTableEntry(id, idx))

    segmentDetectionTable.sortable("refresh")
    segmentDetectionTable.accordion("refresh")
}

async function buildSegmentDetectionTableEntry(id: string, idx: number) {
    const entry = $(segmentDetectionTableEntryTemplate.html())
    entry.attr(DataAttributeElementId, id)
    segmentDetectionTable.append(entry)

    const binding = new Databinding.BindingContext(gobConfig)
    entry.data(JQueryDataKey, binding)

    const configKey = `${ConfigKeyData}.${id}`

    const lblEntryIndex = entry.find(".js-header_index")
    const lblEntryName = entry.find(".js-header_name")
    const btnDeleteEntry = entry.find(".js-delete-entry")
    const chkEnableEntry = entry.find(".js-entry-active")
    const selSegmentType = entry.find(".js-segment-selector")
    const txtStartToken = entry.find(".js-start-token")
    const lblStartTokenUnicode = entry.find(".js-start-token-unicode")
    const txtEndToken = entry.find(".js-end-token")
    const lblEndTokenUnicode = entry.find(".js-end-token-unicode")

    lblEntryIndex.text(idx+1)

    btnDeleteEntry.on("click", async (event) => {
        const result = await Dialog.showConfirmationDialog({
            dialogText: "config.formatting.tbl.segment.entry.action.delete.confirm",
        })

        if (result !== 1)
            return

        try {
            gobConfig.remove(configKey)

            const order = gobConfig.get(ConfigKeyOrder)
            _.remove(order, e => e === id)
            gobConfig.set(ConfigKeyOrder, order)
        } catch (e1) {
            console.error(e1)
        }
    })

    Databinding.bindCheckbox(binding, chkEnableEntry, { configKey: `${configKey}.active` })

    Databinding.setConfigKey(selSegmentType, `${configKey}.type`)
    Databinding.bindElement(binding, selSegmentType)

    Databinding.setConfigKey(txtStartToken, `${configKey}.startTokens`)
    Databinding.bindElement(binding, txtStartToken, {
        elementToConfig: (element) => convertTokenInput(element.val()),
        configToElement: (element, value) => element.val(value.join(", "))
    })

    Databinding.setConfigKey(txtEndToken, `${configKey}.endTokens`)
    Databinding.bindElement(binding, txtEndToken, {
        elementToConfig: (element) => convertTokenInput(element.val()),
        configToElement: (element, value) => element.val(value.join(", "))
    })

    binding.bindCallback(selSegmentType, () => updateEntryHeader())
    binding.bindCallback(txtStartToken, (value) => {
        updateEntryHeader()
        lblStartTokenUnicode.text(toUnicodeString(value))
    })
    binding.bindCallback(txtEndToken, (value) => {
        updateEntryHeader()
        lblEndTokenUnicode.text(toUnicodeString(value))
    })

    Databinding.bindListener(binding, "behaviour.language", (value) => {
        updateEntryHeader(value)
    })

    await gobLocale.updateElement(entry)

    async function updateEntryHeader(locale?: string) {
        const txtStart = gobConfig.get(Databinding.getConfigKey(txtStartToken)).join(", ")
        const txtEnd = gobConfig.get(Databinding.getConfigKey(txtEndToken)).join(", ")
        const txtType = selSegmentType.find("option:selected").text()
        const localization = await gobLocale.get("config.formatting.tbl.segment.entry.header", locale)
        const label = Utility.formatString(localization, txtType, txtStart, txtEnd)
        lblEntryName.text(label)
    }

    binding.loadBindings()
}

binding.bindConfigListener(ConfigKeyOrder, Databinding.createConfigListener(() => buildSegmentDetectionTable(), null, true), () => buildSegmentDetectionTable())

binding.loadBindings()

function convertTokenInput(str: string) {
    const text = str.trim()
    const split = text.split(" ")
    const data: string[] = []
    for (let s of split) {
        if (s.length === 0)
            continue

        s = Utility.decodeUnicode(s)
        data.push(s)
    }
    return data;
}

function toUnicodeString(arr: string[]): string {
    return arr.map(s => Utility.encodeUnicode(s)).join(" | ")
}

// --------------------------------------------------------------------------------------------------------

const configKeys = new Set<string>(["behaviour.segment.order", "behaviour.segment.data", "style.segment", "style.channel.base.font-family", "style.chat-history.font-size", "style.chat-history.gap", "style.chatui.font-size"])
Components.makeCopyProfileButton($("#cp-formatting_copyprofile"), {
        configKeys: Array.from(configKeys)
    })


//# sourceURL=config_roleplay.js