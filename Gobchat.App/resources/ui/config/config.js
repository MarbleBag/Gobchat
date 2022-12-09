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
import * as Config from '../modules/Config.js';
import * as Locale from '../modules/Locale.js';

// initialize global variables
jQuery(function ($) {
    window.GobchatAPI = window.opener.GobchatAPI
    window.Gobchat = window.opener.Gobchat
    window.console = window.opener.console

    window.gobConfig = new Config.Config()
    window.gobConfig.loadFromLocalStore(true)

    window.gobLocale = new Locale.Manager()

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
    const binding = GobConfigHelper.makeDatabinding(gobConfig)

    // update all text on language change
    gobLocale.setLocale(gobconfig.get("behaviour.language"))
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

    buildConfigNavigation()

    binding.initialize()
})

    async function initializeGeneralDatabinding() {
        


        


        await makeNavigationElement($("#cmain_navbar"))

        
    }   

    initializeGeneralDatabinding()

    async function makeNavigationElement(navBar) {
        const navAttribute = "data-gob-nav-target"
        const navIdAttribute = "data-gob-nav-id"
        const $navBar = $(navBar)
        const $navPanel = $(`#${navBar.attr(navAttribute)}`)

        const $navElements = $navBar.find(`.gob-nav-element[${navAttribute}]`)

        const awaitPromises = []
        $navElements.each(function () {
            const $panelElement = $("<div class='gob-nav-panel-content' />").appendTo($navPanel)
            const pageToLoad = $(this).attr(navAttribute)
            $panelElement.attr(navIdAttribute, pageToLoad)
            awaitPromises.push($panelElement.loadThen(pageToLoad))

            if ($(this).hasClass("active"))
                $panelElement.addClass("active")

            $(this).on("click", function () {
                $navBar.find("> .gob-nav-element.active").removeClass("active")
                $(this).addClass("active")
                $navPanel.find("> .gob-nav-panel-content.active").removeClass("active")
                $panelElement.addClass("active")
            })
        })
        await Promise.all(awaitPromises)
    }
})