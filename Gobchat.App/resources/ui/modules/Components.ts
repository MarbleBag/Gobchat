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
    $element.addClass("gob-config-icon-button")

    const getConfigKey = targetElement ?
        () => Databinding.getConfigKey(targetElement) :
        () => Databinding.getConfigKey(element)

    $element.on("click", () => gobConfig.reset(getConfigKey()))

    if ($element.attr("data-gob-locale-title") === null)
        $element.attr("data-gob-locale-title", "config.main.button.reset.tooltip")

    if (targetElement && $(targetElement).hasClass("is-disabled"))
        $element.addClass("is-disabled")
}

interface CopyProfileOptionTypes {
    callback: null | ((profileId: string) => boolean)
    configKeys: string[]
}

const DefaultCopyProfileOptions: CopyProfileOptionTypes = {
    callback: null,
    configKeys: []
}

export type CopyProfileOptions = Partial<CopyProfileOptionTypes>

export function makeCopyProfileButton(element: HTMLElement | JQuery, userOptions?: CopyProfileOptions) {
    const $element = $(element)
    const options = !userOptions ? DefaultCopyProfileOptions : $.extend({}, DefaultCopyProfileOptions, userOptions)

    $element.addClass("gob-config-copypage-button")

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

        options.configKeys.forEach(key => {
            try {
                dstProfile.copyFrom(srcProfile, key)
            } catch (e1) {
                console.error(`Profile copy Error in key '${key}'. Reason: ${e1}`)
            }
        })
    }

    $element.on("click", event => Dialog.showProfileIdSelectionDialog(copyProfile, { exclude: gobConfig.activeProfile }))
    $element.addClass("gob-button-copypage")
    $element.attr("data-gob-locale-title", "config.main.profile.copypage")

    const checkCopyProfileState = () => $element.attr("disabled", (gobConfig.profileIds.length <= 1))
    gobConfig.addProfileEventListener(event => checkCopyProfileState())
    checkCopyProfileState()
}