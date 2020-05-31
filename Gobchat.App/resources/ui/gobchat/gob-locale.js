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
    async function updateLocale(htmlRoot, locale) {
        const localizedElements = $(htmlRoot).find(".gob-locale-text .gob-locale-title")

        const ids = []

        localizedElements.each(() => {
            const $this = $(this)
            const id = $this.attr("data-gob-locale")
            if (id)
                ids.push($(this).attr(id))
        })

        const lookup = await GobchatAPI.getLocalizedStrings(locale, ids)

        localizedElements.each(() => {
            const $this = $(this)
            const id = $this.attr("data-gob-locale")
            const txt = lookup[id]

            if ($this.hasClass("gob-locale-title")) {
                $this.prop("title", txt);
            } else if ($this.hasClass("gob-locale-text")) {
                $this.text(txt)
            }
        })
    }

    async function updateElementLocale(htmlElement, locale) {
        const $this = $(this)
        const id = $this.attr("data-gob-locale")
        if (!id) return

        const txt = await GobchatAPI.getLocalizedString(locale, id)

        if ($this.hasClass("gob-locale-title")) {
            $this.prop("title", txt);
        } else if ($this.hasClass("gob-locale-text")) {
            $this.text(txt)
        }
    }

    Gobchat.changeLanguageOnPage = updateLocale
    Gobchat.changeLanguageOnElement = updateElementLocale

    return Gobchat
}(Gobchat || {}));