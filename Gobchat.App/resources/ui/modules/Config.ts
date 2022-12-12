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

//requieres Gobchat.DefaultProfileConfig, uploaded by backend
import * as Utility from './CommonUtility.js'
import { EventDispatcher } from './EventDispatcher.js'

/**
 * 
 * @param {Object} map
 * @param {Array} allowedKeys
 */
function removeInvalidKeys(map: Object, allowedKeys: string[]) {
    const availableKeys = Object.keys(map)
    const invalidKeys = availableKeys.filter((k) => { return _.indexOf(allowedKeys, k) === -1 })
    invalidKeys.forEach((k) => { delete map[k] }) //remove keys which are not allowed
}

/**
 * removes every value from data that is the same as in 	extendedData
 * @param {Object} data
 * @param {Object} extendedData
 */
function retainChangesIterator(data: Object, extendedData: Object) {
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

    //dataIteratorHelper(data, extendedData, callbackHelper)
}

/**
 * 
 * @param {Object} source
 * @param {Object} destination
 * @param {Function} ignoreFunc
 */
function removeMissingObjects(source, destination, ignoreFunc = null) {
    var path = [];
    var changes = new Set();

    const callbacks = {
        onArray: function (source, destination) {
            return false
        },
        onCompare: function (source, destination) {
            return false
        },
        onObject: function (source, destination) {
            const availableKeys = Object.keys(destination)
            const allowedKeys = Object.keys(source)
            const keysToRemove = availableKeys.filter((k) => { return !_.includes(allowedKeys, k) })

            for (let key of keysToRemove) {
                path.push(key)
                const fullPath = path.join(".")
                if (!ignoreFunc(fullPath)) {
                    delete destination[key]
                    changes.add(fullPath)
                }
                path.pop()
            }

            for (let key of Object.keys(destination)) {
                path.push(key)
                const fullPath = path.join(".")
                if (!ignoreFunc(fullPath)) {
                    objectTreeIteratorHelper(source[key], destination[key], callbacks)
                }
                path.pop()
            }

            return false
        }
    }

    const needsToBeReplaced = objectTreeIteratorHelper(source, destination, callbacks)
    return [Array.from(changes), needsToBeReplaced]
}

/**
 * Will merge every value from source into destination
 * @param {Object} source
 * @param {Object} destination
 * @param {Boolean} copyOnWrite
 * @param {Function} ignoreFunc
 */
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

/**
 * 
 * @param {Object} objA
 * @param {Object} objB
 * @param {Function} callbacks
 */
function objectTreeIteratorHelper(objA, objB, callbacks) {
    if (Utility.isArray(objA)) {
        if (Utility.isArray(objB)) {
            return callbacks.onArray(objA, objB, callbacks)
        } else {
            return false //invalid
        }
    } else if (Utility.isArray(objB)) {
        return false //invalid
    } else if (Utility.isObject(objA)) {
        if (Utility.isObject(objB)) {
            return callbacks.onObject(objA, objB, callbacks)
        } else {
            return false //invalid
        }
    } else if (Utility.isObject(objB)) {
        return false //invalid
    } else {
        return callbacks.onCompare(objA, objB, callbacks)
    }
}

interface JsonProfileConfig {
    profile: {id: string, name: string}
}

/**
 * @param {String} key
 */
function breakKeyDown(key) {
    if (key == undefined || key == null || key.length === 0) return []
    const parts = key.split(".")
    return parts
}

/**
 * @param {String} key
 * @param {Object} config
 */
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

/**
 * 
 * @param {String} key
 * @param {Object} config
 * @param {any} value
 * @param {Boolean} remove
 */
function resolvePath(key: string, config: object, value: object = undefined, remove: boolean = false) {
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

    if (keySteps.length === 0) 
        return _config
    

    const targetKey = keySteps[keySteps.length - 1]
    if (value !== undefined) 
        _config[targetKey] = value != null ? copyByJson(value) : null
    
    if (remove) 
        delete _config[targetKey]
    
    return _config[targetKey]
}

/**
 * 
 * @param {Object} source
 * @param {String} key
 * @param {Object} destination
 * @param {Boolean} doCopy
 */
