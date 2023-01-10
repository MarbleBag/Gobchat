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

export class LocaleManager {
    #locale: string = 'en'
    #eventDispatcher: EventDispatcher = new EventDispatcher()

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
                results[key] = Utility.formatString(localeLookup[key], params)
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
    const selectedElements = $(htmlElement).find(selector).addBack(selector)
    if (selectedElements.length == 0)
        return

    const stringIds: string[] = []

    selectedElements.each(function () {
        const $this = $(this)
        const textId = $this.attr(AttributeTextKey)
        if (textId && typeof textId === "string") 
            // const tooltipId = `${textId}.tooltip` // needs to be streamlined to autodetect inputs and annotate them with the tooltip class
            stringIds.push(textId)
        

        const tooltipId = $this.attr(AttributeTooltipKey)
        if (tooltipId && typeof tooltipId === "string")
            stringIds.push(tooltipId)
    })

    if (stringIds.length == 0)
        return

    const lookup = await GobchatAPI.getLocalizedStrings(locale, stringIds)

    selectedElements.each(function () {
        const $this = $(this)

        const textId = $this.attr(AttributeTextKey)
        if (textId && typeof textId === "string") {
            const text = lookup[textId]
            const elementHtml = $this.html()
            if(elementHtml){
                const html = $("<div></div>").html(text).html()
                if(elementHtml !== html)
                    $this.html(html)
            }else{
                $this.html(text)
            }
        }

        const tooltipId = $this.attr(AttributeTooltipKey)
        if (tooltipId && typeof tooltipId === "string") {
            const text = lookup[tooltipId]            
            if($this.attr("title") !== text)
                $this.attr("title", text)
            //$this.attr(AttributeTooltip, txt)            
        }
    })
}