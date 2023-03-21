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

export function isString(value: unknown): value is string {
    return typeof value === 'string' || value instanceof String
}

export function isNonEmptyString(value: unknown): value is string {
    return isString(value) && (value as string).trim().length > 0
}

export function isBoolean(value: unknown): value is boolean {
    return typeof value === 'boolean' || value instanceof Boolean
}

export function isNumber(value: unknown): value is number {
    return typeof value === 'number' && isFinite(value)
}

export function isFunction(value: unknown): value is Function {
    return typeof value === 'function';
}

export function isArray(value: unknown): value is Array<unknown> {
    return Array.isArray(value)
    //return value && typeof value === 'object' && value.constructor === Array
}

export function isObject(value: unknown): value is object {
    return Object.prototype.toString.call(value) === "[object Object]"
    //return value && typeof value === 'object' && value.constructor === Object;
}

export function isjQuery(value: unknown): value is JQuery {
    return (value && (value instanceof jQuery || value.constructor.prototype.jquery));
}

interface ExtendOptionsDefault {

}
export type ExtendOptions = Partial<ExtendOptionsDefault>
export function extendObjectX<A extends object, B extends object>(target: A, obj1: B): A & B
export function extendObjectX<A extends object, B extends object, C extends object>(target: A, obj1: B, obj2: C): A & B & C
export function extendObjectX<A extends object, B extends object, C extends object, D extends object>(target: A, obj1: B, obj2: C, obj3: D): A & B & C & D
export function extendObjectX<A, B, C, D>(){
    
}

export function extend<A extends object, B extends object>(dst: A, src1: B): A & B
export function extend<A extends object, B extends object, C extends object>(dst: A, src1: B, src2: C): A & B & C
export function extend<A extends object, B extends object, C extends object, D extends object>(dst: A, src1: B, src2: C, src3: D): A & B & C & D
export function extend<A extends object, B extends object, C extends object, D extends object>(dst: A, src1?: B, src2?: C, src3?: D): A & B & C & D {
    const objects = [src1, src2, src3].filter(o => o !== null && o !== undefined)
    return Object.assign(dst, ...objects)
}

export function extendObject<A extends object, B extends object>(base: A, overwrites: B | B[], deepExtend: boolean = false,
    onlyOverwrite: boolean = false, ignoreOverwriteProperty: "non" | "undefined" | "null" | "both" = "both"): A {

    const objects = ([] as B[]).concat(overwrites || [])
    if (objects.length === 0)
        return base
    

    const assign = (() => {
        switch (ignoreOverwriteProperty) {
            case "non":
                return (a, b, key) => a[key] = b[key]
            case "undefined":
                return (a, b, key) => {
                    if (b[key] !== undefined)
                        a[key] = b[key]
                }
            case "null":
                return (a, b, key) => {
                    if (b[key] !== null)
                        a[key] = b[key]
                }
            default:
                return (a, b, key) => {
                    if (b[key] !== undefined && b[key] !== null)
                        a[key] = b[key]
                }
        }
    })()

    if (onlyOverwrite) {
        const keys = Object.keys(base)
        for (let i = 0; i < objects.length; ++i) {
            if (!objects[i])
                continue

            for (let key of keys) {
                if (key in objects[i]) {
                    assign(base, objects[i], key)
                }
            }
        }
    } else {
        if (ignoreOverwriteProperty === "non") {
            for (let i = 0; i < objects.length; ++i) {
                if (!objects[i])
                    continue

                base = Object.assign(base, objects[i])
            }
        } else {
            const keys = Object.keys(base)
            for (let i = 0; i < objects.length; ++i) {
                if (!objects[i])
                    continue

                for (let key of keys) {
                    assign(base, objects[i], key)
                }
            }
        }
    }

    return base
}

export function toFloat(value: string | number | boolean | undefined | null): number | null
export function toFloat(value: string | number | boolean | undefined | null, fallback: number): number 
export function toFloat(value: string | number | boolean | undefined | null, fallback?: number): number | null {
    if (isNumber(value))
        return value

    if (isString(value))
        return parseFloat(value)

    if (isBoolean(value))
        return value ? 1 : 0

    return fallback !== null && fallback !== undefined ? fallback : null
}

export function toInt(value: string | number | boolean | undefined | null): number | null
export function toInt(value: string | number | boolean | undefined | null, fallback: number): number
export function toInt(value: string | number | boolean | undefined | null, fallback?: number): number | null {
    if (isNumber(value))
        return Math.round(value)

    if (isString(value))
        return parseInt(value)

    if (isBoolean(value))
        return value ? 1 : 0

    return fallback !== null && fallback !== undefined ? fallback : null
}

export function extractNumbers(value: string): number[] {
    const result = value.match(/\d+\.\d+|\d+/g)
    if (result === null)
        return []

    return result.map(element => +element) 
}

export function extractFirstNumber(value: string): number | null {
    const result = value.match(/\d+\.\d+|\d+/)
    if (result === null)
        return null
    return +result[0]
}

export function generateId(length: number, exclude?:string[]): string {
    if(!exclude)
        return Math.random().toString(36).substr(2, Math.max(1, length))
    
    while(true){
        const newKey = Math.random().toString(36).substr(2, Math.max(1, length))
        if(exclude.every(e => e !== newKey))
            return newKey
    }
}

export function formatString(text: string, ...args: (string|number)[]) {
    for (const key in args) {
        text = text.replace(new RegExp("\\{" + key + "\\}", "gi"), args[key].toString())
    }
    return text
}

export function encodeHtmlEntities(str: string): string {
    return str.replace(/[\u00A0-\u9999<>&](?!#)/gim, function (i) {
        return '&#' + i.charCodeAt(0) + ';';
    });
}

export function decodeHtmlEntities(str: string): string {
    return str.replace(/&#([0-9]{1,3});/gi, function (match, num) {
        return String.fromCharCode(parseInt(num));
    });
}

export function decodeUnicode(str: string): string {
    return str.replace(/[uU]\+([\da-fA-F]{4})/g,
        function (match, num) {
            return String.fromCharCode(parseInt(num, 16));
        });
}

export function encodeUnicode(str: string): string {
    return Array.from(str)
        .map(char => char.codePointAt(0))
        .filter(code => typeof code === "number")
        .map(code => code!.toString(16))
        .map(hex => "U+" + "0000".substring(0, 4 - hex.length) + hex)
        .join("")
}

export function decodeKeyEventToText(keyEvent: KeyboardEvent, ignoreEnter: boolean = true): string | null {
    if (ignoreEnter && keyEvent.key === "Enter")
        return null

    if (keyEvent.key === "Shift" || keyEvent.key === "Control" || keyEvent.key === "Alt")
        return ""

    let msg = ""
    if (keyEvent.shiftKey) msg += "Shift + "
    if (keyEvent.altKey) msg += "Alt + "
    if (keyEvent.ctrlKey) msg += "Ctrl + "

    var keyEnum = Gobchat.KeyCodeToKeyEnum(keyEvent.keyCode)
    if (keyEnum === null) {
        msg = ""
    } else {
        msg += keyEnum
    }
    return msg
}