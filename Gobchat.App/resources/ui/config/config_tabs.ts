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
import * as Chat from "/module/Chat"
import * as Dialog from "/module/Dialog"
import * as Locale from "/module/Locale"


const ConfigKeyOrder = "behaviour.chattabs.sorting"
const ConfigKeyData = "behaviour.chattabs.data"
const ConfigKeyDataTemplate = "behaviour.chattabs.data-template"
const DataAttributeElementId = "data-gob-entryid"
const JQueryDataKey = "configbinding"
const cssActive = "is-active"

const binding = new Databinding.BindingContext(gobConfig)

$("#cp-tabs_tabs-table_add").on("click", function () {
    const tabsById = gobConfig.get(ConfigKeyData)
    const id = Utility.generateId(6, Object.keys(tabsById))

    const newTab = gobConfig.getDefault(ConfigKeyDataTemplate)
    newTab.id = id
    newTab.visible = true
    newTab.name = `${newTab.name} ${Object.keys(tabsById).length}`

    tabsById[id] = newTab
    gobConfig.set(ConfigKeyData, tabsById)

    const sorting = gobConfig.get(ConfigKeyOrder)
    sorting.push(id)
    gobConfig.set(ConfigKeyOrder, sorting)
})

const tableTabs = $("#cp-tabs_tabs-table > tbody")
const $templateTableTabsEntry = $("#cp-tabs_template_tabs-table_entry")

function buildTableTabs() {
    tableTabs.children().each(function () {
        $(this).data<Databinding.BindingContext>(JQueryDataKey).clearBindings()
    })
    tableTabs.empty()

    const entryIds = gobConfig.get(ConfigKeyOrder)
    entryIds.forEach(id => buildTableTabsEntry(id))
}

function swapArrayIndex(arr, index1, index2){
    try {
        [arr[index1], arr[index2]] = [arr[index2], arr[index1]]
    } catch (e) {
        console.error(e)
    }
}

function buildTableTabsEntry(entryId) {
    const entry = $($templateTableTabsEntry.html())
    entry.attr(DataAttributeElementId, entryId)
    tableTabs.append(entry)

    const binding = new Databinding.BindingContext(gobConfig)
    entry.data(JQueryDataKey, binding)

    const configKey = `${ConfigKeyData}.${entryId}`

    const lblName = entry.find(".js-name")
    Databinding.bindElement(binding, lblName, { configKey: `${configKey}.name`})

    const chkVisible = entry.find(".js-visible")
    Databinding.bindCheckbox(binding, chkVisible, { configKey: `${configKey}.visible` })
    //chkVisible.on("click", function (event) { event.stopPropagation() })

    entry.find(".js-action-config").on("click", function (event) {
        buildConfigForTab(entryId)
    })

    const orderedEntryIds = gobConfig.get(ConfigKeyOrder, [])

    entry.find(".js-action-delete")
        .prop("disabled", orderedEntryIds.length <= 1)
        .on("click", async function (event) {
            const result = await Dialog.showConfirmationDialog({
                dialogText: "config.tabs.tbl.tabs.entry.action.delete.confirm",
            })

            if (result === 1) {
                try {
                    gobConfig.remove(configKey)
                    const order = gobConfig.get(ConfigKeyOrder)
                    _.remove(order, e => e === entryId)
                    gobConfig.set(ConfigKeyOrder, order)
                } catch (ex) {
                    console.error(ex)
                }
            }
        })
    
    entry.find(".js-action-moveup")
        .prop("disabled", orderedEntryIds.indexOf(entryId) === 0)
        .on("click", function (event) {
            const entryIds = gobConfig.get(ConfigKeyOrder)
            const idx = entryIds.indexOf(entryId)
            swapArrayIndex(entryIds, idx, idx - 1)
            gobConfig.set(ConfigKeyOrder, entryIds)
        })

    entry.find(".js-action-movedown")
        .prop("disabled", orderedEntryIds.indexOf(entryId) === orderedEntryIds.length - 1)
        .on("click", function (event) {
            const entryIds = gobConfig.get(ConfigKeyOrder)
            const idx = entryIds.indexOf(entryId)
            swapArrayIndex(entryIds, idx, idx + 1)
            gobConfig.set(ConfigKeyOrder, entryIds)
        })

    entry.on("click", function (event) {
        buildConfigForTab(entryId)
    })

    binding.loadBindings()
}

