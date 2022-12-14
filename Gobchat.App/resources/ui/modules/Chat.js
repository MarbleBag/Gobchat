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
var __classPrivateFieldSet = (this && this.__classPrivateFieldSet) || function (receiver, state, value, kind, f) {
    if (kind === "m") throw new TypeError("Private method is not writable");
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a setter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot write private member to an object whose class did not declare it");
    return (kind === "a" ? f.call(receiver, value) : f ? f.value = value : state.set(receiver, value)), value;
};
var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var _ChatManager_control, _ChatManager_cmdManager, _ChatManager_msgBuilder, _ChatManager_audioPlayer, _ChatManager_hideInfo, _ChatManager_hideError, _ChatManager_onNewMessageEvent, _ChatManager_onNewMessage, _ScrollControl_control, _ScrollControl_scrollToBottom, _ScrollControl_onScroll, _AudioPlayer_lastSoundPlayed;
import * as Cmd from './Command.js';
export class ChatManager {
    constructor() {
        _ChatManager_control.set(this, null);
        _ChatManager_cmdManager.set(this, void 0);
        _ChatManager_msgBuilder.set(this, void 0);
        _ChatManager_audioPlayer.set(this, void 0);
        _ChatManager_hideInfo.set(this, false);
        _ChatManager_hideError.set(this, false);
        _ChatManager_onNewMessageEvent.set(this, (event) => {
            if (!!event.detail.messages) {
                for (let message of event.detail.messages) {
                    __classPrivateFieldGet(this, _ChatManager_onNewMessage, "f").call(this, message);
                }
            }
        });
        _ChatManager_onNewMessage.set(this, (message) => {
            if (__classPrivateFieldGet(this, _ChatManager_hideInfo, "f") && message.channel === Gobchat.ChannelEnum.GOBCHATINFO)
                return;
            if (__classPrivateFieldGet(this, _ChatManager_hideError, "f") && message.channel === Gobchat.ChannelEnum.GOBCHATERROR)
                return;
            if (message.channel === Gobchat.ChannelEnum.ECHO) {
                const joinedMessageContent = message.content.map(e => e.text).join();
                __classPrivateFieldGet(this, _ChatManager_cmdManager, "f").processCommand(joinedMessageContent);
            }
            const messageAsHtml = __classPrivateFieldGet(this, _ChatManager_msgBuilder, "f").build(message);
            //TODO
            __classPrivateFieldGet(this, _ChatManager_audioPlayer, "f").playSoundIfAllowed(message);
        });
        __classPrivateFieldSet(this, _ChatManager_cmdManager, new Cmd.CommandManager(), "f");
        __classPrivateFieldSet(this, _ChatManager_audioPlayer, new AudioPlayer(), "f");
        __classPrivateFieldSet(this, _ChatManager_msgBuilder, new MessageBuilder(), "f");
        document.addEventListener("ChatMessagesEvent", __classPrivateFieldGet(this, _ChatManager_onNewMessageEvent, "f"));
    }
    showGobInfo(on) {
    }
    showGobError(on) {
    }
    test() {
        this.control($(".gob-chat_box"));
    }
    control(chatElement) {
        if (__classPrivateFieldGet(this, _ChatManager_control, "f") !== null) {
        }
        __classPrivateFieldSet(this, _ChatManager_control, $(chatElement), "f");
    }
}
_ChatManager_control = new WeakMap(), _ChatManager_cmdManager = new WeakMap(), _ChatManager_msgBuilder = new WeakMap(), _ChatManager_audioPlayer = new WeakMap(), _ChatManager_hideInfo = new WeakMap(), _ChatManager_hideError = new WeakMap(), _ChatManager_onNewMessageEvent = new WeakMap(), _ChatManager_onNewMessage = new WeakMap();
class ScrollControl {
    constructor() {
        _ScrollControl_control.set(this, null);
        _ScrollControl_scrollToBottom.set(this, void 0);
        _ScrollControl_onScroll.set(this, ($element) => {
            //const $this = $(this)
            //const closeToBottom = ($this.scrollTop() + $this.innerHeight() + 5 >= $this[0].scrollHeight) // +5px for 'being very close'
            // this.#scrollToBottom = closeToBottom
        });
    }
    control($element) {
        if (__classPrivateFieldGet(this, _ScrollControl_control, "f") !== null) {
            const x = $(document).on("scroll", __classPrivateFieldGet(this, _ScrollControl_onScroll, "f"));
        }
    }
}
_ScrollControl_control = new WeakMap(), _ScrollControl_scrollToBottom = new WeakMap(), _ScrollControl_onScroll = new WeakMap();
class MessageBuilder {
    build(message) {
        throw new Error('Method not implemented.');
    }
}
class AudioPlayer {
    constructor() {
        _AudioPlayer_lastSoundPlayed.set(this, void 0);
        __classPrivateFieldSet(this, _AudioPlayer_lastSoundPlayed, new Date(), "f");
    }
    playSoundIfAllowed(message) {
        const data = gobConfig.get("behaviour.mentions.data.base");
        if (!data.playSound || data.volume <= 0 || !data.soundPath)
            return;
        if (!message.containsMentions)
            return;
        if (message.source.visibility === 0) {
            const ignoreDistance = gobConfig.get("behaviour.fadeout.mention", false);
            if (!ignoreDistance)
                return;
        }
        const time = new Date();
        if (time.valueOf() - __classPrivateFieldGet(this, _AudioPlayer_lastSoundPlayed, "f").valueOf() < data.soundInterval)
            return;
        __classPrivateFieldSet(this, _AudioPlayer_lastSoundPlayed, time, "f");
        const audio = new Audio(data.soundPath);
        audio.volume = data.volume;
        audio.play();
    }
    playSound() {
        const data = gobConfig.get("behaviour.mentions.data.base");
        if (!data.playSound || data.volume <= 0 || !data.soundPath)
            return;
        __classPrivateFieldSet(this, _AudioPlayer_lastSoundPlayed, new Date(), "f");
        const audio = new Audio(data.soundPath);
        audio.volume = data.volume;
        audio.play();
    }
}
_AudioPlayer_lastSoundPlayed = new WeakMap();
