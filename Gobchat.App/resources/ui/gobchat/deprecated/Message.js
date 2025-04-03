/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
    class MessageSegment {
        constructor(messageSegmentEnum, messageText) {
            this.segmentType = messageSegmentEnum
            this.messageText = messageText
            this.segmentDetails = null
        }
    }
    Gobchat.MessageSegment = MessageSegment

    class MessageSource {
        constructor(sourceId) {
            this.sourceId = sourceId
            this.ffGroupId = null //a number, only set if the source is a player, which is also in a group of the ingame friendlist

            this.playerName = null
            this.prefix = null
        }
    }
    Gobchat.MessageSource = MessageSource

    class Message {
        constructor(timestamp, messageSource, channelEnum, messageSegments) {
            this.timestamp = timestamp
            this.source = messageSource
            this.channel = channelEnum
            this.segments = messageSegments
        }
    }
    Gobchat.Message = Message

    return Gobchat
}(Gobchat || {}));