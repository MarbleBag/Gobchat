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
var _StyleLoader_styles, _StyleLoader_activeStyles, _StyleLoader_activeStyleSheetIds, _StyleLoader_filePrefix;
import * as Utility from './CommonUtility.js';
export class StyleLoader {
    constructor(filePrefix) {
        _StyleLoader_styles.set(this, {});
        _StyleLoader_activeStyles.set(this, []);
        _StyleLoader_activeStyleSheetIds.set(this, []);
        _StyleLoader_filePrefix.set(this, void 0);
        __classPrivateFieldSet(this, _StyleLoader_filePrefix, filePrefix ? filePrefix : null, "f");
    }
    async initialize() {
        __classPrivateFieldSet(this, _StyleLoader_styles, {}, "f");
        const json = await GobchatAPI.readTextFromFile("ui/styles/styles.json");
        const styles = JSON.parse(json);
        for (let style of styles) {
            const styleKey = style.label.toLowerCase().trim();
            if (styleKey.length == 0)
                continue;
            __classPrivateFieldGet(this, _StyleLoader_styles, "f")[styleKey] = {
                label: style.label,
                files: [].concat(style.files || []).filter(e => Utility.isString(e)).map(e => e.trim()).filter(e => e.length > 0)
            };
        }
    }
    get styles() {
        return Object.keys(__classPrivateFieldGet(this, _StyleLoader_styles, "f"))
            .map(key => {
            return {
                id: key,
                label: __classPrivateFieldGet(this, _StyleLoader_styles, "f")[key].label
            };
        });
    }
    get activeStyles() {
        return [].concat(__classPrivateFieldGet(this, _StyleLoader_activeStyles, "f") || []);
    }
    async activateStyles(styleIds) {
        styleIds = [].concat(styleIds || []).filter(e => Utility.isString(e)).map(e => e.toLowerCase());
        for (let styleId of styleIds)
            if (!__classPrivateFieldGet(this, _StyleLoader_styles, "f")[styleId])
                throw new Error(`Style with id '${styleId}' not available`);
        const $head = $("head");
        const $body = $("body");
        // use hide / show to trigger a reflow, so the new loaded style gets applied everywhere.
        // Sometimes, without this, styles aren't applied to scrollbars. Still no idea why.
        $body.hide();
        for (let id of __classPrivateFieldGet(this, _StyleLoader_activeStyleSheetIds, "f"))
            $(`#${id}`).remove();
        __classPrivateFieldSet(this, _StyleLoader_activeStyleSheetIds, [], "f");
        __classPrivateFieldSet(this, _StyleLoader_activeStyles, [], "f");
        const awaitPromises = [];
        for (let styleId of styleIds) {
            __classPrivateFieldGet(this, _StyleLoader_activeStyles, "f").push(styleId);
            const style = __classPrivateFieldGet(this, _StyleLoader_styles, "f")[styleId];
            const randomIdPrefix = Utility.generateId(8);
            for (let file of style.files) {
                const id = `gobstyle-${randomIdPrefix}-${file.replace(/[\s\\/]+/g, '_').replace(/[^-\.\w]+/g, '')}`;
                const $link = $(`<link rel="stylesheet" type="text/css" href="">`).attr('id', id);
                __classPrivateFieldGet(this, _StyleLoader_activeStyleSheetIds, "f").push(id);
                awaitPromises.push(new Promise(function (resolve, reject) {
                    $link.one("load", () => resolve());
                    $link.one("error", () => reject());
                }));
                const path = __classPrivateFieldGet(this, _StyleLoader_filePrefix, "f") ? `${__classPrivateFieldGet(this, _StyleLoader_filePrefix, "f")}/${file}` : file;
                $link.attr("href", path).appendTo($head);
            }
        }
        awaitPromises.push(new Promise((resolve, reject) => {
            window.requestAnimationFrame(() => resolve());
        }));
        const results = await Promise.allSettled(awaitPromises);
        $body.show();
        let errorMsg = "";
        for (const result of results) {
            if (result.status === "rejected")
                errorMsg += result.reason + '\n';
        }
        if (errorMsg.length > 0)
            throw new Error(errorMsg);
    }
}
_StyleLoader_styles = new WeakMap(), _StyleLoader_activeStyles = new WeakMap(), _StyleLoader_activeStyleSheetIds = new WeakMap(), _StyleLoader_filePrefix = new WeakMap();
