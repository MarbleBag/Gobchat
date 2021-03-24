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

'use strict'

var Gobchat = (function (Gobchat) {
    function loadStyles(folder) {
    }

    class StyleLoader {
        constructor(target, filePrefix) {
            this._activeStyle = null
            this._styles = {}
            this._styleSheets = []
            this._$target = $(target)
            this._prefix = filePrefix ? filePrefix : null
        }

        async loadStyles() {
            this._styles = {}

            try {
                const json = await GobchatAPI.readTextFromFile("ui/styles/styles.json")
                const styles = JSON.parse(json);

                for (let style of styles) {
                    try {
                        const styleKey = style.label.toLowerCase()
                        this._styles[styleKey] = {
                            label: style.label,
                            files: [].concat(style.files || [])
                        }
                    } catch (e1) {
                        console.error(e1)
                    }
                }
            } catch (e1) {
                console.error(e1)
            }
        }

        get stylesheetIds() {
            return [].concat(this._styleSheets)
        }

        get styles() {
            return Object.entries(this._styles).map(e => e[1].label)
        }

        get activeStyle() {
            return this._activeStyle
        }

        async activateStyle(styleName) {
            const styleKey = styleName.toLowerCase()
            const selectedStyle = this._styles[styleKey]
            if (!selectedStyle) {
                console.error(`Style '${styleName}' not available`)
                return false
            }

            this._activeStyle = styleName
            const $styleSheets = this._$target
            const styleFiles = selectedStyle.files;
            const awaitPromises = []

            for (let sheetId of this._styleSheets) //remove all, onload is only triggered once for any link element
                $styleSheets.find(`#${sheetId}`).remove()
            this._styleSheets = []

            for (let i = 0; i < styleFiles.length; ++i) {
                const styleFile = styleFiles[i]

                const styleSheetId = `dcssstyle-${Gobchat.generateId(8)}`
                const $styleSheet = $(`<link rel="stylesheet" type="text/css" id="${styleSheetId}" href="">`)
                this._styleSheets.push(styleSheetId)

                awaitPromises.push(new Promise(function (resolve, reject) {
                    $styleSheet.one("load", () => resolve())
                    $styleSheet.one("error", () => reject())
                }))

                const path = this._prefix ? `${this._prefix}/${styleFile}` : styleFile
                $styleSheet.attr("href", path).appendTo($styleSheets)
            }

            awaitPromises.push(new Promise((resolve, reject) => {
                window.requestAnimationFrame(() => resolve())
            }))

            await Promise.all(awaitPromises)
            return true
        }
    }

    Gobchat.StyleLoader = StyleLoader

    return Gobchat
}(Gobchat || {}));