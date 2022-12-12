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

const cssMarkedbySearch = "gob-chat_entry--marked-by-search"
const cssActiveSelection = "gob-chat_entry--selected-by-search"
const cssChatMessage = "gob-chat_entry"

const selector_chat_history = ".js-history"

const selector_input = "> .js-input"
const selector_counter = "> .js-counter"
const selector_up = "> .js-up"
const selector_down = "> .js-down"
const selector_reset = "> .js-reset"
const selector_markedBySearch = `.${cssMarkedbySearch}`
const selector_activeSelection = `.${cssActiveSelection}`
const selector_visible_messages = `.${cssChatMessage}:visible`

export class ChatSearch {
    #searchElement = null
    #chatElement = null

    constructor() {
    }

    control($searchElement, $chatElement) {
        if (this.#searchElement !== null) {
            this.#searchElement.find(selector_input).off("keyup", this.#onInputEnter)
            this.#searchElement.find(selector_counter).off("click", this.scrollToSelection)
            this.#searchElement.find(selector_up).off("click", this.moveSelectionUp)
            this.#searchElement.find(selector_down).off("click", this.moveSelectionDown)
            this.#searchElement.find(selector_reset).off("click", this.clearSearch)        
        }

        if (this.#chatElement !== null)
            this.#removeMessageMarkers()
        
        this.#searchElement = $searchElement.first()
        this.#chatElement = $chatElement.find(selector_chat_history).addBack(selector_chat_history).first()

        this.#searchElement.find(selector_input).on("keyup", this.#onInputEnter)
        this.#searchElement.find(selector_counter).on("click", this.scrollToSelection)
        this.#searchElement.find(selector_up).on("click", this.moveSelectionUp)
        this.#searchElement.find(selector_down).on("click", this.moveSelectionDown)
        this.#searchElement.find(selector_reset).on("click", this.clearSearch)        
    }

    #onInputEnter = (event) => {
        if (event.keyCode === 13) // enter
            this.startSearch()
    }

    #updateCounterValue = () => {
        const $markedMessages = this.#chatElement.find(selector_markedBySearch)
        const $activeSelection = this.#chatElement.find(selector_activeSelection)
        const max = $markedMessages.length
        const current = max > 0 ? $markedMessages.index($activeSelection) : 0
        this.#searchElement.find(selector_counter).text(`${max - current} / ${max}`)
    }

    #removeMessageMarkers = () => {
        this.#chatElement.find(selector_markedBySearch).removeClass(cssMarkedbySearch)
        this.#chatElement.find(selector_activeSelection).removeClass(cssActiveSelection)
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
        if (value === this.visible)
            return

        if (value) {
            this.#searchElement.show()
            this.#searchElement.find(selector_input).focus()
        } else {
            this.#searchElement.hide()
            this.clearSearch()
        }
    }

    clearSearch = () => {        
        this.#removeMessageMarkers()
        this.#searchElement.find(selector_input).val("").focus()     
    }

    scrollToSelection = () => {
        const $selectedElement = this.#chatElement.find(selector_activeSelection)
        if ($selectedElement.length === 0)
            return

        const selectedElement = $selectedElement[0]
        const scrollableFrame = this.#chatElement[0]

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

        this.#chatElement.find(selector_visible_messages).each(function () {
            if ($(this).text().toLowerCase().indexOf(text) >= 0)
                $(this).addClass(cssMarkedbySearch)
        })

        const $markedMessages = this.#chatElement.find(selector_markedBySearch)

        if ($markedMessages.length === 0) {
            this.#updateCounterValue()
        } else {
            $markedMessages.last().addClass(cssActiveSelection)
            this.#updateCounterValue()
            this.scrollToSelection()
        }
    }

    startSearch = () => {
        this.search(this.#searchElement.find(selector_input).val())
    }

    moveSelectionUp = () => {
        const $activeSelection = this.#chatElement.find(selector_activeSelection)
        $activeSelection.removeClass(cssActiveSelection)

        let $prevMarked = $activeSelection.prevAll(selector_markedBySearch).first()

        if ($prevMarked.length === 0) //wrap around
            $prevMarked = this.#chatElement.find(selector_markedBySearch).last()

        $prevMarked.addClass(cssActiveSelection)

        this.#updateCounterValue()
        this.scrollToSelection()
    }

    moveSelectionDown = () => {
        const $activeSelection = this.#chatElement.find(selector_activeSelection)
        $activeSelection.removeClass(cssActiveSelection)

        let $nextMarked = $activeSelection.nextAll(selector_markedBySearch).first()

        if ($nextMarked.length === 0) //wrap around
            $nextMarked = this.#chatElement.find(selector_markedBySearch).first()

        $nextMarked.addClass(cssActiveSelection)

        this.#updateCounterValue()
        this.scrollToSelection()
    }
}