function copyValueForKey(source: GobchatConfig, key: string, destination, doCopy) {
    let val = resolvePath(key, source)
    if (doCopy)
        val = copyByJson(val)
    buildPath(key, destination)
    resolvePath(key, destination, val)
}

/**
 * 
 * @param {Object} obj
 */
function copyByJson(obj) {
    if (obj === undefined || obj === null)
        throw new Error("Value is null")
    return JSON.parse(JSON.stringify(obj))
}

export class InvalidKeyError extends Error {
    constructor(message: string) {
        super(message)
        this.name = "InvalidKeyError"
    }
}

export class GobchatConfig {
    #eventDispatcher: EventDispatcher = null
    #defaultProfile: object = null
    #activeProfileId: string = null
    #activeProfile: ConfigProfile = null
    #profiles: object = {}
    #isSynced: boolean = false

    #OnPropertyChange: Function = null

    constructor(isSynced: boolean = false) {
        this.#eventDispatcher = new EventDispatcher()
        if (Gobchat.DefaultProfileConfig === undefined || Gobchat.DefaultProfileConfig === null)
            throw new Error("Gobchat.DefaultProfileConfig is null")
        this.#defaultProfile = copyByJson(Gobchat.DefaultProfileConfig)

        this.#activeProfileId = null
        this.#activeProfile = null
        this.#profiles = {}
        this.#isSynced = isSynced

        const self = this
        this.#OnPropertyChange = function (event) { //used as an event listener, so we need to keep a ref to this around
            const isActiveProfile = event.source === self.activeProfile
            self.#eventDispatcher.dispatch(`property:${event.key}`, { "key": event.key, "source": event.source, "isActive": isActiveProfile })
        }

