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

jQuery.fn.loadThen = function (url, params) {
    const self = this

    if (self.length <= 0) { //no element shortcut
        return jQuery.Deferred().resolveWith(self, [""]);
    }

    return jQuery.Deferred(function (promise) {
        self.load(url, params, function (responseText, textStatus, jqXHR) {
            if (textStatus == "error") {
                promise.reject(this, jqXHR);
            } else {
                promise.resolve(this);
            }
        });
    }).promise();
}