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
var _ChatControl_instances, _ChatControl_cmdManager, _ChatControl_msgBuilder, _ChatControl_audioPlayer, _ChatControl_tabControl, _ChatControl_chatBox, _ChatControl_hideInfo, _ChatControl_hideError, _ChatControl_onNewMessageEvent, _ChatControl_onNewMessage, _AudioPlayer_lastSoundPlayed, _TabBarControl_instances, _TabBarControl_tabbar, _TabBarControl_databinding, _TabBarControl_onScrollBtnClick, _TabBarControl_onWheelScroll, _TabBarControl_onTabClick, _TabBarControl_activateTab, _TabBarControl_activeTabId_get, _TabBarControl_updateTabs, _TabBarControl_sortTabs, _TabBarControl_scrollTabs, _ScrollControl_control, _ScrollControl_scrollToBottom, _ScrollControl_onScroll;
import * as Cmd from './Command.js';
import * as Constants from './Constants.js';
import * as Databinding from './Databinding.js';
import * as Utility from './CommonUtility.js';
//#endregion
export class ChatControl {
    constructor() {
        _ChatControl_instances.add(this);
        _ChatControl_cmdManager.set(this, void 0);
        _ChatControl_msgBuilder.set(this, void 0);
        _ChatControl_audioPlayer.set(this, void 0);
        _ChatControl_tabControl.set(this, void 0);
        _ChatControl_chatBox.set(this, null);
        _ChatControl_hideInfo.set(this, false);
        _ChatControl_hideError.set(this, false);
        _ChatControl_onNewMessageEvent.set(this, (event) => {
            if (!!event.detail.messages) {
                for (let message of event.detail.messages) {
                    __classPrivateFieldGet(this, _ChatControl_instances, "m", _ChatControl_onNewMessage).call(this, message);
                }
            }
        });
        __classPrivateFieldSet(this, _ChatControl_cmdManager, new Cmd.CommandManager(), "f");
        __classPrivateFieldSet(this, _ChatControl_audioPlayer, new AudioPlayer(), "f");
        __classPrivateFieldSet(this, _ChatControl_msgBuilder, new MessageBuilder(), "f");
        __classPrivateFieldSet(this, _ChatControl_tabControl, new TabBarControl(), "f");
    }
    destructor() {
        this.control(null);
    }
    showGobInfo(on) {
        __classPrivateFieldSet(this, _ChatControl_hideError, on, "f");
    }
    showGobError(on) {
        __classPrivateFieldSet(this, _ChatControl_hideInfo, on, "f");
    }
    control(chatBox) {
        if (__classPrivateFieldGet(this, _ChatControl_chatBox, "f")) {
            document.removeEventListener("ChatMessagesEvent", __classPrivateFieldGet(this, _ChatControl_onNewMessageEvent, "f"));
        }
        __classPrivateFieldSet(this, _ChatControl_chatBox, $(chatBox), "f");
        __classPrivateFieldGet(this, _ChatControl_tabControl, "f").control(__classPrivateFieldGet(this, _ChatControl_chatBox, "f").find(ChatControl.selector_tabbar));
        if (__classPrivateFieldGet(this, _ChatControl_chatBox, "f").length > 0)
            document.addEventListener("ChatMessagesEvent", __classPrivateFieldGet(this, _ChatControl_onNewMessageEvent, "f"));
    }
}
_ChatControl_cmdManager = new WeakMap(), _ChatControl_msgBuilder = new WeakMap(), _ChatControl_audioPlayer = new WeakMap(), _ChatControl_tabControl = new WeakMap(), _ChatControl_chatBox = new WeakMap(), _ChatControl_hideInfo = new WeakMap(), _ChatControl_hideError = new WeakMap(), _ChatControl_onNewMessageEvent = new WeakMap(), _ChatControl_instances = new WeakSet(), _ChatControl_onNewMessage = function _ChatControl_onNewMessage(message) {
    if (__classPrivateFieldGet(this, _ChatControl_hideInfo, "f") && message.channel === Gobchat.ChannelEnum.GOBCHATINFO)
        return;
    if (__classPrivateFieldGet(this, _ChatControl_hideError, "f") && message.channel === Gobchat.ChannelEnum.GOBCHATERROR)
        return;
    if (message.channel === Gobchat.ChannelEnum.ECHO) {
        const joinedMessageContent = message.content.map(e => e.text).join();
        __classPrivateFieldGet(this, _ChatControl_cmdManager, "f").processCommand(joinedMessageContent);
    }
    __classPrivateFieldGet(this, _ChatControl_audioPlayer, "f").playSoundIfAllowed(message);
    const messageAsHtml = __classPrivateFieldGet(this, _ChatControl_msgBuilder, "f").build(message);
    __classPrivateFieldGet(this, _ChatControl_chatBox, "f").find(ChatControl.selector_chat_history).append(messageAsHtml);
    //TODO
};
ChatControl.selector_chat_history = ".gob-chat_history";
ChatControl.selector_tabbar = ".gob-chat_tabbar";
class MessageBuilder {
    build(message) {
        const $body = $("<div></div>")
            .addClass("gob-chat-entry")
            .addClass(MessageBuilder.getMessageChannelCssClass(message))
            .addClass(MessageBuilder.getMessageTriggerGroupCssClass(message))
            .addClass(MessageBuilder.getMessageVisibilityCssClass(message));
        $("<span></span>")
            .addClass("gob-chat-entry__time")
            .text(`[${MessageBuilder.getMessageTimestamp(message)}] `)
            .appendTo($body);
        const $content = $("<span></span>")
            .addClass("gob-chat-entry__text")
            .appendTo($body);
        const sender = MessageBuilder.getSender(message);
        if (sender !== null) {
            $("<span></span>")
                .addClass("gob-chat-entry__sender")
                .text(`${sender} `)
                .appendTo($content);
        }
        for (let messageSegment of message.content) {
            $("<span></span>")
                .addClass("gob-chat_entry__text__segment")
                .addClass(MessageBuilder.getMessageSegmentClass(messageSegment.type))
                .text(messageSegment.text)
                .appendTo($content);
        }
        return $body[0];
    }
    static getMessageChannelCssClass(message) {
        const channelName = Constants.ChannelEnumToKey[message.channel];
        const data = Gobchat.Channels[channelName];
        return `gob-chat-entry--channel-${data.internalName}`;
    }
    static getMessageTriggerGroupCssClass(message) {
        if (message.source.triggerGroupId)
            return `gob-chat-entry--trigger-group-${message.source.triggerGroupId}`;
        return null;
    }
    static getMessageVisibilityCssClass(message) {
        if (!message.source)
            return null;
        const visibility = message.source.visibility;
        if (visibility >= 100)
            return null;
        const ignoreDistance = gobConfig.get("behaviour.rangefilter.ignoreMention", false);
        if (ignoreDistance && message.containsMentions)
            return null;
        const fadeOutStepSize = (100 / Constants.RangeFilterFadeOutLevels);
        const visibilityLevel = ((visibility + fadeOutStepSize - 1) / fadeOutStepSize) >> 0; //truncat decimals, makes the LSV an integer
        return `gob-chat-entry--fadeout-${visibilityLevel}`;
    }
    static getMessageSegmentClass(segmentType) {
        switch (segmentType) {
            case Gobchat.MessageSegmentEnum.SAY: return "gob-chat-entry__text__segment--say";
            case Gobchat.MessageSegmentEnum.EMOTE: return "gob-chat-entry__text__segment--emote";
            case Gobchat.MessageSegmentEnum.OOC: return "gob-chat-entry__text__segment--ooc";
            case Gobchat.MessageSegmentEnum.MENTION: return "gob-chat-entry__text__segment--mention";
            case Gobchat.MessageSegmentEnum.WEBLINK: return "gob-chat-entry__text__segment--link";
            default: return null;
        }
    }
    static getMessageTimestamp(message) {
        function twoDigits(v) {
            return v < 10 ? '0' + v : v.toString();
        }
        const asDate = new Date(message.timestamp);
        const hours = twoDigits(asDate.getHours());
        const minutes = twoDigits(asDate.getMinutes());
        return `${hours}:${minutes}`;
    }
    static getSender(message) {
        const senderRaw = MessageBuilder.getSenderFromSource(message.source);
        return MessageBuilder.formatSenderAccordingToChannel(senderRaw, message.channel);
    }
    static getSenderFromSource(messageSource) {
        if (messageSource === null || messageSource.original === null)
            return null;
        if (messageSource.characterName !== null && messageSource.characterName !== undefined) {
            let prefix = "";
            if (messageSource.party >= 0)
                prefix += `[${messageSource.party + 1}]`;
            if (messageSource.alliance >= 0)
                prefix += `[${String.fromCharCode('A'.charCodeAt(0) + messageSource.alliance)}]`;
            if (messageSource.ffGroup >= 0)
                prefix += Constants.FFGroupUnicodes[messageSource.ffGroup].char;
            return `${prefix}${messageSource.characterName}`;
        }
        return messageSource.original;
    }
    static formatSenderAccordingToChannel(sender, channel) {
        switch (channel) {
            case Gobchat.ChannelEnum.GOBCHATINFO:
            case Gobchat.ChannelEnum.GOBCHATERROR: return `[${sender}]`;
            case Gobchat.ChannelEnum.ECHO: return "Echo:";
            case Gobchat.ChannelEnum.EMOTE: return sender;
            case Gobchat.ChannelEnum.TELLSEND: return `>> ${sender}:`;
            case Gobchat.ChannelEnum.TELLRECIEVE: return `${sender} >>`;
            case Gobchat.ChannelEnum.ERROR: return null;
            case Gobchat.ChannelEnum.ANIMATEDEMOTE: return null; //source is set, but the animation message already contains the source name
            case Gobchat.ChannelEnum.PARTY: return `(${sender})`;
            case Gobchat.ChannelEnum.ALLIANCE: return `<${sender}>`;
            case Gobchat.ChannelEnum.GUILD:
            case Gobchat.ChannelEnum.LINKSHELL_1:
            case Gobchat.ChannelEnum.LINKSHELL_2:
            case Gobchat.ChannelEnum.LINKSHELL_3:
            case Gobchat.ChannelEnum.LINKSHELL_4:
            case Gobchat.ChannelEnum.LINKSHELL_5:
            case Gobchat.ChannelEnum.LINKSHELL_6:
            case Gobchat.ChannelEnum.LINKSHELL_7:
            case Gobchat.ChannelEnum.LINKSHELL_8:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_1:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_2:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_3:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_4:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_5:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_6:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_7:
            case Gobchat.ChannelEnum.CROSSWORLDLINKSHELL_8:
                return `[${MessageBuilder.getChannelAbbreviation(channel)}]<${sender}>`;
            default:
                if (sender !== null && sender !== undefined)
                    return sender + ":";
                return null;
        }
    }
    static getChannelAbbreviation(channel) {
        if (channel in MessageBuilder.AbbreviationCache)
            return MessageBuilder.AbbreviationCache[channel];
        return channel?.toString() ?? '';
    }
}
MessageBuilder.AbbreviationCache = [];
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
/**
 *  Controls the content of a Tabbar, scrolling and animation.
 *  The controlled tabbar element will fire a 'change' event on any button press
 */
