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

// import { buildNavigationElement } from "../gobchat/gob-navbar"
jQuery(function ($) {
    $("#cmain_saveconfig").on("click", function () {
        window.gobconfig.saveToLocalStore()
        window.saveConfig()
    })

    $("#cmain_saveandexitconfig").on("click", function () {
        window.gobconfig.saveToLocalStore()
        window.saveConfig()
        window.close()
    })

    /*
    window.addEventListener('resize', function (event) {
        // values from outer width/Height are not correct.
        const width = window.outerWidth
        const height = window.innerHeight
        if (width != null && width != undefined && width > 100)
            gobconfig.set("behaviour.frame.config.size.width", width)
        if (height != null && height != undefined && height > 100)
            gobconfig.set("behaviour.frame.config.size.height", height)        
    }, true);
    */

    $("#cmain_cancelconfig").on("click", async function () {        
        const result = await GobConfigHelper.showConfirmationDialog({ dialogText: "config.main.nav.cancel.dialog" })
        if (result)
            window.close()
    })

    $("#cmain_closegobchat").on("click", async function () {        
        const result = await GobConfigHelper.showConfirmationDialog({ dialogText: "config.main.nav.closegobchat.dialog" })
        if (result) {
            window.close()
            GobchatAPI.closeGobchat()
        }
    })

    async function initializeGeneralDatabinding() {
        const generalBinding = GobConfigHelper.makeDatabinding(gobconfig)

        window.gobLocale = new Gobchat.LocaleManager()

        gobLocale.setLocale(gobconfig.get("behaviour.language"))
        generalBinding.bindConfigListener("behaviour.language", (value) => {
            gobLocale.setLocale(value)
            gobLocale.updateElement($(document))
        })

        window.gobStyles = new Gobchat.StyleLoader(document.head, "..")
        await gobStyles.loadStyles()
        generalBinding.bindConfigListener("style.theme", async (value) => {
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

        await makeNavigationElement($("#cmain_navbar"))

        generalBinding.initialize()
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
});