/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

Databinding.bindElement(binding, $("#cp-app_language"))

try {
    const dpdThemes = $("#cp-app_theme")
    dpdThemes.empty()
    for (let style of gobStyles.styles) {
        $('<option>', {
            text: style.label,
            value: style.label
        }).appendTo(dpdThemes)
    }
    Databinding.bindElement(binding, $("#cp-app_theme"))
} catch (e1) {
    console.error(e1)
}

// setup checkboxes
Databinding.bindCheckbox(binding, $("#cp-app_autodetectemote"))

Databinding.bindCheckbox(binding, $("#cp-app_hide"))

Databinding.bindCheckbox(binding, $("#cp-app_checkupdates"))

Databinding.bindCheckbox(binding, $("#cp-app_checkbetaupdates"))

{
    const available = await GobchatAPI.isFeaturePlayerLocationAvailable()
    $("#cp-app_characterlocations_feature").toggle(!available)
}

Databinding.bindCheckbox(binding, $("#cp-app_actor_updateActive"))

Databinding.bindCheckbox(binding, $("#cp-app_rangefilter_mention"))

Databinding.bindDropdown(binding, $("#cp-app_config_font-size_selector"))

const dpdProcessSelector = $("#cp-app_process_selector")
$("#cp-app_process_selector_refresh").on("click", function () {
    const $icon = $("#cp-app_process_selector_refresh").find("svg");
    (async () => {
        try {
            //$icon.addClass("fa-spin")

            const defaultElement = dpdProcessSelector.find("[value='-1']")
            const previousSelected = dpdProcessSelector.val()
            dpdProcessSelector.empty().append(defaultElement)

            const availableProcesses = await GobchatAPI.getAttachableFFXIVProcesses()
            for (const processId of availableProcesses)
                dpdProcessSelector.append(new Option(`FFXIV: ${processId}`, processId.toString()))

            if (dpdProcessSelector.find(`[value='${previousSelected}'`).length > 0) {
                dpdProcessSelector.val(previousSelected)
            } else {
                dpdProcessSelector.val("-1")
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

        const txtLabel = $("#cp-app_process_info")
        const icon = $("#cp-app_process_selector_link").find("svg")

        async function updateLabel() {
            try {
                const connectionInfo = await GobchatAPI.getAttachedFFXIVProcess()
                const connectionState = connectionInfo.Item1 //0 - none, 1 - connected, 2 - not found, 3 - searching
                const processId = connectionInfo.Item2

                switch (connectionState) {
                    case 0:
                        break
                    case 1:
                        txtLabel.text(Utility.formatString(txtConnectedTo, processId));
                        icon.removeClass("fa-spin")
                        clearInterval(process_IntervalTimer)
                        process_IntervalTimer = 0
                        break;
                    case 2:
                        txtLabel.text(txtNotConnected);
                        break;
                    case 3:
                        txtLabel.text(txtSearch);
                        break
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

$("#cp-app_process_selector_link").on("click", async function () {
    try {
        $("#cp-app_process_selector_link").find("svg").addClass("fa-spin")

        const processId = dpdProcessSelector.val()
        if (processId != null && processId != undefined)
            GobchatAPI.attachToFFXIVProcess(parseInt(processId))

        await process_UpdateLabel()
    } catch (e) {
        console.error(e)
    }
})

const parseNonNegativeNumber = (element: JQuery) => {
    const value = parseInt(element.val())
    return Utility.isNumber(value)  && value >= 0 ? value : undefined
}

const parseNumber = (element: JQuery) => {
    const value = parseInt(element.val())
    return Utility.isNumber(value) ? value : undefined
}

Databinding.bindElement(binding, $("#cp-app_frame_x"), { elementToConfig: parseNumber })
Databinding.bindElement(binding, $("#cp-app_frame_y"), { elementToConfig: parseNumber })
Databinding.bindElement(binding, $("#cp-app_frame_height"), { elementToConfig: parseNonNegativeNumber })
Databinding.bindElement(binding, $("#cp-app_frame_width"), { elementToConfig: parseNonNegativeNumber })

const clrChatboxBackground = $("#cp-app_chat-history_backgroundcolor")
Components.makeColorSelector(clrChatboxBackground)
Databinding.bindColorSelector(binding, clrChatboxBackground)
Components.makeResetButton($("#cp-app_chat-history_backgroundcolor_reset"), clrChatboxBackground)

const clrSearchMarked = $("#cp-app_search_marked")
Components.makeColorSelector(clrSearchMarked)
Databinding.bindColorSelector(binding, clrSearchMarked)
Components.makeResetButton($("#cp-app_search_marked_reset"), clrSearchMarked)

const clrSearchSelected = $("#cp-app_search_selected")
Components.makeColorSelector(clrSearchSelected)
Databinding.bindColorSelector(binding, clrSearchSelected)
Components.makeResetButton($("#cp-app_search_selected_reset"), clrSearchSelected)

// setup hotkey group
// setup hotkey field
const getHotkey = (element: JQuery, event: KeyboardEvent) => Utility.decodeKeyEventToText(event, true)
Databinding.bindElement(binding, $("#cp-app_hotkey_show"), { elementKey: "keydown", elementToConfig: getHotkey })
Components.makeResetButton($("#cp-app_hotkey_show_reset"), $("#cp-app_hotkey_show"))

// setup ungrouped
Databinding.bindElement(binding, $("#cp-app_chat_updateInterval"), { elementToConfig: parseNonNegativeNumber })
Components.makeResetButton($("#cp-app_chat_updateInterval_reset"), $("#cp-app_chat_updateInterval"))

Databinding.bindElement(binding, $("#cp-app_actor_updateInterval"), { elementToConfig: parseNonNegativeNumber })
Components.makeResetButton($("#cp-app_actor_updateInterval_reset"), $("#cp-app_actor_updateInterval"))

// activate bindings
binding.loadBindings()

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
$(`#cp-app [${Databinding.HtmlAttribute.ConfigKey}]:not(.button)`).each(function () {
    const key = Databinding.getConfigKey(this)
    if (key !== null && key.length > 0)
        configKeys.add(key)
})

const btnCopyProfile = $("#cp-app_copyprofile")
Components.makeCopyProfileButton(btnCopyProfile,
    {
        //configKeys: _.map(allFields, e => e.attr(GobConfigHelper.ConfigKeyAttribute))
        configKeys: Array.from(configKeys)
    })

//# sourceURL=config_app.js
