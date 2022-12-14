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
var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var _EventDispatcher_listenersByTopic;
export class EventDispatcher {
    constructor() {
        _EventDispatcher_listenersByTopic.set(this, new Map([]));
    }
    dispatch(topic, data) {
        const listeners = __classPrivateFieldGet(this, _EventDispatcher_listenersByTopic, "f")[topic]; //.get(topic)
        if (listeners) {
            const callbacks = listeners.slice(0);
            callbacks.forEach((callback) => callback(data));
        }
    }
    on(topic, callback) {
        if (!callback)
            return false;
        let listeners = __classPrivateFieldGet(this, _EventDispatcher_listenersByTopic, "f")[topic]; //.get(topic)
        if (!listeners) {
            listeners = [];
            __classPrivateFieldGet(this, _EventDispatcher_listenersByTopic, "f")[topic] = listeners; // .set(topic, listeners)
        }
        listeners.push(callback);
        return true;
    }
    off(topic, callback) {
        let listeners = __classPrivateFieldGet(this, _EventDispatcher_listenersByTopic, "f").get(topic);
        if (listeners) {
            const idx = listeners.indexOf(callback);
            if (idx > -1)
                listeners.splice(idx, 1);
            if (listeners.length === 0)
                __classPrivateFieldGet(this, _EventDispatcher_listenersByTopic, "f").delete(topic);
            return idx > -1;
        }
        return false;
    }
}
_EventDispatcher_listenersByTopic = new WeakMap();
