/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

declare interface JQuery<T = HTMLElement> extends Iterable<T> {
    filterAndFind(selector: string): JQuery<T>
    appendEvenly(element: any): JQuery<T>
}

$.fn.filterAndFind = function<T>(this: JQuery<T>, selector: string): JQuery<T>{
    return this.filter(selector).add(this.find(selector))
}

$.fn.assertSetIsNotEmpty = function<T>(this: JQuery<T>): any{
    if(this.length === 0)
        throw new Error("jquery set is empty")
    return this
}

$.fn.isSetEmpty = function<T>(this: JQuery<T>): boolean{
    return this.length === 0
}

$.fn.isSetNotEmpty = function<T>(this: JQuery<T>): boolean{
    return this.length !== 0
}

$.fn.appendEvenly = function <T extends HTMLElement>(this: JQuery<T>, element: any): JQuery<T> {
    if (this.length === 0)
        return this.append(element)

    let minimum = this[0].childNodes.length
    let index = 0

    for (let i = 1; i < this.length; ++i) {
        const numberOfChilds = this[i].childNodes.length
        if (numberOfChilds < minimum) {
            minimum = numberOfChilds
            index = i
        }
    }

    $(this[index]).append(element)

    return this
}