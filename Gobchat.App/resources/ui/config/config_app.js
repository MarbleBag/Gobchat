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

(async function () {
    const binding = GobConfigHelper.makeDatabinding(gobconfig)

    GobConfigHelper.bindElement(binding, $("#capp_language"))

    GobConfigHelper.bindElement(binding, $("#capp_theme"))

    // setup checkboxes
    GobConfigHelper.bindCheckbox(binding, $("#capp_chatlog_active"))

    const $txtChatlogPath = $("#capp_chatlog_path")
    GobConfigHelper.makeResetButton($("#capp_chatlog_path_reset"), $txtChatlogPath)

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

    $("#capp_chatlog_path_select").on("click", function () {
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

    GobConfigHelper.bindElement(binding, $("#capp_chatlog_format"))

    binding.bindConfigListener($("#capp_chatlog_format"), value => {
        $("#capp_chatlog_format_selector").val(value)
        const selectedFormat = $("#capp_chatlog_format_selector").val()
        if (selectedFormat === null)
            $("#capp_chatlog_format_selector").val("")
    })

    $("#capp_chatlog_format_selector").on("change", function () {
        const selectedFormat = $(this).val()
        if (selectedFormat.length > 0)
            $("#capp_chatlog_format").val(selectedFormat).change()
    })

    GobConfigHelper.bindCheckbox(binding, $("#capp_autodetectemote"))

    GobConfigHelper.bindCheckbox(binding, $("#capp_userMention"))

    GobConfigHelper.bindCheckbox(binding, $("#capp_hide"))

    const $ckbUpdate = $("#capp_checkupdates")
    GobConfigHelper.bindCheckbox(binding, $ckbUpdate)
    binding.bindConfigListener(GobConfigHelper.getConfigKey($ckbUpdate), value => {
        //ckbBetaUpdate.attr("disabled", !value)
        $("[for='capp_checkupdates']").toggleClass("disabled", !value)
    })

    GobConfigHelper.bindCheckbox(binding, $("#capp_checkbetaupdates"))

    {
        const available = await GobchatAPI.isFeaturePlayerLocationAvailable()
        $("#capp_characterlocations_feature").toggle(!available)
    }

    GobConfigHelper.bindCheckbox(binding, $("#capp_actor_updateActive"))

    GobConfigHelper.bindCheckbox(binding, $("#capp_rangefilter_mention"))

    // setup font group
    // setup font family
    GobConfigHelper.bindElement(binding, $("#capp_font_family"))
    GobConfigHelper.makeResetButton($("#capp_font_family_reset"))

    const $dpdProcessSelector = $("#capp_process_selector")
    $("#capp_process_selector_refresh").on("click", function () {
        const $icon = $("#capp_process_selector_refresh").find("svg");
        (async () => {
            try {
                //$icon.addClass("fa-spin")

                const defaultElement = $dpdProcessSelector.find("[value='-1']")
                const previousSelected = $dpdProcessSelector.val()
                $dpdProcessSelector.empty().append(defaultElement)

                const availableProcesses = await GobchatAPI.getAttachableFFXIVProcesses()
                for (const processId of availableProcesses)
                    $dpdProcessSelector.append(new Option(`FFXIV: ${processId}`, processId))

                if ($dpdProcessSelector.find(`[value='${previousSelected}'`).length > 0) {
                    $dpdProcessSelector.val(previousSelected)
                } else {
                    $dpdProcessSelector.val("-1")
                    await GobchatAPI.attachToFFXIVProcess(-1)
                }

                //$icon.removeClass("fa-spin")

                await process_UpdateLabel()
            } catch (e) {
                console.error(e)
            }
        })();
    })

    let process_IntervalTimer = 0
    async function process_UpdateLabel() {
        try {
            const txtSearch = await goblocale.get("config.app.process.info.search")
            const txtNotConnected = await goblocale.get("config.app.process.info.notconnected")
            const txtConnectedTo = await goblocale.get("config.app.process.info.connected")

            const $txtLabel = $("#capp_process_info")
            const $icon = $("#capp_process_selector_link").find("svg")

            async function updateLabel() {
                try {
                    const connectionInfo = await GobchatAPI.getAttachedFFXIVProcess()
                    const connectionState = connectionInfo.Item1 //0 - none, 1 - connected, 2 - not found, 3 - searching
                    const processId = connectionInfo.Item2

                    if (connectionState === 0) {
                    } else if (connectionState === 1) {
                        $txtLabel.text(Gobchat.formatString(txtConnectedTo, processId));
                        $icon.removeClass("fa-spin")
                        clearInterval(process_IntervalTimer)
                        process_IntervalTimer = 0
                    } else if (connectionState === 2) {
                        $txtLabel.text(txtNotConnected);
                    } else if (connectionState === 3) {
                        $txtLabel.text(txtSearch);
                    }
                } catch (e) {
                    console.error(e)
                }
            }

            clearInterval(process_IntervalTimer)
            process_IntervalTimer = setInterval(updateLabel, 1000)
        } catch (e) {
            console.error(e)
            throw e
        }
    }

    $("#capp_process_selector_link").on("click", function () {
        (async () => {
            try {
                $("#capp_process_selector_link").find("svg").addClass("fa-spin")

                const processId = $dpdProcessSelector.val()
                if (processId != null && processId != undefined)
                    GobchatAPI.attachToFFXIVProcess(parseInt(processId))

                await process_UpdateLabel()
            } catch (e) {
                console.error(e)
            }
        })();
    })

    const parseNumber = ($element) => {
        const value = parseInt($element.val())
        return Gobchat.isNumber(value) && value >= 0 ? value : undefined
    }

    GobConfigHelper.bindElement(binding, $("#capp_rangefilter_cutoff"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#capp_rangefilter_cutoff_reset"))

    GobConfigHelper.bindElement(binding, $("#capp_rangefilter_fadeout"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#capp_rangefilter_fadeout_reset"))

    GobConfigHelper.bindElement(binding, $("#capp_rangefilter_startopacity"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#capp_rangefilter_startopacity_reset"))

    GobConfigHelper.bindElement(binding, $("#capp_rangefilter_endopacity"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#capp_rangefilter_endopacity_reset"))

    GobConfigHelper.bindElement(binding, $("#capp_frame_x"), { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, $("#capp_frame_y"), { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, $("#capp_frame_height"), { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, $("#capp_frame_width"), { elementGetAccessor: parseNumber })

    const clrChatboxBackground = $("#capp_chatbox_backgroundcolor")
    GobConfigHelper.makeColorSelector(clrChatboxBackground)
    GobConfigHelper.bindColorSelector(binding, clrChatboxBackground)
    GobConfigHelper.makeResetButton($("#capp_chatbox_backgroundcolor_reset"))

    // setup font size
    GobConfigHelper.bindElement(binding, $("#capp_font_size"), {
        elementGetAccessor: ($element) => Gobchat.isNumber($element.val()) ? Math.round($element.val()) + "px" : ($element.val() || "medium")
    })

    const clrSearchMarked = $("#capp_search_marked")
    GobConfigHelper.makeColorSelector(clrSearchMarked)
    GobConfigHelper.bindColorSelector(binding, clrSearchMarked)
    GobConfigHelper.makeResetButton($("#capp_search_marked_reset"))

    const clrSearchSelected = $("#capp_search_selected")
    GobConfigHelper.makeColorSelector(clrSearchSelected)
    GobConfigHelper.bindColorSelector(binding, clrSearchSelected)
    GobConfigHelper.makeResetButton($("#capp_search_selected_reset"))

    // setup hotkey group
    // setup hotkey field
    const getHotkey = ($element, event) => GobConfigHelper.decodeHotkey(event, true)
    GobConfigHelper.bindElement(binding, $("#capp_hotkey_show"), { elementKey: "keydown", elementGetAccessor: getHotkey })
    GobConfigHelper.makeResetButton($("#capp_hotkey_show_reset"))

    // setup ungrouped
    GobConfigHelper.bindElement(binding, $("#capp_chat_updateInterval"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#capp_chat_updateInterval_reset"))

    GobConfigHelper.bindElement(binding, $("#capp_actor_updateInterval"), { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("capp_actor_updateInterval_reset"))

    // activate bindings
    binding.initialize()

    // setup profile copy
    /*
    const allFields = [
        dpdLanguage, dpdTheme,
        ckbChatLog, ckbAutodetect, ckbUserMention, ckbHide, ckbUpdate, ckbBetaUpdate, ckbRangeFilterMention,
        txtRangeFilterCutOff, txtRangeFilterFadeOut, txtRangeFilterStartOpacity, txtRangeFilterEndOpacity,
        txtFrameX, txtFrameY, txtFrameHeight, txtFrameWidth,
        clrChatboxBackground,
        clrSearchMarked, clrSearchSelected,
        txtFontFamily, dpdFontSize,
        txtHotkeyShow,
        txtChatUpdateInterval, txtActorUpdateInterval,
        ckbActorActive
    ]
    */

    const configKeys = []
    $(`#capp [${GobConfigHelper.ConfigKeyAttribute}]:not(.button)`).each(function () {
        const key = GobConfigHelper.getConfigKey(this)
        if (key !== null && key !== undefined && key.length > 0 && !_.includes(configKeys, key))
            configKeys.push(key)
    })

    const btnCopyProfile = $("#capp_copyprofile")
    GobConfigHelper.makeCopyProfileButton(btnCopyProfile,
        {
            //configKeys: _.map(allFields, e => e.attr(GobConfigHelper.ConfigKeyAttribute))
            configKeys: configKeys
        })
}());

//# sourceURL=config_app.js