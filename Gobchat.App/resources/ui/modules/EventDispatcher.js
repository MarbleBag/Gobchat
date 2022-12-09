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

'use strict'

export class EventDispatcher {
    constructor() {
        this.listenersByTopic = new Map([])
    }
    dispatch(topic, data) {
        const listeners = this.listenersByTopic.get(topic)
        if (listeners) {
            const callbacks = listeners.slice(0)
            callbacks.forEach((callback) => { callback(data) })
        }
    }
    on(topic, callback) {
        if (!callback) return false
        let listeners = this.listenersByTopic.get(topic)
        if (!listeners) {
            listeners = []
            this.listenersByTopic.set(topic, listeners)
        }
        listeners.push(callback)
        return true
    }
    off(topic, callback) {
        let listeners = this.listenersByTopic.get(topic)
        if (listeners) {
            const idx = listeners.indexOf(callback)
            if (idx > -1) listeners.splice(idx, 1)
            if (listeners.length === 0) this.listenersByTopic.delete(topic)
            return idx > -1
        }
        return false
    }
}