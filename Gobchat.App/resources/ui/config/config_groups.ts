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
import * as Dialog from "/module/Dialog"
import * as Components from "/module/Components"
import * as Utility from "/module/CommonUtility"

const DataAttributeElementId = "data-gob-entryid"
const ConfigKeyOrder = "behaviour.groups.sorting"
const ConfigKeyData = "behaviour.groups.data"
const ConfigKeyDataTemplate = "behaviour.groups.data-template"
const JQueryDataKey = "configbinding"

const tblGroups = $("#cp-groups_group-table")
const tmplGroupsEntryTemplate = $("#cp-groups_template_group-table_entry")

async function populateGroupTable() {
    tblGroups.children().each(function () {
        $(this).data<Databinding.BindingContext>(JQueryDataKey).clearBindings()
    })
    tblGroups.empty()

    const groupIds = gobConfig.get(ConfigKeyOrder) as string[]
    groupIds.forEach(async (id, idx) => await buildGroupTableEntry(id, idx))

    tblGroups.sortable("refresh")
    tblGroups.accordion("refresh")

    await gobLocale.updateElement(tblGroups)
}

async function buildGroupTableEntry(groupId: string, groupIndex: number) {
    const entry = $(tmplGroupsEntryTemplate.html())
    entry.attr(DataAttributeElementId, groupId)
    tblGroups.append(entry)

    const binding = new Databinding.BindingContext(gobConfig)
    entry.data(JQueryDataKey, binding)

    const configKey = `${ConfigKeyData}.${groupId}`
    const groupData = gobConfig.get(configKey)

    const isNonCustomGroup = "ffgroup" in groupData

    const lblEntryIndex = entry.find(".js-header_index")
    const lblEntryName = entry.find(".js-header_name")
    const btnDeleteEntry = entry.find(".js-delete-entry")
    const chkEnableEntry = entry.find(".js-entry-active")
    const txtGroupName = entry.find(".js-txt-name")
    const btnGroupNameReset = entry.find(".js-txt-name_reset")

    const txtTriggers = entry.find(".js-group-triggers")

    lblEntryIndex.text(groupIndex + 1)

    btnDeleteEntry.on("click", async (event) => {
        const result = await Dialog.showConfirmationDialog({
            dialogText: "config.groups.tbl.group.entry.deleteconfirm",
        })

        if (result === 1) {
            try {
                gobConfig.remove(configKey)
                const order = gobConfig.get(ConfigKeyOrder) as string[]
                _.remove(order, e => e === groupId)
                gobConfig.set(ConfigKeyOrder, order)
            } catch (e1) {
                console.error(e1)
            }
        }
    })

    Databinding.bindCheckbox(binding, chkEnableEntry, { configKey: `${configKey}.active` })

    Databinding.setConfigKey(txtGroupName, `${configKey}.name`)
    Databinding.bindElement(binding, txtGroupName)
    Components.makeResetButton(btnGroupNameReset, txtGroupName)
    Databinding.bindText(binding, lblEntryName, { configKey: Databinding.getConfigKey(txtGroupName)! })

    Databinding.bindElement(binding, txtTriggers, {
        configKey: `${configKey}.trigger`,
        elementToConfig: (element) => {
            let words = element.val().split(",")
            words = words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
            element.val(words.join(", "))
            return words
        },
        configToElement: (element, storedValue) => {
            element.val((storedValue || []).join(", "))
        }
    })

    function makeColorSelector(classId: string, configKey: string) {
        const selector = entry.find(`.${classId}`)
        const btnReset = entry.find(`.${classId}_reset`)

        Databinding.setConfigKey(selector, configKey)
        Components.makeColorSelector(selector)
        Databinding.bindColorSelector(binding, selector)

        Components.makeResetButton(btnReset, selector)

        if (!isNonCustomGroup)
            btnReset.hide()
    }

    makeColorSelector("js-sender-fgcolor", `${configKey}.style.header.color`)
    makeColorSelector("js-sender-bgcolor", `${configKey}.style.header.background-color`)
    makeColorSelector("js-msg-bgcolor", `${configKey}.style.body.background-color`)

    if (isNonCustomGroup) {
        btnDeleteEntry.prop("disabled", true).hide() // default groups are non deletable

        Databinding.bindListener(binding, "behaviour.language", async (value) => {
            const label = entry.find(".js-ffgroup-icon")
            const localization = await gobLocale.get(label.attr("data-gob-locale-id-text") as string, value)
            const name = gobConfig.get(`${configKey}.hiddenName`, "")
            const txt = Utility.formatString(localization, name)
            label.text(txt)
        })

        entry.find(".js-color-selections").addClass("gob-config-group_item-3-small-columns")

        entry.find(".js-mode-custom").hide()
    } else {
        btnGroupNameReset.prop("disabled", true).hide()

        entry.find(".js-color-selections").addClass("gob-config-group_item-2-small-columns")

        entry.find(".js-mode-noncustom").hide()
    }

    binding.loadBindings()
}

function makeNewGroup(addFront) {
    const groups = gobConfig.get(ConfigKeyData)
    const groupId = Utility.generateId(6, Object.keys(groups))

    groups[groupId] = gobConfig.getDefault(ConfigKeyDataTemplate)
    groups[groupId].id = groupId

    gobConfig.set(ConfigKeyData, groups)
    const sorting = gobConfig.get(ConfigKeyOrder)
    if (addFront) {
        sorting.unshift(groupId)
    } else {
        sorting.push(groupId)
    }
    gobConfig.set(ConfigKeyOrder, sorting)
}

const btnAddNewGroupTop = $("#cp-groups_addnewgrouptop")
btnAddNewGroupTop.on("click", function () {
    makeNewGroup(true)
})

const btnAddNewGroupBottom = $("#cp-groups_addnewgroupbot")
btnAddNewGroupBottom.on("click", function () {
    makeNewGroup(false)
})

tblGroups.accordion({
    heightStyle: "content",
    header: "> div > h4",
})
    .sortable({
        axis: "y",
        handle: "h4",
        stop: function (event, ui) {
            let order: string[] = []
            tblGroups.find(`> [${DataAttributeElementId}]`).each(function () {
                order.push($(this).attr(DataAttributeElementId) as string)
            })
            order = order.filter(e => e !== null && e !== undefined && e.length > 0)
            gobConfig.set(ConfigKeyOrder, order)
        }
    });

const binding = new Databinding.BindingContext(gobConfig)
Databinding.bindCheckbox(binding, $("#cp-groups_updateChat"))
binding.bindConfigListener(ConfigKeyOrder, Databinding.createConfigListener(() => populateGroupTable(), null, true), () => populateGroupTable())
binding.loadBindings()

const copyKeys = ["behaviour.groups.data", "behaviour.groups.sorting", "behaviour.groups.updateChat"]
Components.makeCopyProfileButton($("#cp-groups_copyprofile"), { configKeys: copyKeys })

//# sourceURL=config_groups.js