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
import * as Utility from "/module/CommonUtility"

const binding = new Databinding.BindingContext(gobConfig);

Databinding.bindElement(binding, $("#capp_language"))

try {
    const dpdThemes = $("#capp_theme")
    dpdThemes.empty()
    for (let style of gobStyles.styles) {
        $('<option>', {
            text: style.label,
            value: style.label
        }).appendTo(dpdThemes)
    }
    Databinding.bindElement(binding, $("#capp_theme"))
} catch (e1) {
    console.error(e1)
}

// setup checkboxes
Databinding.bindCheckbox(binding, $("#capp_autodetectemote"))

Databinding.bindCheckbox(binding, $("#capp_userMention"))

Databinding.bindCheckbox(binding, $("#capp_hide"))

const $ckbUpdate = $("#capp_checkupdates")
Databinding.bindCheckbox(binding, $ckbUpdate)
binding.bindConfigListener(Databinding.getConfigKey($ckbUpdate), value => {
    $(`[for='${$ckbUpdate.attr("id")}']`).toggleClass("is-disabled", !value)
})

Databinding.bindCheckbox(binding, $("#capp_checkbetaupdates"))

{
    const available = await GobchatAPI.isFeaturePlayerLocationAvailable()
    $("#capp_characterlocations_feature").toggle(!available)
}

Databinding.bindCheckbox(binding, $("#capp_actor_updateActive"))

Databinding.bindCheckbox(binding, $("#capp_rangefilter_mention"))

// setup font group
// setup font family
Databinding.bindElement(binding, $("#capp_font_family"))
Components.makeResetButton($("#capp_font_family_reset"))

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
                $dpdProcessSelector.append(new Option(`FFXIV: ${processId}`, processId.toString()))

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
        const txtSearch = await gobLocale.get("config.app.process.info.search")
        const txtNotConnected = await gobLocale.get("config.app.process.info.notconnected")
        const txtConnectedTo = await gobLocale.get("config.app.process.info.connected")

        const $txtLabel = $("#capp_process_info")
        const $icon = $("#capp_process_selector_link").find("svg")

        async function updateLabel() {
            try {
                const connectionInfo = await GobchatAPI.getAttachedFFXIVProcess()
                const connectionState = connectionInfo.Item1 //0 - none, 1 - connected, 2 - not found, 3 - searching
                const processId = connectionInfo.Item2

                if (connectionState === 0) {
                } else if (connectionState === 1) {
                    $txtLabel.text(Utility.formatString(txtConnectedTo, processId));
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
    return Utility.isNumber(value) && value >= 0 ? value : undefined
}

Databinding.bindElement(binding, $("#capp_rangefilter_cutoff"), { elementGetAccessor: parseNumber })
Components.makeResetButton($("#capp_rangefilter_cutoff_reset"))

Databinding.bindElement(binding, $("#capp_rangefilter_fadeout"), { elementGetAccessor: parseNumber })
Components.makeResetButton($("#capp_rangefilter_fadeout_reset"))

Databinding.bindElement(binding, $("#capp_rangefilter_startopacity"), { elementGetAccessor: parseNumber })
Components.makeResetButton($("#capp_rangefilter_startopacity_reset"))

Databinding.bindElement(binding, $("#capp_rangefilter_endopacity"), { elementGetAccessor: parseNumber })
Components.makeResetButton($("#capp_rangefilter_endopacity_reset"))

Databinding.bindElement(binding, $("#capp_frame_x"), { elementGetAccessor: parseNumber })
Databinding.bindElement(binding, $("#capp_frame_y"), { elementGetAccessor: parseNumber })
Databinding.bindElement(binding, $("#capp_frame_height"), { elementGetAccessor: parseNumber })
Databinding.bindElement(binding, $("#capp_frame_width"), { elementGetAccessor: parseNumber })

const clrChatboxBackground = $("#capp_chatbox_backgroundcolor")
Components.makeColorSelector(clrChatboxBackground)
Databinding.bindColorSelector(binding, clrChatboxBackground)
Components.makeResetButton($("#capp_chatbox_backgroundcolor_reset"))

{
    function makeFontSizeSelector($input: JQuery, $selector: JQuery) {
        const regexValue = /\d+\.?\d*/

        Databinding.bindElement(binding, $input, { elementGetAccessor: parseNumber })

        binding.bindConfigListener($input, function (fontSize) {
            fontSize = parseFloat(fontSize)
            const baseSize = parseFloat(gobConfig.get("style.base-font-size"))
            const factor = fontSize / baseSize

            $selector.val(factor + "")
            if ($selector.val() === null)
                $selector.val("")
        })

        $input.on('change', function () {
            let value = parseFloat($(this).val()) || 0
            if (value < 8)
                value = 8
            $(this).val(value)
        })

        $selector.on("change", function () {
            const selectedFormat = $(this).val()
            if (selectedFormat.length > 0) {
                const baseSize = parseFloat(gobConfig.get("style.base-font-size"))
                const factor = parseFloat(selectedFormat)
                $input.val(baseSize * factor).change()
            }
        })
    }

    makeFontSizeSelector($("#capp_chat_font-size"), $("#capp_chat_font-size_selector"))
    makeFontSizeSelector($("#capp_config_font-size"), $("#capp_config_font-size_selector"))
}

const clrSearchMarked = $("#capp_search_marked")
Components.makeColorSelector(clrSearchMarked)
Databinding.bindColorSelector(binding, clrSearchMarked)
Components.makeResetButton($("#capp_search_marked_reset"))

const clrSearchSelected = $("#capp_search_selected")
Components.makeColorSelector(clrSearchSelected)
Databinding.bindColorSelector(binding, clrSearchSelected)
Components.makeResetButton($("#capp_search_selected_reset"))

// setup hotkey group
// setup hotkey field
const getHotkey = ($element, event) => Utility.decodeKeyEventToText(event, true)
Databinding.bindElement(binding, $("#capp_hotkey_show"), { elementKey: "keydown", elementGetAccessor: getHotkey })
Components.makeResetButton($("#capp_hotkey_show_reset"))

// setup ungrouped
Databinding.bindElement(binding, $("#capp_chat_updateInterval"), { elementGetAccessor: parseNumber })
Components.makeResetButton($("#capp_chat_updateInterval_reset"))

Databinding.bindElement(binding, $("#capp_actor_updateInterval"), { elementGetAccessor: parseNumber })
Components.makeResetButton($("capp_actor_updateInterval_reset"))

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

const configKeys = new Set<string>()
$(`#capp [${Databinding.DataAttributeConfigKey}]:not(.button)`).each(function () {
    const key = Databinding.getConfigKey(this)
    if (key !== null && key.length > 0)
        configKeys.add(key)
})

const btnCopyProfile = $("#capp_copyprofile")
Components.makeCopyProfileButton(btnCopyProfile,
    {
        //configKeys: _.map(allFields, e => e.attr(GobConfigHelper.ConfigKeyAttribute))
        configKeys: Array.from(configKeys)
    })

//# sourceURL=config_app.js
