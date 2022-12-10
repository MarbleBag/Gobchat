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

'use strict'

import * as Utility from './CommonUtility.js'

export class StyleLoader {
    constructor(filePrefix) {
        this._styles = {}
        this._activeStyles = []        
        this._activeStyleSheetIds = []
        this._filePrefix = filePrefix ? filePrefix : null
    }

    async initialize() {
        this._styles = {}
        
        const json = await GobchatAPI.readTextFromFile("ui/styles/styles.json")
        const styles = JSON.parse(json);

        for (let style of styles) {
            const styleKey = style.label.toLowerCase().trim()
            if (styleKey.length == 0)
                continue

            this._styles[styleKey] = {
                label: style.label,
                files: [].concat(style.files || []).filter(e => Utility.isString(e)).map(e => e.trim()).filter(e => e.length > 0)
            }
        }
    }

    get styles() {
        return Object.keys(this._styles).map(key => { id = key, label = this._styles[key].label })
    }

    get activeStyles() {
        return [].concat(this._activeStyles || [])
    }

    async activateStyles(styleIds) {
        styleIds = [].concat(styleIds || []).filter(e => Utility.isString(e)).map(e => e.toLowerCase())

        for (let styleId of styleIds)
            if (!this._styles[styleId])
                throw new Error(`Style with id '${styleId}' not available`)

        const $head = $("head")
        const $body = $("body")

        // use hide / show to trigger a reflow, so the new loaded style gets applied everywhere.
        // Sometimes, without this, styles aren't applied to scrollbars. Still no idea why.
        $body.hide()

        for (let id of this._activeStyleSheetIds)
            $(`#${id}`).remove()

        this._activeStyleSheetIds = []
        this._activeStyles = []
        
        const awaitPromises = []
        
        for (let styleId of styleIds) {
            this._activeStyles.push(styleId)

            const style = this._styles[styleId]
            const randomIdPrefix = Utility.generateId(8)

            for (let file of style.files) {
                const id = `gobstyle-${randomIdPrefix}-${file.replace(/[\s\\/]+/g, '_').replace(/[^-\.\w]+/g,'')}`

                const $link = $(`<link rel="stylesheet" type="text/css" href="">`).attr('id', id)
                this._activeStyleSheetIds.push(id)

                awaitPromises.push(new Promise(function (resolve, reject) {
                    $link.one("load", () => resolve())
                    $link.one("error", () => reject())
                }))

                const path = this._filePrefix ? `${this._filePrefix}/${file}` : file
                $link.attr("href", path).appendTo($head)
            }
        }

        awaitPromises.push(new Promise((resolve, reject) => {
            window.requestAnimationFrame(() => resolve())
        }))

        const results = await Promise.allSettled(awaitPromises)

        $body.show()

        const errorMsg = ""
        for (const result in results) {
            if (result.status === "rejected")
                errorMsg += result.reason + '\n'
        }
        if (errorMsg.length > 0)
            throw new Error(errorMsg)
    }

}

