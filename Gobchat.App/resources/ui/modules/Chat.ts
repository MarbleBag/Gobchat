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
export type MessageSegmentEnum = number

// created by backend
export interface MessageSegment {
    type: MessageSegmentEnum
    text: string
}

// created by backend
export interface ChatMessage {
    source: ChatMessageSource
    timestamp: Date
    channel: ChatChannelEnum
    content: MessageSegment[]
    containsMentions: boolean
}

//#endregion

export class ChatControl {
    static readonly selector_chat_history = ".gob-chat_history"
    static readonly selector_tabbar = ".gob-chat_tabbar"

    #cmdManager: Cmd.CommandManager
    #msgBuilder: MessageBuilder
    #audioPlayer: AudioPlayer
    #tabControl: TabBarControl

    #chatBox: JQuery = null

    #hideInfo: boolean = false
    #hideError: boolean = false

    constructor() {
        this.#cmdManager = new Cmd.CommandManager()
        this.#audioPlayer = new AudioPlayer()
        this.#msgBuilder = new MessageBuilder()
        this.#tabControl = new TabBarControl()
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

        const messageAsHtml = this.#msgBuilder.build(message)
        this.#chatBox.find(ChatControl.selector_chat_history).append(messageAsHtml)

        this.#tabControl.scrollToBottomIfNeeded()
        this.#tabControl.applyAnimationToTab(message.channel, message.containsMentions)
        this.#audioPlayer.playSoundIfAllowed(message)
    }

    showGobInfo(on: boolean): void {
        this.#hideError = on
    }

    showGobError(on: boolean): void {
        this.#hideInfo = on
    }

