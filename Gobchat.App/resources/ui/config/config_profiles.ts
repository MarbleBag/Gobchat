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

import * as Dialog from "/module/Dialog"
import "/module/JQueryExtensions"

const AttributeProfileId = "data-profile-id"

//setup create profile
$("#cp-profiles_profile_new").on("click", function (event) {
    gobConfig.createNewProfile()
})

//setup import profile
$("#cp-profiles_profile_import").on("click", async function (event) {
    const stringifiedProfile = await GobchatAPI.importProfile()
    if (stringifiedProfile === undefined || stringifiedProfile === null || stringifiedProfile.length == 0) {
        Dialog.showErrorDialog({ dialogText: "config.profiles.importprofile.error" });
        return
    }

    const newProfile = JSON.parse(stringifiedProfile)
    gobConfig.importProfile(newProfile)
})

const profileTable = $("#cp-profiles_profiles > tbody")
const template = $("#cp-profiles_template_profile-table_entry")

async function populateProfileTable() {
    profileTable.children(":not(.gob-config_cp-profile-table_header)").remove()

    gobConfig.profileIds.forEach((profileId) => {
        const profile = gobConfig.getProfile(profileId)
        if (profile === null)
            return

        const rowElement = $(template.html())
            .attr(AttributeProfileId, profile.profileId)

        profileTable.append(rowElement)

        const txtProfileName = rowElement.find(".js-name")
        const btnActiveProfile = rowElement.find(".js-activate")
        const btnExportProfile = rowElement.find(".js-export")
        const btnCloneProfile = rowElement.find(".js-clone")
        const btnCopyProfile = rowElement.find(".js-copy")
        const btnDefaultProfile = rowElement.find(".js-reset")
        const btnDeleteProfile = rowElement.find(".js-delete")

        txtProfileName.on("change", function (event) {
            profile.profileName = event.target.value || "Unnamed"
        })
        txtProfileName.val(profile.profileName)

        btnActiveProfile.on("click", function (event) {
            gobConfig.activeProfileId = profile.profileId
        })
        if (gobConfig.activeProfileId === profile.profileId)
            btnActiveProfile.attr("disabled", true)

        btnExportProfile.on("click", async function (event) {
            const selection = await GobchatAPI.saveFileDialog("Json files (*.json)|*.json", `profile_${profile.profileId}.json`)
            if (selection === null || selection === undefined || selection.length === 0)
                return
            await GobchatAPI.writeTextToFile(selection, JSON.stringify(profile.config))
        })

        btnCloneProfile.on("click", function (event) {
            const newProfileId = gobConfig.createNewProfile()
            gobConfig.copyProfile(profile.profileId, newProfileId)
        })

        btnCopyProfile.on("click", function (event) {
            Dialog.showProfileIdSelectionDialog(selectedId => gobConfig.copyProfile(selectedId, profile.profileId), { exclude: [profile.profileId] })
        })
        if (gobConfig.profileIds.length <= 1)
            btnCopyProfile.attr("disabled", true)

        btnDefaultProfile.on("click", async function (event) {
            const result = await Dialog.showConfirmationDialog({
                dialogText: "config.profiles.profile.reset.dialog.text",
            })

            if (result === 1) 
                profile.restoreDefaultConfig()            
        })

        btnDeleteProfile.on("click", async function (event) {
            const result = await Dialog.showConfirmationDialog({
                dialogText: "config.profiles.profile.delete.dialog.text",
            })

            if (result === 1) 
                gobConfig.deleteProfile(profile.profileId)            
        })
        if (gobConfig.profileIds.length <= 1)
            btnDeleteProfile.attr("disabled", true)
    })

    await gobLocale.updateElement(profileTable)
}

gobConfig.addProfileEventListener(async (event) => { await populateProfileTable() })
await populateProfileTable()

gobConfig.addPropertyEventListener("profile.name", (event) => {
    const profile = gobConfig.getProfile(event.sourceProfileId)
    if (profile)
        profileTable.find(`[${AttributeProfileId}='${profile.profileId}'] .js-name`).val(profile.get("profile.name"))
})


//# sourceURL=config_profiles.js