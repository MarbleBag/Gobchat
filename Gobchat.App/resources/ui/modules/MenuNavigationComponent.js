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

const attributeNavTarget = "data-gob-nav-target"
const attributeNavId = "data-gob-nav-id"
const attributePanel = "data-gob-nav-panel"
const attributePanelTemplate = "data-gob-nav-template"

const cssNav = "gob-config_navigation"
const cssNavControl = "gob-config_navigation_entry"
const cssPanel = "gob-config_navigation_panel"
const cssPanelEntry = "gob-config_navigation_panel_entry"
const cssActive = "active"

const selector_controls = `> .${cssNavControl}`
const selector_allActivePanels = `> .${cssPanelEntry}.${cssActive}`
const selector_allActiveControls = `> .${cssNavControl}.${cssActive}`
const selector_allButFirstActiveControl = `> .${cssNavControl}.${cssActive}:not(:first)`
const selector_panelInTemplate = `[${attributePanel}]`

function loadThen($element, url, params) {
    if ($element.length <= 0)
        return Promise.resolve()

    return new Promise((resolve, reject) => {
        /*
        fetch(url)
            .then(response => {
                try {
                    const html = response.text
                    element.innerHtml = html
                    resolve()
                } catch (error) {
                    reject(error)
                }
            })
            .catch((error) => reject(error)
        */

        $element.load(url, params, (response, status, jqXHR) => {
            if (status === "error")
                reject(jqXHR)
            else 
                resolve()            
        })
    })
}

export async function makeControl($element) {
    const awaitPromises = []

    $element.each(function () {
        const $navigationBar = $(this)
        const panelTemplateSelector = `#${$navigationBar.attr(attributePanelTemplate)}`
        const panelStackSelector = `#${$navigationBar.attr(attributeNavTarget)}`

        const $panelTemplate = $(panelTemplateSelector)
        const $panelStack = $(panelStackSelector)
        const $navigationEntries = $navigationBar.find(selector_controls)

        $navigationBar.find(selector_allButFirstActiveControl).removeClass(cssActive)
        $panelStack.find(selector_allActivePanels).removeClass(cssActive)

        $navigationEntries.each(function () {
            const $control = $(this)
            const navigationTarget = $control.attr(attributeNavTarget)

            const selector_panelWhichIsLinkedToTarget = `> .${cssPanelEntry}[${attributeNavId}="${navigationTarget}"]`
            let $panel = $panelStack.find(selector_panelWhichIsLinkedToTarget)

            if (!$panel.length) { // no panel found
                $panel = $($panelTemplate.html())
                    .addClass(cssPanelEntry)
                    .attr(attributeNavId, navigationTarget)
                    .appendTo($panelStack)

                $control.on("click", function () {
                    $navigationBar.find(selector_allActiveControls).removeClass(cssActive)
                    $panelStack.find(selector_allActivePanels).removeClass(cssActive)
                    $control.addClass(cssActive)
                    $panel.addClass(cssActive)
                })

                if (navigationTarget.endsWith(".html")) {                    
                    $panel.find(selector_panelInTemplate).addBack(selector_panelInTemplate).each(function () {
                        const promise = loadThen($(this), navigationTarget)
                        awaitPromises.push(promise)
                    })
                }
            }

            if ($control.hasClass(cssActive))
                $panel.addClass(cssActive)
        })
    })

    const results = await Promise.allSettled(awaitPromises)
    let errorMsg = ""
    for (const result of results) {
        if (result.status === "rejected")
            errorMsg += result.reason + '\n'
    }
    if (errorMsg.length > 0)
        throw new Error(errorMsg)
}

