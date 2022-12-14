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
var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var _CommandManager_instances, _CommandManager_commandMaps, _CommandManager_handlers, _CommandManager_getHandler;
import * as Utility from './CommonUtility.js';
const commandPrefix = "gc";
export class CommandManager {
    constructor() {
        _CommandManager_instances.add(this);
        _CommandManager_commandMaps.set(this, new Map());
        _CommandManager_handlers.set(this, []);
        this.registerCmdHandler(new PlayerGroupCommandHandler());
        this.registerCmdHandler(new ProfileSwitchCommandHandler());
        this.registerCmdHandler(new CloseCommandHandler());
        this.registerCmdHandler(new ConfigOpenCommandHandler());
        this.registerCmdHandler(new ConfigResetCommandHandler());
        this.registerCmdHandler(new PlayerCountCommandHandler());
        this.registerCmdHandler(new PlayerListCommandHandler());
        this.registerCmdHandler(new PlayerDistanceCommandHandler());
        this.registerCmdHandler(new DisableGobInfoHandler());
    }
    registerCmdHandler(commandHandler) {
        for (let cmd of commandHandler.acceptedCommandNames) {
            __classPrivateFieldGet(this, _CommandManager_commandMaps, "f").set(cmd, commandHandler);
        }
        __classPrivateFieldGet(this, _CommandManager_handlers, "f").push(commandHandler);
    }
    async processCommand(message) {
        if (message === null || message === undefined)
            return;
        message = message.trim();
        if (!message.startsWith(commandPrefix))
            return;
        const [cmdHandle, cmd, args] = __classPrivateFieldGet(this, _CommandManager_instances, "m", _CommandManager_getHandler).call(this, message.substring(commandPrefix.length + 1).trim());
        if (cmdHandle) {
            await cmdHandle.execute(this, cmd, args);
        }
        else {
            const availableCmds = Array.from(__classPrivateFieldGet(this, _CommandManager_commandMaps, "f").keys()).join(", ");
            const msg = await gobLocale.getAndFormat("main.cmdmanager.availablecmds", availableCmds);
            GobchatAPI.sendInfoChatMessage(msg);
        }
    }
}
_CommandManager_commandMaps = new WeakMap(), _CommandManager_handlers = new WeakMap(), _CommandManager_instances = new WeakSet(), _CommandManager_getHandler = function _CommandManager_getHandler(msg) {
    for (const handler of __classPrivateFieldGet(this, _CommandManager_handlers, "f")) {
        for (let cmd of handler.acceptedCommandNames) {
            if (msg.startsWith(cmd)) {
                return [handler, cmd, msg.substring(cmd.length + 1).trim()];
            }
        }
    }
    return [null, null, null];
};
const playerGroupRegEx = /(\d+)\b\s+\b(add|remove|clear)\b\s*.*?\b(([ \w'`´-]+)(\s*\[\w+\])?)?/i;
class PlayerGroupCommandHandler {
    get acceptedCommandNames() {
        return ["group", "g"];
    }
    async execute(commandManager, commandName, args) {
        const localization = await gobLocale.getAll([
            "main.cmdmanager.cmd.group.invalid.cmd", "main.cmdmanager.cmd.group.invalid.grpidx", "main.cmdmanager.cmd.group.invalid.nosupport", "main.cmdmanager.cmd.group.invalid.name",
            "main.cmdmanager.cmd.group.grouped", "main.cmdmanager.cmd.group.notgrouped",
            "main.cmdmanager.cmd.group.add", "main.cmdmanager.cmd.group.remove", "main.cmdmanager.cmd.group.remove.all"
        ]);
        const result = playerGroupRegEx.exec(args);
        if (!result) {
            GobchatAPI.sendErrorChatMessage(localization["main.cmdmanager.cmd.group.invalid.cmd"]);
            return;
        }
        function getMatchingGroup(result, idx) {
            const r = result.length > idx - 1 ? result[idx] : null;
            return r === undefined ? null : r;
        }
        const groupIdx = parseInt(getMatchingGroup(result, 1), 10); //a number
        const task = getMatchingGroup(result, 2).toLowerCase(); //either add, remove or clear
        const playerNameComposite = getMatchingGroup(result, 4); //may be null
        const serverName = getMatchingGroup(result, 5); //may be null
        function isAvailable(str) {
            return str !== null && str !== undefined && str.length > 0;
        }
        if ((task === "add" || task === "remove") && !isAvailable(getMatchingGroup(result, 3))) { //add and remove also need a target name
            GobchatAPI.sendErrorChatMessage(localization["main.cmdmanager.cmd.group.invalid.cmd"]);
            return;
        }
        const groupsorting = gobConfig.get("behaviour.groups.sorting");
        if (groupIdx <= 0 || groupsorting.length < groupIdx) {
            GobchatAPI.sendErrorChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.invalid.grpidx"], groupsorting.length));
            return;
        }
        const groupId = groupsorting[groupIdx - 1];
        const group = gobConfig.get(`behaviour.groups.data.${groupId}`);
        if (!("trigger" in group)) {
            GobchatAPI.sendErrorChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.invalid.nosupport"], groupIdx, group.name, group.hiddenName));
            return;
        }
        if (task === "clear") {
            gobConfig.set(`behaviour.groups.data.${groupId}.trigger`, []);
            GobchatAPI.sendInfoChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.remove.all"], groupIdx, group.name));
            gobConfig.saveConfig();
            return; //done
        }
        let playerNameAndServer = null;
        if (serverName !== null) { //server is given in the form of '[server]'
            playerNameAndServer = playerNameComposite.trim() + " " + serverName.trim();
        }
        else {
            playerNameAndServer = playerNameComposite.trim();
        }
        playerNameAndServer = playerNameAndServer.toLowerCase().replace(/\s\s+/g, ' ');
        if (playerNameAndServer.length === 0) {
            GobchatAPI.sendErrorChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.invalid.name"], playerNameAndServer));
            return;
        }
        if (task === "add") {
            if (!_.includes(group.trigger, playerNameAndServer)) {
                group.trigger.push(playerNameAndServer);
                gobConfig.set(`behaviour.groups.data.${groupId}.trigger`, group.trigger);
                GobchatAPI.sendInfoChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.add"], playerNameAndServer, groupIdx, group.name));
                await gobConfig.saveConfig();
            }
            else {
                GobchatAPI.sendInfoChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.grouped"], playerNameAndServer, groupIdx, group.name));
            }
        }
        else if (task === "remove") {
            if (_.includes(group.trigger, playerNameAndServer)) {
                _.remove(group.trigger, (i) => { return i === playerNameAndServer; });
                gobConfig.set(`behaviour.groups.data.${groupId}.trigger`, group.trigger);
                GobchatAPI.sendInfoChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.remove"], playerNameAndServer, groupIdx, group.name));
                await gobConfig.saveConfig();
            }
            else {
                GobchatAPI.sendInfoChatMessage(Utility.formatString(localization["main.cmdmanager.cmd.group.notgrouped"], playerNameAndServer, groupIdx, group.name));
            }
        }
    }
}
class ProfileSwitchCommandHandler {
    get acceptedCommandNames() {
        return ["profile load"];
    }
    async execute(commandManager, commandName, args) {
        const profileIds = gobConfig.profiles;
        if (Utility.isString(args) && args.length > 0) {
            args = args.toLowerCase();
            for (var profileId of profileIds) {
                const profile = gobConfig.getProfile(profileId);
                if (args === profile.profileName.toLowerCase()) {
                    gobConfig.activeProfile = profile.profileId;
                    GobchatAPI.sendInfoChatMessage(await gobLocale.getAndFormat("main.cmdmanager.cmd.profile.load", profile.profileName));
                    return;
                }
            }
        }
        // args wasn't a valid argument
        const sprofiles = profileIds.map(e => gobConfig.getProfile(e)).map(e => e.profileName).join(", ");
        GobchatAPI.sendErrorChatMessage(await gobLocale.getAndFormat("main.cmdmanager.cmd.profile.load.invalid", sprofiles));
    }
}
class CloseCommandHandler {
    get acceptedCommandNames() {
        return ["close"];
    }
    async execute(commandManager, commandName, args) {
        GobchatAPI.closeGobchat();
    }
}
class PlayerCountCommandHandler {
    get acceptedCommandNames() {
        return ["player count"];
    }
    async execute(commandManager, commandName, args) {
        const count = await GobchatAPI.getPlayerCount();
        GobchatAPI.sendInfoChatMessage(await gobLocale.getAndFormat("main.cmdmanager.cmd.playercount", count));
    }
}
class PlayerListCommandHandler {
    get acceptedCommandNames() {
        return ["player list"];
    }
    async execute(commandManager, commandName, args) {
        const list = await GobchatAPI.getPlayersAndDistance();
        GobchatAPI.sendInfoChatMessage(await gobLocale.getAndFormat("main.cmdmanager.cmd.playerlist", list.join(", ")));
    }
}
class PlayerDistanceCommandHandler {
    get acceptedCommandNames() {
        return ["player distance"];
    }
    async execute(commandManager, commandName, args) {
        const distance = await GobchatAPI.getPlayerDistance(args);
        GobchatAPI.sendInfoChatMessage(await gobLocale.getAndFormat("main.cmdmanager.cmd.playerdistance", `${distance.toFixed(2)}y`));
    }
}
class DisableGobInfoHandler {
    get acceptedCommandNames() {
        return ["info on", "info off", "error on", "error off"];
    }
    async execute(commandManager, commandName, args) {
        if ("info on" === commandName) {
            window.gobChatManager.showGobInfo(true);
        }
        else if ("info off" === commandName) {
            window.gobChatManager.showGobInfo(false);
        }
        else if ("error on" === commandName) {
            window.gobChatManager.showGobError(true);
        }
        else if ("error off" === commandName) {
            window.gobChatManager.showGobError(false);
        }
    }
}
class ConfigOpenCommandHandler {
    get acceptedCommandNames() {
        return ["config open"];
    }
    async execute(commandManager, commandName, args) {
        window.openGobConfig();
    }
}
class ConfigResetCommandHandler {
    get acceptedCommandNames() {
        return ["config reset frame"];
    }
    async execute(commandManager, commandName, args) {
        GobchatAPI.sendInfoChatMessage(await gobLocale.get("main.cmdmanager.cmd.config.reset.frame"));
        gobConfig.reset(`behaviour.frame.chat.size`);
        gobConfig.reset(`behaviour.frame.chat.position`);
        await gobConfig.saveConfig();
    }
}
