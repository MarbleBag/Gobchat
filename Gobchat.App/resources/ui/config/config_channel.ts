﻿/*******************************************************************************
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

import * as Databinding from "./../modules/Databinding.js"
import * as Components from "./../modules/Components.js"

const binding = new Databinding.BindingContext(gobConfig)

const table = $("#cchannel_channels > tbody")
const rowTemplate = $('#cchannel_template_channelentry')

function buildChannelEntry(channelData) {
    const rowEntry = $(rowTemplate.html()).appendTo(table)

    const lblName = rowEntry.find(".entry-label")
    const chkMention = rowEntry.find(".entry-mention")
    const chkRoleplay = rowEntry.find(".entry-roleplay")
    //const chkRangefilter = rowEntry.find(".entry-rangefilter")
    //const chkLog = rowEntry.find(".entry-log")
    const clrSelectorFG = rowEntry.find(".entry-color-forground")
    const btnResetFG = rowEntry.find(".entry-color-forground-reset")
    const clrSelectorBG = rowEntry.find(".entry-color-background")
    const btnResetBG = rowEntry.find(".entry-color-background-reset")

    lblName.attr("data-gob-locale-text", `${channelData.translationId}`)
    lblName.attr("data-gob-locale-title", `${channelData.tooltipId}`)

    Databinding.setConfigKey(chkMention, "behaviour.channel.mention")
    Databinding.setConfigKey(chkRoleplay, "behaviour.channel.roleplay")
    //Databinding.setConfigKey(chkRangefilter, "behaviour.channel.rangefilter")
    //Databinding.setConfigKey(chkLog, "behaviour.channel.log")

    const channelEnums = [].concat(channelData.chatChannel || [])
    if (channelEnums.length === 0) {
        chkMention.hide()
        chkRoleplay.hide()
        //chkRangefilter.hide()
        //chkLog.hide()
    } else {
        Databinding.bindCheckboxArray(binding, chkMention, channelEnums)
        Databinding.bindCheckboxArray(binding, chkRoleplay, channelEnums)
        //Databinding.bindCheckboxArray(binding, chkRangefilter, channelEnums)
        //Databinding.bindCheckboxArrayInverse(binding, chkLog, channelEnums)
    }

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

binding.initialize()


const copyKeys = new Set<string>(["behaviour.channel.roleplay", "behaviour.channel.mention"])
table.find(".entry-color-forground, .entry-color-background").each(function () {
    const configId = Databinding.getConfigKey(this)
    if (configId)
        copyKeys.add(configId)
})

Components.makeCopyProfileButton($("#cchannel_copyprofile"), { configKeys: Array.from(copyKeys) })

//# sourceURL=config_channel.js