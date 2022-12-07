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

(function (undefined) {
    const binding = GobConfigHelper.makeDatabinding(gobconfig)

    GobConfigHelper.bindCheckbox(binding, $("#c_rangefilter_mention"))

    const parseNumber = ($element) => {
        const value = parseInt($element.val())
        return Gobchat.isNumber(value) && value >= 0 ? value : undefined
    }

    GobConfigHelper.bindElement(binding, $("#c_rangefilter_cutoff"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#c_rangefilter_cutoff_reset"))

    GobConfigHelper.bindElement(binding, $("#c_rangefilter_fadeout"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#c_rangefilter_fadeout_reset"))

    GobConfigHelper.bindElement(binding, $("#c_rangefilter_startopacity"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#c_rangefilter_startopacity_reset"))

    GobConfigHelper.bindElement(binding, $("#c_rangefilter_endopacity"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#c_rangefilter_endopacity_reset"))

    const $table1 = $("#c_rangefilter_table-1 > tbody")
    const $table2 = $("#c_rangefilter_table-2 > tbody")
    const $tableEntryTemplate = $('#c_rangefilter_template_tableentry')

    Object.entries(Gobchat.Channels).forEach((entry) => {
        const channelData = entry[1]
        if (!channelData.relevant)
            return
        addEntryToTable(channelData)
    })

    function addEntryToTable(channelData) {
        const channelEnums = [].concat(channelData.chatChannel || [])
        if (channelEnums.length === 0)
            return // channel is not associated with any ingame channel

        const $entry = $($tableEntryTemplate.html())
        const $table = $table1.children().length <= $table2.children().length ? $table1 : $table2
        $table.append($entry) // append alternately

        $entry.find(".js-label")
            .attr("data-gob-locale-text", `${channelData.translationId}`)
            .attr("data-gob-locale-title", `${channelData.tooltipId}`)

        const $ckbApplyFilter = $entry.find(".js-filter")
            .databindKey("behaviour.channel.rangefilter")

        GobConfigHelper.bindCheckboxArray(binding, $ckbApplyFilter, channelEnums)
    }

    binding.initialize()

    {
        const configKeys = []
        $(`#c_rangefilter [${GobConfigHelper.ConfigKeyAttribute}]:not(.button)`).each(function () {
            const key = GobConfigHelper.getConfigKey(this)
            if (key !== null && key !== undefined && key.length > 0 && !_.includes(configKeys, key))
                configKeys.push(key)
        })

        const btnCopyProfile = $("#c_rangefilter_copyprofile")
        GobConfigHelper.makeCopyProfileButton(btnCopyProfile, { configKeys: configKeys })
    }

}());