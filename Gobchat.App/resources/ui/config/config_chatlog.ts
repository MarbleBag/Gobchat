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

const chkEnableChahlog = $("#cp-chatlog_active")
const txtChatlogPath = $("#cp-chatlog_path")
const btnChatlogPathReset = $("#cp-chatlog_path_reset")
const btnChatlogPathSelect = $("#cp-chatlog_path_select")
const txtChatlogFormat = $("#cp-chatlog_format")
const selChatlogFormat = $("#cp-chatlog_format_selector")

const chatlogTable = $("#cp-chatlog_table > tbody")
const templateChatlogTableEntry = $('#cp-chatlog_template_table_entry')

Databinding.bindCheckbox(binding, chkEnableChahlog)
Components.makeResetButton(btnChatlogPathReset, txtChatlogPath)

txtChatlogPath.on("change", async function () {
    try { // show absolute path to user, but if possible, only store a relative path and/or symbolic link
        const currentPath = txtChatlogPath.val()
        const relCurrentPath = await GobchatAPI.getRelativeChatLogPath(currentPath)
        gobConfig.set(Databinding.getConfigKey(txtChatlogPath), relCurrentPath)
        const absCurrentPath = await GobchatAPI.getAbsoluteChatLogPath(relCurrentPath)
        txtChatlogPath.val(absCurrentPath)
    } catch (e) {
        console.error(e)
    }
})

binding.bindCallback(Databinding.getConfigKey(txtChatlogPath), async function (path) {
    try { // show absolute path to user
        const absCurrentPath = await GobchatAPI.getAbsoluteChatLogPath(path)
        txtChatlogPath.val(absCurrentPath)
    } catch (e) {
        console.error(e)
    }
})

btnChatlogPathSelect.on("click", async function () {
    try { // open directory selector in previously selected directory
        const relCurrentPath = gobConfig.get(Databinding.getConfigKey(txtChatlogPath))
        const absCurrentPath = await GobchatAPI.getAbsoluteChatLogPath(relCurrentPath)

        const absNewPath = await GobchatAPI.openDirectoryDialog(absCurrentPath)
        const relNewPath = await GobchatAPI.getRelativeChatLogPath(absNewPath) // only store a relative path and/or symbolic link
        gobConfig.set(Databinding.getConfigKey(txtChatlogPath), relNewPath)
    } catch (e) {
        console.error(e)
    }
})

Databinding.bindElement(binding, txtChatlogFormat)

binding.bindCallback(txtChatlogFormat, value => {
    selChatlogFormat.val(value)
    const selectedFormat = selChatlogFormat.val()
    if (selectedFormat === null)
        selChatlogFormat.val("")
})

selChatlogFormat.on("change", function () {
    const selectedFormat = $(this).val()
    if (selectedFormat.length > 0)
        txtChatlogFormat.val(selectedFormat).change()
})


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

    const id = `cp-chatlog_table_entry-${chatlogTable.children().length}`

    const entry = $(templateChatlogTableEntry.html())
    entry.appendTo(chatlogTable)

    entry.find(".js-label")
        .attr(Locale.HtmlAttribute.TextId, `${channelData.translationId}`)
        .attr(Locale.HtmlAttribute.TooltipId, `${channelData.tooltipId}`)
        .attr("for", id)

    const chkLog = entry.find(".js-checkbox")
        .attr("id", id)

    Databinding.bindCheckboxArrayInverse(binding, chkLog, channelEnums, { configKey: "behaviour.channel.log" })
}

binding.loadBindings()

{
    const configKeys = new Set<string>(["behaviour.channel.log"])
    $(`#cp-chatlog [${Databinding.HtmlAttribute.ConfigKey}]:not(.button)`).each(function () {
        const key = Databinding.getConfigKey(this)
        if (key !== null && key !== undefined && key.length > 0)
            configKeys.add(key)
    })
    Components.makeCopyProfileButton($("#cp-chatlog_copyprofile"), { configKeys: Array.from(configKeys) })
}
