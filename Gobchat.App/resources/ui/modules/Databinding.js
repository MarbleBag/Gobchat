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
var _BindingContext_instances, _BindingContext_bindings, _BindingContext_config, _BindingContext_loadValues, _BindingContext_loadBindings, _BindingContext_unloadBindings, _Binding_isBindActive, _Binding_initializeFunction, _Binding_doBind, _Binding_doUnbind;
export const DataAttributeConfigKey = "data-gob-configkey";
export function getConfigKey(element) {
    return $(element).attr(DataAttributeConfigKey);
}
export function setConfigKey(element, configKey) {
    return $(element).attr(DataAttributeConfigKey, configKey);
}
/**
 * Wraps the given delegate in a GobConfig listener. The delegate will be called if the active profile changes or an observed property changes
 *
 * @param delegate
 * @param profileToObserve
 * @param onlyIfProfileIsActive
 */
function createGobConfigListener(delegate, profileToObserve = null, onlyIfProfileIsActive = false) {
    return (event) => {
        switch (event.type) {
            case 'profile':
                if (event.action === 'active' && (profileToObserve === null || profileToObserve === event.newProfileId))
                    delegate(null);
                break;
            case 'property':
                if ((!onlyIfProfileIsActive || (onlyIfProfileIsActive && event.isActiveProfile)) && (profileToObserve === null || profileToObserve === event.sourceProfileId))
                    delegate(event.key);
                break;
        }
    };
}
/**
 * Adds or removes values from an array. Does not add duplicates.
 * @param {Array} array
 * @param {Array} values
 * @param {Boolean} doSet If true, adds values to array
 * @returns {Boolean} true, if the content of array changed
 */
