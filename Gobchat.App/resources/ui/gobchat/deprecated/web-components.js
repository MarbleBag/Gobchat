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

// Custom element 'stacked-panel'
// Will only show one of its children at any time and hides everything else. The child can either be specified via id or index.
(function () {
    function getActiveElement($base) {
        return $base.children("active:visible")
    }

    function getElementIdx($element) {
        if ($element.length > 0)
            return $element.index()
        return null
    }

    function getElementId($element) {
        const id = $element.attr("data-gob-panel-id")
        if (id === undefined)
            return null
        return id
    }

    function getElementIdxOrId($element) {
        const id = getElementId($element)
        if (id !== null)
            return id
        return getElementIdx($element)
    }

    function isElementActive($active, idxOrId) {
        if (idxOrId === null)
            return false
        if ($active.length === 0)
            return false

        if (isFinite(idxOrId))
            return getElementIdx($active) === idxOrId
        else
            return getElementId($active) === idxOrId
    }

    window.customElements.define("stacked-panel", class extends HTMLElement {
        constructor() {
            super();
        }

        showPanel(idxOrId) {
            const $this = $(this)

            //hide non active childs, in case some where added since the last time this method ran
            $this.children().not(".active").hide()

            const $activePanel = getActiveElement($this)
            if (isElementActive($activePanel, idxOrId))
                return //nothing more to do

            //remove active class from all childs - normally there should just be one, but the html can be modified everywhere
            $this.children(".active")
                .removeClass("active")
                .hide()

            const $target = this.getPanel(idxOrId)
            if ($target)
                $target.addClass("active").show()

            const changeEvent = new Event("change", { cancelable: false, target: this })
            this.dispatchEvent(changeEvent)
        }

        // returns the active panel or null, if there is no active element
        getActivePanel() {
            return getActiveElement($(this))
        }

        // returns an index or null, if there is no active element
        getActivePanelIdx() {
            return getElementIdx(getActiveElement($(this)))
        }

        // returns an id or null, if there is no active element or the active element has no id
        getActivePanelId() {
            return getElementId(getActiveElement($(this)))
        }

        // returns an id or an index, if the active element has no id or null, if there is no active element
        getActivePanelIdxOrId() {
            return getElementIdxOrId(getActiveElement($(this)))
        }

        getPanel(idxOrId) {
            let $target = null

            if (isFinite(idxOrId))
                $target = $(this).children().eq(idxOrId)
            else
                $target = $(this).children(`[data-gob-panel-id=${idxOrId}]`)

            return $target
        }

        getAllPanelIds() {
            const results = []
            $(this).children("[data-gob-panel-id]").each(function () {
                results.push($(this).attr("data-gob-panel-id"))
            })
            return results
        }

        getChildCount() {
            return $(this).children().length
        }

        showFirstPanel() {
            this.showPanel(0)
        }

        updatePanel() {
            const idx = $(this).children(".active").index()
            this.showPanel(idx || 0)
        }

        connectedCallback() {
            this.updatePanel()
        }

        disconnectedCallback() {
        }
    });
}());

(function () {
    function getPanelId(element) {
        return $(element).attr("data-panel-id")
    }

    function getParent(element) {
        const $parent = $(element).parent()
        if ($parent.length === 0)
            return null
        return $parent[0]
    }

    window.customElements.define("cm-panel", class extends HTMLElement {
        constructor() {
            super()

            this._isConnected = false
            this._controls = []
        }

        connectedCallback() {
            this._isConnected = true
            this._controls.forEach(c => activateControl(this, c))
        }

        disconnectedCallback() {
            this._isConnected = false
            this._controls.forEach(c => deactivateControl(this, c))
        }

        addControl(control) {
            if (!control)
                return
            this._controls.push(control)

            initializeControl(this, control)
            if (this._isConnected)
                activateControl(this, control)
        }

        removeControl(control) {
            if (!control)
                return
            const idx = this._controls.indexOf(control)
            if (idx < 0)
                return
            this._controls.splice(idx)
            deactivateControl(this, control)
            deinitializeControl(this, control)
        }

        activate() {
            const parent = getParent(this)
            if (parent && parent.showPanel) {
                parent.showPanel(getPanelId(this))
            }
        }

        remove() {
            const parent = getParent(this)
            $(this).remove()
            if (parent && parent.updatePanel)
                parent.updatePanel()
        }

        get isActive() {
            return $(this).hasClass("active")
        }
    })
}());

// input field with drop down
(function () {
    const template = `
        <div class="wc-si-options" style="display:none;">
            <slot></slot>
        </div>
    `

    const template2 = `
        <input  class="wc-si-vl" type="text"/>
        <button class="wc-si-drop"></button>
    `

    window.customElements.define("select-input", class extends HTMLElement {
        constructor() {
            super()

            /*this.innerHTML = `
                <select class="si-sl"></select>
                <input  class="si-vl" type="text"/>
                <input  class="si-id" type="hidden"/>
            `*/
        }

        connectedCallback() {
            console.log("U no work?")
            this.innerHTML = template2
        }

        disconnectedCallback() {
        }
    })
}());