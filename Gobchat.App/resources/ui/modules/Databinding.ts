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

// <reference path="./../types/jquery/jquery.d.ts" />

'use strict'

import * as Utility from './CommonUtility.js'
import * as Config from './Config.js'

export module HtmlAttribute {
    export const ConfigKey = "data-gob-configkey"
}

export function getConfigKey(element: HTMLElement | JQuery): string | null {
    /*
    const key = $(element).attr(HtmlAttribute.ConfigKey)
    if (typeof key === 'string')
        return key
    return null
    */

    if (Utility.isjQuery(element)) {
        if (element.length === 0)
            throw new Error("Invalid element. No element found")
        if (element.length > 1)
            throw new Error("Invalid element. More than one element found")

        const key = element.attr(HtmlAttribute.ConfigKey)
        if (key === null || key === undefined)
            return null

        return key.toString()
    } else {
        if (!element)
            throw new Error("Invalid element. No element found")

        const key = element.getAttribute(HtmlAttribute.ConfigKey)
        return key
    }
}

export function setConfigKey(element: HTMLElement | JQuery, configKey: string): void {
    // return $(element).attr(HtmlAttribute.ConfigKey, configKey)

    if (Utility.isjQuery(element)) {
        element.attr(HtmlAttribute.ConfigKey, configKey)
    } else {
        element.setAttribute(HtmlAttribute.ConfigKey, configKey)
    }
}

/**
 * Adds or removes values from an array. Does not add duplicates.
 * @param {Array} array 
 * @param {Array} values 
 * @param {Boolean} doSet If true, adds values to array
 * @returns {Boolean} true, if the content of array changed
 */
function setValuesInArray<T>(array: T[], values: T[], doSet: boolean) {
    let changed = false

    if (doSet) {
        values.forEach(value => {
            if (!_.includes(array, value)) {
                array.push(value)
                changed = true
            }
        })
    } else {
        const removedElements =
            _.remove(array, arrayValue => {
                return _.includes(values, arrayValue)
            })

        changed = removedElements.length > 0
    }

    return changed
}

module BindingContext {
    export interface BindElementOptionTypes<T> {
        disabled: boolean
        elementKey: null | "change" | "keydown" | "keyup"
        configKey: string
        /* Called to retrieve a value from the element */
        elementToConfig: null | (($element: JQuery, event: any, storedValue: T) => T | undefined)
        /* Called to set a value on the element */
        configToElement: null | (($element: JQuery, storedValue: T) => void)
    }

    export interface ElementBindContext<T> extends BindElementOptionTypes<T> {
        onElementChange: null | (($event) => void)
        configListener: null | ConfigListener
        initializer: null | ((config: Config.GobchatConfig) => void)
    }
}

export type BindElementOptions<T> = Partial<BindingContext.BindElementOptionTypes<T>>

export class BindingContext {
    #bindings: Binding[]
    #config: Config.GobchatConfig
    #isInitialized: boolean

    constructor(gobConfig: Config.GobchatConfig) {
        this.#bindings = []
        this.#config = gobConfig
        this.#isInitialized = false
    }

    get isLoaded(): boolean {
        return this.#isInitialized
    }

