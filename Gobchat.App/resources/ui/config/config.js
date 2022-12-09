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

import * as Databinding from '../modules/Databinding.js';
//import * as Config from '../modules/Config.js';
import * as Locale from '../modules/Locale.js';
//import * as Styles from '../modules/Styles.js';
import makeNavigationElement from '../modules/ConfigNavigationElement.js'

// initialize global variables
jQuery(function ($) {
    window.GobchatAPI = window.opener.GobchatAPI
    window.Gobchat = window.opener.Gobchat
    window.console = window.opener.console

    window.gobConfig = new Gobchat.GobchatConfig()
    window.gobConfig.loadFromLocalStore(true)

    window.gobLocale = new Locale.LocaleManager()

    window.gobStyles = new Gobchat.StyleLoader(document.head, "..")
})

// initialize main buttons
jQuery(function ($) {
    $("#c_main_save-config").on("click", function () {
        window.gobConfig.saveToLocalStore()
        window.saveConfig()
    })

    $("#c_main_save-and-exit-config").on("click", function () {
        window.gobConfig.saveToLocalStore()
        window.saveConfig()
        window.close()
    })

    $("#c_main_cancel-config").on("click", async function () {
        const result = await GobConfigHelper.showConfirmationDialog({ dialogText: "config.main.nav.cancel.dialog" })
        if (result)
            window.close()
    })

    $("#c_main_close-gobchat").on("click", async function () {
        const result = await GobConfigHelper.showConfirmationDialog({ dialogText: "config.main.nav.closegobchat.dialog" })
        if (result) {
            window.close()
            GobchatAPI.closeGobchat()
        }
    })
})

jQuery(async function ($) {
    const binding = new Databinding.BindingContext(gobConfig)

    // update all text on language change
    gobLocale.setLocale(gobConfig.get("behaviour.language"))
    binding.bindConfigListener("behaviour.language", (value) => {
        gobLocale.setLocale(value)
        gobLocale.updateElement($(document))
    })

    await gobStyles.loadStyles()
    binding.bindConfigListener("style.theme", async (value) => {
        try {
            $("body").hide()
            await gobStyles.activateStyle(value)
            // use hide / show to trigger a reflow, so the new loaded style gets applied everywhere.
            // Sometimes, without this, styles aren't applied to scrollbars. Still no idea why.
            $("body").show()
        } catch (e1) {
            console.error(e1)
        }
    })

    await makeNavigationElement($(".gob-config_navigation"))

    binding.initialize()
})
