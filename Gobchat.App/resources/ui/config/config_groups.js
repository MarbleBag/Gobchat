/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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

(async function () {
    const GroupIdAttribute = "data-gob-groupid"
    const ConfigKeyGroupSorting = "behaviour.groups.sorting"
    const ConfigKeyGroups = "behaviour.groups.data"

    const tblGroups = $("#cgroups_groups")
    async function populateGroupTable() {
        clearGroupTable()

        const groupIds = gobconfig.get(ConfigKeyGroupSorting)
        groupIds.forEach(async groupId => await addGroupToTable(groupId))

        tblGroups.sortable("refresh")
        tblGroups.accordion("refresh")

        tblGroups.find(".js-group-index").each(function (idx) {
            $(this).text(idx + 1)
        })

        await goblocale.updateElement(tblGroups)
    }

    function clearGroupTable() {
        tblGroups.find("div[data-gob-groupid]").each(function (idx) {
            $(this).data("configbinding").clear()
        })
        tblGroups.empty()
    }

    const tmplGroupEntry = $("#cgroups_template_groupentry")
    async function addGroupToTable(groupId) {
        const binding = GobConfigHelper.makeDatabinding(gobconfig)

        const groupConfigKey = `${ConfigKeyGroups}.${groupId}`
        const groupData = gobconfig.get(groupConfigKey)

        const groupEntry = $(tmplGroupEntry.html())
        groupEntry.appendTo(tblGroups)

        groupEntry.attr(GroupIdAttribute, groupId)
        groupEntry.data("configbinding", binding)

        const lblGroupName = groupEntry.find(".js-group-name")
        GobConfigHelper.setConfigKey(lblGroupName, groupConfigKey + ".name")
        GobConfigHelper.bindText(binding, lblGroupName)

        const btnDeleteGroup = groupEntry.find(".js-delete-group")
        btnDeleteGroup.on("click", (event) => {
            event.stopPropagation();
            (async () => {
                const result = await GobConfigHelper.showConfirmationDialog({
                    dialogText: "config.groups.entry.deleteconfirm",
                })

                if (result === 1) {
                    try {
                        gobconfig.remove(groupConfigKey)
                        const order = gobconfig.get(ConfigKeyGroupSorting)
                        _.remove(order, e => e === groupId)
                        gobconfig.set(ConfigKeyGroupSorting, order)
                    } catch (e1) {
                        console.error(e1)
                    }
                }
            })();
        })

        const isFFGroup = "ffgroup" in groupData
        if (isFFGroup)
            btnDeleteGroup.attr("disabled", true).hide() //ff groups can't be deleted

        if (isFFGroup) {
            GobConfigHelper.bindListener(binding, "behaviour.language", async (value) => {
                const $label = groupEntry.find(".js-ffgroup")
                const localization = await goblocale.get($label.attr("data-gob-locale-id-text"), value)
                const name = gobconfig.get(`${groupConfigKey}.hiddenName`, "")
                const txt = Gobchat.formatString(localization, name)
                $label.text(txt)
            })
        }

        const txtGroupName = groupEntry.find(".js-txt-name")
        GobConfigHelper.setConfigKey(txtGroupName, groupConfigKey + ".name")
        GobConfigHelper.bindElement(binding, txtGroupName)

        const btnGroupNameReset = groupEntry.find(".js-txt-name-reset")
        GobConfigHelper.setConfigKey(btnGroupNameReset, GobConfigHelper.getConfigKey(txtGroupName))
        GobConfigHelper.makeResetButton(btnGroupNameReset)
        if (!isFFGroup)
            btnGroupNameReset.attr("disabled", true).hide()

        const ckbIsActive = groupEntry.find(".js-chk-active")
        GobConfigHelper.setConfigKey(ckbIsActive, groupConfigKey + ".active")
        GobConfigHelper.bindCheckbox(binding, ckbIsActive)

        function makeColorSelector(classId, configKey) {
            const colorSelector = groupEntry.find(`.${classId}`)
            GobConfigHelper.setConfigKey(colorSelector, configKey)
            GobConfigHelper.makeColorSelector(colorSelector)
            GobConfigHelper.bindColorSelector(binding, colorSelector)

            const resetButton = groupEntry.find(`.${classId}-reset`)
            GobConfigHelper.setConfigKey(resetButton, configKey)
            GobConfigHelper.makeResetButton(resetButton)

            if (!isFFGroup)
                resetButton.hide()
        }

        makeColorSelector("js-sender-fgcolor", groupConfigKey + ".style.header.color")
        makeColorSelector("js-sender-bgcolor", groupConfigKey + ".style.header.background-color")
        makeColorSelector("js-msg-bgcolor", groupConfigKey + ".style.body.background-color")

        const txtTriggers = groupEntry.find(".js-group-triggers")
        txtTriggers.val(gobconfig.get(groupConfigKey + ".trigger", []).join(", "))
        txtTriggers.on("change", (event) => {
            let words = (event.target.value || "").split(",")
            words = words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0)
            gobconfig.set(groupConfigKey + ".trigger", words)
            event.target.value = words.join(", ")
        })

        if (isFFGroup) { // hide custom nodes
            groupEntry.find(".js-mode-custom").hide()
        } else { // hide non custom nodes
            groupEntry.find(".js-mode-noncustom").hide()
        }

        binding.initialize()
    }

    function makeNewGroup(addFront) {
        const groups = gobconfig.get(ConfigKeyGroups)
        const groupId = GobConfigHelper.generateId(6, Object.keys(groups))

        groups[groupId] = {
            name: "Unnamed",
            id: groupId,
            active: true,
            trigger: [],
            style: {
                body: { "background-color": null, },
                header: { "background-color": null, "color": null, },
            },
        }

        gobconfig.set(ConfigKeyGroups, groups)
        const sorting = gobconfig.get(ConfigKeyGroupSorting)
        if (addFront) {
            sorting.unshift(groupId)
        } else {
            sorting.push(groupId)
        }
        gobconfig.set(ConfigKeyGroupSorting, sorting)
    }

    const btnAddNewGroupTop = $("#cgroups_addnewgrouptop")
    btnAddNewGroupTop.on("click", function () {
        makeNewGroup(true)
    })

    const btnAddNewGroupBottom = $("#cgroups_addnewgroupbot")
    btnAddNewGroupBottom.on("click", function () {
        makeNewGroup(false)
    })

    tblGroups.accordion({
        heightStyle: "content",
        header: "> div > h4",
    })
        .sortable({
            axis: "y",
            handle: "h4",
            stop: function (event, ui) {
                //update sort order
                let order = []
                tblGroups.find("div[data-gob-groupid]").each(function () {
                    order.push($(this).attr(GroupIdAttribute))
                    //order.push($(this).data("groupId"))
                })

                order = order.filter(e => e !== null && e !== undefined && e.length > 0)
                gobconfig.set(ConfigKeyGroupSorting, order)

                // Refresh accordion to handle new order
                // updateGroupRenderer()
            }
        });

    gobconfig.addProfileEventListener(async (event) => {
        if (event.type === "active")
            await populateGroupTable()
    })
    gobconfig.addPropertyEventListener(ConfigKeyGroupSorting, async (event) => {
        if (event.isActive)
            await populateGroupTable()
    })

    await populateGroupTable()

    const btnCopyProfile = $("#cgroups_copyprofile")
    const copyKeys = ["behaviour.groups.data", "behaviour.groups.sorting"]
    GobConfigHelper.makeCopyProfileButton(btnCopyProfile, { configKeys: copyKeys })
}())

//# sourceURL=config_groups.js