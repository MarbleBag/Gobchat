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

(async () => {
    const keyTabsData = "behaviour.chattabs.data"
    const keyTabsSorting = "behaviour.chattabs.sorting"
    const pagebinding = GobConfigHelper.makeDatabinding(gobconfig)

    $("#ctabs_newtab").on("click", function () {
        const data = gobconfig.get(keyTabsData)
        const id = GobConfigHelper.generateId(6, Object.keys(data))

        const newConfig = gobconfig.getDefault(`${keyTabsData}.default`)
        newConfig.id = id
        newConfig.visible = true
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

        const $entryName = $entry.find(".js-name")
        GobConfigHelper.setConfigKey($entryName, `${entryConfigKey}.name`)
        GobConfigHelper.bindElement(binding, $entryName)

        const $entryVisible = $entry.find(".js-visible")
        GobConfigHelper.setConfigKey($entryVisible, `${entryConfigKey}.visible`)
        GobConfigHelper.bindCheckbox(binding, $entryVisible)
        $entryVisible.on("click", function (event) { event.stopPropagation() })

        $entry.find(".js-action-config").on("click", function (event) {
            event.stopPropagation()
            buildConfig(entryId)
        })

        $entry.find(".js-action-delete")
            .toggleClass("disabled", gobconfig.get(keyTabsSorting, []).length <= 1)
            .on("click", function (event) {
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
        $entry.find(".js-action-mup")
            .toggleClass("disabled", entryIds.indexOf(entryId) === 0)
            .on("click", function (event) {
                event.stopPropagation()
                const entryIds = gobconfig.get(keyTabsSorting)
                const idx = entryIds.indexOf(entryId)
                swapArrayIndex(entryIds, idx, idx - 1)
                gobconfig.set(keyTabsSorting, entryIds)
            })

        $entry.find(".js-action-mdown")
            .toggleClass("disabled", entryIds.indexOf(entryId) === entryIds.length - 1)
            .on("click", function (event) {
                event.stopPropagation()
                const entryIds = gobconfig.get(keyTabsSorting)
                const idx = entryIds.indexOf(entryId)
                swapArrayIndex(entryIds, idx, idx + 1)
                gobconfig.set(keyTabsSorting, entryIds)
            })

        $entry.on("click", function (event) {
            event.stopPropagation()
            buildConfig(entryId)
        })

        binding.initialize()
    }

    function buildConfig(id) {
        $tableTabs.children(".active").removeClass("active")
        $tableTabs.children(`[data-gob-entryid=${id}]`).addClass("active")

        const $config = $("#ctabs_tab_config")
        let binding = $config.data("configbinding")
        if (binding) binding.clear()

        const $tbl1 = $("#ctabs_tab_channel1 > tbody")
        $tbl1.empty()
        const $tbl2 = $("#ctabs_tab_channel2 > tbody")
        $tbl2.empty()

        if (!id) {
            $("#ctabs_config_tabname").html("")
            return
        }

        $("#ctabs_config_tabname").html(
            Gobchat.encodeHtmlEntities(
                gobconfig.get(`${keyTabsData}.${id}`).name
            )
        )

        binding = GobConfigHelper.makeDatabinding(gobconfig)
        $config.data("configbinding", binding)

        $("#ctabs_formatting_mention")
            .databindKey(`${keyTabsData}.${id}.formatting.mentions`)
            .databind(binding, { type: "checkbox" })

        $("#ctabs_formatting_roleplay")
            .databindKey(`${keyTabsData}.${id}.formatting.roleplay`)
            .databind(binding, { type: "checkbox" })

        $("#ctabs_formatting_timestamp")
            .databindKey(`${keyTabsData}.${id}.formatting.timestamps`)
            .databind(binding, { type: "checkbox" })

        $("#ctabs_formatting_rangefilter")
            .databindKey(`${keyTabsData}.${id}.formatting.rangefilter`)
            .databind(binding, { type: "checkbox" })

        const $rowTemplate = $("#ctabs_template_channelentry")
        function buildChannelEntry(channelData) {
            const $table = $tbl1.children().length <= $tbl2.children().length ? $tbl1 : $tbl2
            const $rowEntry = $($rowTemplate.html()).appendTo($table)

            $rowEntry.find(".js-label")
                .attr("data-gob-locale-text", `${channelData.translationId}`)
                .attr("data-gob-locale-title", `${channelData.tooltipId}`)

            const $chkVisible = $rowEntry.find(".js-visible")
                .databindKey(`${keyTabsData}.${id}.channel.visible`)

            const channelEnums = [].concat(channelData.chatChannel || [])
            if (channelEnums.length === 0) {
                $chkVisible.hide()
            } else {
                GobConfigHelper.bindCheckboxArray(binding, $chkVisible, channelEnums)
            }
        }

        Object.entries(Gobchat.Channels).forEach((entry) => {
            const channelData = entry[1]
            if (!channelData.relevant) return
            buildChannelEntry(channelData)
        })

        gobLocale.updateElement($tbl1)
        gobLocale.updateElement($tbl2)
        binding.initialize()
    }

    GobConfigHelper.bindListener(pagebinding, keyTabsSorting, function (entryIds) {
        buildTableTabs(entryIds)
        if (entryIds.length > 0)
            buildConfig(entryIds[0])
        else
            buildConfig(null)
    })

    GobConfigHelper.bindElement(pagebinding, $("#ctabs_effects_message"), { elementGetAccessor: ($element) => parseInt($element.val()) })
    GobConfigHelper.bindElement(pagebinding, $("#ctabs_effects_mention"), { elementGetAccessor: ($element) => parseInt($element.val()) })

    pagebinding.initialize()

    //doesn't work. Old tabs are keept around?
    GobConfigHelper.makeCopyProfileButton($("#ctabs_copyprofile"),
        {
            configKeys: [keyTabsData, keyTabsSorting, "behaviour.chattabs.effect"]
        })
})()

//# sourceURL=config_tabs.js