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

var GobConfigHelper = (function (GobConfigHelper, undefined) {
    GobConfigHelper.generateId = function (length, ids) {
        let id = null
        do {
            id = Gobchat.generateId(length)
        } while (_.includes(ids, id))
        return id
    }

    GobConfigHelper.showProfileIdSelectionDialog = function (callback, options) {
        {
            (async () => {
                let defOptions = { exclude: [] }
                defOptions = $.extend(defOptions, options)

                let profiles = gobconfig.profiles
                if (defOptions.exclude)
                    profiles = _.without(profiles, defOptions.exclude)

                const $selector = $("<select/>")
                profiles.forEach((profileId) => {
                    var profile = gobconfig.getProfile(profileId)
                    $selector.append(new Option(profile.profileName, profileId))
                })

                const result = await GobConfigHelper.showMessageDialog(
                    {
                        title: "config.profiles.dialog.copyprofilepage.title",
                        dialogContent: $selector,
                        dialogType: "OkCancel"
                    }
                )

                if (result === 1)
                    callback($selector.val())
            })()
        }
    }

    GobConfigHelper.showErrorDialog = async function (options) {
        let defOptions = {
            title: "config.main.dialog.title.error",
            dialogType: "ok",
            dialogIcon: "warning"
        }
        defOptions = $.extend(defOptions, options)
        return GobConfigHelper.showMessageDialog(defOptions)
    }

    GobConfigHelper.showConfirmationDialog = async function (options) {
        let defOptions = {
            title: "config.main.dialog.title.confirm",
            dialogType: "yesno",
            dialogIcon: "warning"
        }
        defOptions = $.extend(defOptions, options)
        return GobConfigHelper.showMessageDialog(defOptions)
    }

    GobConfigHelper.showMessageDialog = function (options) {
        return new Promise(async function (resolve, reject) {
            try {
                let defOptions = {
                    resizable: false,
                    width: 600,
                    modal: true,
                    autoOpen: false,
                    buttons: {},
                    dialogType: "ok",
                    dialogIcon: "",
                    dialogText: "",
                    dialogContent: null,
                    localized: true
                }
                defOptions = $.extend(defOptions, options)

                if (Object.keys(defOptions.buttons).length == 0) {
                    switch (defOptions.dialogType.toLocaleLowerCase()) {
                        case "yesno":
                            defOptions.buttons = {
                                "config.main.dialog.btn.yes": 1,
                                "config.main.dialog.btn.no": 0
                            }
                            break;
                        case "okcancel":
                            defOptions.buttons = {
                                "config.main.dialog.btn.ok": 1,
                                "config.main.dialog.btn.cancel": 0
                            }
                            break;
                        case "yes":
                            defOptions.buttons = { "config.main.dialog.btn.yes": 0 }
                        case "ok":
                        default:
                            defOptions.buttons = { "config.main.dialog.btn.ok": 0 }
                    }
                }

                if (defOptions.localized) {
                    const lookupKeys = [defOptions.title].concat(Object.keys(defOptions.buttons))
                    if (defOptions.dialogText.length > 0)
                        lookupKeys.push(defOptions.dialogText)

                    const locales = await goblocale.getAll(lookupKeys)
                    defOptions.title = locales[defOptions.title]
                    defOptions.buttons = _.mapKeys(defOptions.buttons, (v, k) => locales[k])

                    if (defOptions.dialogText.length > 0)
                        defOptions.dialogText = locales[defOptions.dialogText]

                    if (defOptions.dialogContent)
                        await goblocale.updateElement(defOptions.dialogContent)
                }

                Object.entries(defOptions.buttons).forEach((entry) => {
                    const key = entry[0]
                    const result = entry[1]
                    defOptions.buttons[key] = function () {
                        $(this).dialog("destroy").remove()
                        resolve(result)
                    }
                })

                const $dialog = $($("#template_config_dialog").html())

                if (defOptions.dialogText.length > 0)
                    $dialog.find("#text").append($("<span/>").html(defOptions.dialogText))
                if (defOptions.dialogContent)
                    $dialog.find("#text").append($(defOptions.dialogContent))

                $dialog.dialog(defOptions)

                switch (defOptions.dialogIcon) {
                    case "warning":
                        $dialog.find("#icon_warning").show()
                        break;
                }

                $dialog.parent().find(".ui-dialog-titlebar-close").on("click", function () {
                    $dialog.dialog("destroy").remove()
                    resolve(0)
                })

                $dialog.dialog("open")
            } catch (e1) {
                console.error(e1)
                reject(e1)
            }
        })
    }

    GobConfigHelper.makeColorSelector = function (element, options) {
        const defaultOptions = {
            hasAlpha: true,
            hasReset: true,
            onBeforeShow: null
        }

        options = $.extend(defaultOptions, options)

        const configKey = element.attr(GobConfigHelper.ConfigKeyAttribute)
        if (configKey === undefined || configKey === null)
            throw new Error(`Attribute '${GobConfigHelper.ConfigKeyAttribute}' not set`)

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
                    gobconfig.set(configKey, color.toString())
                else
                    gobconfig.set(configKey, null)
            },
            beforeShow: function (color) {
                if (options.onBeforeShow) {
                    const result = options.onBeforeShow(input)
                    return result === undefined || result === null || Boolean(result)
                }
                return true
            }
        })
    }

    GobConfigHelper.makeResetButton = function (element, targetElement) {
        const $element = $(element)
        $element.addClass("gob-button-tertiary")
        $element.addClass("gob-button-reset")
        const keyFunction = (targetElement != null && targetElement != undefined) ?
            () => GobConfigHelper.getConfigKey(targetElement) :
            () => GobConfigHelper.getConfigKey(element)
        if (targetElement != null && targetElement != undefined)
            $element.attr("disabled", $(targetElement).attr("disabled"))
        if ($element.attr("data-gob-locale-title") == null )
            $element.attr("data-gob-locale-title", "config.main.button.reset.tooltip")
        $element.on("click", () => gobconfig.reset(keyFunction()))
    }

    GobConfigHelper.makeCopyProfileButton = function (element, options) {
        const $element = $(element)
        const defOptions = { callback: undefined, configKeys: [] }
        options = $.extend(defOptions, options)

        $element.addClass("gob-button-tertiary")
        $element.addClass("gob-button-copypage")

        function copyProfile(profileId) {
            if (options.callback) {
                let result = options.callback()
                if (!(result === undefined || result === null) || !Boolean(result))
                    return
            }

            const srcProfile = gobconfig.getProfile(profileId)
            const dstProfile = gobconfig.getProfile(gobconfig.activeProfile)
            options.configKeys.forEach(key => {
                try {
                    dstProfile.copyFrom(srcProfile, key)
                } catch (e1) {
                    console.error(`Profile copy Error in key '${key}'. Reason: ${e1}`)
                }
            })
        }

        $element.on("click", event => GobConfigHelper.showProfileIdSelectionDialog(copyProfile, { exclude: gobconfig.activeProfile }))
        $element.addClass("gob-button-copypage")
        $element.attr("data-gob-locale-title", "config.main.profile.copypage")

        const checkCopyProfileState = () => $element.attr("disabled", (gobconfig.profiles.length <= 1))
        gobconfig.addProfileEventListener(event => checkCopyProfileState())
        checkCopyProfileState()
    }

    GobConfigHelper.decodeHotkey = function (keyEvent, ignoreEnter) {
        if (ignoreEnter && event.keyCode == 13) // enter
            return null

        if (event.keyCode === 16 || event.keyCode === 17 || event.keyCode === 18)
            return ""

        keyEvent.preventDefault()

        let msg = ""
        if (keyEvent.shiftKey) msg += "Shift + "
        if (keyEvent.altKey) msg += "Alt + "
        if (keyEvent.ctrlKey) msg += "Ctrl + "

        var keyEnum = Gobchat.KeyCodeToKeyEnum(keyEvent.keyCode)
        if (keyEnum === null) {
            msg = ""
        } else {
            msg += keyEnum
        }
        return msg
    }

    return GobConfigHelper
}(GobConfigHelper || {}));