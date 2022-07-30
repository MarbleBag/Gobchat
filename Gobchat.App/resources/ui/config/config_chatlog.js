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

    GobConfigHelper.bindCheckbox(binding, $("#cchatlog_active"))

    const $txtChatlogPath = $("#cchatlog_path")
    GobConfigHelper.makeResetButton($("#cchatlog_path_reset"), $txtChatlogPath)

    $txtChatlogPath.on("change", function () {
        (async () => {
            try {
                let newPath = $txtChatlogPath.val()
                const parsedPath = await GobchatAPI.getRelativeChatLogPath(newPath)
                gobconfig.set(GobConfigHelper.getConfigKey($txtChatlogPath), parsedPath)
                newPath = await GobchatAPI.getAbsoluteChatLogPath(parsedPath)
                $txtChatlogPath.val(newPath)
            } catch (e) {
                console.error(e)
            }
        })();
    })

    binding.bindConfigListener(GobConfigHelper.getConfigKey($txtChatlogPath), path => {
        (async () => {
            try {
                path = await GobchatAPI.getAbsoluteChatLogPath(path)
                $txtChatlogPath.val(path)
            } catch (e) {
                console.error(e)
            }
        })();
    })

    $("#cchatlog_path_select").on("click", function () {
        (async () => {
            try {
                let oldPath = gobconfig.get(GobConfigHelper.getConfigKey($txtChatlogPath))
                oldPath = await GobchatAPI.getAbsoluteChatLogPath(oldPath)
                let newPath = await GobchatAPI.openDirectoryDialog(oldPath)
                newPath = await GobchatAPI.getRelativeChatLogPath(newPath)
                gobconfig.set(GobConfigHelper.getConfigKey($txtChatlogPath), newPath)
            } catch (e) {
                console.error(e)
            }
        })();
    })

    GobConfigHelper.bindElement(binding, $("#cchatlog_format"))

    binding.bindConfigListener($("#cchatlog_format"), value => {
        $("#cchatlog_format_selector").val(value)
        const selectedFormat = $("#cchatlog_format_selector").val()
        if (selectedFormat === null)
            $("#cchatlog_format_selector").val("")
    })

    $("#cchatlog_format_selector").on("change", function () {
        const selectedFormat = $(this).val()
        if (selectedFormat.length > 0)
            $("#cchatlog_format").val(selectedFormat).change()
    })


    const $tbl1 = $("#cchatlog_channels1 > tbody")
    const $tbl2 = $("#cchatlog_channels2 > tbody")

    const $rowTemplate = $('#cchatlog_table_template')

    function buildChannelEntry(channelData) {
        const $table = $tbl1.children().length <= $tbl2.children().length ? $tbl1 : $tbl2
        const $rowEntry = $($rowTemplate.html()).appendTo($table)

        const $lblName = $rowEntry.find(".js-label")
        const $chkLog = $rowEntry.find(".js-log")

        $lblName.attr("data-gob-locale-text", `${channelData.translationId}`)
        $lblName.attr("data-gob-locale-title", `${channelData.tooltipId}`)

        GobConfigHelper.setConfigKey($chkLog, "behaviour.channel.log")

        const channelEnums = [].concat(channelData.chatChannel || [])
        if (channelEnums.length === 0) {
            $chkLog.hide()
        } else {
            GobConfigHelper.bindCheckboxArrayInverse(binding, $chkLog, channelEnums)
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

    GobConfigHelper.makeCopyProfileButton($("#cchatlog_copyprofile"), { configKeys: ["behaviour.channel.log"] })
}());

//# sourceURL=config_logging.js