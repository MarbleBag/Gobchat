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
import * as Databinding from "/module/Databinding";
import * as Dialog from "/module/Dialog";
import * as Components from "/module/Components";
import * as Utility from "/module/CommonUtility";
const GroupIdAttribute = "data-gob-groupid";
const ConfigKeyGroupSorting = "behaviour.groups.sorting";
const ConfigKeyGroups = "behaviour.groups.data";
const tblGroups = $("#cp-groups_groups");
async function populateGroupTable() {
    clearGroupTable();
    const groupIds = gobConfig.get(ConfigKeyGroupSorting);
    groupIds.forEach(async (groupId) => await addGroupToTable(groupId));
    tblGroups.sortable("refresh");
    tblGroups.accordion("refresh");
    tblGroups.find(".js-group-index").each(function (idx) {
        $(this).text(idx + 1);
    });
    await gobLocale.updateElement(tblGroups);
}
function clearGroupTable() {
    tblGroups.find("div[data-gob-groupid]").each(function (idx) {
        $(this).data("configbinding").clear();
    });
    tblGroups.empty();
}
const tmplGroupEntry = $("#cp-groups_template_groupentry");
async function addGroupToTable(groupId) {
    const binding = new Databinding.BindingContext(gobConfig);
    const groupConfigKey = `${ConfigKeyGroups}.${groupId}`;
    const groupData = gobConfig.get(groupConfigKey);
    const groupEntry = $(tmplGroupEntry.html());
    groupEntry.appendTo(tblGroups);
    groupEntry.attr(GroupIdAttribute, groupId);
    groupEntry.data("configbinding", binding);
    const lblGroupName = groupEntry.find(".js-group-name");
    Databinding.setConfigKey(lblGroupName, groupConfigKey + ".name");
    Databinding.bindText(binding, lblGroupName);
    const btnDeleteGroup = groupEntry.find(".js-delete-group");
    btnDeleteGroup.on("click", (event) => {
        event.stopPropagation();
        (async () => {
            const result = await Dialog.showConfirmationDialog({
                dialogText: "config.groups.entry.deleteconfirm",
            });
            if (result === 1) {
                try {
                    gobConfig.remove(groupConfigKey);
                    const order = gobConfig.get(ConfigKeyGroupSorting);
                    _.remove(order, e => e === groupId);
                    gobConfig.set(ConfigKeyGroupSorting, order);
                }
                catch (e1) {
                    console.error(e1);
                }
            }
        })();
    });
    const isFFGroup = "ffgroup" in groupData;
    if (isFFGroup)
        btnDeleteGroup.attr("disabled", true).hide(); //ff groups can't be deleted
    if (isFFGroup) {
        Databinding.bindListener(binding, "behaviour.language", async (value) => {
            const $label = groupEntry.find(".js-ffgroup");
            const localization = await gobLocale.get($label.attr("data-gob-locale-id-text"), value);
            const name = gobConfig.get(`${groupConfigKey}.hiddenName`, "");
            const txt = Utility.formatString(localization, name);
            $label.text(txt);
        });
    }
    const txtGroupName = groupEntry.find(".js-txt-name");
    Databinding.setConfigKey(txtGroupName, groupConfigKey + ".name");
    Databinding.bindElement(binding, txtGroupName);
    const btnGroupNameReset = groupEntry.find(".js-txt-name-reset");
    Databinding.setConfigKey(btnGroupNameReset, Databinding.getConfigKey(txtGroupName));
    Components.makeResetButton(btnGroupNameReset);
    if (!isFFGroup)
        btnGroupNameReset.attr("disabled", true).hide();
    const ckbIsActive = groupEntry.find(".js-chk-active");
    Databinding.setConfigKey(ckbIsActive, groupConfigKey + ".active");
    Databinding.bindCheckbox(binding, ckbIsActive);
    function makeColorSelector(classId, configKey) {
        const colorSelector = groupEntry.find(`.${classId}`);
        Databinding.setConfigKey(colorSelector, configKey);
        Components.makeColorSelector(colorSelector);
        Databinding.bindColorSelector(binding, colorSelector);
        const resetButton = groupEntry.find(`.${classId}-reset`);
        Databinding.setConfigKey(resetButton, configKey);
        Components.makeResetButton(resetButton);
        if (!isFFGroup)
            resetButton.hide();
    }
    makeColorSelector("js-sender-fgcolor", groupConfigKey + ".style.header.color");
    makeColorSelector("js-sender-bgcolor", groupConfigKey + ".style.header.background-color");
    makeColorSelector("js-msg-bgcolor", groupConfigKey + ".style.body.background-color");
    const txtTriggers = groupEntry.find(".js-group-triggers");
    txtTriggers.val(gobConfig.get(groupConfigKey + ".trigger", []).join(", "));
    txtTriggers.on("change", (event) => {
        let words = (event.target.value || "").split(",");
        words = words.filter(w => w !== null && w !== undefined).map(w => w.toLowerCase().trim()).filter(w => w.length > 0);
        gobConfig.set(groupConfigKey + ".trigger", words);
        event.target.value = words.join(", ");
    });
    if (isFFGroup) { // hide custom nodes
        groupEntry.find(".js-mode-custom").hide();
    }
    else { // hide non custom nodes
        groupEntry.find(".js-mode-noncustom").hide();
    }
    binding.initialize();
}
function makeNewGroup(addFront) {
    const groups = gobConfig.get(ConfigKeyGroups);
    const groupId = Utility.generateId(6, Object.keys(groups));
    groups[groupId] = {
        name: "Unnamed",
        id: groupId,
        active: true,
        trigger: [],
        style: {
            body: { "background-color": null, },
            header: { "background-color": null, "color": null, },
        },
    };
    gobConfig.set(ConfigKeyGroups, groups);
    const sorting = gobConfig.get(ConfigKeyGroupSorting);
    if (addFront) {
        sorting.unshift(groupId);
    }
    else {
        sorting.push(groupId);
    }
    gobConfig.set(ConfigKeyGroupSorting, sorting);
}
const btnAddNewGroupTop = $("#cp-groups_addnewgrouptop");
btnAddNewGroupTop.on("click", function () {
    makeNewGroup(true);
});
const btnAddNewGroupBottom = $("#cp-groups_addnewgroupbot");
btnAddNewGroupBottom.on("click", function () {
    makeNewGroup(false);
});
tblGroups.accordion({
    heightStyle: "content",
    header: "> div > h4",
})
    .sortable({
    axis: "y",
    handle: "h4",
    stop: function (event, ui) {
        //update sort order
        let order = [];
        tblGroups.find("div[data-gob-groupid]").each(function () {
            order.push($(this).attr(GroupIdAttribute));
            //order.push($(this).data("groupId"))
        });
        order = order.filter(e => e !== null && e !== undefined && e.length > 0);
        gobConfig.set(ConfigKeyGroupSorting, order);
        // Refresh accordion to handle new order
        // updateGroupRenderer()
    }
});
gobConfig.addProfileEventListener(async (event) => {
    if (event.action === "active")
        await populateGroupTable();
});
gobConfig.addPropertyEventListener(ConfigKeyGroupSorting, async (event) => {
    if (event.isActiveProfile)
        await populateGroupTable();
});
await populateGroupTable();
const btnCopyProfile = $("#cp-groups_copyprofile");
const copyKeys = ["behaviour.groups.data", "behaviour.groups.sorting"];
Components.makeCopyProfileButton(btnCopyProfile, { configKeys: copyKeys });
