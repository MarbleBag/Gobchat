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
import * as Databinding from '../modules/Databinding';
import * as Config from '../modules/Config';
import * as Locale from '../modules/Locale';
import * as Styles from '../modules/Style';
import * as Dialog from '../modules/Dialog';
import { makeControl as makeNavControl } from '../modules/MenuNavigationComponent.js';
/*
declare global {
    var GobchatAPI: any
    var Gobchat: any
    var gobConfig: any
    var gobLocale: any
    var gobStyles: any
    function saveConfig()
}
*/
// initialize global variables
jQuery(async function () {
    window.GobchatAPI = window.opener.GobchatAPI;
    window.Gobchat = window.opener.Gobchat;
    window.console = window.opener.console;
    window.gobConfig = new Config.GobchatConfig();
    window.gobConfig.loadFromLocalStore(true);
    window.gobLocale = new Locale.LocaleManager();
    window.gobStyles = new Styles.StyleLoader("..");
    await gobStyles.initialize();
    const binding = new Databinding.BindingContext(gobConfig);
    // update all text on language change
    gobLocale.setLocale(gobConfig.get("behaviour.language"));
    binding.bindConfigListener("behaviour.language", (value) => {
        gobLocale.setLocale(value);
        gobLocale.updateElement($(document));
    });
    binding.bindConfigListener("style.theme", async (value) => {
        try {
            await gobStyles.activateStyles(value);
        }
        catch (e1) {
            await gobStyles.activateStyles();
        }
    });
    await makeNavControl($(".gob-config_navigation"));
    binding.initialize();
});
// initialize main buttons
jQuery(function ($) {
    $("#c_main_save-config").on("click", function () {
        window.gobConfig.saveToLocalStore();
        window.saveConfig();
    });
    $("#c_main_save-and-exit-config").on("click", function () {
        window.gobConfig.saveToLocalStore();
        window.saveConfig();
        window.close();
    });
    $("#c_main_cancel-config").on("click", async function () {
        const result = await Dialog.showConfirmationDialog({ dialogText: "config.main.nav.cancel.dialog" });
        if (result)
            window.close();
    });
    $("#c_main_close-gobchat").on("click", async function () {
        const result = await Dialog.showConfirmationDialog({ dialogText: "config.main.nav.closegobchat.dialog" });
        if (result) {
            window.close();
            GobchatAPI.closeGobchat();
        }
    });
});
