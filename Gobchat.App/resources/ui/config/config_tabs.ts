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
import { Locale } from "../modules/GobModule";


const keyTabsData = "behaviour.chattabs.data"
const keyTabsSorting = "behaviour.chattabs.sorting"
const pagebinding = new Databinding.BindingContext(gobConfig)

$("#ctabs_newtab").on("click", function () {
    const data = gobConfig.get(keyTabsData)
    const id = Utility.generateId(6, Object.keys(data))

    const newConfig = gobConfig.getDefault(`${keyTabsData}.default`)
    newConfig.id = id
    newConfig.visible = true
    newConfig.name = `${newConfig.name} ${Object.keys(data).length}`

    data[id] = newConfig
    gobConfig.set(keyTabsData, data)

    const sorting = gobConfig.get(keyTabsSorting)
    sorting.push(id)
    gobConfig.set(keyTabsSorting, sorting)
})

const $tableTabs = $("#ctabs_tabs > tbody")

function buildTableTabs() {
    $tableTabs.find("[data-gob-entryid]").each(function () {
        $(this).data<Databinding.BindingContext>("configbinding").clear()
    })
    $tableTabs.empty()

    const entryIds = gobConfig.get(keyTabsSorting)
    entryIds.forEach(id => buildTableTabsEntry(id))
}

const swapArrayIndex = (arr, index1, index2) => {
    try {
        [arr[index1], arr[index2]] = [arr[index2], arr[index1]]
    } catch (e) {
        console.error(e)
    }
}

const $templateTableTabsEntry = $("#ctabs_template_tblentry")
function buildTableTabsEntry(entryId) {
    const binding = new Databinding.BindingContext(gobConfig)

    const entryConfigKey = `${keyTabsData}.${entryId}`

    const $entry = $($templateTableTabsEntry.html())
        .attr("data-gob-entryid", entryId)
        .data("configbinding", binding)
        .appendTo($tableTabs)

    const $entryName = $entry.find(".js-name")
    Databinding.setConfigKey($entryName, `${entryConfigKey}.name`)
    Databinding.bindElement(binding, $entryName)

    const $entryVisible = $entry.find(".js-visible")
    Databinding.setConfigKey($entryVisible, `${entryConfigKey}.visible`)
    Databinding.bindCheckbox(binding, $entryVisible)
    $entryVisible.on("click", function (event) { event.stopPropagation() })

    $entry.find(".js-action-config").on("click", function (event) {
        event.stopPropagation()
        buildConfig(entryId)
    })

    $entry.find(".js-action-delete")
        .toggleClass("disabled", gobConfig.get(keyTabsSorting, []).length <= 1)
        .on("click", function (event) {
            event.stopPropagation()
            {
                (async () => {
                    const result = await Dialog.showConfirmationDialog({
                        dialogText: "config.tabs.tabtable.action.delete.confirm",
                    })

                    if (result === 1) {
                        try {
                            gobConfig.remove(entryConfigKey)
                            const order = gobConfig.get(keyTabsSorting)
                            _.remove(order, e => e === entryId)
                            gobConfig.set(keyTabsSorting, order)
                        } catch (ex) {
                            console.error(ex)
                        }
                    }
                })()
            }
        })

    const entryIds = gobConfig.get(keyTabsSorting)
    $entry.find(".js-action-mup")
        .toggleClass("disabled", entryIds.indexOf(entryId) === 0)
        .on("click", function (event) {
            event.stopPropagation()
            const entryIds = gobConfig.get(keyTabsSorting)
            const idx = entryIds.indexOf(entryId)
            swapArrayIndex(entryIds, idx, idx - 1)
            gobConfig.set(keyTabsSorting, entryIds)
        })

    $entry.find(".js-action-mdown")
        .toggleClass("disabled", entryIds.indexOf(entryId) === entryIds.length - 1)
        .on("click", function (event) {
            event.stopPropagation()
            const entryIds = gobConfig.get(keyTabsSorting)
            const idx = entryIds.indexOf(entryId)
            swapArrayIndex(entryIds, idx, idx + 1)
            gobConfig.set(keyTabsSorting, entryIds)
        })

    $entry.on("click", function (event) {
        event.stopPropagation()
        buildConfig(entryId)
    })

    binding.initialize()
}

