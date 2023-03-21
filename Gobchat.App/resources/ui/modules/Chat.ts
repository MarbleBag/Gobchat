/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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
import * as Constants from './Constants.js'
import * as Databinding from './Databinding.js'
import * as Utility from './CommonUtility.js'

//#region backend generated types

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
export type MessageSegmentEnum = number

// created by backend
export interface ChatMessage {
    source: ChatMessageSource
    timestamp: Date
    channel: ChatChannelEnum
    content: ChatMessageSegment[]
    containsMentions: boolean
}

// created by backend
export interface ChatMessageSource {
    original: string
    characterName: string
    triggerGroupId: string
    ffGroup: number
    party: number
    alliance: number
    visibility: number
    isAPlayer: boolean
    isUser: boolean
    isApp: boolean
}

// created by backend
export interface ChatMessageSegment {
    type: MessageSegmentEnum
    text: string
}

//#endregion

export module CssClass {
    export const Chat = "gob-chat"
    export const Chat_Toolbar = "gob-chat-toolbar"

    export const Chat_Tabs = "gob-chat-tabbar"
    export const Chat_Tabs_Content = "gob-chat-tabbar_content"
    export const Chat_Tabs_Content_Tab = "gob-chat-tabbar_content_tab"
    export const Chat_Tabs_Content_Tab_Mention_Partial = "gob-chat-tabbar_content_tab--mention-{0}"
    export const Chat_Tabs_Content_Tab_NewMessage_Partial = "gob-chat-tabbar_content_tab--new-message-{0}"

    export const Chat_Search = "gob-chat-toolbar_search"

    export const Chat_History = "gob-chat_history"
    export const Chat_History_Tab_Partial = "gob-chat_history--tab-{0}"

    export const ChatEntry = "gob-chat-entry"
    export const ChatEntry_MarkedbySearch = "gob-chat_entry--marked-by-search"
    export const ChatEntry_SelectedBySearch = "gob-chat_entry--selected-by-search"
    export const ChatEntry_Sender = "gob-chat-entry_sender"
    export const ChatEntry_Time = "gob-chat-entry_time"
    export const ChatEntry_Text = "gob-chat-entry_text"
    export const ChatEntry_FadeOut_Partial = "gob-chat-entry--fadeout-{0}"
    export const ChatEntry_Channel_Partial = "gob-chat-entry--channel-{0}"
    export const ChatEntry_TriggerGroup_Partial = "gob-chat-entry--trigger-group-{0}"
    export const ChatEntry_Segment = "gob-chat_entry_text_segment"
    export const ChatEntry_Segment_Say = "gob-chat-entry_text_segment--say"
    export const ChatEntry_Segment_Emote = "gob-chat-entry_text_segment--emote"
    export const ChatEntry_Segment_Ooc = "gob-chat-entry_text_segment--ooc"
    export const ChatEntry_Segment_Mention = "gob-chat-entry_text_segment--mention"
    export const ChatEntry_Segment_Link = "gob-chat-entry_text_segment--link"
}

export module HtmlAttribute {
    export const ChatEntry_Source = "data-source"
    export const ChatEntry_Friendgroup = "data-friendgroup"
    export const ChatEntry_TriggerId = "data-triggerid"
}

export class ChatControl {
    static readonly selector_chat_history = `.${CssClass.Chat_History}`
    static readonly selector_tabbar = `.${CssClass.Chat_Tabs}`
    static readonly selector_search = `.${CssClass.Chat_Search}`

    #cmdManager: Cmd.CommandManager
    #tabControl: TabBarControl
    #searchControl: ChatSearchControl
    #groupControl: ChatGroupControl

    #databinding: Databinding.BindingContext | null = null
    #chatBox: JQuery = $()
    #chatHistory: JQuery = $()

    #hideInfo: boolean = false
    #hideError: boolean = false

    constructor() {
        this.#cmdManager = new Cmd.CommandManager()
        this.#tabControl = new TabBarControl()
        this.#searchControl = new ChatSearchControl()
        this.#groupControl = new ChatGroupControl()
    }

    destructor() {
        this.control(null)
    }

