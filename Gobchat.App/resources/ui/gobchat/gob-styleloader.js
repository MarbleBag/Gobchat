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

'use strict'

var Gobchat = (function (Gobchat) {
    function loadStyles(folder) {
    }

    class StyleLoader {
        constructor(target, filePrefix) {
            this._activeStyle = null
            this._styles = {}
            this._styleSheets = []
            this._target = $(target)
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

        activateStyle(styleName) {
            const styleKey = styleName.toLowerCase()
            const selectedStyle = this._styles[styleKey]
            if (!selectedStyle) {
                console.error(`Style '${styleName}' not available`)
                return false
            }

            this._activeStyle = styleName

            const styleFiles = selectedStyle.files;
            if (styleFiles.length < this._styleSheets.length) {
                const deleteSheets = this._styleSheets.splice(styleFiles.length)
                for (let sheetId of deleteSheets)
                    this._target.find(`#${sheetId}`).remove()
            }

            while (styleFiles.length > this._styleSheets.length) {
                const styleSheetId = Gobchat.generateId(8)
                $(`<link rel="stylesheet" id="${styleSheetId}" href="">`).appendTo(this._target)
                this._styleSheets.push(styleSheetId)
            }

            for (let i = 0; i < styleFiles.length; ++i) {
                const styleSheetId = this._styleSheets[i]
                const styleSheet = this._target.find(`#${styleSheetId}`)
                const styleFile = styleFiles[i]
                const path = this._prefix ? `${this._prefix}/${styleFile}` : styleFile
                styleSheet.attr("href", path)
            }

            return true
        }
    }

    Gobchat.StyleLoader = StyleLoader

    return Gobchat
}(Gobchat || {}));