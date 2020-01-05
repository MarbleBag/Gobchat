'use strict'

//requieres Gobchat.DefaultProfileConfig

var Gobchat = (function (Gobchat, undefined) {
    function removeInvalidKeys(map, allowedKeys) {
        const availableKeys = Object.keys(map)
        const invalidKeys = availableKeys.filter((k) => { return _.indexOf(allowedKeys, k) === -1 })
        invalidKeys.forEach((k) => { delete map[k] }) //remove keys which are not allowed
    }

    //removes every value from data that is the same as in 	extendedData
    function retainChangesIterator(data, extendedData) {
        const callbackHelper = {
            onArray: function (data, extendedData) {
                //return _.isEqual(data,extendedData) //same objects can be removed
                return _.isEqual(_.sortBy(data), _.sortBy(extendedData)) //same objects can be removed
            },
            onCompare: function (data, extendedData) {
                return data == extendedData //same objects can be removed
            },
            onObject: function (data, extendedData, callbackHelper) {
                removeInvalidKeys(data, Object.keys(extendedData))
                for (let key of Object.keys(data)) {
                    if (objectTreeIteratorHelper(data[key], extendedData[key], callbackHelper)) //delete on true
                        delete data[key]
                }
                return Object.keys(data).length == 0 //if data is empty, it can be deleted
            }
        }

        dataIteratorHelper(data, extendedData, callbackHelper)
    }

    //Will merge every value from extendedData into data
    function writeObject(source, destination, copyOnWrite = false, ignoreFunc = null) {
        var path = [];
        var changes = new Set();

        const callbacks = {
            onArray: function (source, destination) {
                return true //lazy, just merge
            },
            onCompare: function (source, destination) {
                return true //lazy, just merge
            },
            onObject: function (source, destination) {
                for (let key of Object.keys(source)) {
                    path.push(key)
                    const fullPath = path.join(".")

                    if (!ignoreFunc(fullPath)) {
                        if (!(key in destination)) {
                            if (copyOnWrite)
                                destination[key] = copyByJson(source[key])
                            else
                                destination[key] = source[key]
                            changes.add(fullPath)
                        } else {
                            if (objectTreeIteratorHelper(source[key], destination[key], callbacks)) { //merge on true
                                if (copyOnWrite)
                                    destination[key] = copyByJson(source[key])
                                else
                                    destination[key] = source[key]
                                changes.add(fullPath)
                            }
                        }
                    }

                    path.pop()
                }
                return false
            }
        }

        const needsToBeReplaced = objectTreeIteratorHelper(source, destination, callbacks)
        return [Array.from(changes), needsToBeReplaced]
    }

    function objectTreeIteratorHelper(objA, objB, callbacks) {
        if (Gobchat.isArray(objA)) {
            if (Gobchat.isArray(objB)) {
                return callbacks.onArray(objA, objB, callbacks)
            } else {
                return false //invalid
            }
        } else if (Gobchat.isArray(objB)) {
            return false //invalid
        } else if (Gobchat.isObject(objA)) {
            if (Gobchat.isObject(objB)) {
                return callbacks.onObject(objA, objB, callbacks)
            } else {
                return false //invalid
            }
        } else if (Gobchat.isObject(objB)) {
            return false //invalid
        } else {
            return callbacks.onCompare(objA, objB, callbacks)
        }
    }

    function breakKeyDown(key) {
        if (key == undefined || key == null || key.length === 0) return []
        const parts = key.split(".")
        return parts
    }

    function buildPath(key, config) {
        let _config = config
        const keySteps = breakKeyDown(key)
        for (let i = 0; i < keySteps.length - 1; ++i) {
            const keyStep = keySteps[i]
            if (!(keyStep in _config)) {
                _config[keyStep] = {}
            }
            _config = _config[keyStep]
        }
    }

    function resolvePath(key, config, value, remove) {
        let _config = config
        const keySteps = breakKeyDown(key)

        for (let i = 0; i < keySteps.length - 1; ++i) {
            const keyStep = keySteps[i]
            if (keyStep in _config) {
                _config = _config[keyStep]
            } else {
                throw new InvalidKeyError(`Config error. Key '${key}' invalid. Unable to ${remove !== undefined ? `set` : `get`} data at '${keyStep}'`);
            }
        }

        if (keySteps.length === 0) {
            return _config
        }

        const targetKey = keySteps[keySteps.length - 1]
        if (value !== undefined) {
            _config[targetKey] = value
        }
        if (remove !== undefined && remove) {
            delete _config[targetKey]
        }
        return _config[targetKey]
    }

    function copyValueForKey(source, key, destination, doCopy) {
        let val = resolvePath(key, source)
        if (doCopy)
            val = copyByJson(val)
        buildPath(key, destination)
        resolvePath(key, destination, val)
    }

    //maybe not fast, but free of hassle :^)
    function copyByJson(obj) {
        if (obj === undefined || obj === null)
            throw new Error("Value is null")
        return JSON.parse(JSON.stringify(obj))
    }

    class InvalidKeyError extends Error {
        constructor(message) {
            super(message)
            this.name = "InvalidKeyError"
        }
    }

    class GobchatConfig {
        constructor(isSynced) {
            this._eventDispatcher = new EventDispatcher()
            if (Gobchat.DefaultProfileConfig === undefined || Gobchat.DefaultProfileConfig === null)
                throw new Error("Gobchat.DefaultProfileConfig is null")
            this._defaultProfile = copyByJson(Gobchat.DefaultProfileConfig)

            this._activeProfileId = null
            this._activeProfile = null
            this._profiles = {}
            this._isSynced = isSynced || false

            const self = this
            this._OnPropertyChange = function (event) { //used as an event listener, so we need to keep a ref to this around
                const isActiveProfile = event.source === self.activeProfile
                self._eventDispatcher.dispatch(`property:${event.key}`, { "key": event.key, "source": event.source, "isActive": isActiveProfile })
            }
        }

        _loadConfig(json) {
            const data = JSON.parse(json)

            const oldProfiles = this._profiles
            this._profiles = {}

            const profileIds = Object.keys(data.profiles)
            profileIds.forEach((profileId) => {
                const profileData = data.profiles[profileId]

                const cleanProfile = copyByJson(this._defaultProfile)
                writeObject(profileData, cleanProfile, false, (p) => false)

                const profile = new ConfigProfile(cleanProfile)
                profile.addPropertyListener("*", this._OnPropertyChange)
                this._profiles[profileId] = profile
            })

            this.activeProfile = data.activeProfile

            Object.keys(oldProfiles).forEach((profileId) => {
                if (!(profileId in this._profiles))
                    this._eventDispatcher.dispatch("profile:", { type: "delete", detail: { id: profileId } })
            })

            profileIds.forEach((profileId) => {
                if (!(profileId in oldProfiles))
                    this._eventDispatcher.dispatch("profile:", { type: "new", detail: { id: profileId } })
            })
        }

        _saveConfig() {
            const data = {
                activeProfile: this._activeProfileId,
                profiles: {}
            }

            Object.keys(this._profiles).forEach((profileId) => {
                const profile = this.getProfile(profileId)
                data.profiles[profileId] = profile.config
            })

            const json = JSON.stringify(data)
            return json
        }

        async loadConfig() {
            const dataJson = await GobchatAPI.getConfig()
            this._loadConfig(dataJson)

            /*
            const keys = this.profiles

            keys.forEach((profileId) => {
                const profile = this._data.profiles[profileId]
                const cleanProfile = copyByJson(this._defaultProfile)

                retainChangesIterator(profile, cleanProfile)
                mergeIterator(cleanProfile, profile)
                this._data.profiles[profileId] = cleanProfile
            })
            */
        }

        async saveConfig() {
            const dataJson = this._saveConfig()
            await GobchatAPI.setConfig(dataJson)
        }

        get activeProfile() {
            return this._activeProfileId
        }

        set activeProfile(profileId) {
            if (this._activeProfileId === profileId)
                return

            if (!this._profiles[profileId])
                throw new Error("Invalid profile id: " + profileId)

            const previousId = this._activeProfileId
            this._activeProfileId = profileId
            this._activeProfile = this._profiles[this._activeProfileId]

            this._eventDispatcher.dispatch("profile:", { type: "active", detail: { old: previousId, new: this._activeProfileId } })

            if (this._isSynced)
                GobchatAPI.setActiveProfile(this._activeProfileId)
        }

        get profiles() {
            return Object.keys(this._profiles)
        }

        createNewProfile() {
            function generateId(ids) {
                let id = null
                do {
                    id = Gobchat.generateId(8)
                } while (_.includes(ids, id))
                return id
            }

            const profile = new ConfigProfile(copyByJson(this._defaultProfile))
            const profileId = generateId(this.profiles)

            profile.config.profile.id = profileId
            profile.config.profile.name = `Profile ${this.profiles.length + 1}`
            profile.addPropertyListener("*", this._OnPropertyChange)

            this._profiles[profileId] = profile

            this._eventDispatcher.dispatch("profile:", { type: "new", detail: { id: profileId } })
            return profileId
        }

        deleteProfile(profileId) {
            const profiles = this.profiles
            if (profiles.length <= 1)
                return //TODO
            if (!_.includes(profiles, profileId))
                return
            delete this._profiles[profileId]
            this.activeProfile = this.profiles[0]
            this._eventDispatcher.dispatch("profile:", { type: "delete", detail: { id: profileId } })
        }

        copyProfile(sourceProfileId, destinationProfileId) {
            const sourceProfile = this.getProfile(sourceProfileId)
            const destinationProfile = this.getProfile(destinationProfileId)
            if (!sourceProfile || !destinationProfile)
                return
            destinationProfile.copyFrom(sourceProfile, "")
        }

        getProfile(profileId) {
            const profile = this._profiles[profileId]
            if (profile === undefined)
                return null
            return profile
        }

        //TODO remove later
        saveToLocalStore() {
            const json = this._saveConfig()
            window.localStorage.setItem("gobchat-config", json)
        }

        //TODO remove later
        loadFromLocalStore() {
            const json = window.localStorage.getItem("gobchat-config")
            window.localStorage.removeItem("gobchat-config")
            if (json === undefined || json === null)
                return
            this._loadConfig(json)
        }

        addProfileEventListener(callback) {
            this._eventDispatcher.on("profile:", callback)
        }

        addPropertyEventListener(topic, callback) {
            this._eventDispatcher.on("property:" + topic, callback)
        }

        get(key, defaultValue) {
            if (!this._activeProfile)
                throw new Error("No active profile")
            return this._activeProfile.get(key, defaultValue)
        }

        set(key, value) {
            if (!this._activeProfile)
                throw new Error("No active profile")
            return this._activeProfile.set(key, value)
        }

        has(key) {
            if (!this._activeProfile)
                throw new Error("No active profile")
            return this._activeProfile.has(key)
        }

        reset(key) {
            if (!this._activeProfile)
                throw new Error("No active profile")
            return this._activeProfile.reset(key)
        }

        remove(key) {
            if (!this._activeProfile)
                throw new Error("No active profile")
            return this._activeProfile.remove(key)
        }

        resetActiveProfile() {
            if (!this._activeProfile)
                throw new Error("No active profile")
            this._activeProfile._restoreDefaultConfig()
        }
    }

    class ConfigProfile {
        constructor(config) {
            if (config === undefined || config === null)
                throw new Error("config is null")

            this._propertyListener = new EventDispatcher()
            this._config = config
        }

        get profileId() {
            return this._config.profile.id
        }

        get profileName() {
            return this._config.profile.name
        }

        set profileName(name) {
            this.set("profile.name", name)
        }

        get config() {
            return this._config
        }

        copyFrom(config, rootKey) {
            const srcRoot = copyByJson(config.get(rootKey))
            if (!this.has(rootKey)) {
                this.set(rootKey, srcRoot)
                return
            }

            let dstRoot = this.get(rootKey)
            const [changes, replace] = writeObject(srcRoot, dstRoot, false, p => p === "profile")
            if (replace)
                dstRoot = srcRoot

            this.set(rootKey, dstRoot)
            this._firePropertyChanges(changes)
        }

        restoreDefaultConfig() {
            this._restoreDefaultConfig(true)
        }

        _restoreDefaultConfig(fireChanges) {
            const oldConfig = this._config
            this._config = copyByJson(Gobchat.DefaultProfileConfig)
            this._config.profile = copyByJson(oldConfig.profile)

            if (fireChanges) {
                const [changes, ...x] = writeObject(Gobchat.DefaultProfileConfig, oldConfig, false, p => p === "profile")
                this._firePropertyChanges(changes)
            }
        }

        //TODO delete
        _overwriteConfig(configData) {
            const [changes, ...x] = writeObject(configData, this._config, false, p => false)
            this._firePropertyChanges(changes)
        }

        getConfigDiff() {
            const config = copyByJson(this._config)
            retainChangesIterator(config, Gobchat.DefaultProfileConfig)

            copyValueForKey(this._config, "profile", config, true)
            copyValueForKey(this._config, "behaviour.groups", config, true)
            // copyValueForKey(this._config, "version", config)
            // copyValueForKey(this._config, "userdata", config)
            return config
        }

        get(key, defaultValue) {
            if (key === null || key.length === 0) {
                return this._config
            }
            try {
                const value = resolvePath(key, this._config)
                return value === undefined ? defaultValue : value
            } catch (error) {
                if (defaultValue !== undefined) {
                    if (error instanceof InvalidKeyError) {
                        return defaultValue
                    }
                }
                throw error
            }
        }

        has(key) {
            return this.get(key, undefined) !== undefined;
        }

        set(key, value) {
            if (key === null || key.length === 0) {
                this._config = value
            }
            resolvePath(key, this._config, value)
            this._firePropertyChange(key)
        }

        reset(key) {
            if (key === null || key.length === 0)
                return

            const original = resolvePath(key, Gobchat.DefaultProfileConfig)
            resolvePath(key, this._config, original)
            this._firePropertyChange(key)
        }

        remove(key) {
            if (key === null || key.length === 0)
                return
            resolvePath(key, this._config, undefined, true)
            this._firePropertyChange(key)
        }

        addPropertyListener(topic, callback) {
            this._propertyListener.on(topic, callback)
        }

        removePropertyListener(topic, callback) {
            this._propertyListener.off(topic, callback)
        }

        _firePropertyChange(propertyPath) {
            this._firePropertyChanges([propertyPath])
        }

        _firePropertyChanges(propertyPaths) {
            const splitted = new Set()

            propertyPaths.forEach(propertyPath => {
                if (splitted.has(propertyPath)) return
                splitted.add(propertyPath)

                let indexOffset = propertyPath.length
                while (true) {
                    indexOffset = propertyPath.lastIndexOf(".", indexOffset)
                    if (indexOffset === -1) break
                    const path = propertyPath.substring(0, indexOffset)
                    if (splitted.has(path)) break
                    splitted.add(path)
                }
            })

            const sorted = Array.from(splitted)

            sorted.sort((a, b) => {
                if (a.startsWith(b))
                    return 1;
                if (b.startsWith(a))
                    return -1;
                return a.localeCompare(b)
            })

            sorted.forEach((propertyPath) => {
                const data = { "key": propertyPath, "source": this.profileId }
                this._propertyListener.dispatch(propertyPath, data)
                this._propertyListener.dispatch("*", data)
            })

            //this._propertyListener.dispatch("*", { "key": "*", "source": this.profileId })
        }
    }

    Gobchat.GobchatConfig = GobchatConfig

    class EventDispatcher {
        constructor() {
            this.listenersByTopic = new Map([])
        }
        dispatch(topic, data) {
            const listeners = this.listenersByTopic.get(topic)
            if (listeners) {
                const callbacks = listeners.slice(0)
                callbacks.forEach((callback) => { callback(data) })
            }
        }
        on(topic, callback) {
            if (!callback) return
            let listeners = this.listenersByTopic.get(topic)
            if (!listeners) {
                listeners = []
                this.listenersByTopic.set(topic, listeners)
            }
            listeners.push(callback)
        }
        off(topic, callback) {
            let listeners = this.listenersByTopic.get(topic)
            if (listeners) {
                const idx = listeners.indexOf(callback)
                if (idx > -1) listeners.splice(idx, 1)
                if (listeners.length === 0) this.listenersByTopic.delete(topic)
            }
        }
    }

    return Gobchat
}(Gobchat || {}));