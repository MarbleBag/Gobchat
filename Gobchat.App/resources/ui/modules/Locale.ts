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

export const AttributeTextKey = "data-gob-locale-text"
export const AttributeTooltipKey = "data-gob-locale-tooltip"
export const AttributeTooltip = "data-gob-tooltip"

export class LocaleManager {
    #locale: string

    setLocale(locale: string) {
        this.#locale = locale
    }

    async loadIntoLookup(keys: string | string[], params?: string[], lookup?: { [s: string]: string }, language?: string): Promise<{ [s: string]: string }> {
        keys = [].concat(keys)
        language = language || this.#locale

        let missingKeys = keys
        if (lookup)
            missingKeys = keys.filter(e => !(lookup[e] === undefined || lookup[e] === null))

        if (missingKeys.length > 0) {
            const localization = await GobchatAPI.getLocalizedStrings(language, missingKeys)
            if (lookup) {
                for (let key in localization)
                    lookup[key] = localization[key]
            }
            else {
                lookup = localization
            }                
        }

        const results = {}
        if (params && params.length > 0) {
            for (let key of keys)
                results[key] = Utility.formatString(lookup[key], params)
        } else {
            for (let key of keys)
                results[key] = lookup[key]
        }
        return results
    }

    async get(key: string, language?: string): Promise<string> {
        const lookup = await this.getAll([key], language)
        return lookup[key]
    }

    async getAll(keys: string[], language?: string): Promise<{ [s: string]: string }> {
        const lookup = await GobchatAPI.getLocalizedStrings(language || this.#locale, keys || [])
        return lookup
    }

    async getAndFormat(key: string, params: any | any[]): Promise<string> {
        params = [].concat(params)
        const value = await this.get(key)
        return Utility.formatString(value, ...params)
    }

    async updateElement(element: HTMLElement | JQuery, language?: string) {
        await updateDomTree(element, language || this.#locale)
    }
}

async function updateDomTree(htmlElement: HTMLElement | JQuery, locale: string) {
    const selector = `[${AttributeTextKey}],[${AttributeTooltipKey}]` 
    const selectedElements = $(htmlElement).find(selector).addBack(selector)
    if (selectedElements.length == 0)
        return

    const stringIds = []

    selectedElements.each(function () {
        const $this = $(this)
        if ($this.attr(AttributeTextKey)) {
            const textId = $this.attr(AttributeTextKey)
            // const tooltipId = `${textId}.tooltip` // needs to be streamlined to autodetect inputs and annotate them with the tooltip class
            stringIds.push(textId)
            // stringIds.push(tooltipId)
        }

        if ($this.attr(AttributeTooltipKey))
            stringIds.push($this.attr(AttributeTooltipKey))
    })

    if (stringIds.length == 0)
        return

    const lookup = await GobchatAPI.getLocalizedStrings(locale, stringIds)

    selectedElements.each(function () {
        const $this = $(this)

        if ($this.attr(AttributeTextKey)) {
            const id = $this.attr(AttributeTextKey)
            const txt = lookup[id]
            $this.html(txt)
        }

        if ($this.attr(AttributeTooltipKey)) {
            const id = $this.attr(AttributeTooltipKey)
            const txt = lookup[id]            
            $this.prop(AttributeTooltip, txt)
            $this.prop("title", txt)
        }
    })
}