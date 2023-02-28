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

'use strict'

import * as Databinding from "./Databinding.js"
import * as Dialog from "./Dialog.js"
import { Utility } from "./GobModule.js"

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
    const configKey = Databinding.getConfigKey(element)
    if (configKey === null)
        throw new Error(`Attribute '${Databinding.DataAttributeConfigKey}' not set`)

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
        hide: function (color) {
            if (color !== null)
                gobConfig.set(configKey, color.toString())
            else
                gobConfig.set(configKey, null)
        },
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
    $element.toggleClass("gob-config-icon-button", true)

    if ($element.attr("data-gob-locale-title") === null)
        $element.attr("data-gob-locale-title", "config.main.button.reset.tooltip")

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
    const options = !userOptions ? DefaultCopyProfileOptions : $.extend({}, DefaultCopyProfileOptions, userOptions)

    $element.toggleClass("gob-config-copypage-button", true)

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

        let configKeys:string[] = []

        if (Utility.isFunction(options.configKeys))
            configKeys = (options.configKeys as any)()
        else
            configKeys = options.configKeys as string[]

        configKeys.forEach(key => {
            try {
                dstProfile.copyFrom(srcProfile, key)
            } catch (e1) {
                console.error(`Profile copy Error in key '${key}'. Reason: ${e1}`)
            }
        })
    }

    $element.on("click", event => Dialog.showProfileIdSelectionDialog(copyProfile, { exclude: [gobConfig.activeProfileId ?? ""] }))
    $element.addClass("gob-button-copypage")
    $element.attr("data-gob-locale-title", "config.main.profile.copypage")

    const checkCopyProfileState = () => $element.attr("disabled", (gobConfig.profileIds.length <= 1))
    gobConfig.addProfileEventListener(event => checkCopyProfileState())
    checkCopyProfileState()
}