'use strict';

var ConfigHelper = (function (ConfigHelper, undefined) {
    //TODO
    //Implement a type of databinding context, so it's easier (even possible) to detach any listener on gobconfig when they are no longer needed
    //This is especially needed for elements which are not persistent

    class DatabindingContext {
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

        get isHooked() {
            return this._isHookActive
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

    //Wraps the given delegate inside a profile listener, which only propagates the call if active profile changes.
    //
    function buildProfileListener(delegate, profileId) {
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

    function buildPropertyListener(delegate, profileId, onlyOnActive) {
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

    ConfigHelper.createConfigListener = function (delegate, options, ) {
        const defOptions = { onProfileId: undefined, onActiveProfile: true }
        options = $.extend(defOptions, options)

        const profileListener = buildProfileListener(delegate, options.onProfileId)
        const propertyListener = buildPropertyListener(delegate, options.onProfileId, options.onActiveProfile)

        return [profileListener, propertyListener]
    }

    ConfigHelper.createConfigListenerDelegate = function (propertyKey, options, delegate) {
        const defOptions = { propertyDefValue: undefined }
        options = $.extend(defOptions, options)

        return (eventPropertyKey) => {
            const key = propertyKey || eventPropertyKey
            if (key)
                delegate(gobconfig.get(propertyKey, options.propertyDefValue))
            else
                delegate()
        }
    }

    ConfigHelper.createHookConfigListener = function (propertyKey, options, delegate) {
        delegate = ConfigHelper.createConfigListenerDelegate(propertyKey, options, delegate)
        const [profileListener, propertyListener] = ConfigHelper.createConfigListener(options, delegate)
        return new ConfigListenerHook(profileListener, propertyKey, propertyListener)
    }

    return ConfigHelper
}(ConfigHelper || {}));