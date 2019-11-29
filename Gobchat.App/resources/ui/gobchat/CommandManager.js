'use strict'

var Gobchat = (function (Gobchat, undefined) {
    class CommandManager {
        constructor(manager, config) {
            this._manager = manager
            this._config = config
            this._cmdMap = new Map()
            this.registerCmdHandler(new PlayerGroupCommandHandler())
        }

        processCommand(message) {
            if (message === null || message === undefined) return
            message = message.trim()
            if (!message.startsWith("gc")) return

            const cmdLine = message.substring(2).trim()

            const cmdIdx = cmdLine.indexOf(' ')
            const cmd = cmdIdx < 0 ? cmdLine : cmdLine.substring(0, cmdIdx)
            const args = cmdIdx < 0 ? "" : cmdLine.substring(cmdIdx + 1)

            const cmdHandle = this._cmdMap.get(cmd)
            if (cmdHandle) {
                cmdHandle.execute(this, cmd, args)
            } else {
                this.sendErrorMessage(`'${cmd}' is not an available command`)
            }
        }

        sendErrorMessage(msg) {
            this._manager.sendErrorMessage(msg)
        }

        sendInfoMessage(msg) {
            this._manager.sendInfoMessage(msg)
        }

        registerCmdHandler(commandHandler) {
            for (let cmd of commandHandler.acceptedCommandNames) {
                this._cmdMap.set(cmd, commandHandler)
            }
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

    const playerGroupRegEx = /(\d+)\b\s+\b(add|remove|clear)\b\s*.*?\b(([ \w'`´]+)(\s*\[\w+\])?)?/i
    class PlayerGroupCommandHandler extends CommandHandler {
        get acceptedCommandNames() {
            return ["group", "g"]
        }

        execute(commandManager, commandName, args) {
            const result = playerGroupRegEx.exec(args)
            if (!result) {
                commandManager.sendErrorMessage("Command 'group' expects: \ngroup groupnumber add/remove playername\ngroup groupnumber clear")
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
                commandManager.sendErrorMessage("Command 'group' expects: \ngroup groupnumber add/remove playername\ngroup groupnumber clear")
                return
            }

            const gobconfig = commandManager._config
            const groupsorting = gobconfig.get("behaviour.groups.sorting")
            if (groupIdx <= 0 || groupsorting.length < groupIdx) {
                commandManager.sendErrorMessage(`Command 'group' expects: groupnumber needs to be a number from [1, ${groupsorting.length}]`)
                return
            }

            const groupId = groupsorting[groupIdx - 1]
            const group = gobconfig.get(`behaviour.groups.data.${groupId}`)

            if (!("trigger" in group)) {
                commandManager.sendErrorMessage(`Command 'group' expects: group [${groupIdx}]${group.name} does not support player names`)
                return
            }

            if (task === "clear") {
                gobconfig.set(`behaviour.groups.data.${groupId}.trigger`, [])
                commandManager.sendInfoMessage(`Removed all players from group [${groupIdx}]${group.name}`)
                gobconfig.saveToPlugin()
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
                commandManager.sendErrorMessage(`Command 'group' unable to read player name '${playerNameAndServer}'`)
                return
            }

            if (task === "add") {
                if (!_.includes(group.trigger, playerNameAndServer)) {
                    group.trigger.push(playerNameAndServer)
                    gobconfig.firePropertyChange(`behaviour.groups.data.${groupId}.trigger`)
                    commandManager.sendInfoMessage(`Added ${playerNameAndServer} to group [${groupIdx}]${group.name}`)
                    gobconfig.saveToPlugin()
                } else {
                    commandManager.sendInfoMessage(`${playerNameAndServer} is already in group [${groupIdx}]${group.name}`)
                }
            } else if (task === "remove") {
                if (_.includes(group.trigger, playerNameAndServer)) {
                    _.remove(group.trigger, (i) => { return i === playerNameAndServer })
                    gobconfig.firePropertyChange(`behaviour.groups.data.${groupId}.trigger`)
                    commandManager.sendInfoMessage(`Removed ${playerNameAndServer} from group [${groupIdx}]${group.name}`)
                    gobconfig.saveToPlugin()
                } else {
                    commandManager.sendInfoMessage(`${playerNameAndServer} is not in group [${groupIdx}]${group.name}`)
                }
            }
        }
    }

    return Gobchat
}(Gobchat || {}));