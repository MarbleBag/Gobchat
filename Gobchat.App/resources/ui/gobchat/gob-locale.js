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
            if ($this.attr(DataAttributeText)) stringIds.push($this.attr(DataAttributeText))
            if ($this.attr(DataAttributeTitle)) stringIds.push($this.attr(DataAttributeTitle))
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

    class GobLocaleManager {
        setLocale(locale) {
            this._locale = locale
        }

        async get(key) {
            const lookup = await getAll([key])
            return lookup[key]
        }

        async getAll(keys) {
            const lookup = await GobchatAPI.getLocalizedStrings(this._locale, keys || [])
            return lookup
        }

        async getAndFormat(key, params) {
            let value = await this.get(key)
            value = Gobchat.formatString(value, params)
            return value
        }

        async updateElement(element) {
            await updateLocaleOnElements(element, this._locale)
        }
    }

    Gobchat.GobLocaleManager = GobLocaleManager

    return Gobchat
}(Gobchat || {}));