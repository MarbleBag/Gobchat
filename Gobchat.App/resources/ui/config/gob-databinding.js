'use strict'

var ConfigHelper = (function (ConfigHelper, undefined) {
    //Wraps the given delegate inside a profile listener, which only propagates the call if active profile changes.
    //
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

    class GobconfigBindingContext {
        constructor(gobconfig) {
            this._bindings = []
            this._config = gobconfig
        }

        initializeValues() {
            for (let binding of this._bindings)
                binding.initializeElement()
        }

        activateBinding() {
            for (let binding of this._bindings)
                binding.bind()
        }

        deactivateBinding() {
            for (let binding of this._bindings)
                binding.unbind()
        }

        bindCheckbox(element, options) {
            const defOptions = {
                elementGetAccessor: ($element) => $element.prop("checked"),
                elementSetAccessor: ($element, value) => $element.prop("checked", value)
            }
            return this.bindElement(element, $.extend(defOptions, options))
        }

        bindCheckboxValue(element, checkValue, uncheckValue, options) {
            const defOptions = {
                elementGetAccessor: ($element) => $element.prop("checked") ? checkValue : uncheckValue,
                elementSetAccessor: ($element, value) => $element.prop("checked", value === checkValue)
            }
            return this.bindElement(element, $.extend(defOptions, options))
        }

        bindColorSelector(element, options) {
            const defOptions = {
                elementKey: null,
                elementGetAccessor: null,
                //  ($element) => {
                //      const color = $element.spectrum("get"); return color === null ? null : color.toString();
                //  },
                elementSetAccessor: ($element, value) => $element.spectrum("set", value)
            }
            return this.bindElement(element, $.extend(defOptions, options))
        }

        bindElement(element, options) {
            const $element = $(element)
            const config = this._config

            const defOptions = {
                elementKey: "change",
                configKey: $element.attr(ConfigHelper.ConfigKeyAttribute),
                elementGetAccessor: ($element) => $element.val(),
                elementSetAccessor: ($element, value) => $element.val(value)
            }
            options = $.extend(defOptions, options)

            if (options.configKey === undefined || options.configKey === null) {
                $element.attr("disabled", true)
                return //done
            }

            const onElementChange = (event) => {
                const result = options.elementGetAccessor($element, event)
                if (result !== undefined)
                    config.set(options.configKey, result)
            }

            const onConfigChange = () => options.elementSetAccessor($element, config.get(options.configKey, null))

            const profileListener = createProfileListener(onConfigChange, null)
            const propertyListener = createPropertyListener(onConfigChange, null, true)

            const initialize = onConfigChange

            const bind = () => {
                //bind element
                if (options.elementKey)
                    $element.on(options.elementKey, onElementChange)

                //bind config
                config.addProfileEventListener(profileListener)
                config.addPropertyEventListener(options.configKey, propertyListener)
            }
            const unbind = () => {
                //unbind element
                if (options.elementKey)
                    $element.off(options.elementKey, onElementChange)

                //unbind config
                config.removeProfileEventListener(profileListener)
                config.removeProfileEventListener(options.configKey, propertyListener)
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

    ConfigHelper.createDatabinding = function (gobconfig) {
        return new GobconfigBindingContext(gobconfig)
    }

    return ConfigHelper
}(ConfigHelper || {}));