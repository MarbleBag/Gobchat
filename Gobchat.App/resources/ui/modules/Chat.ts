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

import * as Cmd from './Command.js'
import * as Style from './Style.js'
import * as Message from './ChatMessage.js'

// created by backend
export type ChatChannelEnum = number

// created by backend
export interface Channel {
    chatChannel: ChatChannelEnum
    clientChannel: number[]
    internalName: string
    configId: string
    relevant: boolean
    abbreviationId: string
    translationId: string
    tooltipId: string
}

// created by backend
export interface ChatMessageSource {
    original: string
    characterName: string
    triggerGroup: string
    ffGroup: number
    party: number
    allianec: number
    visibility: number
    isAPlayer: boolean
    isUser: boolean
    isApp: boolean
}

// created by backend
export type MessageSegmentEnum = number

// created by backend
export interface MessageSegment {
    type: MessageSegmentEnum
    text: string
}

// created by backend
export interface ChatMessage {
    source: ChatMessageSource
    timeStamp: Date
    channel: ChatChannelEnum
    content: MessageSegment[]
    containsMentions: boolean
}

export class ChatManager {
    #control: JQuery = null
    #cmdManager: Cmd.CommandManager
    #msgBuilder: MessageBuilder
    #audioPlayer: AudioPlayer

    #hideInfo: boolean = false
    #hideError: boolean = false

    constructor() {
        this.#cmdManager = new Cmd.CommandManager()
        this.#audioPlayer = new AudioPlayer()
        this.#msgBuilder = new MessageBuilder()

        document.addEventListener("ChatMessagesEvent", this.#onNewMessageEvent)
    }

    #onNewMessageEvent = (event: ChatMessagesEvent): void => { // bind
        if (!!event.detail.messages) {
            for (let message of event.detail.messages) {
                this.#onNewMessage(message)
            }
        }
    }

    #onNewMessage = (message: ChatMessage): void => {
        if (this.#hideInfo && message.channel === Gobchat.ChannelEnum.GOBCHATINFO)
            return

        if (this.#hideError && message.channel === Gobchat.ChannelEnum.GOBCHATERROR)
            return

        if (message.channel === Gobchat.ChannelEnum.ECHO) {
            const joinedMessageContent = message.content.map(e => e.text).join()
            this.#cmdManager.processCommand(joinedMessageContent)
        }

        const messageAsHtml = this.#msgBuilder.build(message)

        //TODO

        this.#audioPlayer.playSoundIfAllowed(message)
    }

    showGobInfo(on: boolean): void {
        this.#hideError = on
    }

    showGobError(on: boolean): void {
        this.#hideInfo = on
    }

    test() {
        this.control($(".gob-chat_box"))
    }

    control(chatElement: Element | JQuery): void {
        if (this.#control !== null) {

        }

        this.#control = $(chatElement)
    }
}

class ScrollControl {
    #control: any = null
    #scrollToBottom: boolean

    constructor() {


       
    }

    #onScroll = ($element) => {
        //const $this = $(this)
        //const closeToBottom = ($this.scrollTop() + $this.innerHeight() + 5 >= $this[0].scrollHeight) // +5px for 'being very close'
       // this.#scrollToBottom = closeToBottom
    }

    control($element){
        if (this.#control !== null) {
            const x = $(document).on("scroll", this.#onScroll)
        }

    }
}

class MessageBuilder {
    build(message: ChatMessage) {
        throw new Error('Method not implemented.')
    }

}

class AudioPlayer {
    #lastSoundPlayed: Date

    constructor() {
        this.#lastSoundPlayed = new Date()
    }

    playSoundIfAllowed(message: ChatMessage): void {
        const data = gobConfig.get("behaviour.mentions.data.base")
        if (!data.playSound || data.volume <= 0 || !data.soundPath)
            return

        if (!message.containsMentions)
            return

        if (message.source.visibility === 0) {
            const ignoreDistance = gobConfig.get("behaviour.fadeout.mention", false)
            if (!ignoreDistance)
                return
        }

        const time = new Date()
        if (time.valueOf() - this.#lastSoundPlayed.valueOf() < data.soundInterval)
            return

        this.#lastSoundPlayed = time
        const audio = new Audio(data.soundPath)
        audio.volume = data.volume
        audio.play()
    }

    playSound(): void {
        const data = gobConfig.get("behaviour.mentions.data.base")
        if (!data.playSound || data.volume <= 0 || !data.soundPath)
            return

        this.#lastSoundPlayed = new Date()
        const audio = new Audio(data.soundPath)
        audio.volume = data.volume
        audio.play()
    }
}