    #onNewMessageEvent = (event: ChatMessagesEvent): void => { // bound to class instance
        if (!!event.detail.messages) {
            for (let message of event.detail.messages) {
                this.#onNewMessage(message)
            }
        }
    }

    #onNewMessage(message: ChatMessage): void {
        if (this.#hideInfo && message.channel === Gobchat.ChannelEnum.GOBCHATINFO)
            return

        if (this.#hideError && message.channel === Gobchat.ChannelEnum.GOBCHATERROR)
            return

        if (message.channel === Gobchat.ChannelEnum.ECHO) {
            const joinedMessageContent = message.content.map(e => e.text).join()
            this.#cmdManager.processCommand(joinedMessageContent)
        }

        const messageAsHtml = MessageBuilder.build(message)
        this.#chatHistory.append(messageAsHtml)

        this.#tabControl.scrollToBottomIfNeeded()
        this.#tabControl.applyNewMessageAnimationToTabs(message.channel, message.containsMentions)
        AudioPlayer.playMentionSoundIfPossible(message)
    }

    showGobInfo(on: boolean): void {
        this.#hideError = on
    }

    showGobError(on: boolean): void {
        this.#hideInfo = on
    }

    toggleSearch(): void {
        this.#searchControl.toggle()
    }

    hideSearch(): void {
        this.#searchControl.hide()
    }

    showSearch(): void {
        this.#searchControl.show()
    }

    control(chatBox: HTMLElement | JQuery | null): void {
        // unbind
        document.removeEventListener("ChatMessagesEvent", this.#onNewMessageEvent as EventListener)
        this.#databinding?.clearBindings()

        // rebind
        this.#chatBox = $(chatBox)
        this.#chatHistory = this.#chatBox.find(ChatControl.selector_chat_history)

        if (this.#chatBox.length === 0)
            throw new Error("No chat html element found")

        if (this.#chatHistory.length === 0)
            throw new Error("No chat history html element found")

        this.#tabControl.control(this.#chatBox.find(ChatControl.selector_tabbar), this.#chatHistory)
        this.#searchControl.control(this.#chatBox.find(ChatControl.selector_search), this.#chatHistory)
        this.#groupControl.control(this.#chatHistory)

        document.addEventListener("ChatMessagesEvent", this.#onNewMessageEvent as EventListener)

        this.#databinding = new Databinding.BindingContext(gobConfig)
        Databinding.bindListener(this.#databinding, "behaviour.language", async () => {
            const channels = Object.values(Gobchat.Channels)

            const requestTranslation = channels.map(data => data.abbreviationId)

            const translations = await gobLocale.getAll(requestTranslation)
            const channelLookup = MessageBuilder.AbbreviationCache
            channelLookup.length = 0

            for (const data of channels) {
                channelLookup[data.chatChannel] = translations[data.abbreviationId]
            }
        })

        this.#databinding.loadBindings()
    }
}

class MessageBuilder {
    public static AbbreviationCache: string[] = []

    public static build(message: ChatMessage): HTMLElement {
        const $body = $("<div></div>")
            .addClass(CssClass.ChatEntry)
            .addClass(MessageBuilder.getMessageChannelCssClass(message))
            .addClass(MessageBuilder.getMessageTriggerGroupCssClass(message))
            .addClass(MessageBuilder.getMessageVisibilityCssClass(message))
            .attr(HtmlAttribute.ChatEntry_Source, MessageBuilder.getSource(message))
            .attr(HtmlAttribute.ChatEntry_Friendgroup, MessageBuilder.getFriendGroup(message))
            .attr(HtmlAttribute.ChatEntry_TriggerId, MessageBuilder.getTriggerGroup(message))

        $("<span></span>")
            .addClass(CssClass.ChatEntry_Time)
            .text(`[${MessageBuilder.formatTimestamp(message)}] `)
            .appendTo($body)

        const $content = $("<span></span>")
            .addClass(CssClass.ChatEntry_Text)
            .appendTo($body)

        const sender = MessageBuilder.formatSender(message)
        if (sender !== null) {
            $("<span></span>")
                .addClass(CssClass.ChatEntry_Sender)
                .text(`${sender} `)
                .appendTo($content)
        }

        for (const messageSegment of message.content) {
            $("<span></span>")
                .addClass(CssClass.ChatEntry_Segment)
                .addClass(MessageBuilder.getMessageSegmentClass(messageSegment.type))
                .text(messageSegment.text)
                .appendTo($content)
        }

        return $body[0]
    }

    static getMessageChannelCssClass(message: ChatMessage): string | null {
        const channelName = Constants.ChannelEnumToKey[message.channel]
        const data = Gobchat.Channels[channelName]
        return Utility.formatString(CssClass.ChatEntry_Channel_Partial, data.internalName)
        // return Utility.formatString(CssClass.ChatEntry_Channel_Partial, message.channel.toString())
    }

    static getMessageTriggerGroupCssClass(message: ChatMessage): string | null {
        if (message.source.triggerGroupId)
            return Utility.formatString(CssClass.ChatEntry_TriggerGroup_Partial, message.source.triggerGroupId)
        return null
    }

    static getTriggerGroup(message: ChatMessage): string | null {
        return message.source.triggerGroupId
    }

    static getMessageVisibilityCssClass(message: ChatMessage): string | null {
        if (!message.source)
            return null

        const visibility = message.source.visibility
        if (visibility >= 100)
            return null

        const ignoreDistance = gobConfig.get("behaviour.rangefilter.ignoreMention", false)
        if (ignoreDistance && message.containsMentions)
            return null

        const steps = gobConfig.get("behaviour.rangefilter.opacitysteps")

        const fadeOutStepSize = (100 / steps)
        const visibilityLevel = ((visibility + fadeOutStepSize - 1) / fadeOutStepSize) >> 0 //truncat decimals, makes the LSV an integer
        return Utility.formatString(CssClass.ChatEntry_FadeOut_Partial, visibilityLevel)
    }

    static getMessageSegmentClass(segmentType: MessageSegmentEnum): string | null {
        switch (segmentType) {
            case Gobchat.MessageSegmentEnum.SAY: return CssClass.ChatEntry_Segment_Say
            case Gobchat.MessageSegmentEnum.EMOTE: return CssClass.ChatEntry_Segment_Emote
            case Gobchat.MessageSegmentEnum.OOC: return CssClass.ChatEntry_Segment_Ooc
            case Gobchat.MessageSegmentEnum.MENTION: return CssClass.ChatEntry_Segment_Mention
            case Gobchat.MessageSegmentEnum.WEBLINK: return CssClass.ChatEntry_Segment_Link
            default: return null
        }
    }

    static formatTimestamp(message: ChatMessage): string {
        function twoDigits(v: number): string {
            return v < 10 ? '0' + v : v.toString()
        }

        const asDate = new Date(message.timestamp)
        const hours = twoDigits(asDate.getHours())
        const minutes = twoDigits(asDate.getMinutes())
        return `${hours}:${minutes}`
    }

    static formatSender(message: ChatMessage): string | null {
        const formatedSource = MessageBuilder.formatSource(message.source)
        return MessageBuilder.formatSourceAccordingToChannel(formatedSource, message.channel)
    }

    static getFriendGroup(message: ChatMessage): string | null {
        const group = message.source.ffGroup
        return group < 0 ? null : group.toString()
    }

    static getSource(message: ChatMessage): string | null {
        if (message.source === null)
            return null
        return message.source.characterName !== null ? message.source.characterName : message.source.original
    }

    static formatSource(messageSource: ChatMessageSource): string | null {
        if (messageSource === null)
            return null

        if (messageSource.characterName !== null) {
            let prefix = ""
            if (messageSource.party >= 0)
                prefix += `[${messageSource.party + 1}]`

            if (messageSource.alliance >= 0)
                prefix += `[${String.fromCharCode('A'.charCodeAt(0) + messageSource.alliance)}]`

            if (messageSource.ffGroup >= 0)
                prefix += Constants.FFGroupUnicodes[messageSource.ffGroup].char

            return `${prefix}${messageSource.characterName}`
        }

        return messageSource.original
    }

    static formatSourceAccordingToChannel(source: string | null, channel: number): string | null {
        switch (channel) {
            case Gobchat.ChannelEnum.GOBCHATINFO:
            case Gobchat.ChannelEnum.GOBCHATERROR: return `[${source}]`
            case Gobchat.ChannelEnum.ECHO: return "Echo:"
            case Gobchat.ChannelEnum.EMOTE: return source
            case Gobchat.ChannelEnum.TELLSEND: return `>> ${source}:`
            case Gobchat.ChannelEnum.TELLRECIEVE: return `${source} >>`
            case Gobchat.ChannelEnum.ERROR: return null
            case Gobchat.ChannelEnum.ANIMATEDEMOTE: return null //source is set, but the animation message already contains the source name
            case Gobchat.ChannelEnum.PARTY: return `(${source})`
            case Gobchat.ChannelEnum.ALLIANCE: return `<${source}>`
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
                return `[${MessageBuilder.getChannelAbbreviation(channel)}]<${source}>`
            default:
                if (source !== null && source !== undefined)
                    return source + ":"
                return null
        }
    }

    static getChannelAbbreviation(channel: number): string {
        if (channel in MessageBuilder.AbbreviationCache)
            return MessageBuilder.AbbreviationCache[channel]
        return channel?.toString() ?? ''
    }
}

module AudioPlayer {
    export interface MentionConfig {
        playSound: boolean,
        volume: number,
        soundPath: string,
        soundInterval: number
        trigger: string[]
        userCanTriggerMention: boolean
    }
}

class AudioPlayer {
    private static lastSoundPlayed: Date = new Date()

    constructor() {
    }

    private static getMentionConfig(): AudioPlayer.MentionConfig {
        return gobConfig.get("behaviour.mentions")
    }

    public static playMentionSoundIfPossible(message: ChatMessage): void {
        const config = AudioPlayer.getMentionConfig()
        if (!config.playSound || config.volume <= 0 || !config.soundPath)
            return

        if (!message.containsMentions)
            return

        if (message.source.visibility === 0) {
            const ignoreDistance = gobConfig.get("behaviour.fadeout.mention", false)
            if (!ignoreDistance)
                return
        }

        const time = new Date()
        if (time.valueOf() - AudioPlayer.lastSoundPlayed.valueOf() < config.soundInterval)
            return

        AudioPlayer.lastSoundPlayed = time
        const audio = new Audio(config.soundPath)
        audio.volume = config.volume
        audio.play()
    }

    public static playMentionSound(): void {
        const config = AudioPlayer.getMentionConfig()
        if (!config.playSound || config.volume <= 0 || !config.soundPath)
            return

        AudioPlayer.lastSoundPlayed = new Date()
        const audio = new Audio(config.soundPath)
        audio.volume = config.volume
        audio.play()
    }
}

/**
 *  Controls the content of a Tabbar, scrolling and animation.
 *  The controlled tabbar element will fire a 'change' event on any button press
 */
class TabBarControl {
    private static readonly ScrollToleranceZone = 5

    private static readonly AttributeTabId = "data-gob-tab-id"

    private static readonly CssNavBar = CssClass.Chat_Tabs
    private static readonly CssNavPanel = CssClass.Chat_History
    private static readonly CssNavPanelActiveTab = CssClass.Chat_History_Tab_Partial

    private static readonly CssActiveTabButton = "is-active"

    private static readonly CssScrollLeftButton = "gob-chat-tabbar_button--left"
    private static readonly CssScrollRightButton = "gob-chat-tabbar_button--right"
    private static readonly CssTabBarContent = CssClass.Chat_Tabs_Content
    private static readonly CssTabButton = CssClass.Chat_Tabs_Content_Tab
    private static readonly CssTabButtonMentionEffect = "gob-chat-tabbar_content_tab--mention-{0}"
    private static readonly CssTabButtonMessageEffect = "gob-chat-tabbar_content_tab--new-message-{0}"


    private static readonly selector_tabbar = `> .${TabBarControl.CssNavBar}`
    private static readonly selector_scrollLeftBtn = `> .${TabBarControl.CssScrollLeftButton}`
    private static readonly selector_scrollRightBtn = `> .${TabBarControl.CssScrollRightButton}`
    private static readonly selector_content = `> .${TabBarControl.CssTabBarContent}`
    private static readonly selector_activeTab = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}.${TabBarControl.CssActiveTabButton}`
    private static readonly selector_tabWithId = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}[${TabBarControl.AttributeTabId}={0}]`
    private static readonly selector_allTabs = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}`

    #databinding: Databinding.BindingContext | null = null
    #channelToTab: { [channelId: number]: string[] } = {}
    #navPanelData: {
        [tabId: string]: {
            scrollPosition: number
        }
    } = {}
    #cssClassForMentionTabEffect: string | null = null
    #cssClassForNewMessageTabEffect: string | null = null
    #isPanelScrolledToBottom: boolean = false

    #tabbar: JQuery = $()
    #navPanel: JQuery = $()

    constructor() {

    }

    control(navBar: HTMLElement | JQuery, navPanel: HTMLElement | JQuery): void {
        // unbind
        this.#tabbar.off("wheel", this.#onNavBarWheelScroll)
        this.#tabbar.find(TabBarControl.selector_scrollLeftBtn).off("click", this.#onNavBarBtnScroll)
        this.#tabbar.find(TabBarControl.selector_scrollRightBtn).off("click", this.#onNavBarBtnScroll)
        this.#tabbar.find(TabBarControl.selector_allTabs).off("click", this.#onTabClick)
        this.#navPanel.off("scroll", this.#onPanelScroll)
        this.#databinding?.clearBindings()

        // rebind
        const $navBar = $(navBar)
        if (!$navBar.hasClass(TabBarControl.CssNavBar))
            throw new Error("navBar not found")

        const $navPanel = $(navPanel)
        if (!$navPanel.hasClass(TabBarControl.CssNavPanel))
            throw new Error("navPanel not found")

        this.#tabbar = $navBar
        this.#tabbar.on("wheel", this.#onNavBarWheelScroll)
        this.#tabbar.find(TabBarControl.selector_scrollLeftBtn).on("click", this.#onNavBarBtnScroll)
        this.#tabbar.find(TabBarControl.selector_scrollRightBtn).on("click", this.#onNavBarBtnScroll)

        this.#navPanel = $navPanel
        this.#navPanel.on("scroll", this.#onPanelScroll)

        this.#isPanelScrolledToBottom = true

        this.#databinding = new Databinding.BindingContext(gobConfig)
        Databinding.bindListener(this.#databinding, "behaviour.chattabs", config => this.#updateTabs(config))
        Databinding.bindListener(this.#databinding, "behaviour.chattabs.data", config => this.#buildChannelToTabMapping(config))
        Databinding.bindListener(this.#databinding, "behaviour.chattabs.effect", (effect) => {
            this.#cssClassForMentionTabEffect = effect.mention > 0 ? Utility.formatString(TabBarControl.CssTabButtonMentionEffect, effect.mention) : null
            this.#cssClassForNewMessageTabEffect = effect.message > 0 ? Utility.formatString(TabBarControl.CssTabButtonMessageEffect, effect.message) : null
        })
        this.#databinding.loadBindings()
    }

    applyNewMessageAnimationToTabs(channel: number, hasMention: boolean): void {
        const affectedTabs = this.#channelToTab[channel] || []
        const activeTabId = this.#activeTabId
        if (_.includes(affectedTabs, activeTabId))
            return // done, message was visible on active tab

        const cssClassForMentionEffect = this.#cssClassForMentionTabEffect
        const cssClassForNewMessageEffect = this.#cssClassForNewMessageTabEffect

        for (const tabId of affectedTabs) {
            if (tabId === activeTabId)
                continue // do not apply any effects to the active tab

            const $tab = this.#getTab(tabId)

            if (hasMention && cssClassForMentionEffect) {
                $tab.removeClass(cssClassForNewMessageEffect)
                    .addClass(cssClassForMentionEffect)
                    .on("click.tab.effects.mention", function () {
                        $(this).off("click.tab.effects.mention")
                            .removeClass(cssClassForMentionEffect)
                    })
                continue //apply only one effect
            }

            if (cssClassForNewMessageEffect) {
                $tab.filter(`:not(.${cssClassForMentionEffect})`)
                    .addClass(cssClassForNewMessageEffect)
                    .on("click.tab.effects.message", function () {
                        $(this).off("click.tab.effects.message")
                            .removeClass(cssClassForNewMessageEffect)
                    })
                continue //apply only one effect
            }
        }
    }

    scrollToBottomIfNeeded(scrollFast: boolean = false): void {
        if (this.#isPanelScrolledToBottom)
            this.#scrollPanelToBottom(scrollFast)
    }

    #onNavBarBtnScroll = (event: any) => { // bound to this class instance
        const scrollDirection = $(event.currentTarget).hasClass(TabBarControl.CssScrollRightButton) ? 1 : -1
        this.#scrollTabs(scrollDirection)
    }

    #onNavBarWheelScroll = (event: any) => { // bound to this class instance
        const scrollDirection = (event.originalEvent as WheelEvent).deltaY > 0 ? 1 : -1
        this.#scrollTabs(scrollDirection)
    }

    #onPanelScroll = (event: any) => { // bound to this class instance
        const $panel = $(event.currentTarget)
        const panelBottom = $panel.scrollTop() + $panel.innerHeight()
        const closeToBottm = panelBottom + TabBarControl.ScrollToleranceZone >= event.currentTarget.scrollHeight
        this.#isPanelScrolledToBottom = closeToBottm
    }

    #onTabClick = (event: any) => { // bound to this class instance
        const id = $(event.currentTarget).attr(TabBarControl.AttributeTabId) as string
        this.#activateTab(id)
    }

    #buildChannelToTabMapping(config: any) {
        this.#channelToTab = {}
        for (let chatTab of Object.values(config) as any[]) {
            if (!chatTab.visible)
                return

            for (let channel of chatTab.channel.visible) {
                if (channel in this.#channelToTab)
                    this.#channelToTab[channel].push(chatTab.id)
                else
                    this.#channelToTab[channel] = [chatTab.id]
            }
        }
    }

    #getTab(idOrIndex: string | number): JQuery {
        idOrIndex ??= 0

        if (Utility.isNumber(idOrIndex)) {
            const $childs = this.#tabbar.find(TabBarControl.selector_content).children()
            const $nextTab = $childs.eq(Math.max(0, Math.min($childs.length, idOrIndex as number)))
            return $nextTab
        } else {
            const selector = Utility.formatString(TabBarControl.selector_tabWithId, idOrIndex)
            const $nextTab = this.#tabbar.find(selector)
            return $nextTab
        }
    }

    #activateTab(idOrIndex: string | number): boolean {
        const lastActiveTabId = this.#activeTabId

        if (lastActiveTabId in this.#navPanelData) {
            this.#navPanelData[lastActiveTabId].scrollPosition = this.#isPanelScrolledToBottom ? -1 : this.#panelScrollPosition
        }

        // deactiate previous active tab
        this.#tabbar.find(TabBarControl.selector_activeTab).removeClass(TabBarControl.CssActiveTabButton)

        // find new active tab
        const $tab = this.#getTab(idOrIndex)
        $tab.addClass(TabBarControl.CssActiveTabButton)

        if ($tab.length === 0) { //there is no tab with this id
            this.#activateTab(0) //fallback
            return false
        }

        const newActiveTabId = this.#activeTabId

        this.#navPanel // used to filter messages depending on which tab is active
            .removeClass(Utility.formatString(TabBarControl.CssNavPanelActiveTab, lastActiveTabId))
            .addClass(Utility.formatString(TabBarControl.CssNavPanelActiveTab, newActiveTabId))

        // restore scroll position
        if (newActiveTabId in this.#navPanelData) {
            if (this.#navPanelData[newActiveTabId].scrollPosition < 0)
                this.#scrollPanelToBottom(true)
            else
                this.#scrollPanelToPosition(this.#navPanelData[newActiveTabId].scrollPosition, true)
        }

        return true
    }

    get #activeTabId(): string {
        const activeTab = this.#tabbar.find(TabBarControl.selector_activeTab)
        if (activeTab.length === 0)
            return "";
        return activeTab.attr(TabBarControl.AttributeTabId) as string
    }

    #updateTabs(config: any): void {
        const configData = config["data"]
        const configSorting = config["sorting"] as string[]
        const newTabsInOrder = configSorting
            .filter(id => configData[id].visible)
            .map(id => { return { id: id, name: configData[id].name } }) as { id: string, name: string }[]

        // remove old tabs and store them in a lookup table
        const $content = this.#tabbar.find(TabBarControl.selector_content)
        const $oldTabs = $content.children().detach()
        const oldTabsLookup: { [id: string]: HTMLElement } = {}
        for (const tab of $oldTabs) {
            const id = tab.getAttribute(TabBarControl.AttributeTabId)
            if(id)
                oldTabsLookup[id] = tab
        }

        // add new tabs or reattach old tabs in order
        for (const entry of newTabsInOrder) {
            if (entry.id in oldTabsLookup) {
                $(oldTabsLookup[entry.id])
                    .text(entry.name)
                    .appendTo($content)
            } else {
                $("<button></button>")
                    .addClass(TabBarControl.CssTabButton)
                    .attr(TabBarControl.AttributeTabId, entry.id)
                    .on("click", this.#onTabClick)
                    .text(entry.name)
                    .appendTo($content)
            }
        }

        // remove old nav panel data
        for (const tabId of Object.keys(this.#navPanelData)) {
            if (!_.includes(configSorting, tabId))
                delete this.#navPanelData[tabId]
        }

        // add new nav panel data
        for (const tabId of configSorting) {
            if (!(tabId in this.#navPanelData)) {
                this.#navPanelData[tabId] = {
                    scrollPosition: -1
                }
            }
        }

        // const idsToRemove = oldTabIds.filter(id => !_.includes(newTabIds, id))
        // for (let id of idsToRemove)
        //     $oldTabs.filter(`[${TabBarControl.attributeTabId}=${id}]`).remove()

        if (!this.#activeTabId)
            this.#activateTab(0)
        this.#scrollTabs(0) //update the scroll view
    }

    /**
     * 
     * @param direction if positive, scroll right, otherwise left
     */
    #scrollTabs(direction: number): void {
        const $content = this.#tabbar.find(TabBarControl.selector_content)
        const numberOfChilds = $content.children().length

        //not perfect, it would be nice to be able to scroll to the 'next' element or to scroll an element into view
        const scrollDistance = Math.max(10, $content.width() / numberOfChilds)
        const newPosition = $content.scrollLeft() + direction * scrollDistance

        $content.animate({
            scrollLeft: newPosition
        }, 50)

        const isAtLeftBorder = newPosition <= 0
        this.#tabbar.find(TabBarControl.selector_scrollLeftBtn)
            .prop("disabled", isAtLeftBorder)

        const scrollWidth = Utility.toFloat($content.prop("scrollWidth"), 0)
        const clientWidth = Utility.toFloat($content.prop("clientWidth"), 0)
        const isAtRightBorder = (scrollWidth - clientWidth) <= newPosition
        this.#tabbar.find(TabBarControl.selector_scrollRightBtn)
            .prop("disabled", isAtRightBorder)
    }

    #scrollPanelToBottom(scrollFast: boolean): void {
        const navPanel = this.#navPanel[0]
        const position = navPanel.scrollHeight - navPanel.clientHeight
        this.#scrollPanelToPosition(position, scrollFast)
    }

    #scrollPanelToPosition(position: number, scrollFast: boolean): void {
        if (scrollFast) {
            this.#navPanel.scrollTop(position)
        } else {
            this.#navPanel.animate({
                scrollTop: position
            }, 10);
        }
    }

    get #panelScrollPosition(): number {
        return this.#navPanel.scrollTop()
    }
}

class ChatSearchControl {
    private static readonly cssMarkedbySearch = CssClass.ChatEntry_MarkedbySearch
    private static readonly cssActiveSelection = CssClass.ChatEntry_SelectedBySearch
    private static readonly cssChatMessage = CssClass.ChatEntry

    private static readonly selector_input = "> .js-input"
    private static readonly selector_counter = "> .js-counter"
    private static readonly selector_up = "> .js-up"
    private static readonly selector_down = "> .js-down"
    private static readonly selector_reset = "> .js-reset"
    private static readonly selector_markedBySearch = `.${this.cssMarkedbySearch}`
    private static readonly selector_activeSelection = `.${this.cssActiveSelection}`
    private static readonly selector_visible_messages = `.${this.cssChatMessage}:visible`

    #searchElement = $()
    #chatHistory = $()

    constructor() {
    }

    control(searchElement: HTMLElement | JQuery, chatHistory: HTMLElement | JQuery): void {
        // unbind
        this.#searchElement.find(ChatSearchControl.selector_input).off("keyup", this.#onInputEnter)
        this.#searchElement.find(ChatSearchControl.selector_counter).off("click", this.scrollToSelection)
        this.#searchElement.find(ChatSearchControl.selector_up).off("click", this.moveSelectionUp)
        this.#searchElement.find(ChatSearchControl.selector_down).off("click", this.moveSelectionDown)
        this.#searchElement.find(ChatSearchControl.selector_reset).off("click", this.clearSearch)
        this.#removeMessageMarkers()

        // rebind
        this.#searchElement = $(searchElement).first()
        this.#chatHistory = $(chatHistory).first()

        this.#searchElement.find(ChatSearchControl.selector_input).on("keyup", this.#onInputEnter)
        this.#searchElement.find(ChatSearchControl.selector_counter).on("click", this.scrollToSelection)
        this.#searchElement.find(ChatSearchControl.selector_up).on("click", this.moveSelectionUp)
        this.#searchElement.find(ChatSearchControl.selector_down).on("click", this.moveSelectionDown)
        this.#searchElement.find(ChatSearchControl.selector_reset).on("click", this.clearSearch)
    }

    #onInputEnter = (event) => {
        if (event.keyCode === 13) // enter
            this.startSearch()
    }

    #updateCounterValue = () => {
        const $markedMessages = this.#chatHistory.find(ChatSearchControl.selector_markedBySearch)
        const $activeSelection = this.#chatHistory.find(ChatSearchControl.selector_activeSelection)
        const max = $markedMessages.length
        const current = max > 0 ? $markedMessages.index($activeSelection) : 0
        this.#searchElement.find(ChatSearchControl.selector_counter).text(`${max - current} / ${max}`)
    }

    #removeMessageMarkers = () => {
        this.#chatHistory.find(ChatSearchControl.selector_markedBySearch).removeClass(ChatSearchControl.cssMarkedbySearch)
        this.#chatHistory.find(ChatSearchControl.selector_activeSelection).removeClass(ChatSearchControl.cssActiveSelection)
        this.#updateCounterValue()
    }

    hide = () => {
        this.visible = false
    }

    show = () => {
        this.visible = true
    }

    toggle = () => {
        this.visible = !this.visible
    }

    get visible() {
        return this.#searchElement.is(":visible")
    }

    set visible(value) {
        if (value) {
            this.#searchElement.show()
            this.#searchElement.find(ChatSearchControl.selector_input).focus()
        } else {
            this.#searchElement.hide()
            this.clearSearch()
        }
    }

    clearSearch = () => {
        this.#removeMessageMarkers()
        this.#searchElement.find(ChatSearchControl.selector_input).val("").focus()
    }

    scrollToSelection = () => {
        const $selectedElement = this.#chatHistory.find(ChatSearchControl.selector_activeSelection)
        if ($selectedElement.length === 0)
            return

        const selectedElement = $selectedElement[0]
        const scrollableFrame = this.#chatHistory[0]

        const containerTop = scrollableFrame.scrollTop
        const containerBot = scrollableFrame.clientHeight + containerTop
        const elementTop = selectedElement.offsetTop - scrollableFrame.offsetTop
        const elementBot = selectedElement.clientHeight + elementTop
        const isVisible = containerTop <= elementTop && elementBot <= containerBot

        if (isVisible)
            return

        const position = elementTop;

        $(scrollableFrame).animate({
            scrollTop: position
        }, 100)
    }

    search = (text) => {
        this.#removeMessageMarkers()

        if (text === null || text === undefined)
            return

        text = text.trim().toLowerCase()
        if (text.length === 0)
            return

        this.#chatHistory.find(ChatSearchControl.selector_visible_messages).each(function () {
            if ($(this).text().toLowerCase().indexOf(text) >= 0)
                $(this).addClass(ChatSearchControl.cssMarkedbySearch)
        })

        const $markedMessages = this.#chatHistory.find(ChatSearchControl.selector_markedBySearch)

        if ($markedMessages.length === 0) {
            this.#updateCounterValue()
        } else {
            $markedMessages.last().addClass(ChatSearchControl.cssActiveSelection)
            this.#updateCounterValue()
            this.scrollToSelection()
        }
    }

    startSearch = () => {
        this.search(this.#searchElement.find(ChatSearchControl.selector_input).val())
    }

    moveSelectionUp = () => {
        const $activeSelection = this.#chatHistory.find(ChatSearchControl.selector_activeSelection)
        $activeSelection.removeClass(ChatSearchControl.cssActiveSelection)

        let $prevMarked = $activeSelection.prevAll(ChatSearchControl.selector_markedBySearch).first()

        if ($prevMarked.length === 0) //wrap around
            $prevMarked = this.#chatHistory.find(ChatSearchControl.selector_markedBySearch).last()

        $prevMarked.addClass(ChatSearchControl.cssActiveSelection)

        this.#updateCounterValue()
        this.scrollToSelection()
    }

    moveSelectionDown = () => {
        const $activeSelection = this.#chatHistory.find(ChatSearchControl.selector_activeSelection)
        $activeSelection.removeClass(ChatSearchControl.cssActiveSelection)

        let $nextMarked = $activeSelection.nextAll(ChatSearchControl.selector_markedBySearch).first()

        if ($nextMarked.length === 0) //wrap around
            $nextMarked = this.#chatHistory.find(ChatSearchControl.selector_markedBySearch).first()

        $nextMarked.addClass(ChatSearchControl.cssActiveSelection)

        this.#updateCounterValue()
        this.scrollToSelection()
    }
}

class ChatGroupControl {

    private static readonly selector_messages = `.${CssClass.ChatEntry}`

    #databinding = new Databinding.BindingContext(gobConfig)
    #chatHistory: JQuery = $()

    control(chatHistory: HTMLElement | JQuery): void {
        this.#databinding.clearBindings()
        this.#chatHistory = $(chatHistory)

        this.#databinding.bindCallback("behaviour.groups.sorting", () => {
            this.#updateTriggerGroupsForChatEntries()
        }, false)


        this.#databinding.bindCallback("behaviour.groups.data", () => {
            this.#updateTriggerGroupsForChatEntries()
        }, false)

        this.#databinding.loadBindings()
    }

    #updateTriggerGroupsForChatEntries() {
        const doUpdate = gobConfig.get("behaviour.groups.updateChat", false)
        if (!doUpdate)
            return

        const groupOrder = gobConfig.get("behaviour.groups.sorting")
        const groups = groupOrder.map(id => gobConfig.get(`behaviour.groups.data.${id}`)) as any[]

        for (const message of this.#chatHistory.find(ChatGroupControl.selector_messages)) {
            const triggerId = message.getAttribute(HtmlAttribute.ChatEntry_TriggerId)
            if (triggerId !== null) {
                const cssClass = Utility.formatString(CssClass.ChatEntry_TriggerGroup_Partial, triggerId)
                message.classList.remove(cssClass)
            }

            let matchingGroupId: string | null = null
            for (const group of groups) {
                if ("ffgroup" in group) {
                    if (message.getAttribute(HtmlAttribute.ChatEntry_Friendgroup) == group.ffgroup) {
                        matchingGroupId = group.id as string
                        break
                    }
                } else {
                    const source = message.getAttribute(HtmlAttribute.ChatEntry_Source)?.toLowerCase()
                    if (source !== undefined) {
                        if (_.includes(group.trigger, source)) {
                            matchingGroupId = group.id as string
                            break
                        }
                    }
                }
            }

            if (matchingGroupId !== null) {
                const cssClass = Utility.formatString(CssClass.ChatEntry_TriggerGroup_Partial, matchingGroupId)
                message.setAttribute(HtmlAttribute.ChatEntry_TriggerId, matchingGroupId)
                message.classList.add(cssClass)
            }
        }
    }
}