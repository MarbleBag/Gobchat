/*******************************************************************************this._config
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

'use strict'

var Gobchat = (function(Gobchat, undefined) {
    class CommandManager {
        constructor() {
            this._cmdMap = new Map()
            this._handlers = []
            this.registerCmdHandler(new PlayerGroupCommandHandler())
            this.registerCmdHandler(new ProfileSwitchCommandHandler())
            this.registerCmdHandler(new CloseCommandHandler())
            this.registerCmdHandler(new ConfigOpenCommandHandler())
            this.registerCmdHandler(new ConfigResetCommandHandler())
            this.registerCmdHandler(new PlayerCountCommandHandler())
            this.registerCmdHandler(new PlayerListCommandHandler())
            this.registerCmdHandler(new PlayerDistanceCommandHandler())
            this.registerCmdHandler(new DisableGobInfoHandler())
        }

        async processCommand(message) {
            if (message === null || message === undefined) return
            message = message.trim()
            if (!message.startsWith("gc")) return

            const [cmdHandle, cmd, args] = this._getHandler(message.substring(2).trim())

            if (cmdHandle) {
                await cmdHandle.execute(this, cmd, args)
            } else {
                const availableCmds = Array.from(this._cmdMap.keys()).join(", ")
                const msg = await this.getTranslationAndFormat("main.cmdmanager.availablecmds", availableCmds)
                this.sendInfoMessage(msg)
            }
        }

        get config() {
            return window.gobconfig
        }

        async getTranslation(key) {
            return await gobLocale.get(key)
        }

        async getTranslations(keys) {
            return await gobLocale.getAll(keys)
        }

        async getTranslationAndFormat(key, params) {
            return await gobLocale.getAndFormat(key, params)
        }

        _getHandler(msg) {
            for (const handler of this._handlers) {
                for (let cmd of handler.acceptedCommandNames) {
                    if (msg.startsWith(cmd)) {
                        return [handler, cmd, msg.substring(cmd.length).trim()]
                    }
                }
            }
            return [null, null, null]
        }

        sendErrorMessage(msg) {
            GobchatAPI.sendErrorChatMessage(msg)
        }

        sendInfoMessage(msg) {
            GobchatAPI.sendInfoChatMessage(msg)
        }

        registerCmdHandler(commandHandler) {
            for (let cmd of commandHandler.acceptedCommandNames) {
                this._cmdMap.set(cmd, commandHandler)
            }
            this._handlers.push(commandHandler)
        }
    }
    Gobchat.CommandManager = CommandManager

    class CommandHandler {
        get acceptedCommandNames() {
            return []
        }

        execute(commandManager, commandName, args) {
        }
    }

    function getNextElement(str, startIdx, delimiter) {
        const idx = str.indexOf(delimiter, startIdx)
        if (idx < 0) return null
        return { txt: str.substring(startIdx, idx), idx: idx }
    }

    const playerGroupRegEx = /(\d+)\b\s+\b(add|remove|clear)\b\s*.*?\b(([ \w'`´-]+)(\s*\[\w+\])?)?/i
    class PlayerGroupCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["group", "g"]
        }

        async execute(commandManager, commandName, args) {
            const localization = await commandManager.getTranslations([
                "main.cmdmanager.cmd.group.invalid.cmd", "main.cmdmanager.cmd.group.invalid.grpidx", "main.cmdmanager.cmd.group.invalid.nosupport", "main.cmdmanager.cmd.group.invalid.name",
                "main.cmdmanager.cmd.group.grouped", "main.cmdmanager.cmd.group.notgrouped",
                "main.cmdmanager.cmd.group.add", "main.cmdmanager.cmd.group.remove", "main.cmdmanager.cmd.group.remove.all"
            ])

            const result = playerGroupRegEx.exec(args)
            if (!result) {
                commandManager.sendErrorMessage(localization["main.cmdmanager.cmd.group.invalid.cmd"])
                return
            }

            function getMatchingGroup(result, idx) {
                const r = result.length > idx - 1 ? result[idx] : null
                return r === undefined ? null : r
            }

            const groupIdx = parseInt(getMatchingGroup(result, 1), 10) 	//a number
            const task = getMatchingGroup(result, 2).toLowerCase() 		//either add, remove or clear
            const playerNameComposite = getMatchingGroup(result, 4) 		//may be null
            const serverName = getMatchingGroup(result, 5) 				//may be null

            function isAvailable(str) {
                return str !== null && str !== undefined && str.length > 0
            }

            if ((task === "add" || task === "remove") && !isAvailable(getMatchingGroup(result, 3))) { //add and remove also need a target name
                commandManager.sendErrorMessage(localization["main.cmdmanager.cmd.group.invalid.cmd"])
                return
            }

            const gobconfig = commandManager.config
            const groupsorting = gobconfig.get("behaviour.groups.sorting")
            if (groupIdx <= 0 || groupsorting.length < groupIdx) {
                commandManager.sendErrorMessage(
                    Gobchat.formatString(localization["main.cmdmanager.cmd.group.invalid.grpidx"], groupsorting.length)
                )
                return
            }

            const groupId = groupsorting[groupIdx - 1]
            const group = gobconfig.get(`behaviour.groups.data.${groupId}`)

            if (!("trigger" in group)) {
                commandManager.sendErrorMessage(
                    Gobchat.formatString(localization["main.cmdmanager.cmd.group.invalid.nosupport"], groupIdx, group.name, group.hiddenName)
                )
                return
            }

            if (task === "clear") {
                gobconfig.set(`behaviour.groups.data.${groupId}.trigger`, [])
                commandManager.sendInfoMessage(
                    Gobchat.formatString(localization["main.cmdmanager.cmd.group.remove.all"], groupIdx, group.name)
                )
                gobconfig.saveConfig()
                return //done
            }

            let playerNameAndServer = null
            if (serverName !== null) { //server is given in the form of '[server]'
                playerNameAndServer = playerNameComposite.trim() + " " + serverName.trim()
            } else {
                playerNameAndServer = playerNameComposite.trim()
            }

            playerNameAndServer = playerNameAndServer.toLowerCase().replace(/\s\s+/g, ' ')

            if (playerNameAndServer.length === 0) {
                commandManager.sendErrorMessage(
                    Gobchat.formatString(localization["main.cmdmanager.cmd.group.invalid.name"], playerNameAndServer)
                )
                return
            }

            if (task === "add") {
                if (!_.includes(group.trigger, playerNameAndServer)) {
                    group.trigger.push(playerNameAndServer)
                    gobconfig.set(`behaviour.groups.data.${groupId}.trigger`, group.trigger)
                    commandManager.sendInfoMessage(
                        Gobchat.formatString(localization["main.cmdmanager.cmd.group.add"], playerNameAndServer, groupIdx, group.name)
                    )
                    gobconfig.saveConfig()
                } else {
                    commandManager.sendInfoMessage(
                        Gobchat.formatString(localization["main.cmdmanager.cmd.group.grouped"], playerNameAndServer, groupIdx, group.name)
                    )
                }
            } else if (task === "remove") {
                if (_.includes(group.trigger, playerNameAndServer)) {
                    _.remove(group.trigger, (i) => { return i === playerNameAndServer })
                    gobconfig.set(`behaviour.groups.data.${groupId}.trigger`, group.trigger)
                    commandManager.sendInfoMessage(
                        Gobchat.formatString(localization["main.cmdmanager.cmd.group.remove"], playerNameAndServer, groupIdx, group.name)
                    )
                    gobconfig.saveConfig()
                } else {
                    commandManager.sendInfoMessage(
                        Gobchat.formatString(localization["main.cmdmanager.cmd.group.notgrouped"], playerNameAndServer, groupIdx, group.name)
                    )
                }
            }
        }
    }

    class ProfileSwitchCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["profile load"]
        }

        async execute(commandManager, commandName, args) {
            const gobconfig = commandManager.config
            const profileIds = gobconfig.profiles

            if (Gobchat.isString(args) && args.length > 0) {
                args = args.toUpperCase()
                for (var profileId of profileIds) {
                    const profile = gobconfig.getProfile(profileId)
                    if (args === profile.profileName.toUpperCase()) {
                        gobconfig.activeProfile = profile.profileId
                        commandManager.sendInfoMessage(
                            await commandManager.getTranslationAndFormat("main.cmdmanager.cmd.profile.load", profile.profileName)
                        )
                        return
                    }
                }
            }

            // args wasn't a valid argument
            const sprofiles = profileIds.map(e => gobconfig.getProfile(e)).map(e => e.profileName).join(", ")
            commandManager.sendErrorMessage(
                await commandManager.getTranslationAndFormat("main.cmdmanager.cmd.profile.load.invalid", sprofiles)
            )
        }
    }

    class CloseCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["close"]
        }

        execute(commandManager, commandName, args) {
            GobchatAPI.closeGobchat()
        }
    }

    class PlayerCountCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["player count"]
        }

        async execute(commandManager, commandName, args) {
            const count = await GobchatAPI.getPlayerCount()
            commandManager.sendInfoMessage(
                await commandManager.getTranslationAndFormat("main.cmdmanager.cmd.playercount", count)
            )
        }
    }

    class PlayerListCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["player list"]
        }

        async execute(commandManager, commandName, args) {
            const list = await GobchatAPI.getPlayersAndDistance()
            commandManager.sendInfoMessage(
                await commandManager.getTranslationAndFormat("main.cmdmanager.cmd.playerlist", list.join(", "))
            )
        }
    }

    class PlayerDistanceCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["player distance"]
        }

        async execute(commandManager, commandName, args) {
            const distance = await GobchatAPI.getPlayerDistance(args)
            commandManager.sendInfoMessage(
                await commandManager.getTranslationAndFormat("main.cmdmanager.cmd.playerdistance", `${distance.toFixed(2)}y`)
            )
        }
    }

    class DisableGobInfoHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["info on", "info off", "error on", "error off"]
        }

        async execute(commandManager, commandName, args) {
            if ("info on" == commandName) {
                window.chatManager.showGobInfo(true)
            } else if ("info off" == commandName) {
                window.chatManager.showGobInfo(false)
            } else if ("error on" == commandName) {
                window.chatManager.showGobError(true)
            } else if ("error off" == commandName) {
                window.chatManager.showGobError(false)
            }
        }
    }

    class ConfigOpenCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["config open"]
        }

        async execute(commandManager, commandName, args) {
            window.openGobconfig()
        }
    }

    class ConfigResetCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["config reset frame"]
        }

        async execute(commandManager, commandName, args) {
            commandManager.sendInfoMessage(
                await commandManager.getTranslation("main.cmdmanager.cmd.config.reset.frame")
            )

            gobconfig.reset(`behaviour.frame.chat.size`)
            gobconfig.reset(`behaviour.frame.chat.position`)
            await gobconfig.saveConfig()
        }
    }

    return Gobchat
}(Gobchat || {}));