function buildConfig(id) {
    $tableTabs.children(".active").removeClass("active")
    $tableTabs.children(`[data-gob-entryid=${id}]`).addClass("active")

    const $config = $("#ctabs_tab_config")
    let binding = $config.data<Databinding.BindingContext>("configbinding")
    if (binding) binding.clear()

    const $tbl1 = $("#ctabs_tab_channel1 > tbody")
    $tbl1.empty()
    const $tbl2 = $("#ctabs_tab_channel2 > tbody")
    $tbl2.empty()

    if (!id) {
        $("#ctabs_config_tabname").html("")
        return
    }

    $("#ctabs_config_tabname").html(
        Utility.encodeHtmlEntities(
            gobConfig.get(`${keyTabsData}.${id}`).name
        )
    )

    binding = new Databinding.BindingContext(gobConfig)
    $config.data("configbinding", binding)

    const ckbMention = $("#ctabs_formatting_mention")
    Databinding.setConfigKey(ckbMention, `${keyTabsData}.${id}.formatting.mentions`)
    Databinding.bindCheckbox(binding, ckbMention)

    const ckbRoleplay = $("#ctabs_formatting_roleplay")
    Databinding.setConfigKey(ckbRoleplay, `${keyTabsData}.${id}.formatting.roleplay`)
    Databinding.bindCheckbox(binding, ckbRoleplay)

    const ckbTimestamp = $("#ctabs_formatting_timestamp")
    Databinding.setConfigKey(ckbTimestamp, `${keyTabsData}.${id}.formatting.timestamps`)
    Databinding.bindCheckbox(binding, ckbTimestamp)

    const ckbRangefilter = $("#ctabs_formatting_rangefilter")
    Databinding.setConfigKey(ckbRangefilter, `${keyTabsData}.${id}.formatting.rangefilter`)
    Databinding.bindCheckbox(binding, ckbRangefilter)

    const $rowTemplate = $("#ctabs_template_channelentry")
    function buildChannelEntry(channelData) {
        const $table = $tbl1.children().length <= $tbl2.children().length ? $tbl1 : $tbl2
        const $rowEntry = $($rowTemplate.html()).appendTo($table)

        $rowEntry.find(".js-label")
            .attr(Locale.AttributeTextKey, `${channelData.translationId}`)
            .attr(Locale.AttributeTooltipKey, `${channelData.tooltipId}`)

        const $chkVisible = $rowEntry.find(".js-visible")
        Databinding.setConfigKey($chkVisible, `${keyTabsData}.${id}.channel.visible`)

        const channelEnums = [].concat(channelData.chatChannel || [])
        if (channelEnums.length === 0) {
            $chkVisible.hide()
        } else {
            Databinding.bindCheckboxArray(binding, $chkVisible, channelEnums)
        }
    }

    Object.entries(Gobchat.Channels).forEach((entry) => {
        const channelData = entry[1]
        if (!channelData.relevant) return
        buildChannelEntry(channelData)
    })

    gobLocale.updateElement($tbl1)
    gobLocale.updateElement($tbl2)
    binding.initialize()
}

Databinding.bindListener(pagebinding, keyTabsSorting, function (entryIds) {
    buildTableTabs()
    if (entryIds.length > 0)
        buildConfig(entryIds[0])
    else
        buildConfig(null)
})

Databinding.bindElement(pagebinding, $("#ctabs_effects_message"), { elementGetAccessor: ($element) => parseInt($element.val()) })
Databinding.bindElement(pagebinding, $("#ctabs_effects_mention"), { elementGetAccessor: ($element) => parseInt($element.val()) })

pagebinding.initialize()

//doesn't work. Old tabs are keept around?
Components.makeCopyProfileButton($("#ctabs_copyprofile"),
    {
        configKeys: [keyTabsData, keyTabsSorting, "behaviour.chattabs.effect"]
    })


//# sourceURL=config_tabs.js