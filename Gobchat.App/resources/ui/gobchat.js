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

/* Resize display */
document.addEventListener("OverlayStateUpdate", function (e) {
    if (!e.detail.isLocked)
        document.documentElement.classList.add("gob-resize-active");
    else
        document.documentElement.classList.remove("gob-resize-active");
});

jQuery(function ($) {
    window.chatManager = new Gobchat.GobchatManager("chatbox_content")
    window.chatManager.init()

    GobchatAPI.setUIReady(true)
})