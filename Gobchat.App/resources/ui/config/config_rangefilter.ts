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
import * as Utility from "/module/CommonUtility"
import * as Components from "/module/Components"
import * as Chat from "/module/Chat"

const binding = new Databinding.BindingContext(gobConfig)

Databinding.bindCheckbox(binding, $("#cp-rangefilter_mention"))

const parseNonNegativeNumber = (element: JQuery) => {
    const value = parseInt(element.val())
    return Utility.isNumber(value) && value >= 0 ? value : undefined
}

Databinding.bindElement(binding, $("#cp-rangefilter_cutoff"), { elementGetAccessor: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_cutoff_reset"))

Databinding.bindElement(binding, $("#cp-rangefilter_fadeout"), { elementGetAccessor: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_fadeout_reset"))

Databinding.bindElement(binding, $("#cp-rangefilter_startopacity"), { elementGetAccessor: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_startopacity_reset"))

Databinding.bindElement(binding, $("#cp-rangefilter_endopacity"), { elementGetAccessor: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_endopacity_reset"))

const $table = $("#cp-rangefilter_table > tbody")
const $tableEntryTemplate = $('#cp-rangefilter_template_table_entry')

Object.entries(Gobchat.Channels).forEach((entry) => {
    const channelData = entry[1]
    if (!channelData.relevant)
        return
    addEntryToTable(channelData)
})

function addEntryToTable(channelData: Chat.Channel) {
    const channelEnums = ([] as Chat.ChatChannelEnum[]).concat(channelData.chatChannel || [])
    if (channelEnums.length === 0)
        return // channel is not associated with any ingame channel

    const $entry = $($tableEntryTemplate.html())
        .appendTo($table)

    $entry.find(".js-label")
        .attr("data-gob-locale-text", `${channelData.translationId}`)
        .attr("data-gob-locale-title", `${channelData.tooltipId}`)

    const $ckbApplyFilter = $entry.find(".js-filter")
    Databinding.setConfigKey($ckbApplyFilter, "behaviour.channel.rangefilter")
    Databinding.bindCheckboxArray(binding, $ckbApplyFilter, channelEnums)
}

binding.initialize()

{
    const configKeys = new Set<string>()
    $(`#cp-rangefilter [${Databinding.DataAttributeConfigKey}]:not(.button)`).each(function () {
        const key = Databinding.getConfigKey(this)
        if (key && key.length > 0)
            configKeys.add(key)
    })

    const btnCopyProfile = $("#cp-rangefilter_copyprofile")
    Components.makeCopyProfileButton(btnCopyProfile, { configKeys: Array.from(configKeys) })
}

