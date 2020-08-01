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

(async function (undefined) {
    $("#cprofiles_copyprofiledialog").hide();

    //setup profile selector
    const profileSelectionDropdown = $("#cmain_profileselect")
    profileSelectionDropdown.on("change", function (event) {
        const profileId = event.target.value
        gobconfig.activeProfile = profileId
    })

    function populateProfileSelection() {
        profileSelectionDropdown.empty()
        gobconfig.profiles
            .map(profileId => {
                var profile = gobconfig.getProfile(profileId)
                return { name: profile.profileName, id: profileId }
            })
            .sort((a, b) => {
                if (a.name < b.name)
                    return -1;
                if (a.name > b.name)
                    return 1;
                return 0;
            })
            .forEach(e => {
                profileSelectionDropdown.append(new Option(e.name, e.id))
            })

        profileSelectionDropdown.val(gobconfig.activeProfile)
    }

    gobconfig.addProfileEventListener((event) => {
        if (event.type === "active")
            profileSelectionDropdown.val(event.detail.new)
        else
            populateProfileSelection()
    })

    gobconfig.addPropertyEventListener("profile.name", (event) => {
        const profileId = profileSelectionDropdown.val()
        populateProfileSelection()
        profileSelectionDropdown.val(profileId)
    })

    populateProfileSelection()

    //setup create profile
    $("#cprofiles_profile_new").on("click", function (event) {
        gobconfig.createNewProfile()
    })

    //setup import profile
    $("#cprofiles_profile_import").on("click", function (event) {
        (async () => {
            let newProfile = await GobchatAPI.importProfile()
            if (newProfile === undefined || newProfile === null || newProfile.length == 0) {
                GobConfigHelper.showErrorDialog({ dialogText: "config.profiles.importprofile.error" });
                return
            }

            newProfile = JSON.parse(newProfile)
            gobconfig.importProfile(newProfile)
        })()
    })

    const profileTable = $("#cprofiles_profiles > tbody")
    const template = $("#cprofiles_template_profiles_entry")

    async function populateProfileTable() {
        profileTable.empty()
        gobconfig.profiles.forEach((profileId) => {
            const profile = gobconfig.getProfile(profileId)

            const rowElement = $(template.html())
            rowElement.appendTo(profileTable)

            rowElement.attr("data-profile-id", profile.profileId)

            const txtProfileName = rowElement.find("#profile_name")
            txtProfileName.on("change", function (event) {
                profile.profileName = event.target.value || "Unnamed"
            })
            txtProfileName.val(profile.profileName)

            const btnActiveProfile = rowElement.find("#profile_activate")
            btnActiveProfile.on("click", function (event) {
                gobconfig.activeProfile = profile.profileId
            })
            if (gobconfig.activeProfile === profile.profileId)
                btnActiveProfile.attr("disabled", true)

            const btnExportProfile = rowElement.find("#profile_export")
            btnExportProfile.on("click", function (event) {
                (async () => {
                    const selection = await GobchatAPI.saveFileDialog("Json files (*.json)|*.json", `profile_${profile.profileId}.json`)
                    if (selection === null || selection === undefined || selection.length === 0)
                        return
                    await GobchatAPI.writeTextToFile(selection, JSON.stringify(profile.config))
                })()
            })

            const btnCloneProfile = rowElement.find("#profile_clone")
            btnCloneProfile.on("click", function (event) {
                const newProfileId = gobconfig.createNewProfile()
                gobconfig.copyProfile(profile.profileId, newProfileId)
            })

            const btnCopyProfile = rowElement.find("#profile_copy")
            btnCopyProfile.on("click", function (event) {
                GobConfigHelper.showProfileIdSelectionDialog(selectedId => gobconfig.copyProfile(selectedId, profile.profileId), { exclude: profile.profileId })
            })

            if (gobconfig.profiles.length <= 1)
                btnCopyProfile.attr("disabled", true)

            const btnDefaultProfile = rowElement.find("#profile_default")
            btnDefaultProfile.on("click", function (event) {
                (async () => {
                    const result = await GobConfigHelper.showConfirmationDialog({
                        dialogText: "config.profiles.profile.reset.dialog.text",
                    })

                    if (result === 1) {
                        profile.restoreDefaultConfig()
                    }
                })()
            })

            const btnDeleteProfile = rowElement.find("#profile_delete")
            btnDeleteProfile.on("click", function (event) {
                (async () => {
                    const result = await GobConfigHelper.showConfirmationDialog({
                        dialogText: "config.profiles.profile.delete.dialog.text",
                    })

                    if (result === 1) {
                        gobconfig.deleteProfile(profile.profileId)
                    }
                })()
            })
            if (gobconfig.profiles.length <= 1)
                btnDeleteProfile.attr("disabled", true)
        })
        await goblocale.updateElement(profileTable)
    }

    gobconfig.addProfileEventListener((event) => {
        (async () => {
            await populateProfileTable()
        })()
    })
    await populateProfileTable()

    gobconfig.addPropertyEventListener("profile.name", (event) => {
        const profile = gobconfig.getProfile(event.source)
        profileTable.find(`tr[data-profile-id='${profile.profileId}']`).find("#profile_name").val(profile.get("profile.name"))
    })
}());

//# sourceURL=config_profiles.js