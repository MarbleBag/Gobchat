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

import * as Utility from './CommonUtility.js'
import { EventDispatcher } from './EventDispatcher.js'

export module HtmlAttribute {
    export const TextId = "data-gob-locale-text"
    export const TooltipId = "data-gob-locale-tooltip"
    export const ActiveLocale = "data-gob-locale-code"
}

export function setLocalizedTextId(element: HTMLElement | JQuery, id: string) {
    $(element).attr(HtmlAttribute.TextId, id).removeAttr(HtmlAttribute.ActiveLocale);
}

export function setLocalizedTooltipId(element: HTMLElement | JQuery, id: string) {
    $(element).attr(HtmlAttribute.TooltipId, id).removeAttr(HtmlAttribute.ActiveLocale);
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


    async loadIntoLookup(keys: string | string[], params?: string[] | null, lookup?: { [s: string]: string } | null, locale?: string | null): Promise<{ [s: string]: string }> {
        const keysToLoad = ([] as string[]).concat(keys)
        let localeLookup = lookup ?? {}
        locale = locale ?? this.#locale
        
        let missingKeys = keysToLoad
        if (Object.keys(localeLookup).length > 0)
            missingKeys = keysToLoad.filter(key => !(localeLookup[key] === undefined || localeLookup[key] === null))        

        if (missingKeys.length > 0) {
            const localization = await GobchatAPI.getLocalizedStrings(locale, missingKeys)
            if (localeLookup) {
                for (const key in localization)
                    localeLookup[key] = localization[key]
            }
            else {
                localeLookup = localization
            }                
        }

        const results = {}
        if (params && params.length > 0) {
            for (const key of keysToLoad)
                results[key] = Utility.formatString(localeLookup[key], ...params)
        } else {
            for (const key of keysToLoad)
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
        await updateElementTree([element], language || this.#locale)
    }

    async updateElements(elements: (HTMLElement | JQuery)[], language?: string) {
        await updateElementTree(elements, language || this.#locale)
    }
}

async function updateElementTree(roots: (HTMLElement | JQuery)[], locale: string) {
    const includeElementsWith = `[${HtmlAttribute.TextId}],[${HtmlAttribute.TooltipId}]`
    const ignoreElementsWith = `[${HtmlAttribute.ActiveLocale}=${locale}]`

    const elements: HTMLElement[] = []
    for (const root of roots) {
        const selectedElements = $(root).find(includeElementsWith).addBack(includeElementsWith).not(ignoreElementsWith)
        elements.push(...selectedElements)
    }

    await updateElements(elements, locale)
}

async function updateElements(elements: HTMLElement[], locale: string) {
    const ids = collectIds(elements)
    await loadLocalizedText(elements, ids, locale)
}

function collectIds(elements: HTMLElement[]) {
    const ids: string[] = []
    for (const htmlElement of elements) {
        const textId = htmlElement.getAttribute(HtmlAttribute.TextId)
        if (textId !== null)
            ids.push(textId)

        const tooltipId = htmlElement.getAttribute(HtmlAttribute.TooltipId)
        if (tooltipId !== null)
            ids.push(tooltipId)
    }
    return ids
}

async function loadLocalizedText(elements: HTMLElement[], ids: string[], locale: string) {
    if (ids.length === 0)
        return

    const lookup = await GobchatAPI.getLocalizedStrings(locale, ids)
    for (const element of elements) {
        element.setAttribute(HtmlAttribute.ActiveLocale, locale)

        const textId = element.getAttribute(HtmlAttribute.TextId)
        if (textId) {
            const text = lookup[textId]
            element.innerHTML = text
        }

        const tooltipId = element.getAttribute(HtmlAttribute.TooltipId)
        if (tooltipId) {
            const text = lookup[tooltipId]
            element.setAttribute("title", text)
        }
    }
}