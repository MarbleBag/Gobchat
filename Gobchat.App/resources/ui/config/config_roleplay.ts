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

'use strict';

import * as Databinding from "/module/Databinding"
import * as Components from "/module/Components"
import * as Utility from "/module/CommonUtility"
import * as Dialog from "/module/Dialog"

const tblRoleplayContent = [
    { styleId: "style.segment.say", translationId: "main.chat.segment.type.say" },
    { styleId: "style.segment.emote", translationId: "main.chat.segment.type.emote" },
    { styleId: "style.segment.mention", translationId: "main.chat.segment.type.mention" },
    { styleId: "style.segment.ooc", translationId: "main.chat.segment.type.ooc" },
]

const ConfigKeyData = "behaviour.segment.data"
const ConfigKeyOrder = "behaviour.segment.order"
const SegmentDatabindingKey = "ConfigBinding"
const DataAttributeElementId = "data-gob-entryid"


const binding = new Databinding.BindingContext(gobConfig)

const $tblRoleplay = $("#cp-roleplay_color-table")
const $rowTemplate = $('#cp-roleplay_template_color-table_entry')
tblRoleplayContent.forEach((entry) => {
    const rowEntry = $($rowTemplate.html())
    rowEntry.appendTo($tblRoleplay)

    const lblName = rowEntry.find(".js-label")
    const clrSelectorFG = rowEntry.find(".js-color-forground")
    const btnResetFG = rowEntry.find(".js-color-forground-reset")

    lblName.attr("data-gob-locale-text", `${entry.translationId}`)
    lblName.attr("data-gob-locale-title", `${entry.translationId}.tooltip`)

    Databinding.setConfigKey(clrSelectorFG, entry.styleId + ".color")
    Components.makeColorSelector(clrSelectorFG)
    Databinding.bindColorSelector(binding, clrSelectorFG)

    btnResetFG.on("click", event => gobConfig.reset(Databinding.getConfigKey(clrSelectorFG)))
})

binding.initialize()

const $segmentsEntryTmpl = $("#cp-roleplay_template_segments-table_entry")
const $tblSegments = $("#cp-roleplay_segments-table")

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

function buildSegmentsTableEntry(entryId: string) {
    if (entryId === null || entryId === undefined || entryId.length === 0)
        throw new Error("Invalid entry id")

    const entry = $($segmentsEntryTmpl.html())
    entry.attr(DataAttributeElementId, entryId)
    entry.appendTo($tblSegments)

    const binding = new Databinding.BindingContext(gobConfig)
    entry.data(SegmentDatabindingKey, binding)

    const configKey = ConfigKeyData + "." + entryId

    const btnDelete = entry.find(".js-delete-entry")
    btnDelete.on("click", event => {
        event.stopPropagation()
        {
            (async () => {
                const result = await Dialog.showConfirmationDialog({
                    dialogText: "config.roleplay.entry.deleteconfirm",
                })

                if (result !== 1)
                    return

                try {
                    gobConfig.remove(configKey)
                    const order = gobConfig.get(ConfigKeyOrder)
                    _.remove(order, e => e === entryId)
                    gobConfig.set(ConfigKeyOrder, order)
                } catch (e1) {
                    console.error(e1)
                }
            })()
        }
    })

    const ckbIsActive = entry.find(".js-entry-active")
    Databinding.setConfigKey(ckbIsActive, configKey + ".active")
    Databinding.bindCheckbox(binding, ckbIsActive)

    const dpdSegmentType = entry.find(".js-segment-selector")
    Databinding.setConfigKey(dpdSegmentType, configKey + ".type")
    Databinding.bindElement(binding, dpdSegmentType)

    const txtStartTokens = entry.find(".js-startTokens")
    Databinding.setConfigKey(txtStartTokens, configKey + ".startTokens")
    Databinding.bindElement(binding, txtStartTokens,
        {
            elementGetAccessor: ($element) => convertTokenInput($element.val()),
            elementSetAccessor: ($element, value) => $element.val(value.join(" "))
        })

    const txtEndTokens = entry.find(".js-endTokens")
    Databinding.setConfigKey(txtEndTokens, configKey + ".endTokens")
    Databinding.bindElement(binding, txtEndTokens,
        {
            elementGetAccessor: ($element) => convertTokenInput($element.val()),
            elementSetAccessor: ($element, value) => $element.val(value.join(" "))
        })

    Databinding.bindListener(binding, Databinding.getConfigKey(dpdSegmentType), (value) => {
        updateEntryHeader()
    })
    Databinding.bindListener(binding, Databinding.getConfigKey(txtStartTokens), (value) => {
        entry.find(".js-startTokens-unicode").text(toUnicodeString(value))
        updateEntryHeader()
    })
    Databinding.bindListener(binding, Databinding.getConfigKey(txtEndTokens), (value) => {
        entry.find(".js-endTokens-unicode").text(toUnicodeString(value))
        updateEntryHeader()
    })
    Databinding.bindListener(binding, "behaviour.language", (value) => {
        updateEntryHeader(value)
    })

    async function updateEntryHeader(locale?: string) {
        const txtStart = gobConfig.get(Databinding.getConfigKey(txtStartTokens)).join(" or ")
        const txtEnd = gobConfig.get(Databinding.getConfigKey(txtEndTokens)).join(" or ")
        const txtType = dpdSegmentType.find("option:selected").text()
        const localization = await gobLocale.get("config.roleplay.entry.header", locale)
        const label = Utility.formatString(localization, [txtType, txtStart, txtEnd])
        entry.find(".js-header-name").text(label)
    }

    binding.initialize()
}

