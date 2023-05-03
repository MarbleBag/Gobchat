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

jQuery.fn.loadThen = function (url, params) {
    const self = this

    if (self.length <= 0) //no element shortcut
        return jQuery.Deferred().resolveWith(self, [""])
    

    return jQuery.Deferred(function (promise) {
        self.load(url, params, function (responseText, textStatus, jqXHR) {
            if (textStatus == "error") {
                promise.reject(this, jqXHR)
            } else {
                promise.resolve(this)
            }
        });
    }).promise()
}

jQuery.fn.databindKey = function (key) {
    if (key) {
        return this.each(function () {
            GobConfigHelper.setConfigKey(this, key)
        })
    }
    return GobConfigHelper.getConfigKey(this)
}

jQuery.fn.databind = function (binding, options) {
    let defOptions = {
        type: "element"
    }
    defOptions = $.extend(defOptions, options)
    const args = Array.prototype.slice.call(arguments, 2);

    return this.each(function () {
        switch (defOptions.type.toLowerCase()) {
            case "element":
                GobConfigHelper.bindElement(binding, this, defOptions)
                break
            case "text":
                GobConfigHelper.bindText(binding, this, defOptions)
                break
            case "checkbox":
                GobConfigHelper.bindCheckbox(binding, this, defOptions)
                break
            case "checkboxvalue":
                GobConfigHelper.bindCheckboxValue(binding, this, args[0], args[1], defOptions)
                break
            case "checkboxarray":
                GobConfigHelper.bindCheckboxArray(binding, this, args[0], defOptions)
                break
            case "color":
                GobConfigHelper.bindColorSelector(binding, this, defOptions)
                break
            case "dropdown":
                GobConfigHelper.bindDropdown(binding, this, defOptions)
                break
        }
    })
}