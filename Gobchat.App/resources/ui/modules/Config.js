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
'use strict';
var __classPrivateFieldSet = (this && this.__classPrivateFieldSet) || function (receiver, state, value, kind, f) {
    if (kind === "m") throw new TypeError("Private method is not writable");
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a setter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot write private member to an object whose class did not declare it");
    return (kind === "a" ? f.call(receiver, value) : f ? f.value = value : state.set(receiver, value)), value;
};
var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var _GobchatConfig_instances, _GobchatConfig_eventDispatcher, _GobchatConfig_defaultProfile, _GobchatConfig_activeProfile, _GobchatConfig_activeProfileId, _GobchatConfig_profiles, _GobchatConfig_isSynced, _GobchatConfig_OnPropertyChange, _GobchatConfig_loadConfig, _GobchatConfig_generateId, _GobchatConfig_saveConfig, _GobchatConfig_storeNewProfile, _ConfigProfile_instances, _ConfigProfile_propertyListener, _ConfigProfile_config, _ConfigProfile_firePropertyChange, _ConfigProfile_firePropertyChanges;
//requieres Gobchat.DefaultProfileConfig, uploaded by backend
import * as Utility from './CommonUtility.js';
import { EventDispatcher } from './EventDispatcher.js';
/**
 *
 * @param map
 * @param allowedKeys
 */
function removeInvalidKeys(map, allowedKeys) {
    const availableKeys = Object.keys(map);
    const invalidKeys = availableKeys.filter((k) => { return _.indexOf(allowedKeys, k) === -1; });
    invalidKeys.forEach((k) => { delete map[k]; }); //remove keys which are not allowed
}
/**
 * removes every value from data that is the same as in 	extendedData
 * @param data
 * @param extendedData
 */
function retainChangesIterator(data, extendedData) {
    const callbackHelper = {
        onArray: function (data, extendedData) {
            //return _.isEqual(data,extendedData) //same objects can be removed
            return _.isEqual(_.sortBy(data), _.sortBy(extendedData)); //same objects can be removed
        },
        onCompare: function (data, extendedData) {
            return data == extendedData; //same objects can be removed
        },
        onObject: function (data, extendedData, callbackHelper) {
            removeInvalidKeys(data, Object.keys(extendedData));
            for (let key of Object.keys(data)) {
                if (objectTreeIteratorHelper(data[key], extendedData[key], callbackHelper)) //delete on true
                    delete data[key];
            }
            return Object.keys(data).length == 0; //if data is empty, it can be deleted
        }
    };
    objectTreeIteratorHelper(data, extendedData, callbackHelper);
}
/**
 *
 * @param source
 * @param destination
 * @param ignoreFunc
 */
function removeMissingObjects(source, destination, ignoreFunc = null) {
    var path = [];
    var changes = new Set();
    const callbacks = {
        onArray: function (source, destination) {
            return false;
        },
        onCompare: function (source, destination) {
            return false;
        },
        onObject: function (source, destination) {
            const availableKeys = Object.keys(destination);
            const allowedKeys = Object.keys(source);
            const keysToRemove = availableKeys.filter((k) => { return !_.includes(allowedKeys, k); });
            for (let key of keysToRemove) {
                path.push(key);
                const fullPath = path.join(".");
                if (!ignoreFunc(fullPath)) {
                    delete destination[key];
                    changes.add(fullPath);
                }
                path.pop();
            }
            for (let key of Object.keys(destination)) {
                path.push(key);
                const fullPath = path.join(".");
                if (!ignoreFunc(fullPath)) {
                    objectTreeIteratorHelper(source[key], destination[key], callbacks);
                }
                path.pop();
            }
            return false;
        }
    };
    const needsToBeReplaced = objectTreeIteratorHelper(source, destination, callbacks);
    return [Array.from(changes), needsToBeReplaced];
}
/**
 * Will merge every value from source into destination
 * @param source
 * @param destination
 * @param copyOnWrite
 * @param ignoreFunc
 */
