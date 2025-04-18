/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
import * as Locale from "/module/Locale"

const binding = new Databinding.BindingContext(gobConfig)

const parseNonNegativeNumber = (element: JQuery) => {
    const value = Utility.toInt(element.val())
    if (value === null)
        return undefined
    return value >= 0 ? value : undefined
}

const txtCutOff = $("#cp-rangefilter_cutoff")
const txtFadeOut = $("#cp-rangefilter_fadeout")
const txtStartOpacity = $("#cp-rangefilter_startopacity")
const txtEndOpacity = $("#cp-rangefilter_endopacity")

const channelTable = $("#cp-rangefilter_channel-table > tbody")
const channelTableEntryTemplate = $("#cp-rangefilter_template_channel-table_entry")

Databinding.bindElement(binding, txtCutOff, { elementToConfig: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_cutoff_reset"), txtCutOff)

Databinding.bindElement(binding, txtFadeOut, { elementToConfig: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_fadeout_reset"), txtFadeOut)

Databinding.bindElement(binding, txtStartOpacity, { elementToConfig: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_startopacity_reset"), txtStartOpacity)

Databinding.bindElement(binding, txtEndOpacity, { elementToConfig: parseNonNegativeNumber })
Components.makeResetButton($("#cp-rangefilter_endopacity_reset"), txtEndOpacity)

Object.values(Gobchat.Channels).forEach(channelData => {
    if (!channelData.relevant)
        return
    addEntryToTable(channelData)
})

function addEntryToTable(channelData: Chat.Channel) {
    const channelEnums = ([] as Chat.ChatChannelEnum[]).concat(channelData.chatChannel || [])
    if (channelEnums.length === 0)
        return // channel is not associated with any ingame channel

    const id = `cp-rangefilter_channel-table_entry-${channelTable.children().length}`

    const entry = $(channelTableEntryTemplate.html())
        .appendTo(channelTable)

    entry.find(".js-label")
        .attr(Locale.HtmlAttribute.TextId, `${channelData.translationId}`)
        .attr(Locale.HtmlAttribute.TooltipId, `${channelData.tooltipId}`)
        .prop("for", id)

    const ckbApply = entry.find(".js-checkbox")
        .prop("id", id)

    Databinding.setConfigKey(ckbApply, "behaviour.channel.rangefilter")
    Databinding.bindCheckboxArray(binding, ckbApply, channelEnums)
}

binding.loadBindings()

Components.makeCopyProfileButton($("#cp-rangefilter_copyprofile"),
    {
        configKeys: () => {
            return $("#cp-rangefilter").find(`input[${Databinding.HtmlAttribute.ConfigKey}]`).map((i, e) => Databinding.getConfigKey(e)!).get()
        }
    })

