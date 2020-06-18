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

'use strict'

jQuery(function ($) {
    $("#gobchat_showconfig").on("click", function (e) {
        const isConfigOpen = localStorage.getItem("gobchat-config-open") || "false"

        if (isConfigOpen === "true")
            return

        localStorage.setItem("gobchat-config-open", "true")

        window.chatManager.config.saveToLocalStore()

        const handle = window.open("config/config.html", 'Settings', 'width=900,height=600')
        handle.saveConfig = function () {
            window.chatManager.config.loadFromLocalStore()
            window.chatManager.updateStyle()
        }

        handle.focus()

        const timer = setInterval(function () {
            if (handle.closed) {
                clearInterval(timer);
                localStorage.removeItem("gobchat-config-open")
            }
        }, 1000);
    })
})