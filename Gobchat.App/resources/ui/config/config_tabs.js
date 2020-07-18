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

'use strict';

(async () => {
    const keyTabsData = "behaviour.chattabs.data"
    const keyTabsSorting = "behaviour.chattabs.sorting"

    $("#ctabs_newtab").on("click", function () {
        const data = gobconfig.get(keyTabsData)
        const id = GobConfigHelper.generateId(6, Object.keys(data))

        const newConfig = gobconfig.getDefault(`${keyTabsData}.default`)
        newConfig.id = id
        newConfig.name = `${newConfig.name} ${Object.keys(data).length}`

        data[id] = newConfig
        gobconfig.set(keyTabsData, data)

        const sorting = gobconfig.get(keyTabsSorting)
        sorting.push(id)
        gobconfig.set(keyTabsSorting, sorting)
    })

    const $tableTabs = $("#ctabs_tabs > tbody")

    function buildTableTabs() {
        $tableTabs.find("[data-gob-entryid]").each(function () {
            $(this).data("configbinding").clear()
        })
        $tableTabs.empty()

        const entryIds = gobconfig.get(keyTabsSorting)
        entryIds.forEach(id => buildTableTabsEntry(id))
    }

    const swapArrayIndex = (arr, index1, index2) => {
        try {
            [arr[index1], arr[index2]] = [arr[index2], arr[index1]]
        } catch (e) {
            console.error(e)
        }
    }

    const $templateTableTabsEntry = $("#ctabs_template_tblentry")
    function buildTableTabsEntry(entryId) {
        const binding = GobConfigHelper.makeDatabinding(gobconfig)

        const entryConfigKey = `${keyTabsData}.${entryId}`

        const $entry = $($templateTableTabsEntry.html())
            .attr("data-gob-entryid", entryId)
            .data("configbinding", binding)
            .appendTo($tableTabs)

        const $entryName = $entry.find(".tmp_entry_name")
        GobConfigHelper.setConfigKey($entryName, `${entryConfigKey}.name`)
        GobConfigHelper.bindElement(binding, $entryName)

        const $entryVisible = $entry.find(".tmp_entry_visible")
        GobConfigHelper.setConfigKey($entryVisible, `${entryConfigKey}.visible`)
        GobConfigHelper.bindCheckbox(binding, $entryVisible)

        $entry.find(".tmp_entry_action_delete").on("click", function (event) {
            event.stopPropagation()
            {
                (async () => {
                    const result = await GobConfigHelper.showConfirmationDialog({
                        dialogText: "config.tabs.tabtable.action.delete.confirm",
                    })

                    if (result === 1) {
                        try {
                            gobconfig.remove(entryConfigKey)
                            const order = gobconfig.get(keyTabsSorting)
                            _.remove(order, e => e === entryId)
                            gobconfig.set(keyTabsSorting, order)
                        } catch (ex) {
                            console.error(ex)
                        }
                    }
                })()
            }
        })

        const entryIds = gobconfig.get(keyTabsSorting)
        $entry.find(".tmp_entry_action_mup")
            .toggle(entryIds.indexOf(entryId) !== 0)
            .on("click", function () {
                const entryIds = gobconfig.get(keyTabsSorting)
                const idx = entryIds.indexOf(entryId)
                swapArrayIndex(entryIds, idx, idx - 1)
                gobconfig.set(keyTabsSorting, entryIds)
            })

        $entry.find(".tmp_entry_action_mdown")
            .toggle(entryIds.indexOf(entryId) !== entryIds.length - 1)
            .on("click", function () {
                const entryIds = gobconfig.get(keyTabsSorting)
                const idx = entryIds.indexOf(entryId)
                swapArrayIndex(entryIds, idx, idx + 1)
                gobconfig.set(keyTabsSorting, entryIds)
            })

        binding.initialize()
    }

    function buildConfig(tabid) {
    }

    gobconfig.addProfileEventListener((event) => {
        if (event.type === "active")
            buildTableTabs()
    })
    gobconfig.addPropertyEventListener(keyTabsSorting, (event) => {
        if (event.isActive)
            buildTableTabs()
    })
    buildTableTabs()
})()