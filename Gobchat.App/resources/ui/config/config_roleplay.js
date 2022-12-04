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

(function () {
    const tblRoleplayContent = [
        { styleId: "style.segment.say", translationId: "main.chat.segment.type.say" },
        { styleId: "style.segment.emote", translationId: "main.chat.segment.type.emote" },
        { styleId: "style.segment.mention", translationId: "main.chat.segment.type.mention" },
        { styleId: "style.segment.ooc", translationId: "main.chat.segment.type.ooc" },
    ]

    const binding = GobConfigHelper.makeDatabinding(gobconfig)

    const tblRoleplay = $("#croleplay_roleplay > tbody")
    const rowTemplate = $('#croleplay_template_roleplayentry')
    tblRoleplayContent.forEach((entry) => {
        const rowEntry = $(rowTemplate.html()).appendTo(tblRoleplay)

        const lblName = rowEntry.find(".entry-label")
        const clrSelectorFG = rowEntry.find(".entry-color-forground")
        const btnResetFG = rowEntry.find(".entry-color-forground-reset")

        lblName.attr("data-gob-locale-text", `${entry.translationId}`)
        lblName.attr("data-gob-locale-title", `${entry.translationId}.tooltip`)

        GobConfigHelper.setConfigKey(clrSelectorFG, entry.styleId + ".color")
        GobConfigHelper.makeColorSelector(clrSelectorFG)
        GobConfigHelper.bindColorSelector(binding, clrSelectorFG)
        btnResetFG.on("click", event => gobconfig.reset(GobConfigHelper.getConfigKey(clrSelectorFG)))
    })

    binding.initialize()

    const segmentsEntryTmpl = $("#croleplay_template_segmentsentry")
    const tblSegments = $("#croleplay_segments_table")
    const ConfigKeyData = "behaviour.segment.data"
    const ConfigKeyOrder = "behaviour.segment.order"

    function convertTokenInput(str) {
        const text = str.trim()
        const split = text.split(" ")
        const data = []
        for (let s of split) {
            if (s.length === 0) continue
            s = Gobchat.decodeUnicode(s)
            data.push(s)
        }
        return data;
    }

    function toUnicodeString(arr) {
        let f = arr.map(s => Gobchat.encodeUnicode(s)).join(" | ")
        return f
    }

    function buildSegmentsTableEntry(entryId) {
        if (entryId === null || entryId === undefined || entryId.length === 0)
            throw new Error("Invalid entry id")

        const entry = $(segmentsEntryTmpl.html())
        entry.attr("data-gob-entryid", entryId)
        entry.appendTo(tblSegments)

        const binding = GobConfigHelper.makeDatabinding(gobconfig)
        entry.data("configbinding", binding)

        const configKey = ConfigKeyData + "." + entryId

        const btnDelete = entry.find(".tmpl-delete-entry")
        btnDelete.on("click", event => {
            event.stopPropagation()
            {
                (async () => {
                    const result = await GobConfigHelper.showConfirmationDialog({
                        dialogText: "config.roleplay.entry.deleteconfirm",
                    })
                    if (result !== 1) return

                    try {
                        gobconfig.remove(configKey)
                        const order = gobconfig.get(ConfigKeyOrder)
                        _.remove(order, e => e === entryId)
                        gobconfig.set(ConfigKeyOrder, order)
                    } catch (e1) {
                        console.error(e1)
                    }
                })()
            }
        })

        const ckbIsActive = entry.find(".tmpl-entry-active")
        GobConfigHelper.setConfigKey(ckbIsActive, configKey + ".active")
        GobConfigHelper.bindCheckbox(binding, ckbIsActive)

        const dpdSegmentType = entry.find(".tmpl-segment-selector")
        GobConfigHelper.setConfigKey(dpdSegmentType, configKey + ".type")
        GobConfigHelper.bindElement(binding, dpdSegmentType)

        const txtStartTokens = entry.find(".tmpl-startTokens")
        GobConfigHelper.setConfigKey(txtStartTokens, configKey + ".startTokens")
        GobConfigHelper.bindElement(binding, txtStartTokens,
            {
                elementGetAccessor: ($element) => convertTokenInput($element.val()),
                elementSetAccessor: ($element, value) => $element.val(value.join(" "))
            })

        const txtEndTokens = entry.find(".tmpl-endTokens")
        GobConfigHelper.setConfigKey(txtEndTokens, configKey + ".endTokens")
        GobConfigHelper.bindElement(binding, txtEndTokens,
            {
                elementGetAccessor: ($element) => convertTokenInput($element.val()),
                elementSetAccessor: ($element, value) => $element.val(value.join(" "))
            })

        GobConfigHelper.bindListener(binding, GobConfigHelper.getConfigKey(dpdSegmentType), (value) => {
            updateEntryHeader()
        })
        GobConfigHelper.bindListener(binding, GobConfigHelper.getConfigKey(txtStartTokens), (value) => {
            entry.find(".tmpl-startTokens-unicode").text(toUnicodeString(value))
            updateEntryHeader()
        })
        GobConfigHelper.bindListener(binding, GobConfigHelper.getConfigKey(txtEndTokens), (value) => {
            entry.find(".tmpl-endTokens-unicode").text(toUnicodeString(value))
            updateEntryHeader()
        })
        GobConfigHelper.bindListener(binding, "behaviour.language", (value) => {
            updateEntryHeader(value)
        })

        async function updateEntryHeader(locale) {
            const txtStart = gobconfig.get(GobConfigHelper.getConfigKey(txtStartTokens)).join(" or ")
            const txtEnd = gobconfig.get(GobConfigHelper.getConfigKey(txtEndTokens)).join(" or ")
            const txtType = dpdSegmentType.find("option:selected").text()
            const localization = await gobLocale.get("config.roleplay.entry.header", locale)
            const label = Gobchat.formatString(localization, [txtType, txtStart, txtEnd])
            entry.find(".tmpl-header-name").text(label)
        }

        binding.initialize()
    }

    function addNewEntrySegmentsTable() {
        const data = gobconfig.get(ConfigKeyData)
        const id = GobConfigHelper.generateId(6, Object.keys(data))

        data[id] = {
            type: 1,
            startTokens: [],
            endTokens: [],
        }

        gobconfig.set(ConfigKeyData, data)
        const order = gobconfig.get(ConfigKeyOrder)
        order.push(id)
        gobconfig.set(ConfigKeyOrder, order)
    }
    const btnAddSegment = $("#croleplay_segments_add")
    btnAddSegment.attr("title", "Adds a new box for roleplay formatting")
    btnAddSegment.on("click", (event) => addNewEntrySegmentsTable())

    const btnTblSegmentsReset = $("#croleplay_segments_reset")
    btnTblSegmentsReset.on("click", (event) => {
        gobconfig.reset(ConfigKeyData)
        gobconfig.reset(ConfigKeyOrder)
    })

    function clearSegmentsTable() {
        tblSegments.find("div[data-gob-entryid]").each(function (idx) {
            $(this).data("configbinding").clear()
        })
        tblSegments.empty()
    }

    function populateSegmentsTable() {
        clearSegmentsTable()
        const ids = gobconfig.get(ConfigKeyOrder)
        ids.forEach(id => buildSegmentsTableEntry(id))

        tblSegments.sortable("refresh")
        tblSegments.accordion("refresh")

        tblSegments.find(".tmpl-header-index").each(function (idx) {
            $(this).text(idx + 1)
        })

        gobLocale.updateElement(tblSegments)
    }

    tblSegments.accordion({
        heightStyle: "content",
        header: "> div > h4",
    })
        .sortable({
            axis: "y",
            handle: "h4",
            stop: function (event, ui) {
                //update sort order
                let order = []
                tblSegments.find("div[data-gob-entryid]").each(function () {
                    order.push($(this).attr("data-gob-entryid"))
                    //order.push($(this).data("groupId"))
                })

                order = order.filter(e => e !== null && e !== undefined && e.length > 0)
                gobconfig.set(ConfigKeyOrder, order)
            }
        });

    gobconfig.addProfileEventListener((event) => {
        if (event.type === "active")
            populateSegmentsTable()
    })
    gobconfig.addPropertyEventListener(ConfigKeyOrder, (event) => {
        if (event.isActive)
            populateSegmentsTable()
    })
    populateSegmentsTable()

    const btnCopyProfile = $("#croleplay_copyprofile")
    const copyKeys = ["behaviour.segment"]
    tblRoleplayContent.forEach(entry => {
        if (entry.styleId === undefined || entry.styleId === null) return
        copyKeys.push(entry.styleId + ".color")
    })
    GobConfigHelper.makeCopyProfileButton(btnCopyProfile, { configKeys: copyKeys })
}());

//# sourceURL=config_roleplay.js