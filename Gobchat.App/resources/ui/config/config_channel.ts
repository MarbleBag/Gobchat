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
import * as Locale from "/module/Locale"

const binding = new Databinding.BindingContext(gobConfig)

const table = $("#cp-channel_channel-table > tbody")
const rowTemplate = $('#cp-channel_template_channel-table_entry')

function buildChannelEntry(channelData) {
    const rowEntry = $(rowTemplate.html())
    rowEntry.appendTo(table)

    const lblName = rowEntry.find(".js-name")
    const clrSelectorFG = rowEntry.find(".js-color-forground")
    const btnResetFG = rowEntry.find(".js-color-forground-reset")
    const clrSelectorBG = rowEntry.find(".js-color-background")
    const btnResetBG = rowEntry.find(".js-color-background-reset")

    lblName.attr(Locale.HtmlAttribute.TextId, `${channelData.translationId}`)
    lblName.attr(Locale.HtmlAttribute.TooltipId, `${channelData.tooltipId}`)

    if (channelData.configId === null) {
        clrSelectorFG.parent().hide()
        clrSelectorBG.parent().hide()
    } else {
        Databinding.setConfigKey(clrSelectorFG, channelData.configId + ".color")
        Databinding.setConfigKey(clrSelectorBG, channelData.configId + ".background-color")

        Components.makeColorSelector(clrSelectorFG)
        Components.makeColorSelector(clrSelectorBG)

        Databinding.bindColorSelector(binding, clrSelectorFG)
        Databinding.bindColorSelector(binding, clrSelectorBG)
        Components.makeResetButton(btnResetFG, clrSelectorFG)
        Components.makeResetButton(btnResetBG, clrSelectorBG)
    }
}

buildChannelEntry({
    translationId: "main.chat.channel.general",
    configId: "style.channel.base",
    relevant: true
})

Object.entries(Gobchat.Channels).forEach((entry) => {
    const channelData = entry[1]
    if (!channelData.relevant)
        return
    buildChannelEntry(channelData)
})

binding.loadBindings()

const copyKeys = new Set<string>(["behaviour.channel.roleplay", "behaviour.channel.mention"])
table.find(".entry-color-forground, .entry-color-background").each(function () {
    const configId = Databinding.getConfigKey(this)
    if (configId)
        copyKeys.add(configId)
})

Components.makeCopyProfileButton($("#cp-channel_copyprofile"), { configKeys: Array.from(copyKeys) })

//# sourceURL=config_channel.js
