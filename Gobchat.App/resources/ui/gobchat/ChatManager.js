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

            this._chatControl = new ChatTabControl(this._databinding)
            this._chatControl.controls(
                {
                    navBar: this._$target.find(".js-chattabs"),
                    navPanel: this._$target.find(".js-chat-history")
                }
            )

            this.updateStyle() //TODO

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
            this._chatControl.scrollToBottomIfNeeded()
        }

        showGobInfo(value) {
            this._hideInfo = !value
        }

        showGobError(value) {
            this._hideError = !value
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

            this._chatControl.getChatPanel().append(messageHtmlElement)
            this._chatControl.scrollToBottomIfNeeded()
            this._chatControl.setButtonAnimation(message.channel, message.containsMentions)

            this._messageSound.checkForSound(message)
        }
    }
    Gobchat.ChatboxManager = ChatboxManager

    class ChatTabControl {
        // this class was not written with the intention to be ever removed from the stack

        constructor(databinding) {
            if (!databinding)
                throw new Error("'databinding' can't be null")

            this._$navBar = $()
            this._$navPanel = $()

            this._navPanelData = {}
            this._channelToTabMap = {}

            this._tabEffect = {
                message: null,
                mention: null
            }

            this._navBarControl = new TabBarControl()
            this._scrollControl = new ScrollControl()

            const control = this
            this._$onTabActivation = function (evt) {
                control._switchToTab(evt.detail.oldValue, evt.detail.newValue)
            }

            GobConfigHelper.bindListener(databinding, "behaviour.chattabs", config => this._updateNavbarContent(config))
            GobConfigHelper.bindListener(databinding, "behaviour.chattabs.data", data => this._buildChannelToTabMap(data))
            GobConfigHelper.bindListener(databinding, "behaviour.chattabs.data", data => this._updateNavPanelData(data))
            GobConfigHelper.bindListener(databinding, "behaviour.chattabs.effect", data => this._updateTabEffects(data))
        }

        setButtonAnimation(channel, hasMention) {
            const effectMessage = this._tabEffect.message
            const effectMention = this._tabEffect.mention

            const affectedTabs = this._channelToTabMap[channel] || []
            const activeTabId = this._navBarControl.getActiveTabId()
            if (_.includes(affectedTabs, activeTabId))
                return // done, message was visible on the active tab, don't apply any effects

            affectedTabs.forEach(id => {
                const $tabs = this._navBarControl.getTab(id)
                    .filter(":not(.active)")

                if ($tabs.length === 0)
                    return

                if (hasMention && effectMention) {
                    $tabs
                        .filter(`:not(.${effectMention})`)
                        .removeClass(effectMessage)
                        .addClass(effectMention)
                        .on("click.tab.effects.mention", function () {
                            $(this).off("click.tab.effects.mention")
                                .removeClass(effectMention)
                        })
                    return //done
                }

                if (effectMessage) {
                    $tabs
                        .filter(`:not(.${effectMessage})`)
                        .filter(`:not(.${effectMention})`)
                        .addClass(effectMessage)
                        .on("click.tab.effects.message", function () {
                            $(this).off("click.tab.effects.message")
                                .removeClass(effectMessage)
                        })
                    return //done
                }
            })
        }

        getChatPanel() {
            return this._$navPanel
        }

        scrollToBottomIfNeeded() {
            this._scrollControl.scrollToBottomIfNeeded()
        }

        controls({ navBar, navPanel }) {
            if (this._initialized)
                this._unbindControls()

            this._initialized = true

            this._$navPanel = $(navPanel)
            this._$navBar = $(navBar)

            this._$navBar.on("change", this._$onTabActivation)

            this._scrollControl.controls(this._$navPanel)
            this._navBarControl.controls(this._$navBar)
        }

        _unbindControls() {
            if (this._$navBar)
                this._$navBar.off("change", this._$onTabActivation)

            this._$navBar = $()
            this._$navPanel = $()

            this._scrollControl.controls(null)
            this._navBarControl.controls(null)

            this._navPanelData = {}

            this._initialized = false
        }

        _switchToTab(deactivateId, activateId) {
            // store old scroll position
            if (deactivateId in this._navPanelData) {
                const data = this._navPanelData[deactivateId]
                data.scrollPosition = this._scrollControl.isScrolledToBottom ? -1 : this._scrollControl.scrollPosition
            }

            // set css class for formatting
            this._$navPanel
                .removeClass(`chat-tab-${deactivateId}`)
                .addClass(`chat-tab-${activateId}`)

            // restore new scroll position
            if (activateId in this._navPanelData) {
                const data = this._navPanelData[activateId]
                if (data.scrollPosition < 0)
                    this._scrollControl.scrollToBottom(true)
                else
                    this._scrollControl.scrollToPosition(data.scrollPosition, true)
            }
        }

        _buildChannelToTabMap(tabModels) {
            this._channelToTabMap = {}

            Object.entries(tabModels).forEach(e => {
                const model = e[1]
                if (!model.visible)
                    return

                model.channel.visible.forEach(c => {
                    if (c in this._channelToTabMap) {
                        this._channelToTabMap[c].push(model.id)
                    } else {
                        this._channelToTabMap[c] = [model.id]
                    }
                })
            })
        }

        _updateTabEffects(effects) {
            this._tabEffect.message = effects.message > 0 ? "tab-effect-message" : null
            this._tabEffect.mention = effects.mention > 0 ? "tab-effect-mention" : null
        }

        _updateNavPanelData(tabModels) {
            const storedData = this._navPanelData;

            // remove old tabs data
            {
                const unusedTabs = Object.keys(storedData).filter(id => !(id in tabModels))
                unusedTabs.forEach(id => delete storedData[id])
            }

            // add new tabs data
            {
                const newTabIds = Object.keys(tabModels).filter(id => !(id in storedData))
                newTabIds.forEach(id => storedData[id] = {
                    scrollPosition: -1 //a number below 0 means scroll to bottom on activation
                })
            }
        }

        _updateNavbarContent(config) {
            const models = config["data"]
            const sorting = config["sorting"]

            const entries = sorting
                .filter(id => models[id].visible)
                .map(id => { return { id: id, name: models[id].name } })

            this._navBarControl.updateTabs(entries)
        }
    }

    // Controls the content of a Tabbar, scrolling and animation.
    // The controlled tabbar element will fire a 'change' event on any button press
    class TabBarControl {
        constructor() {
            this._initialized = false

            this._activeId = null
            this._$element = $()
            this._$btnPanel = $()

            const control = this

            this._$onWheel = function (evt) {
                evt = evt.originalEvent
                control._scrollTab(evt.deltaY > 0 ? 1 : -1)
            } //turns a vertical scroll into horizontal

            this._$onBtnScroll = function (evt) {
                control._scrollTab($(this).hasClass("js-nav-right") ? 1 : -1)
            }
        }

        controls(element) {
            if (this._initialized)
                this._unbindControls()

            if (!element)
                return

            this._initialized = true
            this._$element = $(element)
            this._$btnPanel = $(element).find(".js-nav-content")

            this._$element.on("wheel", this._$onWheel)
            this._$element.find(".js-nav-left,.js-nav-right").on("click", this._$onBtnScroll)
        }

        getTab(idOrIndx) {
            return this._getTab(idOrIndx)
        }

        removeTab(id) {
            if (this._removeTab(id) && this.getActiveTabId() === id)
                this.activateTab(0)
        }

        removeAllTabs() {
            this._removeAllTabs()
            this.activateTab(0)
        }

        updateTabLabel(id, name) {
            this._$btnPanel.find(`[data-gob-tab-id=${id}] > span`).html(Gobchat.encodeHtmlEntities(name))
        }

        updateTabs(entries) {
            const activeTabId = this.getActiveTabId()

            // while more complex then a removeAll() and readding tabs, this preserves all changes to the tab buttons, including any animations

            const updateIds = entries.map(e => e.id)
            const allTabs = this.getAllTabIds()
            const tabsToRemove = allTabs.filter(id => !_.includes(updateIds, id))

            entries.forEach(entry => {
                if (_.includes(allTabs, entry.id))
                    this.updateTabLabel(entry.id, entry.name)
                else
                    this._addTab(entry.id, entry.name)
            })

            tabsToRemove.forEach(id => this._removeTab(id))

            this.sortTabs(updateIds)

            // this._removeAllTabs()
            // entries.forEach(entry => this._addTab(entry.id, entry.name))

            // if there was no active tab or it can't be reactivated, try to activate the first one
            if (activeTabId === null || !this.activateTab(activeTabId))
                this.activateTab(0)

            this._scrollTab(0)
        }

        getAllTabIds() {
            const result = []
            this._$btnPanel.children().each(function () {
                const id = $(this).attr("data-gob-tab-id")
                result.push(id)
            })
            return result
        }

        getTabCount() {
            return this._$btnPanel.children().length
        }

        getActiveTabId() {
            return this._activeId
        }

        // activates the tab with the given id or index, returns true if at least one tab can be activated
        activateTab(idOrIdx) {
            const $btn = this._getTab(idOrIdx)
            this._activateTab($btn)
            return $btn.length > 0
        }

        sortTabs(ids) {
            const $tabBar = this._$btnPanel
            const $buttons = $tabBar.children().detach()
            const lookup = {}
            $buttons.each(function () {
                const id = $(this).attr("data-gob-tab-id")
                lookup[id] = $(this)
            })

            ids.forEach(id => {
                const $btn = lookup[id]
                if ($btn) {
                    $btn.appendTo($tabBar)
                    delete lookup[id]
                }
            })

            //Add unsorted btns to the end
            Object.entries(lookup).forEach(e => $tabBar.append(e[1]))
        }

        //not perfect, it would be nice to be able to scroll to the 'next' element or to scroll an element into view
        _scrollTab(direction) {
            const $btnPanel = this._$btnPanel
            const $element = this._$element
            let position = $btnPanel.scrollLeft()

            const scrollDistance = Math.max(10, (this._$btnPanel.width() / 2))

            if (direction < 0)
                position -= scrollDistance
            else if (direction > 0)
                position += scrollDistance

            $btnPanel.animate({
                scrollLeft: position
            }, 50);

            {
                const isAtLeftBorder = position <= 0
                $element.find(".js-nav-left")
                    .toggleClass("disabled", isAtLeftBorder)
            } // update left button

            {
                const scrollWidth = $btnPanel.prop("scrollWidth") || 0
                const clientWidth = $btnPanel.prop("clientWidth") || 0
                const isAtRightBorder = (scrollWidth - clientWidth) <= position
                $element.find(".js-nav-right")
                    .toggleClass("disabled", isAtRightBorder)
            } // update right button
        }

        _getTab(idOrIdx) {
            if (isFinite(idOrIdx))
                return this._$btnPanel.children().eq(idOrIdx)
            else
                return this._$btnPanel.find(`[data-gob-tab-id=${idOrIdx}]`)
        }

        _removeAllTabs() {
            this._$btnPanel.empty()
        }

        _addTab(id, name) {
            const $buttons = this._$btnPanel
            const control = this

            if ($buttons.find(`[data-gob-tab-id=${id}]`).length > 0)
                throw new Error(`Id ${id} already in use`)

            $("<button/>")
                .appendTo($buttons)
                .attr("data-gob-tab-id", id)
                .addClass("chat-tabnav-btn")
                .append(
                    $("<span></span>").html(Gobchat.encodeHtmlEntities(name))
                )
                .on("click", function () {
                    control._activateTab(this)
                }) //setup click event
        }

        _removeTab(id) {
            return this._$btnPanel.find(`[data-gob-tab-id=${id}]`)
                .remove()
                .length > 0
        }

        _unbindControls() {
            if (this._$btnPanel) {
                this._$btnPanel.off("wheel", this._$onWheel)
                this.removeAllTabs()
            }

            if (this._$element) {
                this._$element.find(".js-nav-left,.js-nav-right").off("click", this._$onBtnScroll)
            }

            this._activeId = null
            this._$element = $()
            this._$btnPanel = $()
            this._initialized = false
        }

        _activateTab(btn) {
            this._$btnPanel.find(".active")
                .removeClass("active")

            const $newActive = $(btn)
                .first()
                .addClass("active")

            const lastId = this._activeId
            const newId = $newActive.attr("data-gob-tab-id") || null
            this._activeId = newId

            if (newId !== lastId)
                this._dispatchActivation(newId, lastId)
        }

        _dispatchActivation(activeTabId, previousTabId) {
            const evt = new Event("change")
            evt.detail = { oldValue: previousTabId, newValue: activeTabId }
            this._$element[0].dispatchEvent(evt)
        }
    }

    // manages vertical scroll of the chat history
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

        controls(target) {
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
            const targetPosition = $target.prop("scrollHeight") - $target.prop("clientHeight")
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

    return Gobchat
}(Gobchat || {}));