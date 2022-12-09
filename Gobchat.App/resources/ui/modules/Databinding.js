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

import * as Utility from './CommonUtility.js'

export const DataAttributeConfigKey = "data-gob-configkey"

export function getConfigKey(element) {
    return $(element).attr(DataAttributeConfigKey)
}

export function setConfigKey(element, configKey) {
    return $(element).attr(DataAttributeConfigKey, configKey)
}

//Wraps the given delegate inside a profile listener, which only propagates the call if active profile changes.
/**
 * Internal helper function * 
 * @param {Function} delegate
 * @param {String} profileId
 */
function createProfileListener(delegate, profileId) {
    if (Utility.isString(profileId)) { //triggers only if the given profile is active
        return (event) => {
            if (event.type === "active" && event.detail.new === profileId)
                delegate(null)
        }
    } else {
        return (event) => {
            if (event.type === "active")
                delegate(null)
        }
    }
}


/**
 * Internal helper function
 * Wraps the given delegate inside a property listener, which only propagates the call if the property changes.
 * @param {Function} delegate Will be called by the listener
 * @param {String} profileId delegate will only be called if the property change event comes from the profile with this id
 * @param {Boolean} onlyOnActive delegate will only be called if the property change event comes from the active profile
 * @returns {Function} 
 */
function createPropertyListener(delegate, profileId, onlyOnActive) {
    if (Utility.isString(profileId)) {
        if (onlyOnActive) {
            return (event) => {
                if (event.isActive && event.source === profileId)
                    delegate(event.key)
            }
        } else {
            return (event) => {
                if (event.source === profileId)
                    delegate(event.key)
            }
        }
    } else {
        if (onlyOnActive) {
            return (event) => {
                if (event.isActive)
                    delegate(event.key)
            }
        } else {
            return (event) => {
                delegate(event.key)
            }
        }
    }
}

function createConfigListener(delegate, options) {
    const defOptions = { onProfileId: undefined, onActiveProfile: true }
    options = $.extend(defOptions, options)

    const profileListener = createProfileListener(delegate, options.onProfileId)
    const propertyListener = createPropertyListener(delegate, options.onProfileId, options.onActiveProfile)

    return [profileListener, propertyListener]
}

/**
 * Adds or removes values from an array. Does not add duplicates.
 * @param {Array} array 
 * @param {Array} values 
 * @param {Boolean} doSet If true, adds values to array
 * @returns {Boolean} true, if the content of array changed
 */
function setValuesInArray(array, values, doSet) {
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
    constructor(gobConfig) {
        this._bindings = []
        this._config = gobConfig
    }

    _loadValues() {
        for (let binding of this._bindings)
            binding.initializeElement()
        return this
    }

    _loadBindings() {
        for (let binding of this._bindings)
            binding.bind()
        return this
    }

    _unloadBindings() {
        for (let binding of this._bindings)
            binding.unbind()
        return this
    }

    initialize() {
        this._loadValues()
        this._loadBindings()
        return this
    }

    clear() {
        this._unloadBindings()
        this._bindings = []
        return this
    }

    bindElement(element, options) {
        const $element = $(element)
        const config = this._config

        const defOptions = {
            disabled: false,
            elementKey: "change",
            configKey: $element.attr(DataAttributeConfigKey),
            elementGetAccessor: ($element) => $element.val(),
            elementSetAccessor: ($element, value) => $element.val(value)
        }
        options = $.extend(defOptions, options)

        if (options.disabled || options.configKey === undefined || options.configKey === null) {
            $element.attr("disabled", true)
            return //done
        }

        const onElementChange = (event) => {
            let result = options.elementGetAccessor($element, event, config.get(options.configKey, null))
            if (result !== undefined)
                config.set(options.configKey, result)
        }

        const onConfigChange = () => options.elementSetAccessor($element, config.get(options.configKey, null))

        const profileListener = createProfileListener(onConfigChange, null)
        const propertyListener = createPropertyListener(onConfigChange, null, true)

        const initialize = onConfigChange

        const bind = () => {
            //bind element
            if (options.elementKey && options.elementGetAccessor)
                $element.on(options.elementKey, onElementChange)

            //bind config
            if (options.elementSetAccessor) {
                config.addProfileEventListener(profileListener)
                config.addPropertyEventListener(options.configKey, propertyListener)
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
                if (!config.removePropertyEventListener(options.configKey, propertyListener))
                    console.log("Error: Databinding. Unable to remove property listener: " + options.configKey)
            }
        }

        const binding = new Binding(initialize, bind, unbind)
        this._bindings.push(binding)
    }

    bindConfigListener(configKey, callback) {
        if (typeof configKey !== 'string')
            configKey = getConfigKey(configKey)

        if (configKey === null || configKey === undefined)
            throw new Error("'configKey' is null")

        const config = this._config
        const onConfigChange = () => callback(config.get(configKey, null))

        const profileListener = createProfileListener(onConfigChange, null)
        const propertyListener = createPropertyListener(onConfigChange, null, true)

        const bind = () => {
            //bind config
            config.addProfileEventListener(profileListener)
            config.addPropertyEventListener(configKey, propertyListener)
        }
        const unbind = () => {
            //unbind config
            config.removeProfileEventListener(profileListener)
            config.removeProfileEventListener(configKey, propertyListener)
        }

        const binding = new Binding(onConfigChange, bind, unbind)
        this._bindings.push(binding)
    }
}

class Binding {
    constructor(initializeFunction, bindFunction, unbindFunction) {
        this._isBindActive = false
        this._initializeFunction = initializeFunction
        this._bindFunction = bindFunction
        this._unbindFunction = unbindFunction
    }

    get isBindActive() {
        return this._isBindActive
    }

    initializeElement() {
        if (this._initializeFunction)
            this._initializeFunction()
    }

    bind() {
        if (this._isBindActive) return
        this._bindFunction()
        this._isBindActive = true
    }

    unbind() {
        if (!this._isBindActive) return
        this._unbindFunction()
        this._isBindActive = false
    }
}

/**
 * @deprecated use new BindingContext(gobConfig) instead
 */
export function makeDatabinding(gobConfig) {
    return new BindingContext(gobConfig)
}

// Helper functions to bind some types of UI elements to a given context

export function bindElement(bindingContext, element, options) {
    return bindingContext.bindElement(element, options)
}

export function bindText(bindingContext, element, options) {
    const defOptions = {
        elementGetAccessor: ($element) => $element.text(),
        elementSetAccessor: ($element, value) => $element.text(value)
    }
    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindTextCollectionfunction (bindingContext, element, options) {
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

export function bindCheckbox(bindingContext, element, options) {
    const defOptions = {
        elementGetAccessor: ($element) => $element.prop("checked"),
        elementSetAccessor: ($element, value) => $element.prop("checked", value)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxValue(bindingContext, element, checkValue, uncheckValue, options) {
    const defOptions = {
        elementGetAccessor: ($element) => $element.prop("checked") ? checkValue : uncheckValue,
        elementSetAccessor: ($element, value) => $element.prop("checked", value === checkValue)
    }

    return bindingContext.bindElement(element, $.extend(defOptions, options))
}

export function bindCheckboxArray(bindingContext, element, values, options) {
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

export function bindCheckboxArrayInverse(bindingContext, element, values, options) {
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

export function bindColorSelector(bindingContext, element, options) {
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

export function  bindDropdown(bindingContext, element, options) {
    //const defOptions = {}
    return bindingContext.bindElement(element, options)
}

export function bindListener(bindingContext, configKey, callback) {
    return bindingContext.bindConfigListener(configKey, callback)
}


