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

//requieres Gobchat.MessageParser
//requieres Gobchat.MessageHtmlBuilder

var Gobchat = (function (Gobchat) {
    class GobchatManager {
        constructor(chatHtmlId) {
            this._chatHtmlId = chatHtmlId
        }

        async init() {
            const self = this

            this._chatConfig = new Gobchat.GobchatConfig(true)

            await this.config.loadConfig()

            this._chatConfig.addProfileEventListener(event => {
                if (event.type === "active") this.updateStyle()
            })
            this._chatConfig.addPropertyEventListener("style", event => {
                if (event.isActive) this.updateStyle()
            })
            this._chatConfig.addPropertyEventListener("behaviour", event => {
                if (event.isActive) this.updateStyle()
            })

            this.updateStyle()

            this._messageHtmlBuilder = new Gobchat.MessageHtmlBuilder(self._chatConfig)
            this._messageSound = new Gobchat.MessageSoundPlayer(self._chatConfig)

            this._cmdManager = new Gobchat.CommandManager(this, self._chatConfig)

            this._scrollbar = new ScrollbarControl(this._chatHtmlId)
            this._scrollbar.init()

            document.addEventListener("ChatMessagesEvent", (e) => { self.onNewMessageEvent(e) })
        }

        get config() {
            return this._chatConfig
        }

        //TODO test
        updateStyle() {
            Gobchat.StyleBuilder.updateStyle(this.config, "custome_style_id")
        }

        scrollToBottomIfNeeded() {
            this._scrollbar.scrollToBottomIfNeeded()
        }

        onNewMessageEvent(messagesEvent) {
            const messageCollection = messagesEvent.detail
            if (!messageCollection) return
            for (let message of messageCollection.messages) {
                this.onNewMessage(message)
            }

            /*
            const message = this._messageParser.parseMessageEvent(messageEvent)
            if (!message) return
            this.onNewMessage(message)
            if (message.channel === Gobchat.ChannelEnum.ECHO) {
                this._cmdManager.processCommand(messageEvent.detail.message)
            }
            */
        }

        onNewMessage(message) {
            const messageHtmlElement = this._messageHtmlBuilder.buildHtmlElement(message)
            $("#" + this._chatHtmlId).append(messageHtmlElement)
            this._scrollbar.scrollToBottomIfNeeded()
            this._messageSound.checkForSound(message)

            if (message.channel === Gobchat.ChannelEnum.ECHO) {
                const msgComplete = message.content.map(e => e.text).join()
                this._cmdManager.processCommand(msgComplete)
            }
        }
    }
    Gobchat.GobchatManager = GobchatManager

    class ScrollbarControl {
        constructor(scrollTargetId) {
            this._scrollTargetId = "#" + scrollTargetId
            this._bScrollToBottom = true
        }

        init() {
            const control = this
            const scrollTarget = this._scrollTargetId
            $(scrollTarget).on('scroll', function () {
                let closeToBottom = ($(this).scrollTop() + $(this).innerHeight() + 5 >= $(this)[0].scrollHeight) // +5px for 'being very close'
                control._bScrollToBottom = closeToBottom
            })

            /*$(scrollTarget).on("wheel",function(event){
                const scrollData = event.originalEvent.deltaY
                const scrollElement = $(scrollTarget)[0]

                const viewPortSize = scrollElement.clientHeight
                const scrollableSpace = scrollElement.scrollHeight

                if( scrollableSpace <= viewPortSize )
                    return

                const scrollDistance = 0 //px

                if(scrollData < 0){
                    scrollElement.scrollTop = Math.max(0,scrollElement.scrollTop - scrollDistance)
                }else{
                    scrollElement.scrollTop = Math.min(scrollableSpace-viewPortSize, scrollElement.scrollTop + scrollDistance)
                }

                //console.log($(scrollTarget)[0].clientHeight)
                //console.log($(scrollTarget)[0].scrollHeight)

                //console.log($(scrollTarget)[0].scrollTop)
            })*/

            /*$(scrollTarget).on("mouseover",function(event){
                $(scrollTarget)[0].focus()
            })*/
        }

        get isScrollingNeeded() {
            return this._bScrollToBottom
        }

        scrollToBottomIfNeeded() {
            if (this._bScrollToBottom) {
                const scrollTarget = this._scrollTargetId
                $(scrollTarget).animate({
                    scrollTop: $(scrollTarget)[0].scrollHeight - $(scrollTarget)[0].clientHeight
                }, 10);

                /*
                const target = $(this._scrollTargetId).first()
                target.animate({
                scrollTop: target.prop("scrollHeight") - target.prop("clientHeight")
                }, 10)
                */
            }
        }
    }

    return Gobchat
}(Gobchat || {}));