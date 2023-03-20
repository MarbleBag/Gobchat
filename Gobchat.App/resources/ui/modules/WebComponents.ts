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

import * as Locale from '/module/Locale'
import * as Databinding from '/module/Databinding'

export default {}

function copyClassesToNode(node: HTMLElement, classes: string[]) {
    const oldClasses = [...node.classList]
    node.className = ""

    for (const clazz of classes)
        node.classList.toggle(clazz, true)

    for (const clazz of oldClasses)
        node.classList.toggle(clazz, true)
}

function replaceNodeWithChilds(node: HTMLElement) {
    const nodeParent = node.parentNode
    if (nodeParent === null)
        return

    if (node.childNodes.length === 0) {
        nodeParent.removeChild(node)
        return
    }

    const classCopy = [...node.classList]
    const childNodes = [...node.childNodes] as HTMLElement[]

    for (const childNode of childNodes)
        copyClassesToNode(childNode, classCopy)

    nodeParent.replaceChild(node, childNodes[0])
    const remainingChilds = childNodes.slice(1)
    childNodes[0].after(...remainingChilds)
}

function manageSlotsInLightDom(htmlElement: HTMLElement, template: HTMLTemplateElement) {
    const nodesWithId: HTMLElement[] = []
    const nodesWithoutId: HTMLElement[] = [] 

    for (const node of htmlElement.childNodes) {
        if (node instanceof HTMLElement) {
            if (node.hasAttribute("slot")) {
                nodesWithId.push(node)
            } else {
                nodesWithoutId.push(node)
            }
        }
    }

    for (const node of nodesWithId) {
        const slotId = node.getAttribute("slot")
        const slotNode = template.content.querySelector<HTMLSlotElement>(`slot[name=${slotId}]`)
        if (slotNode)
            slotNode.appendChild(node)
    }

    const allUnnamedSlots = template.content.querySelectorAll<HTMLSlotElement>('slot:not([name])')
    let currentUnnamedSlotIndex = 0

    for (const node of nodesWithoutId) {
        if (currentUnnamedSlotIndex >= allUnnamedSlots.length)
            break // no more 'any' slots available

        const slotNode = allUnnamedSlots[currentUnnamedSlotIndex]        
        slotNode.appendChild(node)

        const limit = getSlotLimit(slotNode)
        if (slotNode.childNodes.length >= limit)
            currentUnnamedSlotIndex += 1
    }

    for (const replaceableSlots of [...template.content.querySelectorAll<HTMLSlotElement>("slot[replaceable]")]) {
        replaceNodeWithChilds(replaceableSlots)
    }

    function getSlotLimit(node: HTMLElement): number {
        const limitAttribute = node.getAttribute("limit")
        if (limitAttribute !== null) {
            const limit = parseInt(limitAttribute)
            if (limit < 1 || isNaN(limit))
                throw new Error(`Slot limit needs to be set to a positive integer. '${limitAttribute}' is not a valid limit.`)
            return limit
        }
        return currentUnnamedSlotIndex + 1 === allUnnamedSlots.length ? Infinity : 1
    }

    htmlElement.replaceChildren(...template.content.childNodes)
}

function getAndDeleteAttribute(htmlElement: HTMLElement, attributeName: string): string | null {
    const value = htmlElement.getAttribute(attributeName)
    htmlElement.removeAttribute(attributeName)
    return value
}

function setAttributeIfAvailable(htmlElement: HTMLElement, attributeName: string, attributeValue: string | null) {
    if (attributeValue) {
        htmlElement.setAttribute(attributeName, attributeValue)
        return true
    }
    return false
}

/*
interface CustomWebElement {
    connectedCallback()
    disconnectedCallback()
    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string)
    static observedAttributes: string[]
}
*/

function clearDatabinding(element: BindableCustomElement & HTMLElement) {
    if (element.databinding)
        element.databinding.clearBindings()

    if (!element.hasAttribute("config-key"))
        element.databinding = null
}

function setupDatabinding(element: BindableCustomElement & HTMLElement) {
    if (element.databinding)
        element.databinding.clearBindings()

    if (element.hasAttribute("config-key")) {
        element.databinding = new Databinding.BindingContext(gobConfig)
    } else {
        element.databinding = null
    }
}

function handleDatabindOnDisconnect(element: HTMLElement & BindableCustomElement) {
    if (element.databinding) {
        element.databinding.clearBindings()
        if (element.getAttribute("config-key") === null)
            element.databinding = null
    }
}

