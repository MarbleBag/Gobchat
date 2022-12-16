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
    #styles: {[key:string]:{label:string, files:string[]}} = {}
    #activeStyles: string[] = []
    #activeStyleSheetIds: string[] = []
    readonly #filePrefix: string

    constructor(filePrefix: string) {
        this.#filePrefix = filePrefix ? filePrefix : null
    }

    async initialize(): Promise<void> {
        this.#styles = {}
        
        const json = await GobchatAPI.readTextFromFile("ui/styles/styles.json")
        const styles = JSON.parse(json);

        for (let style of styles) {
            const styleKey = style.label.toLowerCase().trim()
            if (styleKey.length == 0)
                continue

            this.#styles[styleKey] = {
                label: style.label,
                files: [].concat(style.files || []).filter(e => Utility.isString(e)).map(e => e.trim()).filter(e => e.length > 0)
            }
        }
    }

    get styles(): {id:string, label:string}[] {
        return Object.keys(this.#styles)
            .map(key => {
                return {
                    id: key,
                    label: this.#styles[key].label
                }
            })
    }

    get activeStyles(): string[] {
        return [].concat(this.#activeStyles || [])
    }

    async activateStyles(styleIds?: string | string[]): Promise<void> {
        styleIds = [].concat(styleIds || []).filter(e => Utility.isString(e)).map(e => e.toLowerCase())

        for (let styleId of styleIds)
            if (!this.#styles[styleId])
                throw new Error(`Style with id '${styleId}' not available`)

        const $head = $("head")
        const $body = $("body")

        // use hide / show to trigger a reflow, so the new loaded style gets applied everywhere.
        // Sometimes, without this, styles aren't applied to scrollbars. Still no idea why.
        $body.hide()

        for (let id of this.#activeStyleSheetIds)
            $(`#${id}`).remove()

        this.#activeStyleSheetIds = []
        this.#activeStyles = []
        
        const awaitPromises: Promise<void>[] = []
        
        for (let styleId of styleIds) {
            this.#activeStyles.push(styleId)

            const style = this.#styles[styleId]
            const randomIdPrefix = Utility.generateId(8)

            for (let file of style.files) {
                const id = `gobstyle-${randomIdPrefix}-${file.replace(/[\s\\/]+/g, '_').replace(/[^-\.\w]+/g,'')}`

                const $link = $(`<link rel="stylesheet" type="text/css" href="">`).attr('id', id)
                this.#activeStyleSheetIds.push(id)

                awaitPromises.push(new Promise(function (resolve, reject) {
                    $link.one("load", () => resolve())
                    $link.one("error", () => reject())
                }))

                const path = this.#filePrefix ? `${this.#filePrefix}/${file}` : file
                $link.attr("href", path).appendTo($head)
            }
        }

        awaitPromises.push(new Promise((resolve, reject) => {
            window.requestAnimationFrame(() => resolve())
        }))

        const results = await Promise.allSettled(awaitPromises)

        $body.show()

        let errorMsg = ""
        for (const result of results) {
            if (result.status === "rejected")
                errorMsg += result.reason + '\n'
        }
        if (errorMsg.length > 0)
            throw new Error(errorMsg)
    }

}

