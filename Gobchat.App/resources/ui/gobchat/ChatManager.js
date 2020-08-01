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

var Gobchat = (function (Gobchat) {
    function getChatPanel(base) {
        return $(base).find(".chat-panel > *").first()
    }

    function getChatTabBar(base) {
        return $(base).find(".chat-tabbar")
    }

    class ChatboxManager {
        constructor() {
            this._initialized = false
            this._$target = null
            this._databinding = null

            this._tabData = {}
            this._cmdManager = null
            this._messageHtmlBuilder = null
            this._messageSound = null
            this._scrollControl = null
        }

        async control(target) {
            if (this._initialized) return
            if (!target)
                throw new Error("target is null")
            this._initialized = true;

            this._$target = $(target)
            this._databinding = GobConfigHelper.makeDatabinding(gobconfig)

            this._cmdManager = new Gobchat.CommandManager()

            this._messageHtmlBuilder = new Gobchat.MessageHtmlBuilder(gobconfig)
            this._messageSound = new Gobchat.MessageSoundPlayer(gobconfig)

            this._scrollControl = new ScrollControl()
            this._scrollControl.control(getChatPanel(this._$target))

            this.updateStyle() //TODO

            GobConfigHelper.bindListener(this._databinding, "behaviour.chattabs.sorting", data => this._manageChatTabs(data))
            GobConfigHelper.bindListener(this._databinding, "behaviour.language", data => {
                (async () => {
                    const channels = Object.entries(Gobchat.Channels)
                        .map(e => e[1])

                    const requestTranslation = channels
                        .map(data => data.abbreviationId)
                        .filter(e => e !== null && e !== undefined)

                    const lookup = await goblocale.getAll(requestTranslation)
                    const result = {}

                    channels.forEach(data => {
                        if (data.abbreviationId)
                            result[data.chatChannel] = lookup[data.abbreviationId]
                    })
                    this._messageHtmlBuilder.abbreviationCache = result
                })();
            })

            this._databinding.initialize()
            document.addEventListener("ChatMessagesEvent", evt => this._onNewMessageEvent(evt))
        }

        updateStyle() {
            Gobchat.ChatStyleBuilder.updateStyle("custome_style_id")
        }

        scrollToBottomIfNeeded() {
            this._scrollControl.scrollToBottomIfNeeded()
        }

        showGobInfo(value) {
            this._hideInfo = !value
        }

        showGobError(value) {
            this._hideError = !value
        }

        _manageChatTabs(tabIds) {
            const $tabBar = getChatTabBar(this._$target)
            const $chatPanel = getChatPanel(this._$target)
            const tabData = this._tabData
            const scrollControl = this._scrollControl
            const tabsModel = gobconfig.get("behaviour.chattabs.data")

            tabIds = tabIds.filter(id => tabsModel[id].visible) //only show tabs which are visible

            function activateChatTab(newTabId) {
                const activeTabId = $chatPanel.attr("data-gob-activetab") || ""
                storeTabSettings(activeTabId)
                setTabActive(activeTabId, newTabId)
                restoreTabSettings(newTabId)
                $chatPanel.attr("data-gob-activetab", newTabId)
            }

            function storeTabSettings(tabId) {
                if (tabId in tabData) {
                    const data = tabData[tabId]
                    data.scrollPosition = scrollControl.isScrolledToBottom ? -1 : scrollControl.scrollPosition
                }
            }

            function setTabActive(oldTabId, newTabId) {
                $chatPanel
                    .removeClass(`chat-tab-${oldTabId}`)
                    .addClass(`chat-tab-${newTabId}`)

                $tabBar.children().removeClass("active")
                $tabBar.children(`[data-gob-tab-id=${newTabId}]`).addClass("active")
            }

            function restoreTabSettings(tabId) {
                if (tabId in tabData) {
                    const data = tabData[tabId]
                    if (data.scrollPosition < 0)
                        scrollControl.scrollToBottom(true)
                    else
                        scrollControl.scrollToPosition(data.scrollPosition, true)
                }
            }

            // remove old tabs data
            {
                const unusedTabs = Object.keys(tabData).filter(id => !_.includes(tabIds, id))
                unusedTabs.forEach(id => delete tabData[id])
            }

            // add new tabs data
            {
                const activeTabIds = Object.keys(tabData)
                const newTabIds = tabIds.filter(id => !_.includes(activeTabIds, id))
                newTabIds.forEach(id => tabData[id] = {
                    scrollPosition: -1 //a number below 0 means scroll to bottom on activation
                })
            }

            //rebuild buttons in the correct order
            {
                $tabBar.empty()
                tabIds.forEach(id => {
                    $("<button/>")
                        .appendTo($tabBar)
                        .attr("data-gob-tab-id", id)
                        .addClass("chat-tabnav-btn")
                        .on("click", function () {
                            const tabId = $(this).attr("data-gob-tab-id")
                            activateChatTab(tabId)
                        })
                        .append(
                            $("<span></span>").html(Gobchat.encodeHtmlEntities(tabsModel[id].name))
                        )
                })
            }

            // check if any tab is active, otherwise activate the first in order
            {
                let activeTab = $chatPanel.attr("data-gob-activetab") || ""
                if (!(activeTab in tabData) && tabIds.length > 0) {
                    activeTab = tabIds[0]
                }
                //ensure everything is set
                activateChatTab(activeTab)
            }
        }

        _onNewMessageEvent(event) {
            const messageCollection = event.detail
            if (!messageCollection) return
            for (let message of messageCollection.messages) {
                this._onNewMessage(message)
            }
        }

        _onNewMessage(message) {
            if (this._hideInfo && message.channel === Gobchat.ChannelEnum.GOBCHATINFO)
                return

            if (this._hideError && message.channel === Gobchat.ChannelEnum.GOBCHATERROR)
                return

            if (message.channel === Gobchat.ChannelEnum.ECHO) {
                const msgComplete = message.content.map(e => e.text).join()
                this._cmdManager.processCommand(msgComplete)
            }

            const messageHtmlElement = this._messageHtmlBuilder.buildHtmlElement(message)
            getChatPanel(this._$target).append(messageHtmlElement)
            this._scrollControl.scrollToBottomIfNeeded()

            this._messageSound.checkForSound(message)
        }
    }
    Gobchat.ChatboxManager = ChatboxManager

    class ScrollControl {
        constructor() {
            this._scrollToBottom = true
            //this._lastScrollPosition = 0

            const control = this
            this._$onScroll = function () {
                const $this = $(this)
                const closeToBottom = ($this.scrollTop() + $this.innerHeight() + 5 >= $this[0].scrollHeight) // +5px for 'being very close'
                //control._lastScrollPosition = $this.scrollTop()
                control._scrollToBottom = closeToBottom
            }
        }

        control(target) {
            if (this._$target)
                this._unbindTarget()
            this._bindTarget(target)
        }

        get isScrolledToBottom() {
            return this._scrollToBottom
        }

        get scrollPosition() {
            return $(this._$target).scrollTop()
        }

        scrollToPosition(position, scrollFast) {
            if (scrollFast) {
                this._$target.scrollTop(position)
            } else {
                this._$target.animate({
                    scrollTop: position
                }, 10);
            }
        }

        scrollToBottom(scrollFast) {
            const $target = this._$target
            const targetPosition = $target[0].scrollHeight - $target[0].clientHeight
            this.scrollToPosition(targetPosition, scrollFast)
        }

        scrollToBottomIfNeeded(scrollFast) {
            if (this.isScrolledToBottom)
                this.scrollToBottom(scrollFast)
        }

        _bindTarget(target) {
            if (!target) return
            this._$target = $(target)
            this._$target.on("scroll", this._$onScroll)
        }

        _unbindTarget() {
            if (!this._$target) return
            this._$target.off("scroll", this._$onScroll)
            this._$target = null
        }
    }

    class TabControl {
        constructor(tabId) {
            if (tabId === null || tabId === undefined)
                throw new Error("Id can't be null")

            this._id = tabId

            this._onStackedPanelChange = function (evt) {
            }.bind(this)
        }

        get isActive() {
            return this._$panel.hasClass("active")
        }

        showTab() {
            if (this._id === null || this._id === undefined)
                throw new Error("Object disposed")
            this._stackedPanel.showPanel(this._id)
            this._scrollControl.scrollToBottomIfNeeded(true)
        }

        remove() {
            //$(this._stackedPanel).off("change", this._onStackedPanelChange)

            this._id = null
            this._$button.remove()
            this._$panel.remove()
            this._stackedPanel = null
        }

        controls(stackedPanel, tabButton) {
            if (this._id === null || this._id === undefined)
                throw new Error("Object disposed")

            this._stackedPanel = stackedPanel
            //$(stackedPanel).on("change", this._onStackedPanelChange)

            this._$button = $(tabButton)

            const panel = stackedPanel.getPanel(this._id)
            if (!panel)
                throw new Error("Panel not defined")
            this._$panel = $(panel)

            this._scrollControl = new ScrollControl()
            this._scrollControl.control(this._$panel)
        }

        acceptsMessage(message) {
            const model = gobconfig.get(`behaviour.chattabs.data.${this._id}`)
            return _.includes(model.channel.visible, message.channel)
        }

        addMessage(htmlMessage) {
            this._$panel.append(htmlMessage)
            this._scrollControl.scrollToBottomIfNeeded()
        }
    }

    return Gobchat
}(Gobchat || {}));