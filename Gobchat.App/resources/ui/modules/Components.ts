/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

'use strict'

import * as Databinding from "/module/Databinding"
import * as Dialog from "/module/Dialog"
import * as Locale from "/module/Locale"
import * as Utility from "/module/CommonUtility"

interface ColorSelectorOptionTypes {
    hasAlpha: boolean
    hasReset: boolean
    onBeforeShow: null | ((color: string) => boolean)
}

const DefaultColorSelectorOptions: ColorSelectorOptionTypes = {
    hasAlpha: true,
    hasReset: true,
    onBeforeShow: null
}

export type ColorSelectorOptions = Partial<ColorSelectorOptionTypes>

function _makeColorSelector(element: JQuery, options: ColorSelectorOptionTypes): void {
    if (element.length < 1)
        throw new Error("An empty element can't be turned into a color selector")

    if (element.length > 1)
        throw new Error(`Unable to turn multiple elements into the same color selector`)

    element.spectrum({
        preferredFormat: "hex3",
        // color: data,
        allowEmpty: true,
        showInput: true,
        showInitial: true,
        showAlpha: options.hasAlpha,
        showPalette: true,
        //palette: spectrumColorPalette,
        showSelectionPalette: true,
        selectionPalette: [],
        maxSelectionSize: 6,
        clickoutFiresChange: false,
        beforeShow: function (color) {
            return options.onBeforeShow ? options.onBeforeShow(color) : true;
        }
    })
}

export function makeColorSelector(element: HTMLElement | JQuery, options?: ColorSelectorOptions): void {
    _makeColorSelector($(element), $.extend({}, DefaultColorSelectorOptions, options))
}

export function makeResetButton(element: HTMLElement | JQuery, targetElement?: HTMLElement | JQuery): void {
    const $element = $(element)
    if ($element.length === 0)
        throw new Error("No html element found")

    $element.toggleClass("gob-config-icon-button", true)
    $element.empty()
    $element.append($("<i class='fas fa-undo-alt'></i>"))

    if ($element.attr(Locale.HtmlAttribute.TooltipId) === null)
        $element.attr(Locale.HtmlAttribute.TooltipId, "config.main.button.reset.tooltip")

    const getConfigKey = targetElement ?
        () => Databinding.getConfigKey(targetElement) :
        () => Databinding.getConfigKey(element)

    $element.on("click", () => gobConfig.reset(getConfigKey()))

    // if (targetElement && ($(targetElement).hasClass("is-disabled") || $(targetElement).prop("disabled")) )
    //     $element.addClass("is-disabled").prop("disabled", true)

    /*
    if ($(targetElement).length > 0) {
        var mutationObserver = new MutationObserver((mutations) => {
            for (const mutation of mutations) {
                if (mutation.attributeName === "disabled") {
                    if ((mutation.target as any).disabled) {
                        $element.toggleClass("is-disabled", true).prop("disabled", true)
                    } else {
                        $element.toggleClass("is-disabled", false).prop("disabled", false)
                    }
                } else if (!mutation.target.isConnected) {
                    mutationObserver.disconnect()
                }
            }
        })
        targetElement = $(targetElement)[0]
        mutationObserver.observe(targetElement, {attributes: true})
    }
    */
}

interface CopyProfileOptionTypes {
    callback: null | ((profileId: string) => boolean)
    configKeys: string[] | (() => string[])
}

const DefaultCopyProfileOptions: CopyProfileOptionTypes = {
    callback: null,
    configKeys: []
}

export type CopyProfileOptions = Partial<CopyProfileOptionTypes>

export function makeCopyProfileButton(element: HTMLElement | JQuery, userOptions?: CopyProfileOptions) {
    const $element = $(element)
    if ($element.length === 0)
        throw new Error("No html element found")

    $element.toggleClass("gob-config-icon-button", true)
    $element.toggleClass("gob-config-copypage-button", true)
    $element.attr(Locale.HtmlAttribute.TooltipId, "config.main.profile.copypage")

    $element.empty()
    $element.append($("<i class='fas fa-clone'></i>"))

    $element.on("click", event => Dialog.showProfileIdSelectionDialog(copyProfile, { exclude: [gobConfig.activeProfileId ?? ""] }))

    const options = !userOptions ? DefaultCopyProfileOptions : $.extend({}, DefaultCopyProfileOptions, userOptions)

    /*{
        const keys = Utility.isFunction(options.configKeys) ? options.configKeys() : options.configKeys
        const keySet = new Set<string>(keys)
        console.log(`Selected keys for copy profile {${$element.attr("id")}} are [${Array.from(keySet).join(", ")}]`)
    }*/

    function copyProfile(profileId: string) {
        if (options.callback) {
            const result = options.callback(profileId)
            if (!result)
                return
        }

        const srcProfile = gobConfig.getProfile(profileId)
        const dstProfile = gobConfig.activeProfile

        if (srcProfile === null)
            console.error(`Profile copy error. Profile '${profileId}' not found`)

        if (dstProfile === null)
            console.error(`Profile copy error. No active profile found`)

        if (srcProfile === null || dstProfile === null)
            return

        const keys = Utility.isFunction(options.configKeys) ? options.configKeys() : options.configKeys
        const keySet = new Set<string>(keys)

        for (const key of keySet) {
            if (key === null || key === "")
                continue

            try {
                dstProfile.copyFrom(srcProfile, key)
            } catch (e1) {
                console.error(`Profile copy Error in key '${key}'. Reason: ${e1}`)
            }
        }
    }

    const checkCopyProfileState = () => $element.prop("disabled", (gobConfig.profileIds.length <= 1))
    gobConfig.addProfileEventListener(event => checkCopyProfileState())
    checkCopyProfileState()
}