    loadBindings(): BindingContext {
        const config = this.#config
        for (const binding of this.#bindings)
            binding.bind(config)

        this.#isInitialized = true
        return this
    }

    unloadBindings(): BindingContext {
        for (const binding of this.#bindings)
            binding.unbind()

        this.#isInitialized = false
        return this
    }

    clearBindings(): BindingContext {
        for (const binding of this.#bindings)
            binding.unbind()

        this.#bindings = []
        this.#isInitialized = false
        return this
    }

    bindElement(element: HTMLElement | JQuery, bindOptions?: BindElementOptions<any>): BindingContext {
        const $element = $(element)

        const defOptions: BindingContext.ElementBindContext<any> = {
            disabled: false,
            elementKey: "change",
            configKey: $element.attr(HtmlAttribute.ConfigKey)?.toString() || "",
            elementToConfig: ($element) => $element.val(),
            configToElement: ($element, value) => $element.val(value),
            onElementChange: null,
            initializer: null,
            configListener: null
        }

        const innerContext = { ...$.extend(defOptions, bindOptions) }

        if (innerContext.disabled || innerContext.configKey === null || innerContext.configKey.length === 0) {
            $element.addClass("is-disabled").prop("disabled", true)
            return this //done
        }

        if (innerContext.configToElement) {
            innerContext.initializer = config => innerContext.configToElement!($element, config.get(innerContext.configKey, null))
            innerContext.configListener = createConfigListener(innerContext.initializer, null, true)
        }

        const bind: Binding.OnBind = (config) => {
            // initialize
            if (innerContext.initializer)
                innerContext.initializer(config)

            if (innerContext.elementToConfig) {
                innerContext.onElementChange = (event) => {
                    const oldValue = config.get(innerContext.configKey, null)
                    const currentValue = innerContext.elementToConfig!($element, event, oldValue)
                    if (currentValue !== undefined)
                        config.set(innerContext.configKey, currentValue)
                }
            }

            // bind element
            if (innerContext.elementKey && innerContext.onElementChange)
                $element.on(innerContext.elementKey, innerContext.onElementChange)

            // bind config
            if (innerContext.configListener) {
                config.addProfileEventListener(innerContext.configListener)
                config.addPropertyEventListener(innerContext.configKey, innerContext.configListener)
            }
        }

        const unbind: Binding.OnUnbind = (config) => {
            // unbind element
            if (innerContext.elementKey && innerContext.onElementChange)
                $element.off(innerContext.elementKey, innerContext.onElementChange)

            innerContext.onElementChange = null

            // unbind config
            if (innerContext.configListener) {
                if (!config.removeProfileEventListener(innerContext.configListener))
                    console.log("Error: Databinding. Unable to remove profile listener")
                if (!config.removePropertyEventListener(innerContext.configKey, innerContext.configListener))
                    console.log("Error: Databinding. Unable to remove property listener: " + innerContext.configKey)
            }
        }

        const binding = new Binding(bind, unbind)
        this.#bindings.push(binding)

        return this
    }

    bindCallback(element: string | HTMLElement | JQuery | null, onChange: (configValue: any) => void): BindingContext {

        let configKey: string | null
        if (typeof element === 'string') {
            configKey = element
        } else {
            if (element === null)
                throw new Error("'element' is null")
            configKey = getConfigKey(element)
        }

        if (configKey === null)
            throw new Error("'configKey' is null")

        const delegate = (config: Config.GobchatConfig) => onChange(config.get(configKey, null))
        const listener = createConfigListener(delegate, null, true)
        return this.bindConfigListener(configKey, listener, delegate /*also used for init*/)
    }

    bindConfigListener(configKey: string | null, listener: ConfigListener, onInitialize: ((config: Config.GobchatConfig) => void) | null = null): BindingContext {
        const bind: Binding.OnBind = (config) => {
            if (onInitialize)
                onInitialize(config)

            config.addProfileEventListener(listener)
            if (configKey !== null)
                config.addPropertyEventListener(configKey, listener)
        }

        const unbind: Binding.OnUnbind = (config) => {
            if (!config.removeProfileEventListener(listener))
                console.log("Error: Databinding. Unable to remove profile listener")
            if (configKey !== null)
                if (!config.removePropertyEventListener(configKey, listener))
                    console.log("Error: Databinding. Unable to remove property listener: " + configKey)
        }

        const binding = new Binding(bind, unbind)
        this.#bindings.push(binding)

        return this
    }
}



class Binding {
    #boundTo: Config.GobchatConfig | null
    #doBind: Binding.OnBind
    #doUnbind: Binding.OnUnbind

    constructor(bindFunction: Binding.OnBind, unbindFunction: Binding.OnUnbind) {
        this.#boundTo = null
        this.#doBind = bindFunction
        this.#doUnbind = unbindFunction
    }

    get isBindActive() {
        return this.#boundTo !== null
    }

    bind(config: Config.GobchatConfig) {
        if (this.#boundTo) {
            if (this.#boundTo === config)
                return
            throw new Error("Binding already active for another config")
        }

        this.#boundTo = config
        this.#doBind(this.#boundTo)
    }

    unbind() {
        if (!this.#boundTo)
            return

        this.#doUnbind(this.#boundTo)
        this.#boundTo = null
    }
}

module Binding {
    export type OnBind = (config: Config.GobchatConfig) => void
    export type OnUnbind = (config: Config.GobchatConfig) => void
}

export type ConfigListenerDelegate = (config: Config.GobchatConfig, propertyKey: string | null) => void
export type ConfigListener = Config.GobchatConfigListener<Config.GobchatConfigEvent>

/**
 * Wraps the given delegate in a GobConfig listener, which can be used for property and profile events. The delegate will only be called according to the choosen arguments.
 * 
 * @param delegate
 * @param profileToObserve Only call delegate if a specific profile changes
 * @param onlyForActiveProfile Only call delegate if a property on the active profile changes
 */
export function createConfigListener(delegate: ConfigListenerDelegate, profileToObserve: string | null = null, onlyForActiveProfile: boolean = false): ConfigListener {
    return (event) => {
        switch (event.type) {
            case 'profile':
                if (event.action === 'active') {
                    if (profileToObserve === null || profileToObserve === event.newProfileId)
                        delegate(event.config, null)
                } else {
                    if (!onlyForActiveProfile)
                        if (profileToObserve === null || profileToObserve === event.profileId)
                            delegate(event.config, null)
                }
                break

            case 'property':
                if (!onlyForActiveProfile || (onlyForActiveProfile && event.isActiveProfile))
                    if (profileToObserve === null || profileToObserve === event.sourceProfileId)
                        delegate(event.config, event.key)
                break
        }
    }
}

/**
 * @deprecated use new BindingContext(gobConfig) instead
 */
export function makeDatabinding(gobConfig: Config.GobchatConfig): BindingContext {
    return new BindingContext(gobConfig)
}

// Helper functions to bind some types of UI elements to a given context

export function bindElement<A>(bindingContext: BindingContext, element: HTMLElement | JQuery, options?: BindElementOptions<A>): BindingContext {
    return bindingContext.bindElement(element, options)
}

export function bindText(bindingContext: BindingContext, element: HTMLElement | JQuery, options?: BindElementOptions<string>): BindingContext {
    const defOptions: BindElementOptions<string> = {
        elementToConfig: ($element) => $element.text(),
        configToElement: ($element, value) => $element.text(value)
    }
    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export interface BindTextCollectionOptions extends BindElementOptions<string[]>{
    joinSequence?: string
}

export function bindTextCollection(bindingContext: BindingContext, element: HTMLElement | JQuery, options?: BindTextCollectionOptions): BindingContext {
    const defOptions = {
        joinSequence: ", "
    }

    function split(value: string): string[] {
        const words = (value || "").split(",")
        return words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
    }

    const _options = $.extend(defOptions, options)
    _options.elementToConfig = ($element) => split($element.val())
    _options.configToElement = ($element, value) => $element.val(value.join(_options.joinSequence))

    return bindingContext.bindElement(element, _options)
}

export function bindCheckbox(bindingContext: BindingContext, element: HTMLElement | JQuery, options?: BindElementOptions<boolean>): BindingContext {
    const defOptions: BindElementOptions<boolean> = {
        elementToConfig: ($element) => $element.prop("checked") as boolean,
        configToElement: ($element, value) => $element.prop("checked", value)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxValue<A>(bindingContext: BindingContext, element: HTMLElement | JQuery, checkValue: A, uncheckValue: A, options?: BindElementOptions<A>): BindingContext {
    const defOptions: BindElementOptions<A> = {
        elementToConfig: ($element) => $element.prop("checked") ? checkValue : uncheckValue,
        configToElement: ($element, value) => $element.prop("checked", value === checkValue)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxArray<A>(bindingContext: BindingContext, element: HTMLElement | JQuery, values: A[], options?: BindElementOptions<A[]>): BindingContext {
    const defOptions: BindElementOptions<A[]> = {
        disabled: values === null || values === undefined || values.length === 0,
        elementToConfig: ($element, event, oldValues) => {
            const checked = $element.prop("checked") as boolean
            const changed = setValuesInArray(oldValues, values, checked)
            if (changed) oldValues.sort()
            return changed ? oldValues : undefined
        },
        configToElement: ($element, value) => {
            const checked = _.every(values, (e) => _.includes(value, e))
            $element.prop("checked", checked)
        }
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxArrayInverse<A>(bindingContext: BindingContext, element: HTMLElement | JQuery, values: A[], options?: BindElementOptions<A[]>): BindingContext {
    const defOptions: BindElementOptions<A[]> = {
        disabled: values === null || values === undefined || values.length === 0,
        elementToConfig: ($element, event, oldValues) => {
            const checked = $element.prop("checked")
            const changed = setValuesInArray(oldValues, values, !checked)
            if (changed) oldValues.sort()
            return changed ? oldValues : undefined
        },
        configToElement: ($element, value) => {
            const checked = _.every(values, (e) => _.includes(value, e))
            $element.prop("checked", !checked)
        }
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindColorSelector(bindingContext: BindingContext, element: HTMLElement | JQuery, options?: BindElementOptions<string>): BindingContext {
    const defOptions: BindElementOptions<string> = {
        elementKey: null,
        elementToConfig: null,
        //  ($element) => {
        //      const color = $element.spectrum("get"); return color === null ? null : color.toString();
        //  },
        configToElement: ($element, value) => $element.spectrum("set", value)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindDropdown(bindingContext: BindingContext, element: HTMLElement | JQuery, options?: BindElementOptions<any>): BindingContext {
    //const defOptions = {}
    return bindingContext.bindElement(element, options)
}

export function bindListener(bindingContext: BindingContext, element: HTMLElement | JQuery | string | null, callback: (configValue: any) => void): BindingContext {
    return bindingContext.bindCallback(element, callback)
}


