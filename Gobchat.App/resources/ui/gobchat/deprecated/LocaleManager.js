/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
    const DataAttributeText = "data-gob-locale-text"
    const DataAttributeTitle = "data-gob-locale-title"
    const LocaleClass = "gob-localized"
    const ElementSelector = `[${DataAttributeText}],[${DataAttributeTitle}]`

    async function updateLocaleOnElements(htmlElement, locale) {
        const selectedElements = $(htmlElement).find(ElementSelector).addBack(ElementSelector)
        if (selectedElements.length == 0)
            return

        const stringIds = []

        selectedElements.each(function () {
            const $this = $(this)
            if ($this.attr(DataAttributeText)) {
                const textId = $this.attr(DataAttributeText)
               // const tooltipId = `${textId}.tooltip` // needs to be streamlined to autodetect inputs and annotate them with the tooltip class
                stringIds.push(textId)
               // stringIds.push(tooltipId)
            }

            if ($this.attr(DataAttributeTitle))
                stringIds.push($this.attr(DataAttributeTitle))
        })

        if (stringIds.length == 0)
            return

        const lookup = await GobchatAPI.getLocalizedStrings(locale, stringIds)

        selectedElements.each(function () {
            const $this = $(this)

            if ($this.attr(DataAttributeText)) {
                const id = $this.attr(DataAttributeText)
                const txt = lookup[id]
                $this.html(txt)
            }

            if ($this.attr(DataAttributeTitle)) {
                const id = $this.attr(DataAttributeTitle)
                const txt = lookup[id]
                $this.prop("title", txt);
            }
        })
    }

    class LocaleManager {
        setLocale(locale) {
            this._locale = locale
        }

        async load(keys, params, lookup, language) {
            keys = [].concat(keys)
            language = language || this._locale

            var missingKeys = keys
            if (lookup)
                missingKeys = keys.filter(e => !(lookup[e] === undefined || lookup[e] === null))

            if (missingKeys.length > 0) {
                const localization = await GobchatAPI.getLocalizedStrings(language, missingKeys)
                if (lookup)
                    Object.entries(localization).forEach((k, v) => lookup[k] = v)
                else
                    lookup = localization
            }

            var results = lookup
            if (Object.keys(lookup).length !== keys.length)
                results = keys.map(e => lookup[e])

            if (params && params.length > 0)
                for (var key in Object.keys(results))
                    results[key] = Gobchat.formatString(results[key], params)

            if (keys.length === 1)
                return results[keys[0]]

            return results
        }

        async get(key, language) {
            const lookup = await this.getAll([key], language)
            return lookup[key]
        }

        async getAll(keys, language) {
            const lookup = await GobchatAPI.getLocalizedStrings(language || this._locale, keys || [])
            return lookup
        }

        async getAndFormat(key, params) {
            let value = await this.get(key)
            value = Gobchat.formatString(value, params)
            return value
        }

        async updateElement(element, language) {
            await updateLocaleOnElements(element, language || this._locale)
        }
    }

    Gobchat.LocaleManager = LocaleManager

    return Gobchat
}(Gobchat || {}));