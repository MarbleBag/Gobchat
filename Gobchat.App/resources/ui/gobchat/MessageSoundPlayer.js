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

var Gobchat = (function (Gobchat, undefined) {
    const MessageSegmentEnum = Gobchat.MessageSegmentEnum

    class MessageSoundPlayer {
        constructor(config) {
            this._config = config
            this._lastSoundPlayed = new Date()
        }

        checkForSound(message) {
            const data = this._config.get("behaviour.mentions.data.base")
            if (!data.playSound || data.volume <= 0 || !data.soundPath)
                return

            const time = new Date()
            if (time - this._lastSoundPlayed < data.soundInterval)
                return

            const hasMention = message.content.some(segment => { return segment.type === MessageSegmentEnum.MENTION })
            if (!hasMention)
                return

            this._lastSoundPlayed = time
            const audio = new Audio(data.soundPath)
            audio.volume = data.volume
            audio.play()
        }
    }

    Gobchat.MessageSoundPlayer = MessageSoundPlayer

    return Gobchat
}(Gobchat || {}));