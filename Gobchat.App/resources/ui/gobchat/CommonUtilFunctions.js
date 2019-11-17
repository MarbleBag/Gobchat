'use strict';

var Gobchat = (function (Gobchat) {
    Gobchat.sendMessageToPlugin = function (obj) {
        if (!obj) return
        let sJson = JSON.stringify(obj)
        console.log(`Send JSON to App:\n${sJson}`)
        GobchatAPI.message(sJson)
    }

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
        return value && typeof value === 'object' && value.constructor === Object;
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

    return Gobchat
}(Gobchat || {}));