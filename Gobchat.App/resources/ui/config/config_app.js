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

    // setup language
    const dpdLanguage = $("#capp_language")
    GobConfigHelper.bindElement(binding, dpdLanguage)

    const dpdTheme = $("#capp_theme")
    GobConfigHelper.bindElement(binding, dpdTheme)

    // setup checkboxes
    const ckbChatLog = $("#capp_makechatlog")
    GobConfigHelper.bindCheckbox(binding, ckbChatLog)

    const ckbAutodetect = $("#capp_autodetectemote")
    GobConfigHelper.bindCheckbox(binding, ckbAutodetect)

    const ckbUserMention = $("#capp_userMention")
    GobConfigHelper.bindCheckbox(binding, ckbUserMention)

    const ckbTimestamp = $("#capp_showTimestamp")
    GobConfigHelper.bindCheckbox(binding, ckbTimestamp)

    const ckbHide = $("#capp_hide")
    GobConfigHelper.bindCheckbox(binding, ckbHide)

    const ckbUpdate = $("#capp_checkupdates")
    GobConfigHelper.bindCheckbox(binding, ckbUpdate)

    binding.bindConfigListener(GobConfigHelper.getConfigKey(ckbUpdate), value => {
        //ckbBetaUpdate.attr("disabled", !value)
        $("[for='capp_checkupdates']").toggleClass("disabled", !value)
    })

    const ckbBetaUpdate = $("#capp_checkbetaupdates")
    GobConfigHelper.bindCheckbox(binding, ckbBetaUpdate)

    {
        const available = await GobchatAPI.isFeaturePlayerLocationAvailable()
        $("#capp_characterlocations_feature").toggle(!available)
    }

    const ckbActorActive = $("#capp_actor_updateActive")
    GobConfigHelper.bindCheckbox(binding, ckbActorActive)

    const ckbRangeFilterActive = $("#capp_rangefilter_active")
    GobConfigHelper.bindCheckbox(binding, ckbRangeFilterActive)

    binding.bindConfigListener(GobConfigHelper.getConfigKey(ckbRangeFilterActive), value => {
        //ckbRangeFilterMention.attr("disabled", !value)
        $("[for='capp_rangefilter_active']").toggleClass("disabled", !value)
    })

    const ckbRangeFilterMention = $("#capp_rangefilter_mention")
    GobConfigHelper.bindCheckbox(binding, ckbRangeFilterMention)

    // setup font group
    // setup font family
    const txtFontFamily = $("#capp_font_family")
    GobConfigHelper.bindElement(binding, txtFontFamily)

    const btnFontFamilyReset = $("#capp_font_family_reset")
    GobConfigHelper.makeResetButton(btnFontFamilyReset)

    const parseNumber = ($element) => {
        const value = parseInt($element.val())
        return Gobchat.isNumber(value) && value > 0 ? value : undefined
    }

    const txtRangeFilterCutOff = $("#capp_rangefilter_cutoff")
    GobConfigHelper.makeResetButton($("#capp_rangefilter_cutoff_reset"))

    const txtRangeFilterFadeOut = $("#capp_rangefilter_fadeout")
    GobConfigHelper.makeResetButton($("#capp_rangefilter_fadeout_reset"))

    const txtRangeFilterStartOpacity = $("#capp_rangefilter_startopacity")
    GobConfigHelper.makeResetButton($("#capp_rangefilter_startopacity_reset"))
    const txtRangeFilterEndOpacity = $("#capp_rangefilter_endopacity")
    GobConfigHelper.makeResetButton($("#capp_rangefilter_endopacity_reset"))

    GobConfigHelper.bindElement(binding, txtRangeFilterCutOff, { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, txtRangeFilterFadeOut, { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, txtRangeFilterStartOpacity, { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, txtRangeFilterEndOpacity, { elementGetAccessor: parseNumber })

    const txtFrameX = $("#capp_frame_x")
    const txtFrameY = $("#capp_frame_y")
    const txtFrameHeight = $("#capp_frame_height")
    const txtFrameWidth = $("#capp_frame_width")

    GobConfigHelper.bindElement(binding, txtFrameX, { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, txtFrameY, { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, txtFrameHeight, { elementGetAccessor: parseNumber })
    GobConfigHelper.bindElement(binding, txtFrameWidth, { elementGetAccessor: parseNumber })

    // setup font size
    const dpdFontSize = $("#capp_font_size")
    GobConfigHelper.bindElement(binding, dpdFontSize, {
        elementGetAccessor: ($element) => Gobchat.isNumber($element.val()) ? Math.round($element.val()) + "px" : ($element.val() || "medium")
    })

    const clrSearchMarked = $("#capp_search_marked")
    GobConfigHelper.makeColorSelector(clrSearchMarked)
    GobConfigHelper.bindColorSelector(binding, clrSearchMarked)

    const btnSearchMarkedReset = $("#capp_search_marked_reset")
    GobConfigHelper.makeResetButton(btnSearchMarkedReset)

    const clrSearchSelected = $("#capp_search_selected")
    GobConfigHelper.makeColorSelector(clrSearchSelected)
    GobConfigHelper.bindColorSelector(binding, clrSearchSelected)

    const btnSearchSelectedReset = $("#capp_search_selected_reset")
    GobConfigHelper.makeResetButton(btnSearchSelectedReset)

    // setup hotkey group
    // setup hotkey field
    const txtHotkeyShow = $("#capp_hotkey_show")
    const getHotkey = ($element, event) => GobConfigHelper.decodeHotkey(event, true)
    GobConfigHelper.bindElement(binding, txtHotkeyShow, { elementKey: "keydown", elementGetAccessor: getHotkey })

    // setup ungrouped
    const txtChatUpdateInterval = $("#capp_chat_updateInterval")
    GobConfigHelper.bindElement(binding, txtChatUpdateInterval, { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("#capp_chat_updateInterval_reset"))

    const txtActorUpdateInterval = $("#capp_actor_updateInterval")
    GobConfigHelper.bindElement(binding, txtActorUpdateInterval, { elementGetAccessor: parseNumber })
    GobConfigHelper.makeResetButton($("capp_actor_updateInterval_reset"))

    // activate bindings
    binding.initialize()

    // setup profile copy
    const allFields = [
        dpdLanguage, dpdTheme,
        ckbChatLog, ckbAutodetect, ckbUserMention, ckbTimestamp, ckbHide, ckbUpdate, ckbBetaUpdate, ckbRangeFilterActive, ckbRangeFilterMention,
        txtRangeFilterCutOff, txtRangeFilterFadeOut, txtRangeFilterStartOpacity, txtRangeFilterEndOpacity,
        txtFrameX, txtFrameY, txtFrameHeight, txtFrameWidth,
        txtFontFamily, dpdFontSize,
        txtHotkeyShow,
        txtChatUpdateInterval, txtActorUpdateInterval,
        ckbActorActive
    ]

    const btnCopyProfile = $("#capp_copyprofile")
    GobConfigHelper.makeCopyProfileButton(btnCopyProfile,
        {
            configKeys: _.map(allFields, e => e.attr(GobConfigHelper.ConfigKeyAttribute))
        })
}());