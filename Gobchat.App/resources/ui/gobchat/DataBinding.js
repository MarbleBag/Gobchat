'use strict'

var Gobchat = (function (Gobchat, undefined) {
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
    }

    class DatabindingContext {
        constructor() {
            this._bindings = []
        }

        activate() {
            for (let binding in this._bindings) {
                binding.activate()
            }
        }

        deactivate() {
            for (let binding in this._bindings) {
                binding.deactivate()
            }
        }

        bindInput(element) {
            const $element = $(element)
            const configKey = $element.attr(ConfigHelper.ConfigKeyAttribute)

            const onConfigSet = (event) => this._config.set(configKey, event.target.checked)
            const onConfigGet = (value) => $element.val(value)

            const onElement = (event) => this._config.set(configKey, event.target.checked)

            const onProfile = buildProfileListener(() =>

            const bindElement = () => $element.on("change", onElement)
                const unbindElement = () => $element.off("change", onElement)
                const binding = new DataBinding(bindElement, unbindElement)
                this._store(binding)
            }
                }
    }

    class Hook {
        constructor() {
            this._isHookActive = false
        }

        get isHooked() {
            return this._isHookActive
        }

        hook() {
            this._isHookActive = true
        }

        unhook() {
            this._isHookActive = false
        }
    }

    class ConfigListenerHook extends Hook {
        constructor(profileListener, propertyKey, propertyListener) {
            super()
            this._profileListener = profileListener
            this._propertyKey = propertyKey
            this._propertyListener = propertyListener
        }

        hook() {
            if (this._profileListener)
                gobconfig.addProfileEventListener(this._profileListener)
            if (this._propertyListener)
                gobconfig.addPropertyEventListener(this._propertyKey, this._propertyListener)
            super.hook()
        }

        unhook() {
            if (this._profileListener)
                gobconfig.removeProfileEventListener(this._profileListener)
            if (this._propertyListener)
                gobconfig.removePropertyEventListener(this._propertyKey, this._propertyListener)
            super.unhook()
        }
    }

    function bindCheckbox(ckbElement, config) {
    }

    function bindCheckbox(config, element) {
        const $element = $(element)
        const configKey = $element.attr(ConfigHelper.ConfigKeyAttribute)

        if (configKey === undefined || configKey === null) {
            $element.attr("disabled", true)
            return null //TODO
        }

        const onElement = (event) => {
            config.set(configKey, event.target.checked)
        }
        const onConfig = (event) => {
            value => $element.prop('checked', value)
        }
    }

    //----------------------------------------------------------------------------
    const dbContext = ConfigHelper.createDatabinding(gobconfig)

    const $txtName = $("#name")
    dbContext.bindInput($txtName)

    //----------------------------------------------------------------------------

    return Gobchat
}(Gobchat || {}));