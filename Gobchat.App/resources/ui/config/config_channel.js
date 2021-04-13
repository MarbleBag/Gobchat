/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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

(function (undefined) {
    const binding = GobConfigHelper.makeDatabinding(gobconfig)

    const table = $("#cchannel_channels > tbody")
    const rowTemplate = $('#cchannel_template_channelentry')

    function buildChannelEntry(channelData) {
        const rowEntry = $(rowTemplate.html()).appendTo(table)

        const lblName = rowEntry.find(".entry-label")
        const chkMention = rowEntry.find(".entry-mention")
        const chkRoleplay = rowEntry.find(".entry-roleplay")
        const chkRangefilter = rowEntry.find(".entry-rangefilter")
        const clrSelectorFG = rowEntry.find(".entry-color-forground")
        const btnResetFG = rowEntry.find(".entry-color-forground-reset")
        const clrSelectorBG = rowEntry.find(".entry-color-background")
        const btnResetBG = rowEntry.find(".entry-color-background-reset")

        lblName.attr("data-gob-locale-text", `${channelData.translationId}`)
        lblName.attr("data-gob-locale-title", `${channelData.tooltipId}`)

        GobConfigHelper.setConfigKey(chkMention, "behaviour.channel.mention")
        GobConfigHelper.setConfigKey(chkRoleplay, "behaviour.channel.roleplay")
        GobConfigHelper.setConfigKey(chkRangefilter, "behaviour.channel.rangefilter")

        const channelEnums = [].concat(channelData.chatChannel || [])
        if (channelEnums.length === 0) {
            chkMention.hide()
            chkRoleplay.hide()
            chkRangefilter.hide()
        } else {
            GobConfigHelper.bindCheckboxArray(binding, chkMention, channelEnums)
            GobConfigHelper.bindCheckboxArray(binding, chkRoleplay, channelEnums)
            GobConfigHelper.bindCheckboxArray(binding, chkRangefilter, channelEnums)
        }

        if (channelData.configId === null) {
            clrSelectorFG.parent().hide()
            clrSelectorBG.parent().hide()
        } else {
            GobConfigHelper.setConfigKey(clrSelectorFG, channelData.configId + ".color")
            GobConfigHelper.setConfigKey(clrSelectorBG, channelData.configId + ".background-color")

            GobConfigHelper.makeColorSelector(clrSelectorFG)
            GobConfigHelper.makeColorSelector(clrSelectorBG)

            GobConfigHelper.bindColorSelector(binding, clrSelectorFG)
            GobConfigHelper.bindColorSelector(binding, clrSelectorBG)

            btnResetFG.attr("disabled", clrSelectorFG.attr("disabled"))
            btnResetBG.attr("disabled", clrSelectorBG.attr("disabled"))
            btnResetFG.on("click", event => gobconfig.reset(GobConfigHelper.getConfigKey(clrSelectorFG)))
            btnResetBG.on("click", event => gobconfig.reset(GobConfigHelper.getConfigKey(clrSelectorBG)))
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

    const btnCopyProfile = $("#cchannel_copyprofile")
    const copyKeys = ["behaviour.channel.mention", "behaviour.channel.roleplay", "behaviour.channel.rangefilter"]
    copyKeys.push("style.channel.base" + ".color")
    copyKeys.push("style.channel.base" + ".background-color")

    Object.entries(Gobchat.Channels).forEach((entry) => {
        const channelData = entry[1]
        if (!channelData.relevant || channelData.configId == null) return
        copyKeys.push(channelData.configId + ".color")
        copyKeys.push(channelData.configId + ".background-color")
    })

    GobConfigHelper.makeCopyProfileButton(btnCopyProfile, { configKeys: copyKeys })
}());

//# sourceURL=config_channel.js