function writeObject(source, destination, copyOnWrite = false, ignoreFunc = null) {
    var path = [];
    var changes = new Set();
    const callbacks = {
        onArray: function (source, destination) {
            return true; //lazy, just merge
        },
        onCompare: function (source, destination) {
            return true; //lazy, just merge
        },
        onObject: function (source, destination) {
            for (let key of Object.keys(source)) {
                path.push(key);
                const fullPath = path.join(".");
                if (!ignoreFunc(fullPath)) {
                    if (!(key in destination)) {
                        if (copyOnWrite)
                            destination[key] = copyByJson(source[key]);
                        else
                            destination[key] = source[key];
                        changes.add(fullPath);
                    }
                    else {
                        if (objectTreeIteratorHelper(source[key], destination[key], callbacks)) { //merge on true
                            if (copyOnWrite)
                                destination[key] = copyByJson(source[key]);
                            else
                                destination[key] = source[key];
                            changes.add(fullPath);
                        }
                    }
                }
                path.pop();
            }
            return false;
        }
    };
    const needsToBeReplaced = objectTreeIteratorHelper(source, destination, callbacks);
    return [Array.from(changes), needsToBeReplaced];
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
            return callbacks.onArray(objA, objB, callbacks);
        }
        else {
            return false; //invalid
        }
    }
    else if (Utility.isArray(objB)) {
        return false; //invalid
    }
    else if (Utility.isObject(objA)) {
        if (Utility.isObject(objB)) {
            return callbacks.onObject(objA, objB, callbacks);
        }
        else {
            return false; //invalid
        }
    }
    else if (Utility.isObject(objB)) {
        return false; //invalid
    }
    else {
        return callbacks.onCompare(objA, objB, callbacks);
    }
}
/**
 * @param key
 */
function breakKeyDown(key) {
    if (key == undefined || key == null || key.length === 0)
        return [];
    const parts = key.split(".");
    return parts;
}
/**
 * @param key
 * @param config
 */
function buildPath(key, config) {
    let _config = config;
    const keySteps = breakKeyDown(key);
    for (let i = 0; i < keySteps.length - 1; ++i) {
        const keyStep = keySteps[i];
        if (!(keyStep in _config)) {
            _config[keyStep] = {};
        }
        _config = _config[keyStep];
    }
}
/**
 *
 * @param key
 * @param config
 * @param value
 * @param remove
 */
function resolvePath(key, config, value = undefined, remove = false) {
    const keySteps = breakKeyDown(key);
    for (let i = 0; i < keySteps.length - 1; ++i) {
        const keyStep = keySteps[i];
        if (keyStep in config) {
            config = config[keyStep];
        }
        else {
            throw new InvalidKeyError(`Config error. Key '${key}' invalid. Unable to ${remove !== undefined ? `set` : `get`} data at '${keyStep}'`);
        }
    }
    if (keySteps.length === 0)
        return config;
    const targetKey = keySteps[keySteps.length - 1];
    if (value !== undefined)
        config[targetKey] = value !== null ? copyByJson(value) : null;
    if (remove)
        delete config[targetKey];
    return config[targetKey];
}
/**
 *
 * @param source
 * @param key
 * @param destination
 * @param doCopy
 */
function copyValueForKey(source, key, destination, doCopy) {
    let val = resolvePath(key, source);
    if (doCopy)
        val = copyByJson(val);
    buildPath(key, destination);
    resolvePath(key, destination, val);
}
/**
 *
 * @param obj
 */
