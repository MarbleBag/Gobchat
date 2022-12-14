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

// compiler doesn't recognize globals.d.ts for this file

'use strict'

import * as Databinding from './modules/Databinding.js'
import * as Config from './modules/Config.js'
import * as Locale from './modules/Locale.js'
import * as Style from './modules/Style.js'
import * as Chat from './modules/Chat.js'
import * as Search from './modules/ChatSearchComponent.js'

// Indicate that window resizing is possible
document.addEventListener("OverlayStateUpdate", function (event) {
    if (!event.detail.isLocked)
        document.documentElement.classList.add("gob-document--resize");
    else
        document.documentElement.classList.remove("gob-document--resize");
});

// initialize global variables
jQuery(async function ($) {
    window.gobConfig = new Config.GobchatConfig(true)
    await gobConfig.loadConfig()
    
    window.gobLocale = new Locale.LocaleManager()

    window.gobStyles = new Style.StyleLoader(".")
    await gobStyles.initialize()
    
    window.gobChatManager = new Chat.ChatManager()
    await gobChatManager.control($(".gob-chat_box"))

    window.chatSearch = new Search.ChatSearch()
    chatSearch.control($(".gob-chat_toolbar_search"), $(".gob-chat_box"))
    chatSearch.hide()
    $("#gob_toggle_search").on("click", () => chatSearch.toggle())

    const binding = new Databinding.BindingContext(gobConfig)

    gobLocale.setLocale(gobConfig.get("behaviour.language"))
    binding.bindConfigListener("behaviour.language", (value) => {
        gobLocale.setLocale(value)
        gobLocale.updateElement($(document))
    })

    binding.bindConfigListener("style.theme", async (value) => {
        try {
            await gobStyles.activateStyles(value)
        } catch (e1) {
            console.error(e1)
            await gobStyles.activateStyles()
        }
    })

    binding.initialize()

    GobchatAPI.setUIReady(true)
});

// feature - open config
jQuery(function ($) {
    const localStorageKey = "gobchat-config-open"

    const navEntries = performance.getEntriesByType("navigation");
    for (let i = 0; i < navEntries.length; ++i) {
        if (navEntries[i].type === "reload") {
            window.localStorage.removeItem(localStorageKey)
            break
        }
    }

    function openGobchatConfig() {
        const isConfigOpen = window.localStorage.getItem(localStorageKey) || "false"

        if (isConfigOpen === "true")
            return

        window.localStorage.setItem(localStorageKey, "true")

        gobConfig.saveToLocalStore()

        const configWidth = gobConfig.get("behaviour.frame.config.size.width")
        const configHeight = gobConfig.get("behaviour.frame.config.size.height")

        const handle = window.open("config/config.html", 'Settings', `width=${configWidth},height=${configHeight}`)
        handle.saveConfig = function () {
            gobConfig.loadFromLocalStore()
            chatManager.updateStyle()
        }

        handle.focus()

        const timer = setInterval(function () {
            if (handle.closed) {
                clearInterval(timer);
                window.localStorage.removeItem(localStorageKey)
            }
        }, 1000);
    }

    $("#gob_show_config").on("click", openGobchatConfig)
    window.openGobConfig = openGobchatConfig
})