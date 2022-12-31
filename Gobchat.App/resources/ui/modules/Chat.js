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
var _ChatControl_instances, _ChatControl_cmdManager, _ChatControl_tabControl, _ChatControl_searchControl, _ChatControl_databinding, _ChatControl_chatBox, _ChatControl_chatHistory, _ChatControl_hideInfo, _ChatControl_hideError, _ChatControl_onNewMessageEvent, _ChatControl_onNewMessage, _TabBarControl_instances, _TabBarControl_databinding, _TabBarControl_channelToTab, _TabBarControl_navPanelData, _TabBarControl_cssClassForMentionTabEffect, _TabBarControl_cssClassForNewMessageTabEffect, _TabBarControl_isPanelScrolledToBottom, _TabBarControl_tabbar, _TabBarControl_navPanel, _TabBarControl_onNavBarBtnScroll, _TabBarControl_onNavBarWheelScroll, _TabBarControl_onPanelScroll, _TabBarControl_onTabClick, _TabBarControl_buildChannelToTabMapping, _TabBarControl_getTab, _TabBarControl_activateTab, _TabBarControl_activeTabId_get, _TabBarControl_updateTabs, _TabBarControl_scrollTabs, _TabBarControl_scrollPanelToBottom, _TabBarControl_scrollPanelToPosition, _TabBarControl_panelScrollPosition_get, _a, _ChatSearchControl_searchElement, _ChatSearchControl_chatHistory, _ChatSearchControl_onInputEnter, _ChatSearchControl_updateCounterValue, _ChatSearchControl_removeMessageMarkers;
import * as Cmd from './Command.js';
import * as Constants from './Constants.js';
import * as Databinding from './Databinding.js';
import * as Utility from './CommonUtility.js';
//#endregion
export var CssClass;
(function (CssClass) {
    CssClass.Chat = "gob-chat_box";
    CssClass.Chat_Toolbar = "gob-chat_toolbar";
    CssClass.Chat_Tabs = "gob-chat_tabbar";
    CssClass.Chat_Tabs_Content = "gob-chat_tabbar_content";
    CssClass.Chat_Tabs_Content_Tab = "gob-chat_tabbar_content_tab";
    CssClass.Chat_Tabs_Content_Tab_Mention_Partial = "gob-chat_tabbar_content_tab--mention-{0}";
    CssClass.Chat_Tabs_Content_Tab_NewMessage_Partial = "gob-chat_tabbar_content_tab--new-message-{0}";
    CssClass.Chat_Search = "gob-chat_toolbar_search";
    CssClass.Chat_History = "gob-chat_history";
    CssClass.Chat_History_Tab_Partial = "gob-chat_history--tab-{0}";
    CssClass.ChatEntry = "gob-chat-entry";
    CssClass.ChatEntry_MarkedbySearch = "gob-chat_entry--marked-by-search";
    CssClass.ChatEntry_SelectedBySearch = "gob-chat_entry--selected-by-search";
    CssClass.ChatEntry_Sender = "gob-chat-entry__sender";
    CssClass.ChatEntry_Time = "gob-chat-entry__time";
    CssClass.ChatEntry_Text = "gob-chat-entry__text";
    CssClass.ChatEntry_FadeOut_Partial = "gob-chat-entry--fadeout-{0}";
    CssClass.ChatEntry_Channel_Partial = "gob-chat-entry--channel-{0}";
    CssClass.ChatEntry_TriggerGroup_Partial = "gob-chat-entry--trigger-group-{0}";
    CssClass.ChatEntry_Segment = "gob-chat_entry__text__segment";
    CssClass.ChatEntry_Segment_Say = "gob-chat-entry__text__segment--say";
    CssClass.ChatEntry_Segment_Emote = "gob-chat-entry__text__segment--emote";
    CssClass.ChatEntry_Segment_Ooc = "gob-chat-entry__text__segment--ooc";
    CssClass.ChatEntry_Segment_Mention = "gob-chat-entry__text__segment--mention";
    CssClass.ChatEntry_Segment_Link = "gob-chat-entry__text__segment--link";
})(CssClass || (CssClass = {}));
export class ChatControl {
    constructor() {
        _ChatControl_instances.add(this);
        _ChatControl_cmdManager.set(this, void 0);
        _ChatControl_tabControl.set(this, void 0);
        _ChatControl_searchControl.set(this, void 0);
        _ChatControl_databinding.set(this, null);
        _ChatControl_chatBox.set(this, $());
        _ChatControl_chatHistory.set(this, $());
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
        __classPrivateFieldSet(this, _ChatControl_tabControl, new TabBarControl(), "f");
        __classPrivateFieldSet(this, _ChatControl_searchControl, new ChatSearchControl(), "f");
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
    toggleSearch() {
        __classPrivateFieldGet(this, _ChatControl_searchControl, "f").toggle();
    }
    hideSearch() {
        __classPrivateFieldGet(this, _ChatControl_searchControl, "f").hide();
    }
    showSearch() {
        __classPrivateFieldGet(this, _ChatControl_searchControl, "f").show();
    }
    control(chatBox) {
        // unbind
        document.removeEventListener("ChatMessagesEvent", __classPrivateFieldGet(this, _ChatControl_onNewMessageEvent, "f"));
        __classPrivateFieldGet(this, _ChatControl_databinding, "f")?.clear();
        // rebind
        __classPrivateFieldSet(this, _ChatControl_chatBox, $(chatBox), "f");
        __classPrivateFieldSet(this, _ChatControl_chatHistory, __classPrivateFieldGet(this, _ChatControl_chatBox, "f").find(ChatControl.selector_chat_history), "f");
        __classPrivateFieldGet(this, _ChatControl_tabControl, "f").control(__classPrivateFieldGet(this, _ChatControl_chatBox, "f").find(ChatControl.selector_tabbar), __classPrivateFieldGet(this, _ChatControl_chatHistory, "f"));
        __classPrivateFieldGet(this, _ChatControl_searchControl, "f").control(__classPrivateFieldGet(this, _ChatControl_chatBox, "f").find(ChatControl.selector_search), __classPrivateFieldGet(this, _ChatControl_chatHistory, "f"));
        if (__classPrivateFieldGet(this, _ChatControl_chatBox, "f").length > 0)
            document.addEventListener("ChatMessagesEvent", __classPrivateFieldGet(this, _ChatControl_onNewMessageEvent, "f"));
        __classPrivateFieldSet(this, _ChatControl_databinding, new Databinding.BindingContext(gobConfig), "f");
        Databinding.bindListener(__classPrivateFieldGet(this, _ChatControl_databinding, "f"), "behaviour.language", async () => {
            const channels = Object.values(Gobchat.Channels);
            const requestTranslation = channels.map(data => data.abbreviationId)
                .filter(id => Utility.isString(id));
            const translations = await gobLocale.getAll(requestTranslation);
            const channelLookup = MessageBuilder.AbbreviationCache;
            channelLookup.length = 0;
            for (let data of channels) {
                if (data.abbreviationId)
                    channelLookup[data.chatChannel] = translations[data.abbreviationId];
            }
        });
        __classPrivateFieldGet(this, _ChatControl_databinding, "f").initialize();
    }
}
_ChatControl_cmdManager = new WeakMap(), _ChatControl_tabControl = new WeakMap(), _ChatControl_searchControl = new WeakMap(), _ChatControl_databinding = new WeakMap(), _ChatControl_chatBox = new WeakMap(), _ChatControl_chatHistory = new WeakMap(), _ChatControl_hideInfo = new WeakMap(), _ChatControl_hideError = new WeakMap(), _ChatControl_onNewMessageEvent = new WeakMap(), _ChatControl_instances = new WeakSet(), _ChatControl_onNewMessage = function _ChatControl_onNewMessage(message) {
    if (__classPrivateFieldGet(this, _ChatControl_hideInfo, "f") && message.channel === Gobchat.ChannelEnum.GOBCHATINFO)
        return;
    if (__classPrivateFieldGet(this, _ChatControl_hideError, "f") && message.channel === Gobchat.ChannelEnum.GOBCHATERROR)
        return;
    if (message.channel === Gobchat.ChannelEnum.ECHO) {
        const joinedMessageContent = message.content.map(e => e.text).join();
        __classPrivateFieldGet(this, _ChatControl_cmdManager, "f").processCommand(joinedMessageContent);
    }
    const messageAsHtml = MessageBuilder.build(message);
    __classPrivateFieldGet(this, _ChatControl_chatHistory, "f").append(messageAsHtml);
    __classPrivateFieldGet(this, _ChatControl_tabControl, "f").scrollToBottomIfNeeded();
    __classPrivateFieldGet(this, _ChatControl_tabControl, "f").applyNewMessageAnimationToTabs(message.channel, message.containsMentions);
    AudioPlayer.playMentionSoundIfPossible(message);
};
ChatControl.selector_chat_history = `.${CssClass.Chat_History}`;
ChatControl.selector_tabbar = `.${CssClass.Chat_Tabs}`;
ChatControl.selector_search = `.${CssClass.Chat_Search}`;
class MessageBuilder {
    static build(message) {
        const $body = $("<div></div>")
            .addClass(CssClass.ChatEntry)
            .addClass(MessageBuilder.getMessageChannelCssClass(message))
            .addClass(MessageBuilder.getMessageTriggerGroupCssClass(message))
            .addClass(MessageBuilder.getMessageVisibilityCssClass(message));
        $("<span></span>")
            .addClass(CssClass.ChatEntry_Time)
            .text(`[${MessageBuilder.getMessageTimestamp(message)}] `)
            .appendTo($body);
        const $content = $("<span></span>")
            .addClass(CssClass.ChatEntry_Text)
            .appendTo($body);
        const sender = MessageBuilder.getSender(message);
        if (sender !== null) {
            $("<span></span>")
                .addClass(CssClass.ChatEntry_Sender)
                .text(`${sender} `)
                .appendTo($content);
        }
        for (let messageSegment of message.content) {
            $("<span></span>")
                .addClass(CssClass.ChatEntry_Segment)
                .addClass(MessageBuilder.getMessageSegmentClass(messageSegment.type))
                .text(messageSegment.text)
                .appendTo($content);
        }
        return $body[0];
    }
    static getMessageChannelCssClass(message) {
        const channelName = Constants.ChannelEnumToKey[message.channel];
        const data = Gobchat.Channels[channelName];
        return Utility.formatString(CssClass.ChatEntry_Channel_Partial, data.internalName);
        // return Utility.formatString(CssClass.ChatEntry_Channel_Partial, message.channel.toString())
    }
    static getMessageTriggerGroupCssClass(message) {
        if (message.source.triggerGroupId)
            return Utility.formatString(CssClass.ChatEntry_TriggerGroup_Partial, message.source.triggerGroupId);
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
        const steps = gobConfig.get("behaviour.rangefilter.opacitysteps");
        const fadeOutStepSize = (100 / steps);
        const visibilityLevel = ((visibility + fadeOutStepSize - 1) / fadeOutStepSize) >> 0; //truncat decimals, makes the LSV an integer
        return Utility.formatString(CssClass.ChatEntry_FadeOut_Partial, visibilityLevel);
    }
    static getMessageSegmentClass(segmentType) {
        switch (segmentType) {
            case Gobchat.MessageSegmentEnum.SAY: return CssClass.ChatEntry_Segment_Say;
            case Gobchat.MessageSegmentEnum.EMOTE: return CssClass.ChatEntry_Segment_Emote;
            case Gobchat.MessageSegmentEnum.OOC: return CssClass.ChatEntry_Segment_Ooc;
            case Gobchat.MessageSegmentEnum.MENTION: return CssClass.ChatEntry_Segment_Mention;
            case Gobchat.MessageSegmentEnum.WEBLINK: return CssClass.ChatEntry_Segment_Link;
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
    }
    static playMentionSoundIfPossible(message) {
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
        if (time.valueOf() - AudioPlayer.lastSoundPlayed.valueOf() < data.soundInterval)
            return;
        AudioPlayer.lastSoundPlayed = time;
        const audio = new Audio(data.soundPath);
        audio.volume = data.volume;
        audio.play();
    }
    static playMentionSound() {
        const data = gobConfig.get("behaviour.mentions.data.base");
        if (!data.playSound || data.volume <= 0 || !data.soundPath)
            return;
        AudioPlayer.lastSoundPlayed = new Date();
        const audio = new Audio(data.soundPath);
        audio.volume = data.volume;
        audio.play();
    }
}
AudioPlayer.lastSoundPlayed = new Date();
/**
 *  Controls the content of a Tabbar, scrolling and animation.
 *  The controlled tabbar element will fire a 'change' event on any button press
 */
class TabBarControl {
    constructor() {
        _TabBarControl_instances.add(this);
        _TabBarControl_databinding.set(this, null);
        _TabBarControl_channelToTab.set(this, {});
        _TabBarControl_navPanelData.set(this, {});
        _TabBarControl_cssClassForMentionTabEffect.set(this, null);
        _TabBarControl_cssClassForNewMessageTabEffect.set(this, null);
        _TabBarControl_isPanelScrolledToBottom.set(this, false);
        _TabBarControl_tabbar.set(this, null);
        _TabBarControl_navPanel.set(this, null);
        _TabBarControl_onNavBarBtnScroll.set(this, (event) => {
            const scrollDirection = $(event.target).hasClass(TabBarControl.CssScrollRightButton) ? 1 : -1;
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollTabs).call(this, scrollDirection);
        });
        _TabBarControl_onNavBarWheelScroll.set(this, (event) => {
            const scrollDirection = event.originalEvent.deltaY > 0 ? 1 : -1;
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollTabs).call(this, scrollDirection);
        });
        _TabBarControl_onPanelScroll.set(this, (event) => {
            const $panel = $(event.target);
            const panelBottom = $panel.scrollTop() + $panel.innerHeight();
            const closeToBottm = panelBottom + TabBarControl.ScrollToleranceZone >= event.target.scrollHeight;
            __classPrivateFieldSet(this, _TabBarControl_isPanelScrolledToBottom, closeToBottm, "f");
        });
        _TabBarControl_onTabClick.set(this, (event) => {
            const id = $(event.target).attr(TabBarControl.AttributeTabId);
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_activateTab).call(this, id);
        });
    }
    control(navBar, navPanel) {
        if (__classPrivateFieldGet(this, _TabBarControl_tabbar, "f")) {
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").off("wheel", __classPrivateFieldGet(this, _TabBarControl_onNavBarWheelScroll, "f"));
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollLeftBtn).off("click", __classPrivateFieldGet(this, _TabBarControl_onNavBarBtnScroll, "f"));
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollRightBtn).off("click", __classPrivateFieldGet(this, _TabBarControl_onNavBarBtnScroll, "f"));
            __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_allTabs).off("click", __classPrivateFieldGet(this, _TabBarControl_onTabClick, "f"));
        }
        if (__classPrivateFieldGet(this, _TabBarControl_navPanel, "f")) {
            __classPrivateFieldGet(this, _TabBarControl_navPanel, "f").off("scroll", __classPrivateFieldGet(this, _TabBarControl_onPanelScroll, "f"));
        }
        __classPrivateFieldGet(this, _TabBarControl_databinding, "f")?.clear();
        __classPrivateFieldSet(this, _TabBarControl_tabbar, null, "f");
        __classPrivateFieldSet(this, _TabBarControl_navPanel, null, "f");
        const $navBar = $(navBar);
        if (!$navBar.hasClass(TabBarControl.CssNavBar))
            throw new Error("navBar not found");
        const $navPanel = $(navPanel);
        if (!$navPanel.hasClass(TabBarControl.CssNavPanel))
            throw new Error("navPanel not found");
        __classPrivateFieldSet(this, _TabBarControl_tabbar, $navBar, "f");
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").on("wheel", __classPrivateFieldGet(this, _TabBarControl_onNavBarWheelScroll, "f"));
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollLeftBtn).on("click", __classPrivateFieldGet(this, _TabBarControl_onNavBarBtnScroll, "f"));
        __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollRightBtn).on("click", __classPrivateFieldGet(this, _TabBarControl_onNavBarBtnScroll, "f"));
        __classPrivateFieldSet(this, _TabBarControl_navPanel, $navPanel, "f");
        __classPrivateFieldGet(this, _TabBarControl_navPanel, "f").on("scroll", __classPrivateFieldGet(this, _TabBarControl_onPanelScroll, "f"));
        __classPrivateFieldSet(this, _TabBarControl_isPanelScrolledToBottom, true, "f");
        __classPrivateFieldSet(this, _TabBarControl_databinding, new Databinding.BindingContext(gobConfig), "f");
        Databinding.bindListener(__classPrivateFieldGet(this, _TabBarControl_databinding, "f"), "behaviour.chattabs", config => __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_updateTabs).call(this, config));
        Databinding.bindListener(__classPrivateFieldGet(this, _TabBarControl_databinding, "f"), "behaviour.chattabs.data", config => __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_buildChannelToTabMapping).call(this, config));
        Databinding.bindListener(__classPrivateFieldGet(this, _TabBarControl_databinding, "f"), "behaviour.chattabs.effect", (effect) => {
            __classPrivateFieldSet(this, _TabBarControl_cssClassForMentionTabEffect, effect.mention > 0 ? Utility.formatString(TabBarControl.CssTabButtonMentionEffect, effect.mention) : null, "f");
            __classPrivateFieldSet(this, _TabBarControl_cssClassForNewMessageTabEffect, effect.message > 0 ? Utility.formatString(TabBarControl.CssTabButtonMessageEffect, effect.message) : null, "f");
        });
        __classPrivateFieldGet(this, _TabBarControl_databinding, "f").initialize();
    }
    applyNewMessageAnimationToTabs(channel, hasMention) {
        const affectedTabs = __classPrivateFieldGet(this, _TabBarControl_channelToTab, "f")[channel] || [];
        const activeTabId = __classPrivateFieldGet(this, _TabBarControl_instances, "a", _TabBarControl_activeTabId_get);
        if (_.includes(affectedTabs, activeTabId))
            return; // done, message was visible on active tab
        const cssClassForMentionEffect = __classPrivateFieldGet(this, _TabBarControl_cssClassForMentionTabEffect, "f");
        const cssClassForNewMessageEffect = __classPrivateFieldGet(this, _TabBarControl_cssClassForNewMessageTabEffect, "f");
        for (let tabId of affectedTabs) {
            if (tabId === activeTabId)
                continue; // do not apply any effects to the active tab
            const $tab = __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_getTab).call(this, tabId);
            if (hasMention && cssClassForMentionEffect) {
                $tab.removeClass(cssClassForNewMessageEffect)
                    .addClass(cssClassForMentionEffect)
                    .on("click.tab.effects.mention", function () {
                    $(this).off("click.tab.effects.mention")
                        .removeClass(cssClassForMentionEffect);
                });
                continue; //apply only one effect
            }
            if (cssClassForNewMessageEffect) {
                $tab.filter(`:not(.${cssClassForMentionEffect})`)
                    .addClass(cssClassForNewMessageEffect)
                    .on("click.tab.effects.message", function () {
                    $(this).off("click.tab.effects.message")
                        .removeClass(cssClassForNewMessageEffect);
                });
                continue; //apply only one effect
            }
        }
    }
    scrollToBottomIfNeeded(scrollFast = false) {
        if (__classPrivateFieldGet(this, _TabBarControl_isPanelScrolledToBottom, "f"))
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollPanelToBottom).call(this, scrollFast);
    }
}
_TabBarControl_databinding = new WeakMap(), _TabBarControl_channelToTab = new WeakMap(), _TabBarControl_navPanelData = new WeakMap(), _TabBarControl_cssClassForMentionTabEffect = new WeakMap(), _TabBarControl_cssClassForNewMessageTabEffect = new WeakMap(), _TabBarControl_isPanelScrolledToBottom = new WeakMap(), _TabBarControl_tabbar = new WeakMap(), _TabBarControl_navPanel = new WeakMap(), _TabBarControl_onNavBarBtnScroll = new WeakMap(), _TabBarControl_onNavBarWheelScroll = new WeakMap(), _TabBarControl_onPanelScroll = new WeakMap(), _TabBarControl_onTabClick = new WeakMap(), _TabBarControl_instances = new WeakSet(), _TabBarControl_buildChannelToTabMapping = function _TabBarControl_buildChannelToTabMapping(config) {
    __classPrivateFieldSet(this, _TabBarControl_channelToTab, {}, "f");
    for (let chatTab of Object.values(config)) {
        if (!chatTab.visible)
            return;
        for (let channel of chatTab.channel.visible) {
            if (channel in __classPrivateFieldGet(this, _TabBarControl_channelToTab, "f"))
                __classPrivateFieldGet(this, _TabBarControl_channelToTab, "f")[channel].push(chatTab.id);
            else
                __classPrivateFieldGet(this, _TabBarControl_channelToTab, "f")[channel] = [chatTab.id];
        }
    }
}, _TabBarControl_getTab = function _TabBarControl_getTab(idOrIndex) {
    idOrIndex ??= 0;
    if (Utility.isNumber(idOrIndex)) {
        const $childs = __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content).children();
        const $nextTab = $childs.eq(Math.max(0, Math.min($childs.length, idOrIndex)));
        return $nextTab;
    }
    else {
        const selector = Utility.formatString(TabBarControl.selector_tabWithId, idOrIndex);
        const $nextTab = __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(selector);
        return $nextTab;
    }
}, _TabBarControl_activateTab = function _TabBarControl_activateTab(idOrIndex) {
    const lastActiveTabId = __classPrivateFieldGet(this, _TabBarControl_instances, "a", _TabBarControl_activeTabId_get);
    if (lastActiveTabId in __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")) {
        __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")[lastActiveTabId].scrollPosition = __classPrivateFieldGet(this, _TabBarControl_isPanelScrolledToBottom, "f") ? -1 : __classPrivateFieldGet(this, _TabBarControl_instances, "a", _TabBarControl_panelScrollPosition_get);
    }
    // deactiate previous active tab
    __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_activeTab).removeClass(TabBarControl.CssActiveTabButton);
    // find new active tab
    const $tab = __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_getTab).call(this, idOrIndex);
    $tab.addClass(TabBarControl.CssActiveTabButton);
    if ($tab.length === 0) { //there is no tab with this id
        __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_activateTab).call(this, 0); //fallback
        return false;
    }
    const newActiveTabId = __classPrivateFieldGet(this, _TabBarControl_instances, "a", _TabBarControl_activeTabId_get);
    __classPrivateFieldGet(this, _TabBarControl_navPanel, "f") // used to filter messages depending on which tab is active
        .removeClass(Utility.formatString(TabBarControl.CssNavPanelActiveTab, lastActiveTabId))
        .addClass(Utility.formatString(TabBarControl.CssNavPanelActiveTab, newActiveTabId));
    // restore scroll position
    if (newActiveTabId in __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")) {
        if (__classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")[newActiveTabId].scrollPosition < 0)
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollPanelToBottom).call(this, true);
        else
            __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollPanelToPosition).call(this, __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")[newActiveTabId].scrollPosition, true);
    }
    return true;
}, _TabBarControl_activeTabId_get = function _TabBarControl_activeTabId_get() {
    return __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_activeTab).attr(TabBarControl.AttributeTabId);
}, _TabBarControl_updateTabs = function _TabBarControl_updateTabs(config) {
    const configData = config["data"];
    const configSorting = config["sorting"];
    const newTabsInOrder = configSorting
        .filter(id => configData[id].visible)
        .map(id => { return { id: id, name: configData[id].name }; });
    // remove old tabs and store them in a lookup table
    const $content = __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_content);
    const $oldTabs = $content.children().detach();
    const oldTabsLookup = {};
    for (let tab of $oldTabs) {
        const id = tab.getAttribute(TabBarControl.AttributeTabId);
        oldTabsLookup[id] = tab;
    }
    // add new tabs or reattach old tabs in order
    for (let entry of newTabsInOrder) {
        if (entry.id in oldTabsLookup) {
            $(oldTabsLookup[entry.id])
                .text(entry.name)
                .appendTo($content);
        }
        else {
            $("<button></button>")
                .addClass(TabBarControl.CssTabButton)
                .attr(TabBarControl.AttributeTabId, entry.id)
                .on("click", __classPrivateFieldGet(this, _TabBarControl_onTabClick, "f"))
                .text(entry.name)
                .appendTo($content);
        }
    }
    // remove old nav panel data
    for (let tabId of Object.keys(__classPrivateFieldGet(this, _TabBarControl_navPanelData, "f"))) {
        if (!_.includes(tabId, configSorting))
            delete __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")[tabId];
    }
    // add new nav panel data
    for (let tabId of configSorting) {
        if (!(tabId in __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f"))) {
            __classPrivateFieldGet(this, _TabBarControl_navPanelData, "f")[tabId] = {
                scrollPosition: -1
            };
        }
    }
    // const idsToRemove = oldTabIds.filter(id => !_.includes(newTabIds, id))
    // for (let id of idsToRemove)
    //     $oldTabs.filter(`[${TabBarControl.attributeTabId}=${id}]`).remove()
    if (!__classPrivateFieldGet(this, _TabBarControl_instances, "a", _TabBarControl_activeTabId_get))
        __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_activateTab).call(this, 0);
    __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollTabs).call(this, 0); //update the scroll view
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
        .toggleClass(TabBarControl.CssDisableScrollButton, isAtLeftBorder);
    const scrollWidth = Utility.toNumber($content.prop("scrollWidth"), 0);
    const clientWidth = Utility.toNumber($content.prop("clientWidth"), 0);
    const isAtRightBorder = (scrollWidth - clientWidth) <= newPosition;
    __classPrivateFieldGet(this, _TabBarControl_tabbar, "f").find(TabBarControl.selector_scrollRightBtn)
        .toggleClass(TabBarControl.CssDisableScrollButton, isAtRightBorder);
}, _TabBarControl_scrollPanelToBottom = function _TabBarControl_scrollPanelToBottom(scrollFast) {
    const navPanel = __classPrivateFieldGet(this, _TabBarControl_navPanel, "f")[0];
    const position = navPanel.scrollHeight - navPanel.clientHeight;
    __classPrivateFieldGet(this, _TabBarControl_instances, "m", _TabBarControl_scrollPanelToPosition).call(this, position, scrollFast);
}, _TabBarControl_scrollPanelToPosition = function _TabBarControl_scrollPanelToPosition(position, scrollFast) {
    if (scrollFast) {
        __classPrivateFieldGet(this, _TabBarControl_navPanel, "f").scrollTop(position);
    }
    else {
        __classPrivateFieldGet(this, _TabBarControl_navPanel, "f").animate({
            scrollTop: position
        }, 10);
    }
}, _TabBarControl_panelScrollPosition_get = function _TabBarControl_panelScrollPosition_get() {
    return __classPrivateFieldGet(this, _TabBarControl_navPanel, "f").scrollTop();
};
TabBarControl.ScrollToleranceZone = 5;
TabBarControl.AttributeTabId = "data-gob-tab-id";
TabBarControl.CssNavBar = CssClass.Chat_Tabs;
TabBarControl.CssNavPanel = CssClass.Chat_History;
TabBarControl.CssNavPanelActiveTab = CssClass.Chat_History_Tab_Partial;
TabBarControl.CssDisableScrollButton = "is-disabled";
TabBarControl.CssActiveTabButton = "is-active";
TabBarControl.CssScrollLeftButton = "gob-chat_tabbar_button--left";
TabBarControl.CssScrollRightButton = "gob-chat_tabbar_button--right";
TabBarControl.CssTabBarContent = CssClass.Chat_Tabs_Content;
TabBarControl.CssTabButton = CssClass.Chat_Tabs_Content_Tab;
TabBarControl.CssTabButtonMentionEffect = "gob-chat_tabbar_content_tab--mention-{0}";
TabBarControl.CssTabButtonMessageEffect = "gob-chat_tabbar_content_tab--new-message-{0}";
TabBarControl.selector_tabbar = `> .${TabBarControl.CssNavBar}`;
TabBarControl.selector_scrollLeftBtn = `> .${TabBarControl.CssScrollLeftButton}`;
TabBarControl.selector_scrollRightBtn = `> .${TabBarControl.CssScrollRightButton}`;
TabBarControl.selector_content = `> .${TabBarControl.CssTabBarContent}`;
TabBarControl.selector_activeTab = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}.${TabBarControl.CssActiveTabButton}`;
TabBarControl.selector_tabWithId = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}[${TabBarControl.AttributeTabId}={0}]`;
TabBarControl.selector_allTabs = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}`;
class ChatSearchControl {
    constructor() {
        _ChatSearchControl_searchElement.set(this, $());
        _ChatSearchControl_chatHistory.set(this, $());
        _ChatSearchControl_onInputEnter.set(this, (event) => {
            if (event.keyCode === 13) // enter
                this.startSearch();
        });
        _ChatSearchControl_updateCounterValue.set(this, () => {
            const $markedMessages = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_markedBySearch);
            const $activeSelection = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_activeSelection);
            const max = $markedMessages.length;
            const current = max > 0 ? $markedMessages.index($activeSelection) : 0;
            __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_counter).text(`${max - current} / ${max}`);
        });
        _ChatSearchControl_removeMessageMarkers.set(this, () => {
            __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_markedBySearch).removeClass(ChatSearchControl.cssMarkedbySearch);
            __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_activeSelection).removeClass(ChatSearchControl.cssActiveSelection);
            __classPrivateFieldGet(this, _ChatSearchControl_updateCounterValue, "f").call(this);
        });
        this.hide = () => {
            this.visible = false;
        };
        this.show = () => {
            this.visible = true;
        };
        this.toggle = () => {
            this.visible = !this.visible;
        };
        this.clearSearch = () => {
            __classPrivateFieldGet(this, _ChatSearchControl_removeMessageMarkers, "f").call(this);
            __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_input).val("").focus();
        };
        this.scrollToSelection = () => {
            const $selectedElement = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_activeSelection);
            if ($selectedElement.length === 0)
                return;
            const selectedElement = $selectedElement[0];
            const scrollableFrame = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f")[0];
            const containerTop = scrollableFrame.scrollTop;
            const containerBot = scrollableFrame.clientHeight + containerTop;
            const elementTop = selectedElement.offsetTop - scrollableFrame.offsetTop;
            const elementBot = selectedElement.clientHeight + elementTop;
            const isVisible = containerTop <= elementTop && elementBot <= containerBot;
            if (isVisible)
                return;
            const position = elementTop;
            $(scrollableFrame).animate({
                scrollTop: position
            }, 100);
        };
        this.search = (text) => {
            __classPrivateFieldGet(this, _ChatSearchControl_removeMessageMarkers, "f").call(this);
            if (text === null || text === undefined)
                return;
            text = text.trim().toLowerCase();
            if (text.length === 0)
                return;
            __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_visible_messages).each(function () {
                if ($(this).text().toLowerCase().indexOf(text) >= 0)
                    $(this).addClass(ChatSearchControl.cssMarkedbySearch);
            });
            const $markedMessages = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_markedBySearch);
            if ($markedMessages.length === 0) {
                __classPrivateFieldGet(this, _ChatSearchControl_updateCounterValue, "f").call(this);
            }
            else {
                $markedMessages.last().addClass(ChatSearchControl.cssActiveSelection);
                __classPrivateFieldGet(this, _ChatSearchControl_updateCounterValue, "f").call(this);
                this.scrollToSelection();
            }
        };
        this.startSearch = () => {
            this.search(__classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_input).val());
        };
        this.moveSelectionUp = () => {
            const $activeSelection = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_activeSelection);
            $activeSelection.removeClass(ChatSearchControl.cssActiveSelection);
            let $prevMarked = $activeSelection.prevAll(ChatSearchControl.selector_markedBySearch).first();
            if ($prevMarked.length === 0) //wrap around
                $prevMarked = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_markedBySearch).last();
            $prevMarked.addClass(ChatSearchControl.cssActiveSelection);
            __classPrivateFieldGet(this, _ChatSearchControl_updateCounterValue, "f").call(this);
            this.scrollToSelection();
        };
        this.moveSelectionDown = () => {
            const $activeSelection = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_activeSelection);
            $activeSelection.removeClass(ChatSearchControl.cssActiveSelection);
            let $nextMarked = $activeSelection.nextAll(ChatSearchControl.selector_markedBySearch).first();
            if ($nextMarked.length === 0) //wrap around
                $nextMarked = __classPrivateFieldGet(this, _ChatSearchControl_chatHistory, "f").find(ChatSearchControl.selector_markedBySearch).first();
            $nextMarked.addClass(ChatSearchControl.cssActiveSelection);
            __classPrivateFieldGet(this, _ChatSearchControl_updateCounterValue, "f").call(this);
            this.scrollToSelection();
        };
    }
    control(searchElement, chatHistory) {
        // unbind
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_input).off("keyup", __classPrivateFieldGet(this, _ChatSearchControl_onInputEnter, "f"));
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_counter).off("click", this.scrollToSelection);
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_up).off("click", this.moveSelectionUp);
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_down).off("click", this.moveSelectionDown);
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_reset).off("click", this.clearSearch);
        __classPrivateFieldGet(this, _ChatSearchControl_removeMessageMarkers, "f").call(this);
        // rebind
        __classPrivateFieldSet(this, _ChatSearchControl_searchElement, $(searchElement).first(), "f");
        __classPrivateFieldSet(this, _ChatSearchControl_chatHistory, $(chatHistory).first(), "f");
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_input).on("keyup", __classPrivateFieldGet(this, _ChatSearchControl_onInputEnter, "f"));
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_counter).on("click", this.scrollToSelection);
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_up).on("click", this.moveSelectionUp);
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_down).on("click", this.moveSelectionDown);
        __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_reset).on("click", this.clearSearch);
    }
    get visible() {
        return __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").is(":visible");
    }
    set visible(value) {
        if (value) {
            __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").show();
            __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").find(ChatSearchControl.selector_input).focus();
        }
        else {
            __classPrivateFieldGet(this, _ChatSearchControl_searchElement, "f").hide();
            this.clearSearch();
        }
    }
}
_a = ChatSearchControl, _ChatSearchControl_searchElement = new WeakMap(), _ChatSearchControl_chatHistory = new WeakMap(), _ChatSearchControl_onInputEnter = new WeakMap(), _ChatSearchControl_updateCounterValue = new WeakMap(), _ChatSearchControl_removeMessageMarkers = new WeakMap();
ChatSearchControl.cssMarkedbySearch = CssClass.ChatEntry_MarkedbySearch;
ChatSearchControl.cssActiveSelection = CssClass.ChatEntry_SelectedBySearch;
ChatSearchControl.cssChatMessage = CssClass.ChatEntry;
ChatSearchControl.selector_input = "> .js-input";
ChatSearchControl.selector_counter = "> .js-counter";
ChatSearchControl.selector_up = "> .js-up";
ChatSearchControl.selector_down = "> .js-down";
ChatSearchControl.selector_reset = "> .js-reset";
ChatSearchControl.selector_markedBySearch = `.${_a.cssMarkedbySearch}`;
ChatSearchControl.selector_activeSelection = `.${_a.cssActiveSelection}`;
ChatSearchControl.selector_visible_messages = `.${_a.cssChatMessage}:visible`;
