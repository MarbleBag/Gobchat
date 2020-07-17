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

// import { buildNavigationElement } from "../gobchat/gob-navbar"
jQuery(function ($) {
    $("#cmain_saveconfig").on("click", function (e) {
        window.gobconfig.saveToLocalStore()
        window.saveConfig()
    });

    $("#cmain_saveandexitconfig").on("click", function (e) {
        window.gobconfig.saveToLocalStore()
        window.saveConfig()
        window.close()
    });

    $("#cmain_cancelconfig").on("click", function (e) {
        {
            (async () => {
                const result = await GobConfigHelper.showConfirmationDialog({ dialogText: "config.main.dialog.nav.cancel.text" })
                if (result)
                    window.close()
            })()
        }
    });

    {
        (async () => {
            window.goblocale = new Gobchat.GobLocaleManager()
            window.goblocale.setLocale(gobconfig.get("behaviour.language"))

            const generalBinding = GobConfigHelper.makeDatabinding(gobconfig)

            generalBinding.bindConfigListener("behaviour.language", (value) => {
                window.goblocale.setLocale(value)
                window.goblocale.updateElement($(document))
            })

            await makeNavigationElement($("#cmain_navbar"))

            const styleLoader = new Gobchat.StyleLoader(document.head, "..")
            await styleLoader.loadStyles()

            try {
                const dpdThemes = $("#capp_theme")
                dpdThemes.empty()
                for (let style of styleLoader.styles) {
                    $('<option>', {
                        text: style,
                        value: style
                    }).appendTo(dpdThemes)
                }
            } catch (e1) {
                console.log(e1)
            }

            generalBinding.bindConfigListener("style.theme", (value) => {
                $("body").hide()
                styleLoader.activateStyle(value)
                $("#capp_theme").val(value)
                $("body").show("fast") //trigger reflow so new style gets applied everywhere (especially scrollbars!) What a shitty bug
            })

            generalBinding.initialize()
        })()
    }

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