        if (this.#isSynced) {
            document.addEventListener("SynchronizeConfigEvent", (e) => { self.loadConfig() })
        }
    }

    #loadConfig(json: string) {
        const data = JSON.parse(json)

        const loadedProfileIds = Object.keys(data.profiles)
        const availableProfileIds = this.profiles
        const newProfileIds = loadedProfileIds.filter(e => !_.includes(availableProfileIds, e))
        const changedProfileIds = loadedProfileIds.filter(e => _.includes(availableProfileIds, e))
        const deletedProfileIds = availableProfileIds.filter(e => !_.includes(loadedProfileIds, e))

        newProfileIds.forEach(profileId => {
            const profileData = data.profiles[profileId]
            const cleanProfile = copyByJson(this.#defaultProfile)
            writeObject(profileData, cleanProfile, false, (p) => false)
            this.#storeNewProfile(cleanProfile)
        })

        this.activeProfile = data.activeProfile

        deletedProfileIds.forEach(profileId => {
            this.deleteProfile(profileId)
        })

        changedProfileIds.forEach(profileId => {
            let cleanProfile = copyByJson(this.#defaultProfile)
            const profileData = data.profiles[profileId]
            writeObject(profileData, cleanProfile, false, (p) => false)
            cleanProfile = new ConfigProfile(cleanProfile)
            this.getProfile(profileId).copyFrom(cleanProfile, "", true)
        })
    }

    #generateId() : string {
        const ids = this.profiles
        let id = null
        do {
            id = Utility.generateId(8)
        } while (_.includes(ids, id))
        return id
    }

    #saveConfig() {
        const data = {
            activeProfile: this.#activeProfileId,
            profiles: {}
        }

        Object.keys(this.#profiles).forEach((profileId) => {
            const profile = this.getProfile(profileId)
            data.profiles[profileId] = profile.config
        })

        const json = JSON.stringify(data)
        return json
    }

    async loadConfig() {
        const dataJson = await GobchatAPI.getConfigAsJson()
        this.#loadConfig(dataJson)

        /*
        const keys = this.profiles

        keys.forEach((profileId) => {
            const profile = this._data.profiles[profileId]
            const cleanProfile = copyByJson(this.#defaultProfile)

            retainChangesIterator(profile, cleanProfile)
            mergeIterator(cleanProfile, profile)
            this._data.profiles[profileId] = cleanProfile
        })
        */
    }

    async saveConfig() {
        const dataJson = this.#saveConfig()
        await GobchatAPI.synchronizeConfig(dataJson)
    }

    get activeProfile(): string {
        return this.#activeProfileId
    }

    set activeProfile(profileId: string) {
        if (this.#activeProfileId === profileId)
            return

        if (!this.#profiles[profileId])
            throw new Error("Invalid profile id: " + profileId)

        const previousId = this.#activeProfileId
        this.#activeProfileId = profileId
        this.#activeProfile = this.#profiles[this.#activeProfileId]

        this.#eventDispatcher.dispatch("profile:", { type: "active", detail: { old: previousId, new: this.#activeProfileId } })

        if (this.#isSynced)
            GobchatAPI.setConfigActiveProfile(this.#activeProfileId)
    }

    get profiles(): string[] {
        return Object.keys(this.#profiles)
    }

    importProfile(profileJson: object) {
        const cleanProfile = copyByJson(this.#defaultProfile)
        writeObject(profileJson, cleanProfile, false, (p) => false)
        const profileId = this.#generateId()
        cleanProfile.profile.id = profileId
        this.#storeNewProfile(cleanProfile)
        return profileId
    }

    createNewProfile(): string {
        const profileId = this.#generateId()
        const newProfileData = copyByJson(this.#defaultProfile)
        newProfileData.profile.id = profileId
        newProfileData.profile.name = `Profile ${this.profiles.length + 1}`
        this.#storeNewProfile(newProfileData)
        return profileId
    }

    #storeNewProfile(data) {
        const profile = new ConfigProfile(data)
        const profileId = profile.profileId

        profile.addPropertyListener("*", this.#OnPropertyChange)
        this.#profiles[profileId] = profile
        this.#eventDispatcher.dispatch("profile:", { type: "new", detail: { id: profileId } })
    }

    deleteProfile(profileId) {
        if (this.profiles.length <= 1)
            return

        if (!_.includes(this.profiles, profileId))
            return

        delete this.#profiles[profileId]

        if (this.activeProfile === profileId)
            this.activeProfile = this.profiles[0]

        this.#eventDispatcher.dispatch("profile:", { type: "delete", detail: { id: profileId } })
    }

    copyProfile(sourceProfileId, destinationProfileId) {
        const sourceProfile = this.getProfile(sourceProfileId)
        const destinationProfile = this.getProfile(destinationProfileId)
        if (!sourceProfile || !destinationProfile)
            return
        destinationProfile.copyFrom(sourceProfile, "")
    }

    getProfile(profileId): ConfigProfile {
        const profile = this.#profiles[profileId]
        if (profile === undefined)
            return null
        return profile
    }

    //TODO remove later
    saveToLocalStore() {
        const json = this.#saveConfig()
        window.localStorage.setItem("gobchat-config", json)
    }

    //TODO remove later
    loadFromLocalStore(keepLocaleStore) {
        const json = window.localStorage.getItem("gobchat-config")
        if (!keepLocaleStore)
            window.localStorage.removeItem("gobchat-config")

        if (json === undefined || json === null)
            return

        //TODO toggle sync, so the change is not immediately propagated back
        const isSynced = this.#isSynced
        this.#isSynced = false
        try {
            this.#loadConfig(json)
        } finally {
            this.#isSynced = isSynced
        }

        if (this.#isSynced)
            this.saveConfig()
    }

    addProfileEventListener(callback) {
        return this.#eventDispatcher.on("profile:", callback)
    }

    removeProfileEventListener(callback) {
        return this.#eventDispatcher.off("profile:", callback)
    }

    addPropertyEventListener(topic, callback) {
        return this.#eventDispatcher.on("property:" + topic, callback)
    }

    removePropertyEventListener(topic, callback) {
        return this.#eventDispatcher.off("property:" + topic, callback)
    }

    get(key: string, defaultValue) {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.get(key, defaultValue)
    }

    getDefault(key: string, defaultValue) {
        try {
            const value = resolvePath(key, this.#defaultProfile)
            return value === undefined ? defaultValue : value !== null ? copyByJson(value) : value
        } catch (error) {
            if (defaultValue !== undefined) {
                if (error instanceof InvalidKeyError) {
                    return defaultValue
                }
            }
            throw error
        }
    }

    set(key: string, value) {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.set(key, value)
    }

    has(key: string) {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.has(key)
    }

    reset(key: string) {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.reset(key)
    }

    remove(key: string) {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.remove(key)
    }

    resetActiveProfile() {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        this.#activeProfile.restoreDefaultConfig(false)
    }
}

class ConfigProfile {
    #propertyListener: EventDispatcher = null
    #config: JsonProfileConfig = null

    constructor(config: JsonProfileConfig) {
        if (config === undefined || config === null)
            throw new Error("config is null")

        this.#propertyListener = new EventDispatcher()
        this.#config = config
    }

    get profileId() {
        return this.#config.profile.id
    }

    get profileName() {
        return this.#config.profile.name
    }

    set profileName(name: string) {
        this.set("profile.name", name)
    }

    get config() {
        return this.#config
    }

    copyFrom(config: ConfigProfile, rootKey: string, copyAll: boolean = false) {
        const srcValue = config.get(rootKey)
        const srcRoot = srcValue != null ? copyByJson(srcValue) : null
        if (!this.has(rootKey)) {
            this.set(rootKey, srcRoot)
            return
        }

        let dstRoot = this.get(rootKey)
        const [removedKeys, _] = copyAll ? removeMissingObjects(srcRoot, dstRoot, p => false) : writeObject(srcRoot, dstRoot, false, p => p === "profile")
        const [writtenKeys, replace] = copyAll ? writeObject(srcRoot, dstRoot, false, p => false) : writeObject(srcRoot, dstRoot, false, p => p === "profile")
        if (replace)
            dstRoot = srcRoot

        const changes = removedKeys.concat(writtenKeys)

        this.set(rootKey, dstRoot)
        this.#firePropertyChanges(changes)
    }

    restoreDefaultConfig(fireChanges: boolean = true) {
        const oldConfig = this.#config
        this.#config = copyByJson(Gobchat.DefaultProfileConfig)
        this.#config.profile = copyByJson(oldConfig.profile)

        if (fireChanges) {
            const [changes, ...x] = writeObject(Gobchat.DefaultProfileConfig, oldConfig, false, p => p === "profile")
            this.#firePropertyChanges(changes)
        }
    }

    get(key: string, defaultValue: object = null) {
        if (key === null || key.length === 0) {
            return this.#config
        }
        try {
            const value = resolvePath(key, this.#config)
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

    has(key: string) {
        return this.get(key, undefined) !== undefined;
    }

    set(key: string, value: any) {
        if (key === null || key.length === 0) {
            this.#config = <JsonProfileConfig> value
        }
        resolvePath(key, this.#config, value)
        this.#firePropertyChange(key)
    }

    reset(key: string) {
        if (key === null || key.length === 0)
            return

        const original = resolvePath(key, Gobchat.DefaultProfileConfig)
        resolvePath(key, this.#config, original)
        this.#firePropertyChange(key)
    }

    remove(key: string) {
        if (key === null || key.length === 0)
            return
        resolvePath(key, this.#config, undefined, true)
        this.#firePropertyChange(key)
    }

    addPropertyListener(topic: string, callback) {
        this.#propertyListener.on(topic, callback)
    }

    removePropertyListener(topic: string, callback) {
        this.#propertyListener.off(topic, callback)
    }

    #firePropertyChange(propertyPath: string) {
        this.#firePropertyChanges([propertyPath])
    }

    #firePropertyChanges(propertyPaths: string[]) {
        const splitted = new Set<string>()

        propertyPaths.forEach(propertyPath => {
            if (splitted.has(propertyPath))
                return

            splitted.add(propertyPath)
            let indexOffset = propertyPath.length
            while (true) {
                indexOffset = propertyPath.lastIndexOf(".", indexOffset - 1) // lastIndexOf includes the given end index
                if (indexOffset === -1)
                    break

                const path = propertyPath.substring(0, indexOffset)
                if (splitted.has(path))
                    break

                splitted.add(path)
            }
        })

        const sorted: string[] = Array.from(splitted)

        sorted.sort((a, b) => {
            if (a.startsWith(b))
                return 1;
            if (b.startsWith(a))
                return -1;
            return a.localeCompare(b)
        })

        sorted.forEach((propertyPath) => {
            const data = { "key": propertyPath, "source": this.profileId }
            this.#propertyListener.dispatch(propertyPath, data)
            this.#propertyListener.dispatch("*", data)
        })

        //this.#propertyListener.dispatch("*", { "key": "*", "source": this.profileId })
    }
}
