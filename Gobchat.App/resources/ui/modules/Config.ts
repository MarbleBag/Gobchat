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

type JToken = JValue | JArray | JObject
type JValue = number | string
type JArray = [JToken]
type JObject = { [key: string]: JToken }

/**
 * 
 * @param map
 * @param allowedKeys
 */
function removeInvalidKeys(map: JObject, allowedKeys: string[]) {
    const availableKeys = Object.keys(map)
    const invalidKeys = availableKeys.filter((k) => { return _.indexOf(allowedKeys, k) === -1 })
    invalidKeys.forEach((k) => { delete map[k] }) //remove keys which are not allowed
}

/**
 * removes every value from data that is the same as in 	extendedData
 * @param data
 * @param extendedData
 */
function retainChangesIterator(data: JObject, extendedData: JObject) {
    const callbackHelper: TreeIteratorCallback = {
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

    objectTreeIteratorHelper(data, extendedData, callbackHelper)
}

/**
 * 
 * @param source
 * @param destination
 * @param ignoreFunc
 */
function removeMissingObjects(source: JObject, destination: JObject, ignoreFunc: (key: string) => boolean): [changedKeys: string[], replaceTree: boolean] {
    var path: string[] = [];
    var changes = new Set<string>();

    const callbacks: TreeIteratorCallback = {
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
 * @param source
 * @param destination
 * @param copyOnWrite
 * @param ignoreFunc
 */
function writeObject(source: JObject, destination: JObject, copyOnWrite: boolean = false, ignoreFunc: (key: string) => boolean): [changedKeys: string[], replaceTree: boolean] {
    var path: string[] = [];
    var changes = new Set<string>();

    const callbacks: TreeIteratorCallback = {
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

interface TreeIteratorCallback {
    onArray(a: JArray, b: JArray, c: TreeIteratorCallback): boolean
    onObject(a: JObject, b: JObject, c: TreeIteratorCallback): boolean
    onCompare(a: JToken, b: JToken, c: TreeIteratorCallback): boolean
}

/**
 * 
 * @param objA
 * @param objB
 * @param callbacks
 */
function objectTreeIteratorHelper(objA: JToken, objB: JToken, callbacks: TreeIteratorCallback) {
    if (Utility.isArray(objA)) {
        if (Utility.isArray(objB)) {
            return callbacks.onArray(objA as JArray, objB as JArray, callbacks)
        } else {
            return false //invalid
        }
    } else if (Utility.isArray(objB)) {
        return false //invalid
    } else if (Utility.isObject(objA)) {
        if (Utility.isObject(objB)) {
            return callbacks.onObject(objA as JObject, objB as JObject, callbacks)
        } else {
            return false //invalid
        }
    } else if (Utility.isObject(objB)) {
        return false //invalid
    } else {
        return callbacks.onCompare(objA, objB, callbacks)
    }
}



/**
 * @param key
 */
function breakKeyDown(key: string | null): string[] {
    if (key == undefined || key == null || key.length === 0)
        return []
    const parts = key.split(".")
    return parts
}

/**
 * @param key
 * @param config
 */
function buildPath(key: string, config: object) {
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
 * @param key
 * @param config
 * @param value
 * @param remove
 */
function resolvePath(key: string | null, config: object, value?: object, remove: boolean = false): any {
    const keySteps = breakKeyDown(key)

    for (let i = 0; i < keySteps.length - 1; ++i) {
        const keyStep = keySteps[i]
        if (keyStep in config) {
            config = config[keyStep]
        } else {
            throw new InvalidKeyError(`Config error. Key '${key}' invalid. Unable to ${remove !== undefined ? `set` : `get`} data at '${keyStep}'`);
        }
    }

    if (keySteps.length === 0)
        return config


    const targetKey = keySteps[keySteps.length - 1]
    if (value !== undefined)
        config[targetKey] = value !== null ? copyByJson(value) : null

    if (remove)
        delete config[targetKey]

    return config[targetKey]
}

/**
 * 
 * @param source
 * @param key
 * @param destination
 * @param doCopy
 */
function copyValueForKey(source: object, key: string, destination: object, doCopy: boolean) {
    let val = resolvePath(key, source)
    if (doCopy)
        val = copyByJson(val)
    buildPath(key, destination)
    resolvePath(key, destination, val)
}

/**
 * 
 * @param obj
 */
function copyByJson<T>(obj: T): T {
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

export interface ActiveProfileConfigEvent {
    config: GobchatConfig
    type: "profile"
    action: "active"
    oldProfileId: string | null
    newProfileId: string
}

export interface ModifyProfileConfigEvent {
    config: GobchatConfig
    type: "profile"
    action: "new" | "delete"
    profileId: string
}

export interface PropertyConfigEvent {
    config: GobchatConfig
    type: "property"
    key: string
    sourceProfileId: string
    isActiveProfile: boolean
}

export type ProfileConfigEvent = ActiveProfileConfigEvent | ModifyProfileConfigEvent
export type GobchatConfigEvent = ProfileConfigEvent | PropertyConfigEvent
export type GobchatConfigListener<A extends GobchatConfigEvent> = (evt: A) => void

/*
export type GobchatConfigEvent = {
    type: "profile"
    action: "active"
    oldProfileId: string
    newProfileId: string
} | {
    type: "profile"
    action: "new" | "delete"
    profileId: string
} | {
    type: "property"
    key: string
    sourceProfileId: string
    isActiveProfile: boolean
}
*/

interface JsonConfigProfile extends JObject {
    profile: { id: string, name: string }
}

export class GobchatConfig {
    #eventDispatcher: EventDispatcher<GobchatConfigEvent>
    #defaultProfile: JsonConfigProfile
    #activeProfile: ConfigProfile | null
    #activeProfileId: string | null
    #profiles: { [id: string]: ConfigProfile }
    #isSynced: boolean = false

    constructor(isSynced: boolean = false) {
        this.#eventDispatcher = new EventDispatcher()
        if (Gobchat.DefaultProfileConfig === undefined || Gobchat.DefaultProfileConfig === null)
            throw new Error("Gobchat.DefaultProfileConfig is null")
        this.#defaultProfile = copyByJson(Gobchat.DefaultProfileConfig)

        this.#activeProfileId = null
        this.#activeProfile = null
        this.#profiles = {}
        this.#isSynced = isSynced

        if (this.#isSynced) {
            document.addEventListener("SynchronizeConfigEvent", (e) => { this.loadConfig() })
        }
    }

    #OnPropertyChange = (event: ConfigProfileEvent) => { //binded to this
        const isActiveProfile = event.source === this.activeProfileId
        this.#eventDispatcher.dispatch(`property:${event.key}`, { config: this,  type: "property", key: event.key, sourceProfileId: event.source, isActiveProfile: isActiveProfile })
    }

    #loadConfig(json: string) {
        const data = JSON.parse(json) as {
            profiles: { [key: string]: JsonConfigProfile },
            activeProfile: string
        }

        const loadedProfileIds = Object.keys(data.profiles)
        const availableProfileIds = this.profileIds
        const newProfileIds = loadedProfileIds.filter(e => !_.includes(availableProfileIds, e))
        const changedProfileIds = loadedProfileIds.filter(e => _.includes(availableProfileIds, e))
        const deletedProfileIds = availableProfileIds.filter(e => !_.includes(loadedProfileIds, e))

        newProfileIds.forEach(profileId => {
            const profileData = data.profiles[profileId]
            const cleanProfile = copyByJson(this.#defaultProfile)
            writeObject(profileData, cleanProfile, false, (p) => false)
            this.#storeNewProfile(cleanProfile)
        })

        this.activeProfileId = data.activeProfile

        deletedProfileIds.forEach(profileId => {
            this.deleteProfile(profileId)
        })

        changedProfileIds.forEach(profileId => {
            const cleanProfile = copyByJson(this.#defaultProfile)
            const profileData = data.profiles[profileId]
            writeObject(profileData, cleanProfile, false, (p) => false)

            const readThis = new ConfigProfile(cleanProfile)
            this.getProfile(profileId)?.copyFrom(readThis, "", true)
        })
    }

    #generateId(): string {
        const ids = this.profileIds
        let id: string
        do {
            id = Utility.generateId(8)
        } while (_.includes(ids, id))
        return id
    }

    #saveConfig(): string {
        const data = {
            activeProfile: this.#activeProfileId,
            profiles: {}
        }

        Object.keys(this.#profiles).forEach((profileId) => {
            const profile = this.getProfile(profileId)
            if (profile)
                data.profiles[profileId] = profile.config
        })

        const json = JSON.stringify(data)
        return json
    }

    async loadConfig(): Promise<void> {
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

    async saveConfig(): Promise<void> {
        const dataJson = this.#saveConfig()
        await GobchatAPI.synchronizeConfig(dataJson)
    }


    get activeProfile(): ConfigProfile | null {
        return this.#activeProfile
    }

    get activeProfileId(): string | null {
        return this.#activeProfileId
    }

    set activeProfileId(profileId: string | null) {
        if (this.#activeProfileId === profileId || profileId === null)
            return

        if (!this.#profiles[profileId])
            throw new Error("Invalid profile id: " + profileId)

        const previousId = this.#activeProfileId
        this.#activeProfileId = profileId
        this.#activeProfile = this.#profiles[this.#activeProfileId]

        this.#eventDispatcher.dispatch("profile:", { config:this, type: "profile", action: "active", oldProfileId: previousId, newProfileId: this.#activeProfileId })

        if (this.#isSynced)
            GobchatAPI.setConfigActiveProfile(this.#activeProfileId)
    }

    get profileIds(): string[] {
        return Object.keys(this.#profiles)
    }

    importProfile(profileJson: JsonConfigProfile): string {
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
        newProfileData.profile.name = `Profile ${this.profileIds.length + 1}`
        this.#storeNewProfile(newProfileData)
        return profileId
    }

    #storeNewProfile(data: JsonConfigProfile): void {
        const profile = new ConfigProfile(data)
        const profileId = profile.profileId

        profile.addPropertyListener("*", this.#OnPropertyChange)
        this.#profiles[profileId] = profile
        this.#eventDispatcher.dispatch("profile:", { config: this, type: "profile", action: "new", profileId: profileId })
    }

    deleteProfile(profileId: string): void {
        if (this.profileIds.length <= 1)
            return

        if (!_.includes(this.profileIds, profileId))
            return

        delete this.#profiles[profileId]

        if (this.activeProfileId === profileId)
            this.activeProfileId = this.profileIds[0]

        this.#eventDispatcher.dispatch("profile:", { config: this, type: "profile", action: "delete", profileId: profileId })
    }

    copyProfile(sourceProfileId: string, destinationProfileId: string): void {
        const sourceProfile = this.getProfile(sourceProfileId)
        const destinationProfile = this.getProfile(destinationProfileId)
        if (!sourceProfile || !destinationProfile)
            return
        destinationProfile.copyFrom(sourceProfile, "")
    }

    getProfile(profileId: string | null): ConfigProfile | null {
        if(profileId === null)
            return null

        const profile = this.#profiles[profileId]
        if (profile === undefined)
            return null
            
        return profile
    }

    //TODO remove later
    saveToLocalStore(): void {
        const json = this.#saveConfig()
        window.localStorage.setItem("gobchat-config", json)
    }

    //TODO remove later
    loadFromLocalStore(keepLocaleStore: boolean = false): void {
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

    addProfileEventListener(callback: GobchatConfigListener<ProfileConfigEvent>): boolean {
        const dispatcher = this.#eventDispatcher
        const isAdded = dispatcher.on("profile:", callback as GobchatConfigListener<GobchatConfigEvent>)
        return isAdded
    }

    removeProfileEventListener(callback: GobchatConfigListener<ProfileConfigEvent>): boolean {
        const dispatcher = this.#eventDispatcher
        const isRemoved = dispatcher.off("profile:", callback as GobchatConfigListener<GobchatConfigEvent>)
        return isRemoved
    }

    addPropertyEventListener(topic: string, callback: GobchatConfigListener<PropertyConfigEvent>): boolean {
        const dispatcher = this.#eventDispatcher
        const isAdded = dispatcher.on("property:" + topic, callback as GobchatConfigListener<GobchatConfigEvent>)
        return isAdded
    }

    removePropertyEventListener(topic: string, callback: GobchatConfigListener<PropertyConfigEvent>): boolean {
        const dispatcher = this.#eventDispatcher
        const isRemoved = dispatcher.off("property:" + topic, callback as GobchatConfigListener<GobchatConfigEvent>)
        return isRemoved
    }

    get(key: string | null, defaultValue?: any): any {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.get(key, defaultValue)
    }

    getDefault(key: string | null, defaultValue?: any): any {
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

    set(key: string | null, value: any): void {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        this.#activeProfile.set(key, value)
    }

    has(key: string | null): boolean {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        return this.#activeProfile.has(key)
    }

    reset(key: string | null): void {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        this.#activeProfile.reset(key)
    }

    remove(key: string | null): void {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        this.#activeProfile.remove(key)
    }

    resetActiveProfile(): void {
        if (!this.#activeProfile)
            throw new Error("No active profile")
        this.#activeProfile.restoreDefaultConfig(false)
    }
}

interface ConfigProfileEvent {
    key: string
    source: string
}

type ConfigProfileEventListener = (evt: ConfigProfileEvent) => void

export class ConfigProfile {
    #propertyListener: EventDispatcher<ConfigProfileEvent>
    #config: JsonConfigProfile

    constructor(config: JsonConfigProfile) {
        if (config === undefined || config === null)
            throw new Error("config is null")

        this.#propertyListener = new EventDispatcher()
        this.#config = config
    }

    get profileId(): string {
        return this.#config.profile.id
    }

    get profileName(): string {
        return this.#config.profile.name
    }

    set profileName(name: string) {
        this.set("profile.name", name)
    }

    get config(): JsonConfigProfile {
        return this.#config
    }

    copyFrom(config: ConfigProfile, rootKey: string, copyAll: boolean = false): void {
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

    restoreDefaultConfig(fireChanges: boolean = true): void {
        const oldConfig = this.#config
        this.#config = copyByJson(Gobchat.DefaultProfileConfig)
        this.#config.profile = copyByJson(oldConfig.profile)

        if (fireChanges) {
            const [changes, ...x] = writeObject(Gobchat.DefaultProfileConfig, oldConfig, false, p => p === "profile")
            this.#firePropertyChanges(changes)
        }
    }

    get(key: string | null, defaultValue: any = null): any {
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

    has(key: string | null): boolean {
        return this.get(key, undefined) !== undefined;
    }

    set(key: string | null, value: any): void {
        if (key === null || key.length === 0) {
            this.#config = <JsonConfigProfile>value
        }
        resolvePath(key, this.#config, value)
        this.#firePropertyChange(key)
    }

    reset(key: string | null): void {
        if (key === null || key.length === 0)
            return

        const original = resolvePath(key, Gobchat.DefaultProfileConfig)
        resolvePath(key, this.#config, original)
        this.#firePropertyChange(key)
    }

    remove(key: string | null): void {
        if (key === null || key.length === 0)
            return
        resolvePath(key, this.#config, undefined, true)
        this.#firePropertyChange(key)
    }

    addPropertyListener(topic: string, callback: ConfigProfileEventListener): void {
        this.#propertyListener.on(topic, callback)
    }

    removePropertyListener(topic: string, callback: ConfigProfileEventListener): void {
        this.#propertyListener.off(topic, callback)
    }

    #firePropertyChange(propertyPath: string | null): void {
        if(propertyPath === null)
            this.#firePropertyChanges([])
        else
            this.#firePropertyChanges([propertyPath])
    }

    #firePropertyChanges(propertyPaths: string[]): void {
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

        sorted.forEach(propertyPath => {
            const data: ConfigProfileEvent = { "key": propertyPath, "source": this.profileId }
            this.#propertyListener.dispatch(propertyPath, data)
            this.#propertyListener.dispatch("*", data)
        })

        //this.#propertyListener.dispatch("*", { "key": "*", "source": this.profileId })
    }
}
