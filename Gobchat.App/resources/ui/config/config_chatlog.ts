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

const binding = new Databinding.BindingContext(gobConfig)

Databinding.bindCheckbox(binding, $("#cp-chatlog_active"))

const $txtChatlogPath = $("#cp-chatlog_path")
Components.makeResetButton($("#cp-chatlog_path_reset"), $txtChatlogPath)

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

$("#cp-chatlog_path_select").on("click", async function () {
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

Databinding.bindElement(binding, $("#cp-chatlog_format"))

binding.bindConfigListener($("#cp-chatlog_format"), value => {
    $("#cp-chatlog_format_selector").val(value)
    const selectedFormat = $("#cp-chatlog_format_selector").val()
    if (selectedFormat === null)
        $("#cp-chatlog_format_selector").val("")
})

$("#cp-chatlog_format_selector").on("change", function () {
    const selectedFormat = $(this).val()
    if (selectedFormat.length > 0)
        $("#cp-chatlog_format").val(selectedFormat).change()
})


const $table = $("#cp-chatlog_table > tbody")
const $tableEntryTemplate = $('#cp-chatlog_template_table_entry')

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

    const id = `cp-chatlog_table_entry-${$table.children().length}`

    const $entry = $($tableEntryTemplate.html())
    $entry.appendTo($table)

    $entry.find(".js-label")
        .attr("data-gob-locale-text", `${channelData.translationId}`)
        .attr("data-gob-locale-title", `${channelData.tooltipId}`)
        .attr("for", id)

    const $chkLog = $entry.find(".js-checkbox")
        .attr("id", id)

    Databinding.setConfigKey($chkLog, "behaviour.channel.log")

    Databinding.bindCheckboxArrayInverse(binding, $chkLog, channelEnums)
}

binding.initialize()

{
    const configKeys = new Set<string>()
    $(`#cp-chatlog [${Databinding.DataAttributeConfigKey}]:not(.button)`).each(function () {
        const key = Databinding.getConfigKey(this)
        if (key !== null && key !== undefined && key.length > 0)
            configKeys.add(key)
    })
    Components.makeCopyProfileButton($("#cp-chatlog_copyprofile"), { configKeys: Array.from(configKeys) })
}