const tabConfigBinding = new Databinding.BindingContext(gobConfig)

function buildConfigForTab(tabId) {
    tableTabs.children(`.${cssActive}`).removeClass(cssActive)
    tableTabs.children(`[${DataAttributeElementId}=${tabId}]`).addClass(cssActive)

    tabConfigBinding.clearBindings()

    const tblChannels = $("#cp-tabs_channel-table > tbody")
    tblChannels.empty()

    const lblName = $("#cp-tabs_tab-config_name")
    Databinding.bindText(tabConfigBinding, lblName, { configKey: `${ConfigKeyData}.${tabId}.name` })

    const ckbMention = $("#cp-tabs_tab-config_mention")
    Databinding.bindCheckbox(tabConfigBinding, ckbMention, { configKey: `${ConfigKeyData}.${tabId}.formatting.mentions` })

    const ckbRoleplay = $("#cp-tabs_tab-config_roleplay")
    Databinding.bindCheckbox(tabConfigBinding, ckbRoleplay, { configKey: `${ConfigKeyData}.${tabId}.formatting.roleplay` })

    const ckbTimestamp = $("#cp-tabs_tab-config_timestamp")
    Databinding.bindCheckbox(tabConfigBinding, ckbTimestamp, { configKey: `${ConfigKeyData}.${tabId}.formatting.timestamps` })

    const ckbRangefilter = $("#cp-tabs_tab-config_rangefilter")
    Databinding.bindCheckbox(tabConfigBinding, ckbRangefilter, { configKey: `${ConfigKeyData}.${tabId}.formatting.rangefilter` })

    const templateTableChannelsEntry = $("#cp-tabs_template_channel-table_entry")
    Object.values(Gobchat.Channels).forEach((channel) => {
        if (!channel.relevant)
            return

        const entry = $(templateTableChannelsEntry.html())
        tblChannels.append(entry)

        entry.find(".js-label")
            .attr(Locale.HtmlAttribute.TextId, `${channel.translationId}`)
            .attr(Locale.HtmlAttribute.TooltipId, `${channel.tooltipId}`)

        const chkVisible = entry.find(".js-visible")
        Databinding.setConfigKey(chkVisible, `${ConfigKeyData}.${tabId}.channel.visible`)

        const channelEnums = ([] as Chat.ChatChannelEnum[]).concat(channel.chatChannel || [])
        if (channelEnums.length === 0) {
            chkVisible.hide()
        } else {
            Databinding.bindCheckboxArray(tabConfigBinding, chkVisible, channelEnums)
        }
    })

    gobLocale.updateElement(tblChannels)

    tabConfigBinding.loadBindings()
}

Databinding.bindListener(binding, ConfigKeyOrder, function (entryIds) {
    buildTableTabs()
    if (entryIds.length > 0)
        buildConfigForTab(entryIds[0])
})

Databinding.bindElement(binding, $("#cp-tabs_effects_message"), { elementToConfig: ($element) => parseInt($element.val()) })
Databinding.bindElement(binding, $("#cp-tabs_effects_mention"), { elementToConfig: ($element) => parseInt($element.val()) })

binding.loadBindings()

Components.makeCopyProfileButton($("#cp-tabs_copyprofile"),
    {
        configKeys: [ConfigKeyData, ConfigKeyOrder, "behaviour.chattabs.effect"]
    })


//# sourceURL=config_tabs.js