function setValuesInArray(array, values, doSet) {
    let changed = false;
    if (doSet) {
        values.forEach((value) => {
            if (!_.includes(array, value)) {
                array.push(value);
                changed = true;
            }
        });
    }
    else {
        const removedElements = _.remove(array, (arrayValue) => {
            return _.includes(values, arrayValue);
        });
        changed = removedElements.length > 0;
    }
    return changed;
}
export class BindingContext {
    constructor(gobConfig) {
        _BindingContext_instances.add(this);
        _BindingContext_bindings.set(this, void 0);
        _BindingContext_config.set(this, void 0);
        __classPrivateFieldSet(this, _BindingContext_bindings, [], "f");
        __classPrivateFieldSet(this, _BindingContext_config, gobConfig, "f");
    }
    initialize() {
        __classPrivateFieldGet(this, _BindingContext_instances, "m", _BindingContext_loadValues).call(this);
        __classPrivateFieldGet(this, _BindingContext_instances, "m", _BindingContext_loadBindings).call(this);
        return this;
    }
    clear() {
        __classPrivateFieldGet(this, _BindingContext_instances, "m", _BindingContext_unloadBindings).call(this);
        __classPrivateFieldSet(this, _BindingContext_bindings, [], "f");
        return this;
    }
    bindElement(element, options) {
        const $element = $(element);
        const config = __classPrivateFieldGet(this, _BindingContext_config, "f");
        const defOptions = {
            disabled: false,
            elementKey: "change",
            configKey: $element.attr(DataAttributeConfigKey),
            elementGetAccessor: ($element) => $element.val(),
            elementSetAccessor: ($element, value) => $element.val(value)
        };
        options = $.extend(defOptions, options);
        if (options.disabled || options.configKey === undefined || options.configKey === null) {
            $element.attr("disabled", "true");
            return this; //done
        }
        const onElementChange = (event) => {
            let result = options.elementGetAccessor($element, event, config.get(options.configKey, null));
            if (result !== undefined)
                config.set(options.configKey, result);
        };
        const onConfigChange = () => options.elementSetAccessor($element, config.get(options.configKey, null));
        const profileListener = createGobConfigListener(onConfigChange, null, true);
        const initialize = onConfigChange;
        const bind = () => {
            //bind element
            if (options.elementKey && options.elementGetAccessor)
                $element.on(options.elementKey, onElementChange);
            //bind config
            if (options.elementSetAccessor) {
                config.addProfileEventListener(profileListener);
                config.addPropertyEventListener(options.configKey, profileListener);
            }
        };
        const unbind = () => {
            //unbind element
            if (options.elementKey && options.elementGetAccessor)
                $element.off(options.elementKey, onElementChange);
            //unbind config
            if (options.elementSetAccessor) {
                if (!config.removeProfileEventListener(profileListener))
                    console.log("Error: Databinding. Unable to remove profile listener");
                if (!config.removePropertyEventListener(options.configKey, profileListener))
                    console.log("Error: Databinding. Unable to remove property listener: " + options.configKey);
            }
        };
        const binding = new Binding(initialize, bind, unbind);
        __classPrivateFieldGet(this, _BindingContext_bindings, "f").push(binding);
        return this;
    }
    bindConfigListener(configKey, callback) {
        if (typeof configKey !== 'string')
            configKey = getConfigKey(configKey);
        if (configKey === null || configKey === undefined)
            throw new Error("'configKey' is null");
        const config = __classPrivateFieldGet(this, _BindingContext_config, "f");
        const onConfigChange = () => callback(config.get(configKey, null));
        const profileListener = createGobConfigListener(onConfigChange, null, true);
        const bind = () => {
            //bind config
            config.addProfileEventListener(profileListener);
            config.addPropertyEventListener(configKey, profileListener);
        };
        const unbind = () => {
            //unbind config
            config.removeProfileEventListener(profileListener);
            config.removePropertyEventListener(configKey, profileListener);
        };
        const binding = new Binding(onConfigChange, bind, unbind);
        __classPrivateFieldGet(this, _BindingContext_bindings, "f").push(binding);
        return this;
    }
}
_BindingContext_bindings = new WeakMap(), _BindingContext_config = new WeakMap(), _BindingContext_instances = new WeakSet(), _BindingContext_loadValues = function _BindingContext_loadValues() {
    for (let binding of __classPrivateFieldGet(this, _BindingContext_bindings, "f"))
        binding.initializeElement();
    return this;
}, _BindingContext_loadBindings = function _BindingContext_loadBindings() {
    for (let binding of __classPrivateFieldGet(this, _BindingContext_bindings, "f"))
        binding.bind();
    return this;
}, _BindingContext_unloadBindings = function _BindingContext_unloadBindings() {
    for (let binding of __classPrivateFieldGet(this, _BindingContext_bindings, "f"))
        binding.unbind();
    return this;
};
class Binding {
    constructor(initializeFunction, bindFunction, unbindFunction) {
        _Binding_isBindActive.set(this, false);
        _Binding_initializeFunction.set(this, void 0);
        _Binding_doBind.set(this, void 0);
        _Binding_doUnbind.set(this, void 0);
        __classPrivateFieldSet(this, _Binding_initializeFunction, initializeFunction, "f");
        __classPrivateFieldSet(this, _Binding_doBind, bindFunction, "f");
        __classPrivateFieldSet(this, _Binding_doUnbind, unbindFunction, "f");
    }
    get isBindActive() {
        return __classPrivateFieldGet(this, _Binding_isBindActive, "f");
    }
    initializeElement() {
        if (__classPrivateFieldGet(this, _Binding_initializeFunction, "f"))
            __classPrivateFieldGet(this, _Binding_initializeFunction, "f").call(this);
    }
    bind() {
        if (__classPrivateFieldGet(this, _Binding_isBindActive, "f"))
            return;
        __classPrivateFieldGet(this, _Binding_doBind, "f").call(this);
        __classPrivateFieldSet(this, _Binding_isBindActive, true, "f");
    }
    unbind() {
        if (!__classPrivateFieldGet(this, _Binding_isBindActive, "f"))
            return;
        __classPrivateFieldGet(this, _Binding_doUnbind, "f").call(this);
        __classPrivateFieldSet(this, _Binding_isBindActive, false, "f");
    }
}
_Binding_isBindActive = new WeakMap(), _Binding_initializeFunction = new WeakMap(), _Binding_doBind = new WeakMap(), _Binding_doUnbind = new WeakMap();
/**
 * @deprecated use new BindingContext(gobConfig) instead
 */
