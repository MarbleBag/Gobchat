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

var GobConfigHelper = (function (module, undefined) {
    //Wraps the given delegate inside a profile listener, which only propagates the call if active profile changes.
    function createProfileListener(delegate, profileId) {
        if (Gobchat.isString(profileId)) { //triggers only if the given profile is active
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

    function createPropertyListener(delegate, profileId, onlyOnActive) {
        if (Gobchat.isString(profileId)) {
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

    function setValuesInArray(array, values, available) {
        let changed = false

        if (available) {
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

    class GobconfigBindingContext {
        constructor(gobconfig) {
            this._bindings = []
            this._config = gobconfig
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
                configKey: $element.attr(module.ConfigKeyAttribute),
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

    module.makeDatabinding = function (gobconfig) {
        return new GobconfigBindingContext(gobconfig)
    }

    module.bindElement = function (bindingContext, element, options) {
        return bindingContext.bindElement(element, options)
    }

    module.bindText = function (bindingContext, element, options) {
        const defOptions = {
            elementGetAccessor: ($element) => $element.text(),
            elementSetAccessor: ($element, value) => $element.text(value)
        }
        return bindingContext.bindElement(element, $.extend(defOptions, options))
    }

    module.bindTextCollection = function (bindingContext, element, options) {
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

    module.bindCheckbox = function (bindingContext, element, options) {
        const defOptions = {
            elementGetAccessor: ($element) => $element.prop("checked"),
            elementSetAccessor: ($element, value) => $element.prop("checked", value)
        }

        return bindingContext.bindElement(element, $.extend(defOptions, options))
    }

    module.bindCheckboxValue = function (bindingContext, element, checkValue, uncheckValue, options) {
        const defOptions = {
            elementGetAccessor: ($element) => $element.prop("checked") ? checkValue : uncheckValue,
            elementSetAccessor: ($element, value) => $element.prop("checked", value === checkValue)
        }

        return bindingContext.bindElement(element, $.extend(defOptions, options))
    }

    module.bindCheckboxArray = function (bindingContext, element, values, options) {
        const defOptions = {
            disabled: values === null || values === undefined || values.length === 0,
            elementGetAccessor: ($element, event, oldValues) => {
                const checked = $element.prop("checked")
                return setValuesInArray(oldValues, values, checked) ? oldValues : undefined
            },
            elementSetAccessor: ($element, value) => {
                const checked = _.every(values, (e) => _.includes(value, e))
                $element.prop("checked", checked)
            }
        }

        return bindingContext.bindElement(element, $.extend(defOptions, options))
    }

    module.bindColorSelector = function (bindingContext, element, options) {
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

    module.bindDropdown = function (bindingContext, element, options) {
        //const defOptions = {}
        return bindingContext.bindElement(element, options)
    }

    module.bindListener = function (bindingContext, configKey, callback) {
        return bindingContext.bindConfigListener(configKey, callback)
    }

    module.ConfigKeyAttribute = "data-gob-configkey"

    module.getConfigKey = function (element) {
        return $(element).attr(module.ConfigKeyAttribute)
    }

    module.setConfigKey = function (element, configKey) {
        return $(element).attr(module.ConfigKeyAttribute, configKey)
    }

    return module
}(GobConfigHelper || {}));