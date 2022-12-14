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

export const DataAttributeConfigKey = "data-gob-configkey"

export function getConfigKey(element: HTMLElement | JQuery) {
    return $(element).attr(DataAttributeConfigKey)
}

export function setConfigKey(element: HTMLElement | JQuery, configKey: string) {
    return $(element).attr(DataAttributeConfigKey, configKey)
}

/**
 * Wraps the given delegate in a GobConfig listener. The delegate will be called if the active profile changes or an observed property changes
 * 
 * @param delegate
 * @param profileToObserve
 * @param onlyIfProfileIsActive
 */
function createGobConfigListener(delegate: (propertyKey: string) => void, profileToObserve: string = null, onlyIfProfileIsActive: boolean = false): Config.GobchatConfigListener {
    return (event) => {
        switch (event.type) {
            case 'profile':
                if (event.action === 'active' && (profileToObserve === null || profileToObserve === event.newProfileId))
                    delegate(null)
                break
            case 'property':
                if ((!onlyIfProfileIsActive || (onlyIfProfileIsActive && event.isActiveProfile)) && (profileToObserve === null || profileToObserve === event.sourceProfileId))
                    delegate(event.key)
                break
        }
    }
}

/**
 * Adds or removes values from an array. Does not add duplicates.
 * @param {Array} array 
 * @param {Array} values 
 * @param {Boolean} doSet If true, adds values to array
 * @returns {Boolean} true, if the content of array changed
 */
function setValuesInArray<T, A extends T>(array: T[], values: A[], doSet: boolean) {
    let changed = false

    if (doSet) {
        values.forEach((value) => {            
            if (!_.includes(array, value)) {
                array.push(value)
                changed = true
            }
        })
    } else {
        const removedElements =
            _.remove(array, (arrayValue) => {
                return _.includes(values, arrayValue)
            })

        changed = removedElements.length > 0
    }

    return changed
}

export class BindingContext {
    #bindings: Binding[]
    #config: Config.GobchatConfig

    constructor(gobConfig: Config.GobchatConfig) {
        this.#bindings = []
        this.#config = gobConfig
    }

    #loadValues(): BindingContext {
        for (let binding of this.#bindings)
            binding.initializeElement()
        return this
    }

    #loadBindings(): BindingContext {
        for (let binding of this.#bindings)
            binding.bind()
        return this
    }

    #unloadBindings(): BindingContext {
        for (let binding of this.#bindings)
            binding.unbind()
        return this
    }

    initialize(): BindingContext {
        this.#loadValues()
        this.#loadBindings()
        return this
    }

    clear(): BindingContext {
        this.#unloadBindings()
        this.#bindings = []
        return this
    }

    bindElement(element: HTMLElement | JQuery, options: any): BindingContext {
        const $element = $(element)
        const config = this.#config

        const defOptions = {
            disabled: false,
            elementKey: "change",
            configKey: $element.attr(DataAttributeConfigKey),
            elementGetAccessor: ($element) => $element.val(),
            elementSetAccessor: ($element, value) => $element.val(value)
        }
        options = $.extend(defOptions, options)

        if (options.disabled || options.configKey === undefined || options.configKey === null) {
            $element.attr("disabled", "true")
            return this //done
        }

        const onElementChange = (event) => {
            let result = options.elementGetAccessor($element, event, config.get(options.configKey, null))
            if (result !== undefined)
                config.set(options.configKey, result)
        }

        const onConfigChange = () => options.elementSetAccessor($element, config.get(options.configKey, null))
        const profileListener = createGobConfigListener(onConfigChange, null, true)

        const initialize = onConfigChange

        const bind = () => {
            //bind element
            if (options.elementKey && options.elementGetAccessor)
                $element.on(options.elementKey, onElementChange)

            //bind config
            if (options.elementSetAccessor) {
                config.addProfileEventListener(profileListener)
                config.addPropertyEventListener(options.configKey, profileListener)
            }
        }
        const unbind = () => {
            //unbind element
            if (options.elementKey && options.elementGetAccessor)
                $element.off(options.elementKey, onElementChange)

            //unbind config
            if (options.elementSetAccessor) {
                if (!config.removeProfileEventListener(profileListener))
                    console.log("Error: Databinding. Unable to remove profile listener")
                if (!config.removePropertyEventListener(options.configKey, profileListener))
                    console.log("Error: Databinding. Unable to remove property listener: " + options.configKey)
            }
        }

        const binding = new Binding(initialize, bind, unbind)
        this.#bindings.push(binding)

        return this
    }

    bindConfigListener(configKey: string, callback: (configValue: any) => void): BindingContext {
        if (typeof configKey !== 'string')
            configKey = getConfigKey(configKey)

        if (configKey === null || configKey === undefined)
            throw new Error("'configKey' is null")

        const config = this.#config
        const onConfigChange = () => callback(config.get(configKey, null))

        const profileListener = createGobConfigListener(onConfigChange, null, true)

        const bind = () => {
            //bind config
            config.addProfileEventListener(profileListener)
            config.addPropertyEventListener(configKey, profileListener)
        }
        const unbind = () => {
            //unbind config
            config.removeProfileEventListener(profileListener)
            config.removePropertyEventListener(configKey, profileListener)
        }

        const binding = new Binding(onConfigChange, bind, unbind)
        this.#bindings.push(binding)

        return this
    }
}

