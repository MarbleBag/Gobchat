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
export function isString(value) {
    return typeof value === 'string' || value instanceof String;
}
export function isBoolean(value) {
    return typeof value === 'boolean' || value instanceof Boolean;
}
export function isNumber(value) {
    return typeof value === 'number' && isFinite(value);
}
export function isFunction(value) {
    return typeof value === 'function';
}
export function isArray(value) {
    return Array.isArray(value);
    //return value && typeof value === 'object' && value.constructor === Array
}
export function isObject(value) {
    return Object.prototype.toString.call(value) === "[object Object]"; //only cross-window reliable solution
    //return value && typeof value === 'object' && value.constructor === Object;
}
export function toNumber(value, fallback) {
    if (isNumber(value))
        return value;
    if (isString(value))
        return parseFloat(value);
    if (isBoolean(value))
        return value ? 1 : 0;
    return fallback !== undefined ? fallback : null;
}
export function generateId(length) {
    return Math.random().toString(36).substr(2, Math.max(1, length));
}
export function formatString(text, ...args) {
    for (let key in args) {
        text = text.replace(new RegExp("\\{" + key + "\\}", "gi"), args[key]);
    }
    return text;
}
export function encodeHtmlEntities(str) {
    return str.replace(/[\u00A0-\u9999<>&](?!#)/gim, function (i) {
        return '&#' + i.charCodeAt(0) + ';';
    });
}
export function decodeHtmlEntities(str) {
    return str.replace(/&#([0-9]{1,3});/gi, function (match, num) {
        return String.fromCharCode(parseInt(num));
    });
}
export function decodeUnicode(str) {
    return str.replace(/[uU]\+([\da-fA-F]{4})/g, function (match, num) {
        return String.fromCharCode(parseInt(num, 16));
    });
}
export function encodeUnicode(str) {
    return Array.from(str)
        .map((v) => v.codePointAt(0).toString(16))
        .map((hex) => "U+" + "0000".substring(0, 4 - hex.length) + hex)
        .join("");
}
export function decodeKeyEventToText(keyEvent, ignoreEnter = true) {
    if (ignoreEnter && keyEvent.keyCode == 13) // enter
        return null;
    if (keyEvent.keyCode === 16 || keyEvent.keyCode === 17 || keyEvent.keyCode === 18)
        return "";
    let msg = "";
    if (keyEvent.shiftKey)
        msg += "Shift + ";
    if (keyEvent.altKey)
        msg += "Alt + ";
    if (keyEvent.ctrlKey)
        msg += "Ctrl + ";
    var keyEnum = Gobchat.KeyCodeToKeyEnum(keyEvent.keyCode);
    if (keyEnum === null) {
        msg = "";
    }
    else {
        msg += keyEnum;
    }
    return msg;
}