function copyByJson(obj) {
    if (obj === undefined || obj === null)
        throw new Error("Value is null");
    return JSON.parse(JSON.stringify(obj));
}
export class InvalidKeyError extends Error {
    constructor(message) {
        super(message);
        this.name = "InvalidKeyError";
    }
}
export class GobchatConfig {
    constructor(isSynced = false) {
        _GobchatConfig_instances.add(this);
        _GobchatConfig_eventDispatcher.set(this, void 0);
        _GobchatConfig_defaultProfile.set(this, void 0);
        _GobchatConfig_activeProfile.set(this, void 0);
        _GobchatConfig_activeProfileId.set(this, void 0);
        _GobchatConfig_profiles.set(this, void 0);
        _GobchatConfig_isSynced.set(this, false);
        _GobchatConfig_OnPropertyChange.set(this, (event) => {
            const isActiveProfile = event.source === this.activeProfile;
            __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").dispatch(`property:${event.key}`, { type: "property", key: event.key, sourceProfileId: event.source, isActiveProfile: isActiveProfile });
        });
        __classPrivateFieldSet(this, _GobchatConfig_eventDispatcher, new EventDispatcher(), "f");
        if (Gobchat.DefaultProfileConfig === undefined || Gobchat.DefaultProfileConfig === null)
            throw new Error("Gobchat.DefaultProfileConfig is null");
        __classPrivateFieldSet(this, _GobchatConfig_defaultProfile, copyByJson(Gobchat.DefaultProfileConfig), "f");
        __classPrivateFieldSet(this, _GobchatConfig_activeProfileId, null, "f");
        __classPrivateFieldSet(this, _GobchatConfig_activeProfile, null, "f");
        __classPrivateFieldSet(this, _GobchatConfig_profiles, {}, "f");
        __classPrivateFieldSet(this, _GobchatConfig_isSynced, isSynced, "f");
        if (__classPrivateFieldGet(this, _GobchatConfig_isSynced, "f")) {
            document.addEventListener("SynchronizeConfigEvent", (e) => { this.loadConfig(); });
        }
    }
    async loadConfig() {
        const dataJson = await GobchatAPI.getConfigAsJson();
        __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_loadConfig).call(this, dataJson);
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
        const dataJson = __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_saveConfig).call(this);
        await GobchatAPI.synchronizeConfig(dataJson);
    }
    get activeProfile() {
        return __classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f");
    }
    set activeProfile(profileId) {
        if (__classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f") === profileId)
            return;
        if (!__classPrivateFieldGet(this, _GobchatConfig_profiles, "f")[profileId])
            throw new Error("Invalid profile id: " + profileId);
        const previousId = __classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f");
        __classPrivateFieldSet(this, _GobchatConfig_activeProfileId, profileId, "f");
        __classPrivateFieldSet(this, _GobchatConfig_activeProfile, __classPrivateFieldGet(this, _GobchatConfig_profiles, "f")[__classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f")], "f");
        __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").dispatch("profile:", { type: "profile", action: "active", oldProfileId: previousId, newProfileId: __classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f") });
        if (__classPrivateFieldGet(this, _GobchatConfig_isSynced, "f"))
            GobchatAPI.setConfigActiveProfile(__classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f"));
    }
    get profiles() {
        return Object.keys(__classPrivateFieldGet(this, _GobchatConfig_profiles, "f"));
    }
    importProfile(profileJson) {
        const cleanProfile = copyByJson(__classPrivateFieldGet(this, _GobchatConfig_defaultProfile, "f"));
        writeObject(profileJson, cleanProfile, false, (p) => false);
        const profileId = __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_generateId).call(this);
        cleanProfile.profile.id = profileId;
        __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_storeNewProfile).call(this, cleanProfile);
        return profileId;
    }
    createNewProfile() {
        const profileId = __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_generateId).call(this);
        const newProfileData = copyByJson(__classPrivateFieldGet(this, _GobchatConfig_defaultProfile, "f"));
        newProfileData.profile.id = profileId;
        newProfileData.profile.name = `Profile ${this.profiles.length + 1}`;
        __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_storeNewProfile).call(this, newProfileData);
        return profileId;
    }
    deleteProfile(profileId) {
        if (this.profiles.length <= 1)
            return;
        if (!_.includes(this.profiles, profileId))
            return;
        delete __classPrivateFieldGet(this, _GobchatConfig_profiles, "f")[profileId];
        if (this.activeProfile === profileId)
            this.activeProfile = this.profiles[0];
        __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").dispatch("profile:", { type: "profile", action: "delete", profileId: profileId });
    }
    copyProfile(sourceProfileId, destinationProfileId) {
        const sourceProfile = this.getProfile(sourceProfileId);
        const destinationProfile = this.getProfile(destinationProfileId);
        if (!sourceProfile || !destinationProfile)
            return;
        destinationProfile.copyFrom(sourceProfile, "");
    }
    getProfile(profileId) {
        const profile = __classPrivateFieldGet(this, _GobchatConfig_profiles, "f")[profileId];
        if (profile === undefined)
            return null;
        return profile;
    }
    //TODO remove later
    saveToLocalStore() {
        const json = __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_saveConfig).call(this);
        window.localStorage.setItem("gobchat-config", json);
    }
    //TODO remove later
    loadFromLocalStore(keepLocaleStore) {
        const json = window.localStorage.getItem("gobchat-config");
        if (!keepLocaleStore)
            window.localStorage.removeItem("gobchat-config");
        if (json === undefined || json === null)
            return;
        //TODO toggle sync, so the change is not immediately propagated back
        const isSynced = __classPrivateFieldGet(this, _GobchatConfig_isSynced, "f");
        __classPrivateFieldSet(this, _GobchatConfig_isSynced, false, "f");
        try {
            __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_loadConfig).call(this, json);
        }
        finally {
            __classPrivateFieldSet(this, _GobchatConfig_isSynced, isSynced, "f");
        }
        if (__classPrivateFieldGet(this, _GobchatConfig_isSynced, "f"))
            this.saveConfig();
    }
    addProfileEventListener(callback) {
        return __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").on("profile:", callback);
    }
    removeProfileEventListener(callback) {
        return __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").off("profile:", callback);
    }
    addPropertyEventListener(topic, callback) {
        return __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").on("property:" + topic, callback);
    }
    removePropertyEventListener(topic, callback) {
        return __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").off("property:" + topic, callback);
    }
    get(key, defaultValue) {
        if (!__classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f"))
            throw new Error("No active profile");
        return __classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f").get(key, defaultValue);
    }
    getDefault(key, defaultValue) {
        try {
            const value = resolvePath(key, __classPrivateFieldGet(this, _GobchatConfig_defaultProfile, "f"));
            return value === undefined ? defaultValue : value !== null ? copyByJson(value) : value;
        }
        catch (error) {
            if (defaultValue !== undefined) {
                if (error instanceof InvalidKeyError) {
                    return defaultValue;
                }
            }
            throw error;
        }
    }
    set(key, value) {
        if (!__classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f"))
            throw new Error("No active profile");
        __classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f").set(key, value);
    }
    has(key) {
        if (!__classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f"))
            throw new Error("No active profile");
        return __classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f").has(key);
    }
    reset(key) {
        if (!__classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f"))
            throw new Error("No active profile");
        return __classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f").reset(key);
    }
    remove(key) {
        if (!__classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f"))
            throw new Error("No active profile");
        return __classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f").remove(key);
    }
    resetActiveProfile() {
        if (!__classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f"))
            throw new Error("No active profile");
        __classPrivateFieldGet(this, _GobchatConfig_activeProfile, "f").restoreDefaultConfig(false);
    }
}
_GobchatConfig_eventDispatcher = new WeakMap(), _GobchatConfig_defaultProfile = new WeakMap(), _GobchatConfig_activeProfile = new WeakMap(), _GobchatConfig_activeProfileId = new WeakMap(), _GobchatConfig_profiles = new WeakMap(), _GobchatConfig_isSynced = new WeakMap(), _GobchatConfig_OnPropertyChange = new WeakMap(), _GobchatConfig_instances = new WeakSet(), _GobchatConfig_loadConfig = function _GobchatConfig_loadConfig(json) {
    const data = JSON.parse(json);
    const loadedProfileIds = Object.keys(data.profiles);
    const availableProfileIds = this.profiles;
    const newProfileIds = loadedProfileIds.filter(e => !_.includes(availableProfileIds, e));
    const changedProfileIds = loadedProfileIds.filter(e => _.includes(availableProfileIds, e));
    const deletedProfileIds = availableProfileIds.filter(e => !_.includes(loadedProfileIds, e));
    newProfileIds.forEach(profileId => {
        const profileData = data.profiles[profileId];
        const cleanProfile = copyByJson(__classPrivateFieldGet(this, _GobchatConfig_defaultProfile, "f"));
        writeObject(profileData, cleanProfile, false, (p) => false);
        __classPrivateFieldGet(this, _GobchatConfig_instances, "m", _GobchatConfig_storeNewProfile).call(this, cleanProfile);
    });
    this.activeProfile = data.activeProfile;
    deletedProfileIds.forEach(profileId => {
        this.deleteProfile(profileId);
    });
    changedProfileIds.forEach(profileId => {
        const cleanProfile = copyByJson(__classPrivateFieldGet(this, _GobchatConfig_defaultProfile, "f"));
        const profileData = data.profiles[profileId];
        writeObject(profileData, cleanProfile, false, (p) => false);
        const readThis = new ConfigProfile(cleanProfile);
        this.getProfile(profileId).copyFrom(readThis, "", true);
    });
}, _GobchatConfig_generateId = function _GobchatConfig_generateId() {
    const ids = this.profiles;
    let id = null;
    do {
        id = Utility.generateId(8);
    } while (_.includes(ids, id));
    return id;
}, _GobchatConfig_saveConfig = function _GobchatConfig_saveConfig() {
    const data = {
        activeProfile: __classPrivateFieldGet(this, _GobchatConfig_activeProfileId, "f"),
        profiles: {}
    };
    Object.keys(__classPrivateFieldGet(this, _GobchatConfig_profiles, "f")).forEach((profileId) => {
        const profile = this.getProfile(profileId);
        data.profiles[profileId] = profile.config;
    });
    const json = JSON.stringify(data);
    return json;
}, _GobchatConfig_storeNewProfile = function _GobchatConfig_storeNewProfile(data) {
    const profile = new ConfigProfile(data);
    const profileId = profile.profileId;
    profile.addPropertyListener("*", __classPrivateFieldGet(this, _GobchatConfig_OnPropertyChange, "f"));
    __classPrivateFieldGet(this, _GobchatConfig_profiles, "f")[profileId] = profile;
    __classPrivateFieldGet(this, _GobchatConfig_eventDispatcher, "f").dispatch("profile:", { type: "profile", action: "new", profileId: profileId });
};
class ConfigProfile {
    constructor(config) {
        _ConfigProfile_instances.add(this);
        _ConfigProfile_propertyListener.set(this, null);
        _ConfigProfile_config.set(this, null);
        if (config === undefined || config === null)
            throw new Error("config is null");
        __classPrivateFieldSet(this, _ConfigProfile_propertyListener, new EventDispatcher(), "f");
        __classPrivateFieldSet(this, _ConfigProfile_config, config, "f");
    }
    get profileId() {
        return __classPrivateFieldGet(this, _ConfigProfile_config, "f").profile.id;
    }
    get profileName() {
        return __classPrivateFieldGet(this, _ConfigProfile_config, "f").profile.name;
    }
    set profileName(name) {
        this.set("profile.name", name);
    }
    get config() {
        return __classPrivateFieldGet(this, _ConfigProfile_config, "f");
    }
    copyFrom(config, rootKey, copyAll = false) {
        const srcValue = config.get(rootKey);
        const srcRoot = srcValue != null ? copyByJson(srcValue) : null;
        if (!this.has(rootKey)) {
            this.set(rootKey, srcRoot);
            return;
        }
        let dstRoot = this.get(rootKey);
        const [removedKeys, _] = copyAll ? removeMissingObjects(srcRoot, dstRoot, p => false) : writeObject(srcRoot, dstRoot, false, p => p === "profile");
        const [writtenKeys, replace] = copyAll ? writeObject(srcRoot, dstRoot, false, p => false) : writeObject(srcRoot, dstRoot, false, p => p === "profile");
        if (replace)
            dstRoot = srcRoot;
        const changes = removedKeys.concat(writtenKeys);
        this.set(rootKey, dstRoot);
        __classPrivateFieldGet(this, _ConfigProfile_instances, "m", _ConfigProfile_firePropertyChanges).call(this, changes);
    }
    restoreDefaultConfig(fireChanges = true) {
        const oldConfig = __classPrivateFieldGet(this, _ConfigProfile_config, "f");
        __classPrivateFieldSet(this, _ConfigProfile_config, copyByJson(Gobchat.DefaultProfileConfig), "f");
        __classPrivateFieldGet(this, _ConfigProfile_config, "f").profile = copyByJson(oldConfig.profile);
        if (fireChanges) {
            const [changes, ...x] = writeObject(Gobchat.DefaultProfileConfig, oldConfig, false, p => p === "profile");
            __classPrivateFieldGet(this, _ConfigProfile_instances, "m", _ConfigProfile_firePropertyChanges).call(this, changes);
        }
    }
    get(key, defaultValue = null) {
        if (key === null || key.length === 0) {
            return __classPrivateFieldGet(this, _ConfigProfile_config, "f");
        }
        try {
            const value = resolvePath(key, __classPrivateFieldGet(this, _ConfigProfile_config, "f"));
            return value === undefined ? defaultValue : value;
        }
        catch (error) {
            if (defaultValue !== undefined) {
                if (error instanceof InvalidKeyError) {
                    return defaultValue;
                }
            }
            throw error;
        }
    }
    has(key) {
        return this.get(key, undefined) !== undefined;
    }
    set(key, value) {
        if (key === null || key.length === 0) {
            __classPrivateFieldSet(this, _ConfigProfile_config, value, "f");
        }
        resolvePath(key, __classPrivateFieldGet(this, _ConfigProfile_config, "f"), value);
        __classPrivateFieldGet(this, _ConfigProfile_instances, "m", _ConfigProfile_firePropertyChange).call(this, key);
    }
    reset(key) {
        if (key === null || key.length === 0)
            return;
        const original = resolvePath(key, Gobchat.DefaultProfileConfig);
        resolvePath(key, __classPrivateFieldGet(this, _ConfigProfile_config, "f"), original);
        __classPrivateFieldGet(this, _ConfigProfile_instances, "m", _ConfigProfile_firePropertyChange).call(this, key);
    }
    remove(key) {
        if (key === null || key.length === 0)
            return;
        resolvePath(key, __classPrivateFieldGet(this, _ConfigProfile_config, "f"), undefined, true);
        __classPrivateFieldGet(this, _ConfigProfile_instances, "m", _ConfigProfile_firePropertyChange).call(this, key);
    }
    addPropertyListener(topic, callback) {
        __classPrivateFieldGet(this, _ConfigProfile_propertyListener, "f").on(topic, callback);
    }
    removePropertyListener(topic, callback) {
        __classPrivateFieldGet(this, _ConfigProfile_propertyListener, "f").off(topic, callback);
    }
}
_ConfigProfile_propertyListener = new WeakMap(), _ConfigProfile_config = new WeakMap(), _ConfigProfile_instances = new WeakSet(), _ConfigProfile_firePropertyChange = function _ConfigProfile_firePropertyChange(propertyPath) {
    __classPrivateFieldGet(this, _ConfigProfile_instances, "m", _ConfigProfile_firePropertyChanges).call(this, [propertyPath]);
}, _ConfigProfile_firePropertyChanges = function _ConfigProfile_firePropertyChanges(propertyPaths) {
    const splitted = new Set();
    propertyPaths.forEach(propertyPath => {
        if (splitted.has(propertyPath))
            return;
        splitted.add(propertyPath);
        let indexOffset = propertyPath.length;
        while (true) {
            indexOffset = propertyPath.lastIndexOf(".", indexOffset - 1); // lastIndexOf includes the given end index
            if (indexOffset === -1)
                break;
            const path = propertyPath.substring(0, indexOffset);
            if (splitted.has(path))
                break;
            splitted.add(path);
        }
    });
    const sorted = Array.from(splitted);
    sorted.sort((a, b) => {
        if (a.startsWith(b))
            return 1;
        if (b.startsWith(a))
            return -1;
        return a.localeCompare(b);
    });
    sorted.forEach((propertyPath) => {
        const data = { "key": propertyPath, "source": this.profileId };
        __classPrivateFieldGet(this, _ConfigProfile_propertyListener, "f").dispatch(propertyPath, data);
        __classPrivateFieldGet(this, _ConfigProfile_propertyListener, "f").dispatch("*", data);
    });
    //this.#propertyListener.dispatch("*", { "key": "*", "source": this.profileId })
};
