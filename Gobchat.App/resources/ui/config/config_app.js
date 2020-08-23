/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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
    $txtChatlogPath.on("change", function () {
        //  const newPath = $txtChatlogPath.val()
        //  gobconfig.set(GobConfigHelper.getConfigKey(), newPath)

        {
            (async () => {
            })()
        }
    })

    binding.bindConfigListener(GobConfigHelper.getConfigKey($txtChatlogPath), value => {
        {
            (async () => {
                //    let path = gobconfig.get(GobConfigHelper.getConfigKey())
                //    path = await GobchatAPI.ComputeAppDataPath(path)
                //    $txtChatlogPath.val(path)
            })()
        }
    })

    $("#capp_chatlog_path_select").on("click", function () {
        {
            (async () => {
                const path = GobchatAPI.openDirectoryDialog()
                console.log("selected folder: " + path)
            })()
        }
    })

    GobConfigHelper.makeResetButton($("#capp_logging_path_reset"))

    GobConfigHelper.bindCheckbox(binding, $("#capp_autodetectemote"))

    GobConfigHelper.bindCheckbox(binding, $("#capp_userMention"))

    GobConfigHelper.bindCheckbox(binding, $("#capp_hide"))

    const ckbUpdate = $("#capp_checkupdates")
    GobConfigHelper.bindCheckbox(binding, ckbUpdate)
    binding.bindConfigListener(GobConfigHelper.getConfigKey(ckbUpdate), value => {
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
        $("#capp_process_selector_refresh").find("svg").addClass("fa-spin")
        {
            (async () => {
                const defaultElement = $dpdProcessSelector.find("[value='-1']")
                $dpdProcessSelector.empty().append(defaultElement)

                const availableProcesses = await GobchatAPI.getAttachableFFXIVProcesses()
                for (const processId of availableProcesses)
                    $dpdProcessSelector.append(new Option(`FFXIV: ${processId}`, processId))

                var connectionInfo = await GobchatAPI.getAttachedFFXIVProcess()
                if (connectionInfo.Item1) {
                    $("#capp_process_info").text(`Connected to ${connectionInfo.Item2}`); //TODO localize
                } else {
                    $("#capp_process_info").text("Not connected"); //TODO localize                    
                }

                $("#capp_process_selector_refresh").find("svg").removeClass("fa-spin")
            })()
        }
    })

    $dpdProcessSelector.on("change", function () {
        {
            (async () => {
            })()
        }
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
    $(`#capp [${GobConfigHelper.ConfigKeyAttribute}]`).each(function () {
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