export function makeDatabinding(gobConfig) {
    return new BindingContext(gobConfig);
}
// Helper functions to bind some types of UI elements to a given context
export function bindElement(bindingContext, element, options) {
    return bindingContext.bindElement(element, options);
}
export function bindText(bindingContext, element, options) {
    const defOptions = {
        elementGetAccessor: ($element) => $element.text(),
        elementSetAccessor: ($element, value) => $element.text(value)
    };
    return bindingContext.bindElement(element, $.extend(defOptions, options));
}
export function bindTextCollectionfunction(bindingContext, element, options) {
    const defOptions = {
        joinSequence: ", "
    };
    function split(value) {
        const words = (value || "").split(",");
        return words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0);
    }
    function join(values, delimiter) {
        return values.join(delimiter);
    }
    options = $.extend(defOptions, options);
    options.elementGetAccessor = ($element) => split($element.val());
    options.elementSetAccessor = ($element, value) => $element.val(join(value, options.joinSequence));
    return bindingContext.bindElement(element, options);
}
export function bindCheckbox(bindingContext, element, options) {
    const defOptions = {
        elementGetAccessor: ($element) => $element.prop("checked"),
        elementSetAccessor: ($element, value) => $element.prop("checked", value)
    };
    return bindingContext.bindElement(element, $.extend(defOptions, options));
}
export function bindCheckboxValue(bindingContext, element, checkValue, uncheckValue, options) {
    const defOptions = {
        elementGetAccessor: ($element) => $element.prop("checked") ? checkValue : uncheckValue,
        elementSetAccessor: ($element, value) => $element.prop("checked", value === checkValue)
    };
    return bindingContext.bindElement(element, $.extend(defOptions, options));
}
export function bindCheckboxArray(bindingContext, element, values, options) {
    const defOptions = {
        disabled: values === null || values === undefined || values.length === 0,
        elementGetAccessor: ($element, event, oldValues) => {
            const checked = $element.prop("checked");
            const changed = setValuesInArray(oldValues, values, checked);
            if (changed)
                oldValues.sort();
            return changed ? oldValues : undefined;
        },
        elementSetAccessor: ($element, value) => {
            const checked = _.every(values, (e) => _.includes(value, e));
            $element.prop("checked", checked);
        }
    };
    return bindingContext.bindElement(element, $.extend(defOptions, options));
}
export function bindCheckboxArrayInverse(bindingContext, element, values, options) {
    const defOptions = {
        disabled: values === null || values === undefined || values.length === 0,
        elementGetAccessor: ($element, event, oldValues) => {
            const checked = $element.prop("checked");
            const changed = setValuesInArray(oldValues, values, !checked);
            if (changed)
                oldValues.sort();
            return changed ? oldValues : undefined;
        },
        elementSetAccessor: ($element, value) => {
            const checked = _.every(values, (e) => _.includes(value, e));
            $element.prop("checked", !checked);
        }
    };
    return bindingContext.bindElement(element, $.extend(defOptions, options));
}
export function bindColorSelector(bindingContext, element, options) {
    const defOptions = {
        elementKey: null,
        elementGetAccessor: null,
        //  ($element) => {
        //      const color = $element.spectrum("get"); return color === null ? null : color.toString();
        //  },
        elementSetAccessor: ($element, value) => $element.spectrum("set", value)
    };
    return bindingContext.bindElement(element, $.extend(defOptions, options));
}
export function bindDropdown(bindingContext, element, options) {
    //const defOptions = {}
    return bindingContext.bindElement(element, options);
}
export function bindListener(bindingContext, configKey, callback) {
    return bindingContext.bindConfigListener(configKey, callback);
}
