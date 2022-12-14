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
'use strict';
var __classPrivateFieldSet = (this && this.__classPrivateFieldSet) || function (receiver, state, value, kind, f) {
    if (kind === "m") throw new TypeError("Private method is not writable");
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a setter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot write private member to an object whose class did not declare it");
    return (kind === "a" ? f.call(receiver, value) : f ? f.value = value : state.set(receiver, value)), value;
};
var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var _LocaleManager_locale;
import * as Utility from './CommonUtility.js';
export const AttributeTextKey = "data-gob-locale-text";
export const AttributeTooltipKey = "data-gob-locale-tooltip";
export const AttributeTooltip = "data-gob-tooltip";
export class LocaleManager {
    constructor() {
        _LocaleManager_locale.set(this, void 0);
    }
    setLocale(locale) {
        __classPrivateFieldSet(this, _LocaleManager_locale, locale, "f");
    }
    async loadIntoLookup(keys, params, lookup, language) {
        keys = [].concat(keys);
        language = language || __classPrivateFieldGet(this, _LocaleManager_locale, "f");
        let missingKeys = keys;
        if (lookup)
            missingKeys = keys.filter(e => !(lookup[e] === undefined || lookup[e] === null));
        if (missingKeys.length > 0) {
            const localization = await GobchatAPI.getLocalizedStrings(language, missingKeys);
            if (lookup) {
                for (let key in localization)
                    lookup[key] = localization[key];
            }
            else {
                lookup = localization;
            }
        }
        const results = {};
        if (params && params.length > 0) {
            for (let key of keys)
                results[key] = Utility.formatString(lookup[key], params);
        }
        else {
            for (let key of keys)
                results[key] = lookup[key];
        }
        return results;
    }
    async get(key, language) {
        const lookup = await this.getAll([key], language);
        return lookup[key];
    }
    async getAll(keys, language) {
        const lookup = await GobchatAPI.getLocalizedStrings(language || __classPrivateFieldGet(this, _LocaleManager_locale, "f"), keys || []);
        return lookup;
    }
    async getAndFormat(key, params) {
        params = [].concat(params);
        const value = await this.get(key);
        return Utility.formatString(value, ...params);
    }
    async updateElement(element, language) {
        await updateDomTree(element, language || __classPrivateFieldGet(this, _LocaleManager_locale, "f"));
    }
}
_LocaleManager_locale = new WeakMap();
async function updateDomTree(htmlElement, locale) {
    const selector = `[${AttributeTextKey}],[${AttributeTooltipKey}]`;
    const selectedElements = $(htmlElement).find(selector).addBack(selector);
    if (selectedElements.length == 0)
        return;
    const stringIds = [];
    selectedElements.each(function () {
        const $this = $(this);
        if ($this.attr(AttributeTextKey)) {
            const textId = $this.attr(AttributeTextKey);
            // const tooltipId = `${textId}.tooltip` // needs to be streamlined to autodetect inputs and annotate them with the tooltip class
            stringIds.push(textId);
            // stringIds.push(tooltipId)
        }
        if ($this.attr(AttributeTooltipKey))
            stringIds.push($this.attr(AttributeTooltipKey));
    });
    if (stringIds.length == 0)
        return;
    const lookup = await GobchatAPI.getLocalizedStrings(locale, stringIds);
    selectedElements.each(function () {
        const $this = $(this);
        if ($this.attr(AttributeTextKey)) {
            const id = $this.attr(AttributeTextKey);
            const txt = lookup[id];
            $this.html(txt);
        }
        if ($this.attr(AttributeTooltipKey)) {
            const id = $this.attr(AttributeTooltipKey);
            const txt = lookup[id];
            $this.prop(AttributeTooltip, txt);
            $this.prop("title", txt);
        }
    });
}
