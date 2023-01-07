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

import * as Databinding from "./../modules/Databinding.js"
import * as Components from "./../modules/Components.js"

const binding = new Databinding.BindingContext(gobConfig)

Databinding.bindCheckbox(binding, $("#c_chatlog_active"))

const $txtChatlogPath = $("#c_chatlog_path")
Components.makeResetButton($("#c_chatlog_path_reset"), $txtChatlogPath)

$txtChatlogPath.on("change", async function () {
    try { // show absolute path to user, but if possible, only store a relative path and/or symbolic link
        const currentPath = $txtChatlogPath.val()
        const relCurrentPath = await GobchatAPI.getRelativeChatLogPath(currentPath)
        gobConfig.set(Databinding.getConfigKey($txtChatlogPath), relCurrentPath)
        const absCurrentPath = await GobchatAPI.getAbsoluteChatLogPath(relCurrentPath)
        $txtChatlogPath.val(absCurrentPath)
    } catch (e) {
        console.error(e)
    }
})

binding.bindConfigListener(Databinding.getConfigKey($txtChatlogPath), async function (path) {
    try { // show absolute path to user
        const absCurrentPath = await GobchatAPI.getAbsoluteChatLogPath(path)
        $txtChatlogPath.val(absCurrentPath)
    } catch (e) {
        console.error(e)
    }
})

$("#c_chatlog_path_select").on("click", async function () {
    try { // open directory selector in previously selected directory
        const relCurrentPath = gobConfig.get(Databinding.getConfigKey($txtChatlogPath))
        const absCurrentPath = await GobchatAPI.getAbsoluteChatLogPath(relCurrentPath)

        const absNewPath = await GobchatAPI.openDirectoryDialog(absCurrentPath)
        const relNewPath = await GobchatAPI.getRelativeChatLogPath(absNewPath) // only store a relative path and/or symbolic link
        gobConfig.set(Databinding.getConfigKey($txtChatlogPath), relNewPath)
    } catch (e) {
        console.error(e)
    }
})

Databinding.bindElement(binding, $("#c_chatlog_format"))

binding.bindConfigListener($("#c_chatlog_format"), value => {
    $("#c_chatlog_format_selector").val(value)
    const selectedFormat = $("#c_chatlog_format_selector").val()
    if (selectedFormat === null)
        $("#c_chatlog_format_selector").val("")
})

$("#c_chatlog_format_selector").on("change", function () {
    const selectedFormat = $(this).val()
    if (selectedFormat.length > 0)
        $("#c_chatlog_format").val(selectedFormat).change()
})


const $table1 = $("#c_chatlog_table-1 > tbody")
const $table2 = $("#c_chatlog_table-2 > tbody")
const $tableEntryTemplate = $('#c_chatlog_template_tableentry')

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

    const $chkLog = $entry.find(".js-log")
    Databinding.setConfigKey($chkLog, "behaviour.channel.log")

    Databinding.bindCheckboxArrayInverse(binding, $chkLog, channelEnums)
}

binding.initialize()

{
    const configKeys = new Set<string>()
    $(`#c_chatlog [${Databinding.DataAttributeConfigKey}]:not(.button)`).each(function () {
        const key = Databinding.getConfigKey(this)
        if (key !== null && key !== undefined && key.length > 0)
            configKeys.add(key)
    })
    Components.makeCopyProfileButton($("#c_cgatlog_copyprofile"), { configKeys: Array.from(configKeys) })
}