class Binding {
    #isBindActive: boolean = false
    #initializeFunction: () => void
    #doBind: () => void
    #doUnbind: () => void

    constructor(initializeFunction: () => void, bindFunction: () => void, unbindFunction: () => void) {
        this.#initializeFunction = initializeFunction
        this.#doBind = bindFunction
        this.#doUnbind = unbindFunction
    }

    get isBindActive() {
        return this.#isBindActive
    }

    initializeElement() {
        if (this.#initializeFunction)
            this.#initializeFunction()
    }

    bind() {
        if (this.#isBindActive) return
        this.#doBind()
        this.#isBindActive = true
    }

    unbind() {
        if (!this.#isBindActive) return
        this.#doUnbind()
        this.#isBindActive = false
    }
}

/**
 * @deprecated use new BindingContext(gobConfig) instead
 */
export function makeDatabinding(gobConfig: Config.GobchatConfig): BindingContext {
    return new BindingContext(gobConfig)
}

// Helper functions to bind some types of UI elements to a given context

export function bindElement(bindingContext: BindingContext, element: HTMLElement | JQuery, options): BindingContext {
    return bindingContext.bindElement(element, options)
}

export function bindText(bindingContext: BindingContext, element: HTMLElement | JQuery, options): BindingContext {
    const defOptions = {
        elementGetAccessor: ($element) => $element.text(),
        elementSetAccessor: ($element, value) => $element.text(value)
    }
    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindTextCollectionfunction(bindingContext: BindingContext, element: HTMLElement | JQuery, options): BindingContext {
    const defOptions = {
        joinSequence: ", "
    }

    function split(value) {
        const words = (value || "").split(",")
        return words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
    }

    function join(values, delimiter) {
        return values.join(delimiter)
    }

    options = $.extend(defOptions, options)
    options.elementGetAccessor = ($element) => split($element.val())
    options.elementSetAccessor = ($element, value) => $element.val(join(value, options.joinSequence))

    return bindingContext.bindElement(element, options)
}

export function bindCheckbox(bindingContext: BindingContext, element: HTMLElement | JQuery, options): BindingContext {
    const defOptions = {
        elementGetAccessor: ($element) => $element.prop("checked"),
        elementSetAccessor: ($element, value) => $element.prop("checked", value)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxValue(bindingContext: BindingContext, element: HTMLElement | JQuery, checkValue, uncheckValue, options): BindingContext {
    const defOptions = {
        elementGetAccessor: ($element) => $element.prop("checked") ? checkValue : uncheckValue,
        elementSetAccessor: ($element, value) => $element.prop("checked", value === checkValue)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxArray(bindingContext: BindingContext, element: HTMLElement | JQuery, values: any[], options): BindingContext {
    const defOptions = {
        disabled: values === null || values === undefined || values.length === 0,
        elementGetAccessor: ($element, event, oldValues) => {
            const checked = $element.prop("checked")
            const changed = setValuesInArray(oldValues, values, checked)
            if (changed) oldValues.sort()
            return changed ? oldValues : undefined
        },
        elementSetAccessor: ($element, value) => {
            const checked = _.every(values, (e) => _.includes(value, e))
            $element.prop("checked", checked)
        }
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxArrayInverse(bindingContext: BindingContext, element: HTMLElement | JQuery, values: any[], options): BindingContext {
    const defOptions = {
        disabled: values === null || values === undefined || values.length === 0,
        elementGetAccessor: ($element, event, oldValues) => {
            const checked = $element.prop("checked")
            const changed = setValuesInArray(oldValues, values, !checked)
            if (changed) oldValues.sort()
            return changed ? oldValues : undefined
        },
        elementSetAccessor: ($element, value) => {
            const checked = _.every(values, (e) => _.includes(value, e))
            $element.prop("checked", !checked)
        }
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindColorSelector(bindingContext: BindingContext, element: HTMLElement | JQuery, options): BindingContext {
    const defOptions = {
        elementKey: null,
        elementGetAccessor: null,
        //  ($element) => {
        //      const color = $element.spectrum("get"); return color === null ? null : color.toString();
        //  },
        elementSetAccessor: ($element, value) => $element.spectrum("set", value)
    }
    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindDropdown(bindingContext: BindingContext, element: HTMLElement | JQuery, options): BindingContext {
    //const defOptions = {}
    return bindingContext.bindElement(element, options)
}

export function bindListener(bindingContext: BindingContext, configKey: string, callback: (configValue: any) => void): BindingContext {
    return bindingContext.bindConfigListener(configKey, callback)
}