    control(chatBox: HTMLElement | JQuery): void {
        if (this.#chatBox) {
            document.removeEventListener("ChatMessagesEvent", this.#onNewMessageEvent)
        }

        this.#chatBox = $(chatBox)
        this.#tabControl.control(this.#chatBox.find(ChatControl.selector_tabbar), this.#chatBox.find(ChatControl.selector_chat_history))

        if (this.#chatBox.length > 0)
            document.addEventListener("ChatMessagesEvent", this.#onNewMessageEvent)
    }
}

class MessageBuilder {
    public static AbbreviationCache: string[] = []

    build(message: ChatMessage): HTMLElement {
        const $body = $("<div></div>")
            .addClass("gob-chat-entry")
            .addClass(MessageBuilder.getMessageChannelCssClass(message))
            .addClass(MessageBuilder.getMessageTriggerGroupCssClass(message))
            .addClass(MessageBuilder.getMessageVisibilityCssClass(message))

        $("<span></span>")
            .addClass("gob-chat-entry__time")
            .text(`[${MessageBuilder.getMessageTimestamp(message)}] `)
            .appendTo($body)

        const $content = $("<span></span>")
            .addClass("gob-chat-entry__text")
            .appendTo($body)

        const sender = MessageBuilder.getSender(message)
        if (sender !== null) {
            $("<span></span>")
                .addClass("gob-chat-entry__sender")
                .text(`${sender} `)
                .appendTo($content)
        }

        for (let messageSegment of message.content) {
            $("<span></span>")
                .addClass("gob-chat_entry__text__segment")
                .addClass(MessageBuilder.getMessageSegmentClass(messageSegment.type))
                .text(messageSegment.text)
                .appendTo($content)
        }

        return $body[0]
    }

    static getMessageChannelCssClass(message: ChatMessage): string {
        const channelName = Constants.ChannelEnumToKey[message.channel]
        const data = Gobchat.Channels[channelName]
        return `gob-chat-entry--channel-${data.internalName}`
    }

    static getMessageTriggerGroupCssClass(message: ChatMessage): string {
        if (message.source.triggerGroupId)
            return `gob-chat-entry--trigger-group-${message.source.triggerGroupId}`
        return null
    }

    static getMessageVisibilityCssClass(message: ChatMessage): string {
        if (!message.source)
            return null

        const visibility = message.source.visibility
        if (visibility >= 100)
            return null

        const ignoreDistance = gobConfig.get("behaviour.rangefilter.ignoreMention", false)
        if (ignoreDistance && message.containsMentions)
            return null

        const fadeOutStepSize = (100 / Constants.RangeFilterFadeOutLevels)
        const visibilityLevel = ((visibility + fadeOutStepSize - 1) / fadeOutStepSize) >> 0 //truncat decimals, makes the LSV an integer
        return `gob-chat-entry--fadeout-${visibilityLevel}`
    }

    static getMessageSegmentClass(segmentType: MessageSegmentEnum): string {
        switch (segmentType) {
            case Gobchat.MessageSegmentEnum.SAY: return "gob-chat-entry__text__segment--say"
            case Gobchat.MessageSegmentEnum.EMOTE: return "gob-chat-entry__text__segment--emote"
            case Gobchat.MessageSegmentEnum.OOC: return "gob-chat-entry__text__segment--ooc"
            case Gobchat.MessageSegmentEnum.MENTION: return "gob-chat-entry__text__segment--mention"
            case Gobchat.MessageSegmentEnum.WEBLINK: return "gob-chat-entry__text__segment--link"
            default: return null
        }
    }

    static getMessageTimestamp(message: ChatMessage): string {
        function twoDigits(v: number): string {
            return v < 10 ? '0' + v : v.toString()
        }

        const asDate = new Date(message.timestamp)
        const hours = twoDigits(asDate.getHours())
        const minutes = twoDigits(asDate.getMinutes())
        return `${hours}:${minutes}`
    }

    static getSender(message: ChatMessage): string {
        const senderRaw = MessageBuilder.getSenderFromSource(message.source)
        return MessageBuilder.formatSenderAccordingToChannel(senderRaw, message.channel)
    }

    static getSenderFromSource(messageSource: ChatMessageSource): string {
        if (messageSource === null || messageSource.original === null)
            return null

        if (messageSource.characterName !== null && messageSource.characterName !== undefined) {
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

    static formatSenderAccordingToChannel(sender: string, channel: number): string {
        switch (channel) {
            case Gobchat.ChannelEnum.GOBCHATINFO:
            case Gobchat.ChannelEnum.GOBCHATERROR: return `[${sender}]`
            case Gobchat.ChannelEnum.ECHO: return "Echo:"
            case Gobchat.ChannelEnum.EMOTE: return sender
            case Gobchat.ChannelEnum.TELLSEND: return `>> ${sender}:`
            case Gobchat.ChannelEnum.TELLRECIEVE: return `${sender} >>`
            case Gobchat.ChannelEnum.ERROR: return null
            case Gobchat.ChannelEnum.ANIMATEDEMOTE: return null //source is set, but the animation message already contains the source name
            case Gobchat.ChannelEnum.PARTY: return `(${sender})`
            case Gobchat.ChannelEnum.ALLIANCE: return `<${sender}>`
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
                return `[${MessageBuilder.getChannelAbbreviation(channel)}]<${sender}>`
            default:
                if (sender !== null && sender !== undefined)
                    return sender + ":"
                return null
        }
    }

    static getChannelAbbreviation(channel: number): string {
        if (channel in MessageBuilder.AbbreviationCache)
            return MessageBuilder.AbbreviationCache[channel]
        return channel?.toString() ?? ''
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

/**
 *  Controls the content of a Tabbar, scrolling and animation.
 *  The controlled tabbar element will fire a 'change' event on any button press
 */
class TabBarControl {
    private static readonly ScrollToleranceZone = 5

    private static readonly AttributeTabId = "data-gob-tab-id"

    private static readonly CssNavBar = "gob-chat_tabbar"
    private static readonly CssNavPanel = "gob-chat_history"   

    private static readonly CssDisableScrollButton = "is-disabled"
    private static readonly CssActiveTabButton = "is-active"
 
    private static readonly CssScrollLeftButton = "gob-chat_tabbar_button--left"
    private static readonly CssScrollRightButton = "gob-chat_tabbar_button--right"
    private static readonly CssTabBarContent = "gob-chat_tabbar_content"
    private static readonly CssTabButton = "gob-chat_tabbar_content_tab"

    private static readonly cssActiveTabId = "gob-chat_tab-{0}"

    private static readonly selector_tabbar = `> .${TabBarControl.CssNavBar}`

    private static readonly selector_scrollLeftBtn = `> .${TabBarControl.CssScrollLeftButton}`
    private static readonly selector_scrollRightBtn = `> .${TabBarControl.CssScrollRightButton}`
    private static readonly selector_content = `> .${TabBarControl.CssTabBarContent}`

    private static readonly selector_activeTab = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}.${TabBarControl.CssActiveTabButton}`

    private static readonly selector_tabWithId = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}[${TabBarControl.AttributeTabId}={0}]`

    private static readonly selector_allTabs = `> .${TabBarControl.CssTabBarContent} > .${TabBarControl.CssTabButton}`

    #databinding: Databinding.BindingContext = null
    #channelToTab: { [channelId: number]: string[] } = {}
    #navPanelData: { [tabId: string]: {
        scrollPosition:number
    }} = {}
    #isPanelScrolledToBottom: boolean = false

    #tabbar: JQuery = null
    #navPanel: JQuery = null

    constructor() {

    }

    control(navBar: HTMLElement | JQuery, navPanel: HTMLElement | JQuery): void {
        if (this.#tabbar) {
            this.#tabbar.off("wheel", this.#onNavBarWheelScroll)
            this.#tabbar.find(TabBarControl.selector_scrollLeftBtn).off("click", this.#onNavBarBtnScroll)
            this.#tabbar.find(TabBarControl.selector_scrollRightBtn).off("click", this.#onNavBarBtnScroll)
            this.#tabbar.find(TabBarControl.selector_allTabs).off("click", this.#onTabClick)
        }

        if(this.#navPanel){
            this.#navPanel.off("scroll", this.#onPanelScroll)
        }

        this.#databinding?.clear()

        this.#tabbar = null
        this.#navPanel = null

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

        this.#databinding = new Databinding.BindingContext(gobConfig)
        Databinding.bindListener(this.#databinding, "behaviour.chattabs", config => this.#updateTabs(config))
        Databinding.bindListener(this.#databinding, "behaviour.chattabs.data", config => { this.#buildChannelToTabMapping(config) })
        Databinding.bindListener(this.#databinding, "behaviour.chattabs.effect", () => { })
        this.#databinding.initialize()
    }

    applyAnimationToTab(channel: number, hasMention: boolean): void {        
        const affectedTabs = this.#channelToTab[channel] || []
        const activeTabId = this.#activeTabId
        if(_.includes(affectedTabs, activeTabId))
            return // done, message was visible on active tab

        //TODO
    }

    scrollToBottomIfNeeded(scrollFast: boolean = false): void {
        if (this.#isPanelScrolledToBottom)
            this.#scrollPanelToBottom(scrollFast)
    }

    #onNavBarBtnScroll = (event: any) => { // bound to this class instance
        const scrollDirection = $(event.target).hasClass(TabBarControl.CssScrollRightButton) ? 1 : -1
        this.#scrollTabs(scrollDirection)
    }

    #onNavBarWheelScroll = (event: any) => { // bound to this class instance
        const scrollDirection = (event.originalEvent as WheelEvent).deltaY > 0 ? 1 : -1
        this.#scrollTabs(scrollDirection)
    }

    #onPanelScroll = (event: any) => {
        const $panel = $(event.target)
        const panelBottom = $panel.scrollTop() + $panel.innerHeight()
        const closeToBottm = panelBottom + TabBarControl.ScrollToleranceZone >= event.target.scrollHeight
        this.#isPanelScrolledToBottom = closeToBottm
    }

    #onTabClick = (event: any) => { // bound to this class instance
        const id = $(event.target).attr(TabBarControl.AttributeTabId) as string
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

    #activateTab(idOrIndex: string | number): boolean {
        idOrIndex ??= 0

        const lastActiveTabId = this.#activeTabId

        if(lastActiveTabId in this.#navPanelData){
            this.#navPanelData[lastActiveTabId].scrollPosition = this.#isPanelScrolledToBottom ? -1 : this.#panelScrollPosition
        }

        // deactiate previous active tab
        this.#tabbar.find(TabBarControl.selector_activeTab).removeClass(TabBarControl.CssActiveTabButton)

        // find new active tab
        if (Utility.isNumber(idOrIndex)) {
            const $childs = this.#tabbar.find(TabBarControl.selector_content).children()
            const $nextTab = $childs.eq(Math.max(0, Math.min($childs.length, idOrIndex as number)))
            $nextTab.addClass(TabBarControl.CssActiveTabButton)
        } else {
            const selector = Utility.formatString(TabBarControl.selector_tabWithId, idOrIndex)
            const $nextTab = this.#tabbar.find(selector)

            if($nextTab.length === 0){ //there is no tab with this id
                this.#activateTab(0) //fallback
                return false
            }

            $nextTab.addClass(TabBarControl.CssActiveTabButton)            
        }
        
        const newActiveTabId = this.#activeTabId

        this.#navPanel // used to filter messages depending on which tab is active
            .removeClass(Utility.formatString(TabBarControl.cssActiveTabId, lastActiveTabId))
            .addClass(Utility.formatString(TabBarControl.cssActiveTabId, newActiveTabId))

        // restore scroll position
        if(newActiveTabId in this.#navPanelData){
            if(this.#navPanelData[newActiveTabId].scrollPosition < 0)
                this.#scrollPanelToBottom(true)
            else
                this.#scrollPanelToPosition(this.#navPanelData[newActiveTabId].scrollPosition, true)
        }

        return true
    }

    get #activeTabId(): string {
        return this.#tabbar.find(TabBarControl.selector_activeTab).attr(TabBarControl.AttributeTabId) as string
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
        const oldTabsLookup: {[id:string]:HTMLElement} = {}
        for (let tab of $oldTabs) {
            const id = tab.getAttribute(TabBarControl.AttributeTabId)
            oldTabsLookup[id] = tab
        }

        // add new tabs or reattach old tabs in order
        for (let entry of newTabsInOrder) {
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
        for(let tabId of Object.keys(this.#navPanelData)){
            if(!_.includes(tabId, configSorting))
                delete this.#navPanelData[tabId]
        }

        // add new nav panel data
        for(let tabId of configSorting){
            if(!(tabId in this.#navPanelData)){
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
            .toggleClass(TabBarControl.CssDisableScrollButton, isAtLeftBorder)

        const scrollWidth = Utility.toNumber($content.prop("scrollWidth"), 0)
        const clientWidth = Utility.toNumber($content.prop("clientWidth"), 0)
        const isAtRightBorder = (scrollWidth - clientWidth) <= newPosition
        this.#tabbar.find(TabBarControl.selector_scrollRightBtn)
            .toggleClass(TabBarControl.CssDisableScrollButton, isAtRightBorder)
    }

    #scrollPanelToBottom(scrollFast:boolean): void {
        const navPanel = this.#navPanel[0]
        const position = navPanel.scrollHeight - navPanel.clientHeight
        this.#scrollPanelToPosition(position, scrollFast)
    }

    #scrollPanelToPosition(position:number, scrollFast:boolean): void {
        if (scrollFast) {
            this.#navPanel.scrollTop(position)
        } else {
            this.#navPanel.animate({
                scrollTop: position
            }, 10);
        }
    }

    get #panelScrollPosition(): number{
        return this.#navPanel.scrollTop()
    }
}