function handleDatabindOnConnect(element: HTMLElement & BindableCustomElement, onBind: (key: string) => void) {
    const key = element.getAttribute("config-key")
    if (key === null) {
        if (element.databinding) {
            element.databinding.clearBindings()
            element.databinding = null
        }
    } else {
        if (element.databinding) {
            if (!element.databinding.isLoaded) {
                onBind(key)
                element.databinding.loadBindings()
            }
        } else {
            element.databinding = new Databinding.BindingContext(gobConfig)
            onBind(key)
            element.databinding.loadBindings()
        }
    }
}

function handleDatabindOnKeyChange(element: HTMLElement & BindableCustomElement, onBind: (key: string) => void) {
    const key = element.getAttribute("config-key")
    if (key === null) {
        if (element.databinding) {
            element.databinding.clearBindings()
            element.databinding = null
        }
    } else {
        if (element.databinding) {
            element.databinding.clearBindings()
            onBind(key)
            if (element.isConnected)
                element.databinding.loadBindings()
        } else {
            element.databinding = new Databinding.BindingContext(gobConfig)
            onBind(key)
            element.databinding.loadBindings()
            if (element.isConnected)
                element.databinding.loadBindings()
        }
    }
}

interface HTMLElementExtension {
    connectedCallback(): void 
    disconnectedCallback(): void 
    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): void 
}

interface BindableCustomElement {
    databinding: Databinding.BindingContext | null
}

class BaseCustomElement extends HTMLElement {
    static AttributePartialId = "partial-id"
    static AttributeConfigKey = "config-key"
    static AttributeLocaleLabel = "locale-label"
    static AttributeLocaleTitle = "locale-tooltip"
    static ObservedDefaultAttributes = [this.AttributePartialId]

    updatePartialId() {
        if (this.hasAttribute(BaseCustomElement.AttributePartialId)) {
            const parent = this.closest("[id]")
            if (parent) {
                const parentId = parent.id
                this.id = `${parentId}_${this.getAttribute(BaseCustomElement.AttributePartialId)}`
            }
        }
    }

    connectedCallback() {
        this.updatePartialId()
    }

    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): boolean {
        switch (attributeName) {
            case BaseCustomElement.AttributePartialId:
                this.updatePartialId()
                return true
        }
        return false
    }
}


class LabeledCheckbox extends BaseCustomElement implements BindableCustomElement {
    static get observedAttributes() { return ["config-key", "locale-label", "locale-tooltip", ...BaseCustomElement.ObservedDefaultAttributes] }

    #inputElement: HTMLInputElement
    #labelElement: HTMLLabelElement
    databinding: Databinding.BindingContext | null = null

    constructor() {
        super()
        //manageSlotsInLightDom(this, GCCheckbox.#template.cloneNode(true) as HTMLTemplateElement)

        this.classList.toggle("gob-config-group_item", true)
        this.innerHTML = ""

        this.#inputElement = document.createElement("input")
        this.#inputElement.type = "checkbox"
        this.#inputElement.classList.add("gob-input-checkbox")
        this.#inputElement.id = crypto.randomUUID()
        this.appendChild(this.#inputElement)

        this.#inputElement.addEventListener("change", (ev) => {
            ev.stopPropagation()
            const changeEvent = new Event("change")
            this.dispatchEvent(changeEvent)
        })

        this.#labelElement = document.createElement("label")
        this.#labelElement.classList.add("gob-config-label")
        this.#labelElement.htmlFor = this.#inputElement.id
        this.appendChild(this.#labelElement)
    }

    get checked(): boolean {
        return this.#inputElement.checked
    }