class TabBarControl {
    constructor() {
        _TabBarControl_instances.add(this);
        _TabBarControl_tabbar.set(this, null);
        _TabBarControl_databinding.set(this, null);
        _TabBarControl_onScrollBtnClick.set(this, (event) => {
            const scrollDirection = $(event.target).hasClass(TabBarControl.cssScrollRightButton) ? 1 : -1;
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollTabs).call(this, scrollDirection);
        });
        _TabBarControl_onWheelScroll.set(this, (event) => {
            const scrollDirection = event.originalEvent.deltaY > 0 ? 1 : -1;
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollTabs).call(this, scrollDirection);
        });
        _TabBarControl_onTabClick.set(this, (event) => {
            const id = $(event.target).attr(TabBarControl.attributeTabId);
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_activateTab).call(this, id);
        });
    }
    control(tabbar) {
        if (__classPrivateFieldGet(this, _TabBarControl_tabbar, "f")) {
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").off("wheel", __classPrivateFieldGet(this, _TabBarControl_onWheelScroll, "f"));
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollLeftBtn).off("click", __classPrivateFieldGet(this, _TabBarControl_onScrollBtnClick, "f"));
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollRightBtn).off("click", __classPrivateFieldGet(this, _TabBarControl_onScrollBtnClick, "f"));
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content_allTabs).off("click", __classPrivateFieldGet(this, _TabBarControl_onTabClick, "f"));
            __classPrivateFieldGet(this, _TabBarControl_databinding, "f")?.clear();
        }
        __classPrivateFieldSet(this, _TabBarControl_tabbar, $(tabbar), "f");
        if (!__classPrivateFieldGet(this, _TabBarControl_tabbar, "f").hasClass(TabBarControl.cssTabbar))
            throw new Error("Tabbar not found");
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").on("wheel", __classPrivateFieldGet(this, _TabBarControl_onWheelScroll, "f"));
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollLeftBtn).on("click", __classPrivateFieldGet(this, _TabBarControl_onScrollBtnClick, "f"));
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollRightBtn).on("click", __classPrivateFieldGet(this, _TabBarControl_onScrollBtnClick, "f"));
        __classPrivateFieldSet(this, _TabBarControl_databinding, new Databinding.BindingContext(gobConfig), "f");
        Databinding.bindListener(__classPrivateFieldGet(this, _TabBarControl_databinding, "f"), "behaviour.chattabs", config => __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_updateTabs).call(this, config));
        Databinding.bindListener(__classPrivateFieldGet(this, _TabBarControl_databinding, "f"), "behaviour.chattabs.data", () => { });
        Databinding.bindListener(__classPrivateFieldGet(this, _TabBarControl_databinding, "f"), "behaviour.chattabs.effect", () => { });
        __classPrivateFieldGet(this, _TabBarControl_databinding, "f").initialize();
    }
}
_TabBarControl_tabbar = new WeakMap(), _TabBarControl_databinding = new WeakMap(), _TabBarControl_onScrollBtnClick = new WeakMap(), _TabBarControl_onWheelScroll = new WeakMap(), _TabBarControl_onTabClick = new WeakMap(), _TabBarControl_instances = new WeakSet(), _TabBarControl_activateTab = function _TabBarControl_activateTab(idOrIndex) {
    idOrIndex ??= 0;
    __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content_activeTab).removeClass(TabBarControl.cssActiveTabButton);
    if (Utility.isNumber(idOrIndex)) {
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content_allTabs).eq(idOrIndex).addClass(TabBarControl.cssActiveTabButton);
    }
    else {
        const selector = Utility.formatString(TabBarControl.selector_content_tabWithid, idOrIndex);
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(selector).addClass(TabBarControl.cssActiveTabButton);
    }
}, _TabBarControl_activeTabId_get = function _TabBarControl_activeTabId_get() {
    return __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content_activeTab).attr(TabBarControl.attributeTabId);
}, _TabBarControl_updateTabs = function _TabBarControl_updateTabs(config) {
    const configData = config["data"];
    const configSorting = config["sorting"];
    const newTabs = configSorting
        .filter(id => configData[id].visible)
        .map(id => { return { id: id, name: configData[id].name }; });
    const $content = __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content);
    const $oldTabs = $content.children();
    const oldTabIds = [];
    for (let tab of $oldTabs) {
        const id = $(tab).attr(TabBarControl.attributeTabId);
        oldTabIds.push(id);
    }
    const newTabIds = newTabs.map(e => e.id);
    for (let entry of newTabs) {
        if (_.includes(oldTabIds, entry.id)) {
            $oldTabs.filter(`[${TabBarControl.attributeTabId}=${entry.id}]`).text(entry.name);
        }
        else {
            $("<button></button>")
                .addClass(TabBarControl.cssTabButton)
                .attr(TabBarControl.attributeTabId, entry.id)
                .on("click", __classPrivateFieldGet(this, _TabBarControl_onTabClick, "f"))
                .text(entry.name)
                .appendTo($content);
        }
    }
    const idsToRemove = oldTabIds.filter(id => !_.includes(newTabIds, id));
    for (let id of idsToRemove)
        $oldTabs.filter(`[${TabBarControl.attributeTabId}=${id}]`).remove();
    __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_sortTabs).call(this, configSorting);
    if (!__classPrivateFieldGet(this, _TabBarControl_instances, "a", _TabBarControl_activeTabId_get))
        __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_activateTab).call(this, 0);
    __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollTabs).call(this, 0); //update the scroll view
}, _TabBarControl_sortTabs = function _TabBarControl_sortTabs(order) {
    const $content = __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content);
    const $tabs = $content.children().detach();
    const lookup = {};
    $tabs.each(function () {
        const id = $(this).attr(TabBarControl.attributeTabId);
        lookup[id] = $(this);
    });
    for (let id of order) {
        const $tab = lookup[id];
        if ($tab) {
            $tab.appendTo($content);
            delete lookup[id];
        }
    }
    //Add unsorted elements to the end
    Object.entries(lookup).forEach(e => $content.append(e[1]));
}, _TabBarControl_scrollTabs = function _TabBarControl_scrollTabs(direction) {
    const $content = __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content);
    const numberOfChilds = $content.children().length;
    //not perfect, it would be nice to be able to scroll to the 'next' element or to scroll an element into view
    const scrollDistance = Math.max(10, $content.width() / numberOfChilds);
    const newPosition = $content.scrollLeft() + direction * scrollDistance;
    $content.animate({
        scrollLeft: newPosition
    }, 50);
    const isAtLeftBorder = newPosition <= 0;
    __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollLeftBtn)
        .toggleClass(TabBarControl.cssDisableScrollButton, isAtLeftBorder);
    const scrollWidth = Utility.toNumber($content.prop("scrollWidth"), 0);
    const clientWidth = Utility.toNumber($content.prop("clientWidth"), 0);
    const isAtRightBorder = (scrollWidth - clientWidth) <= newPosition;
    __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollRightBtn)
        .toggleClass(TabBarControl.cssDisableScrollButton, isAtRightBorder);
};
TabBarControl.attributeTabId = "data-gob-tab-id";
TabBarControl.cssDisableScrollButton = "is-disabled";
TabBarControl.cssActiveTabButton = "is-active";
TabBarControl.cssTabbar = "gob-chat_tabbar";
TabBarControl.cssScrollLeftButton = "gob-chat_tabbar_button--left";
TabBarControl.cssScrollRightButton = "gob-chat_tabbar_button--right";
TabBarControl.cssTabBarContent = "gob-chat_tabbar_content";
TabBarControl.cssTabButton = "gob-chat_tabbar_content_tab";
TabBarControl.selector_scrollLeftBtn = `> .${TabBarControl.cssScrollLeftButton}`;
TabBarControl.selector_scrollRightBtn = `> .${TabBarControl.cssScrollRightButton}`;
TabBarControl.selector_content = `> .${TabBarControl.cssTabBarContent}`;
TabBarControl.selector_content_activeTab = `> .${TabBarControl.cssTabBarContent} > .${TabBarControl.cssTabButton}.${TabBarControl.cssActiveTabButton}`;
TabBarControl.selector_content_tabWithid = `> .${TabBarControl.cssTabBarContent} > .${TabBarControl.cssTabButton}[${TabBarControl.attributeTabId}={0}]`;
TabBarControl.selector_content_allTabs = `> .${TabBarControl.cssTabBarContent} > .${TabBarControl.cssTabButton}`;
class TabControl {
    control() {
        if (false) {
        }
    }
}
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
