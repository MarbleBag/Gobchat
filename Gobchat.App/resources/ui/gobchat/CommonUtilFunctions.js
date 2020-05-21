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

'use strict';

var Gobchat = (function (Gobchat) {
    Gobchat.isString = function (value) {
        return typeof value === 'string' || value instanceof String
    }

    Gobchat.isBoolean = function (value) {
        return typeof value === 'boolean' || value instanceof Boolean
    }

    Gobchat.isNumber = function (value) {
        return typeof value === 'number' && isFinite(value)
    }

    Gobchat.isFunction = function (value) {
        return typeof value === 'function';
    }

    Gobchat.isArray = function (value) {
        return Array.isArray(value)
        //return value && typeof value === 'object' && value.constructor === Array
    }

    Gobchat.isObject = function (value) {
        return Object.prototype.toString.call(value) === "[object Object]" //only cross-window reliable solution
        //return value && typeof value === 'object' && value.constructor === Object;
    }

    Gobchat.generateId = function (length) {
        return Math.random().toString(36).substr(2, Math.max(1, length))
    }

    Gobchat.formatString = function (str) {
        if (arguments.length > 1) {
            let args = Array.prototype.slice.call(arguments, 1);
            const t = typeof args[0]
            args = ("string" === t || "number" === t) ? Array.prototype.slice.call(args) : args[0]
            for (let key in args) {
                str = str.replace(new RegExp("\\{" + key + "\\}", "gi"), args[key])
            }
        }
        return str
    }

    Gobchat.encodeHtmlEntities = function (str) {
        return str.replace(/[\u00A0-\u9999<>&](?!#)/gim, function (i) {
            return '&#' + i.charCodeAt(0) + ';';
        });
    }

    Gobchat.decodeHtmlEntities = function (str) {
        return str.replace(/&#([0-9]{1,3});/gi, function (match, num) {
            return String.fromCharCode(parseInt(num));
        });
    }

    Gobchat.decodeUnicode = function (str) {
        return str.replace(/[uU]\+([\da-fA-F]{4})/g,
            function (match, num) {
                return String.fromCharCode(parseInt(num, 16));
            });
    }

    Gobchat.encodeUnicode = function (str) {
        return Array.from(str)
            .map((v) => v.codePointAt(0).toString(16))
            .map((hex) => "U+" + "0000".substring(0, 4 - hex.length) + hex)
            .join("")
    }

    function unicodeToChar(text) {
    }

    return Gobchat
}(Gobchat || {}));