    set checked(value: boolean) {
        this.#inputElement.checked = value
    }

    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): boolean {
        const isHandled = super.attributeChangedCallback(attributeName, oldValue, newValue)
        if (isHandled)
            return true

        switch (attributeName) {
            case "config-key":
                handleDatabindOnKeyChange(this, this.#onBind)
                break

            case "locale-label":
                setAttributeIfAvailable(this.#labelElement, Locale.HtmlAttribute.TextId, newValue)
                gobLocale.updateElement(this.#labelElement)
                break

            case "locale-tooltip":
                if (newValue === null) {
                    this.#inputElement.removeAttribute(Locale.HtmlAttribute.TooltipId)
                    this.#labelElement.removeAttribute(Locale.HtmlAttribute.TooltipId)
                } else {
                    if (newValue.length === 0 && this.hasAttribute("locale-label"))
                        newValue = this.getAttribute("locale-label") + ".tooltip"

                    this.#inputElement.setAttribute(Locale.HtmlAttribute.TooltipId, newValue)
                    this.#labelElement.setAttribute(Locale.HtmlAttribute.TooltipId, newValue)

                    gobLocale.updateElement(this.#inputElement)
                    gobLocale.updateElement(this.#labelElement)
                }
                break
        }

        return true
    }

    #onBind = (key: string) => {
        Databinding.setConfigKey(this.#inputElement, key)
        Databinding.bindCheckbox(this.databinding!, this.#inputElement)
    }

    connectedCallback() {
        super.connectedCallback()
        handleDatabindOnConnect(this, this.#onBind)
    }

    disconnectedCallback() {
        handleDatabindOnDisconnect(this)
    }
}
customElements.define("config-checkbox", LabeledCheckbox)

namespace LocalizedElement2 {
    export const ObservedAttributes = ["A"]
}

const LocalizedElement = function (Superclass) {
    return class extends Superclass implements HTMLElementExtension {
        constructor() {
            super()
        }
        connectedCallback(): void {
            if (Superclass.connectedCallback)
                Superclass.connectedCallback()
            gobLocale.addLocaleChangeListener(this.#onLocaleChange)
        }

        disconnectedCallback(): void {
            if (Superclass.disconnectedCallback)
                Superclass.disconnectedCallback()
            gobLocale.removeLocaleChangeListener(this.#onLocaleChange)
        }

        attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): void {
            if (Superclass.attributeChangedCallback)
                Superclass.attributeChangedCallback(attributeName, oldValue, newValue)


        }

        #onLocaleChange = () => {

        }
    }
}

class Checkbox extends HTMLElement {
    static get observedAttributes() { return ["config-key", "locale-tooltip"] }


    constructor() {
        super()
    }

    connectedCallback(): void {

    }

    disconnectedCallback(): void {

    }

    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): void {

    }
}

class ConfigGroup extends HTMLElement {
    static #template = document.createElement("template")
    static {
        this.#template.innerHTML = `
<div class="gob-config-group_title"></div>
<slot replaceable></slot>
`
    }

    constructor() {
        super()
        const template = ConfigGroup.#template.cloneNode(true) as HTMLTemplateElement

        this.classList.toggle("gob-config-group", true)

        const localeTitle = getAndDeleteAttribute(this, "locale-groupname")
        if (localeTitle) {
            template.content.querySelector(".gob-config-group_title")!.setAttribute(Locale.HtmlAttribute.TextId, localeTitle)
        } else {
            template.content.querySelector(".gob-config-group_title")!.setAttribute("hidden", "")
        }

        manageSlotsInLightDom(this, template)
    }
}
customElements.define("config-group", ConfigGroup)

class ConfigPage extends HTMLElement {
    static #template = document.createElement("template")
    static {
        this.#template.innerHTML = `
<slot replaceable></slot>
`
    }

    constructor() {
        super()
        manageSlotsInLightDom(this, ConfigPage.#template.cloneNode(true) as HTMLTemplateElement)
        this.classList.toggle("gob-config-page", true)
    }
}
customElements.define("config-page", ConfigPage)

class ConfigCopyPageButton extends BaseCustomElement {
    static get observedAttributes() { return BaseCustomElement.ObservedDefaultAttributes }

    constructor() {
        super()
        this.classList.add("gob-config-copypage-button")
        this.innerHTML = '<i class="fas fa-clone"></i>'
        // this.innerHTML = `<button class="gob-config-copypage-button"><i class="fas fa-clone"></i></button>`
    }

    connectedCallback() {
        super.connectedCallback()
    }

    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): boolean {
        const isHandled = super.attributeChangedCallback(attributeName, oldValue, newValue)
        return isHandled
    }
}
customElements.define("config-copypage-button", ConfigCopyPageButton)

class ConfigResetButton extends BaseCustomElement {
    static get observedAttributes() { return BaseCustomElement.ObservedDefaultAttributes }

    constructor() {
        super()
        this.classList.add("gob-config-icon-button")
        this.innerHTML = '<i class="fas fa-undo-alt"></i>'

        /*
        this.innerHTML = 
`<button id="cp-app_chat-history_backgroundcolor_reset" class="gob-config-icon-button" data-gob-locale-title="config.main.button.reset.tooltip"
        data-gob-configkey="style.chatbox.background-color"
        >
    <i class='fas fa-undo-alt'></i>
</button>`
        this.classList.add("gob-config-copypage-button-wrapper")
        this.style.display = "inline"
*/
    }

    connectedCallback() {
        super.connectedCallback()
    }

    attributeChangedCallback(attributeName: string, oldValue: string, newValue: string): boolean {
        const isHandled = super.attributeChangedCallback(attributeName, oldValue, newValue)
        return isHandled
    }
}
customElements.define("config-reset-button", ConfigResetButton)