function addNewEntrySegmentsTable() {
    const data = gobConfig.get(ConfigKeyData)
    const id = Utility.generateId(6, Object.keys(data))

    data[id] = {
        type: 1,
        startTokens: [],
        endTokens: [],
    }

    gobConfig.set(ConfigKeyData, data)
    const order = gobConfig.get(ConfigKeyOrder)
    order.push(id)
    gobConfig.set(ConfigKeyOrder, order)
}
const btnAddSegment = $("#cp-roleplay_segments_add")
btnAddSegment.on("click", (event) => addNewEntrySegmentsTable())

const btnTblSegmentsReset = $("#cp-roleplay_segments_reset")
btnTblSegmentsReset.on("click", (event) => {
    gobConfig.reset(ConfigKeyData)
    gobConfig.reset(ConfigKeyOrder)
})

function clearSegmentsTable() {
    $tblSegments.children().each(function () {
        $(this).data<Databinding.BindingContext>(SegmentDatabindingKey).clear()
    })
    $tblSegments.empty()
}

function populateSegmentsTable() {
    clearSegmentsTable()
    const ids = gobConfig.get(ConfigKeyOrder) as string[]
    ids.forEach(id => buildSegmentsTableEntry(id))

    $tblSegments.sortable("refresh")
    $tblSegments.accordion("refresh")

    $tblSegments.find(".tmpl-header-index").each(function (idx) {
        $(this).text(idx + 1)
    })

    gobLocale.updateElement($tblSegments)
}

$tblSegments.accordion({
    heightStyle: "content",
    header: "> div > h4",
})
    .sortable({
        axis: "y",
        handle: "h4",
        stop: function (event, ui) {
            //update sort order
            let order: string[] = []
            $tblSegments.find(`div[${DataAttributeElementId}]`).each(function () {
                order.push($(this).attr(DataAttributeElementId) as string)
                //order.push($(this).data("groupId"))
            })

            order = order.filter(e => e !== null && e !== undefined && e.length > 0)
            gobConfig.set(ConfigKeyOrder, order)
        }
    });

gobConfig.addProfileEventListener((event) => {
    if (event.action === "active")
        populateSegmentsTable()
})
gobConfig.addPropertyEventListener(ConfigKeyOrder, (event) => {
    if (event.isActiveProfile)
        populateSegmentsTable()
})
populateSegmentsTable()

const btnCopyProfile = $("#cp-roleplay_copyprofile")
const copyKeys = ["behaviour.segment"]
tblRoleplayContent.forEach(entry => {
    if (entry.styleId === undefined || entry.styleId === null) return
    copyKeys.push(entry.styleId + ".color")
})
Components.makeCopyProfileButton(btnCopyProfile, { configKeys: copyKeys })


//# sourceURL=config_roleplay.js