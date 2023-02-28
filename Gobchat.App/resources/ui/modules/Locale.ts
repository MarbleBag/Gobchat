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
import { EventDispatcher } from './EventDispatcher.js'

export const AttributeTextKey = "data-gob-locale-text"
export const AttributeTooltipKey = "data-gob-locale-tooltip"
export const AttributeTooltip = "data-gob-tooltip"
export const AttributeActiveLocale = "data-gob-locale-code"

export function setLocalizedText(element: HTMLElement | JQuery, key: string) {
    $(element).attr(AttributeTextKey, key).removeAttr(AttributeActiveLocale);
}

export function setLocalizedTooltip(element: HTMLElement | JQuery, key: string) {
    $(element).attr(AttributeTooltipKey, key).removeAttr(AttributeActiveLocale);
}

export class LocaleManager {
    #locale: string = 'en'
    #eventDispatcher: EventDispatcher = new EventDispatcher()

    get locale(): string {
        return this.#locale
    }

    setLocale(locale: string) {
        const oldLocale = this.#locale
        this.#locale = locale ?? this.#locale
        const localeChanged = oldLocale !== this.#locale
        if (localeChanged)
            this.#eventDispatcher.dispatch("change", {})
    }

    addLocaleChangeListener(callback: () => void) {
        this.#eventDispatcher.on("change", callback)
    }

    removeLocaleChangeListener(callback: () => void) {
        this.#eventDispatcher.off("change", callback)
    }


    async loadIntoLookup(keys: string | string[], params?: string[] | null, lookup?: { [s: string]: string } | null, language?: string | null): Promise<{ [s: string]: string }> {
        const keysToLoad = ([] as string[]).concat(keys)
        let localeLookup = lookup ?? {}
        language = language ?? this.#locale
        
        let missingKeys = keysToLoad
        if (Object.keys(localeLookup).length > 0)
            missingKeys = keysToLoad.filter(key => !(localeLookup[key] === undefined || localeLookup[key] === null))        

        if (missingKeys.length > 0) {
            const localization = await GobchatAPI.getLocalizedStrings(language, missingKeys)
            if (localeLookup) {
                for (let key in localization)
                    localeLookup[key] = localization[key]
            }
            else {
                localeLookup = localization
            }                
        }

        const results = {}
        if (params && params.length > 0) {
            for (let key of keysToLoad)
                results[key] = Utility.formatString(localeLookup[key], ...params)
        } else {
            for (let key of keysToLoad)
                results[key] = localeLookup[key]
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
    const selectedElements = $(htmlElement).find(selector).addBack(selector).not(`[${AttributeActiveLocale}=${locale}]`)
    if (selectedElements.length == 0)
        return

    const stringIds: string[] = []

    for (const selectedElement of selectedElements) {
        const textId = selectedElement.getAttribute(AttributeTextKey)
        if (textId)
            stringIds.push(textId)

        const tooltipId = selectedElement.getAttribute(AttributeTooltipKey)
        if (tooltipId)
            stringIds.push(tooltipId)

        selectedElement.setAttribute(AttributeActiveLocale, locale)
    }

    if (stringIds.length == 0)
        return

    const lookup = await GobchatAPI.getLocalizedStrings(locale, stringIds)

    for (const selectedElement of selectedElements) {
        const textId = selectedElement.getAttribute(AttributeTextKey)
        if (textId) {
            const text = lookup[textId]
            selectedElement.innerHTML = text
        }

        const tooltipId = selectedElement.getAttribute(AttributeTooltipKey)
        if (tooltipId) {
            const text = lookup[tooltipId]
            selectedElement.setAttribute("title", text)
        }
    }
}