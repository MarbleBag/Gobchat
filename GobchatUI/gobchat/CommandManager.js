'use strict'

var Gobchat = (function(Gobchat,undefined){
		
	class CommandManager{
		constructor(manager, config){
			this._manager = manager
			this._config = config
			this._cmdMap = new Map()
			this.registerCmdHandler(new PlayerGroupCommandHandler())
		}
		
		processCommand(message){			
			if(message === null || message === undefined) return
			message = message.trim()
			if(!message.startsWith("gc")) return
			
			const cmdLine = message.substring(2).trim()
			
			const cmdIdx = cmdLine.indexOf(' ')
			const cmd = cmdIdx < 0 ? cmdLine : cmdLine.substring(0,cmdIdx)
			const args = cmdIdx < 0 ? "" : cmdLine.substring(cmdIdx+1)
			
			const cmdHandle = this._cmdMap.get(cmd)
			if(cmdHandle){
				cmdHandle.execute(this, cmd, args)
			}else{
				this.sendErrorMessage(`'${cmd}' is not an available command`)
			}
		}
		
		sendErrorMessage(msg){
			this._manager.sendErrorMessage(msg)
		}
		
		sendInfoMessage(msg){
			this._manager.sendInfoMessage(msg)
		}
		
		registerCmdHandler(commandHandler){
			for(let cmd of commandHandler.acceptedCommandNames){
				this._cmdMap.set(cmd, commandHandler)
			}
		}		
		
	}
	Gobchat.CommandManager = CommandManager
	
	class CommandHandler{
		get acceptedCommandNames(){
			return []
		}
		
		execute(commandManager, commandName, args){
			
		}
	}
	
	
	function getNextElement(str, startIdx, delimiter){
		const idx = str.indexOf(delimiter, startIdx)
		if( idx < 0 ) return null
		return {txt: str.substring(startIdx,idx),idx: idx}
	}
	
	
	const playerGroupRegEx = /(\d+)\b\s+\b(add|remove|clear)\b\s*.*?\b([ \w\[\]'`´]+)?/i
	class PlayerGroupCommandHandler extends CommandHandler{
		get acceptedCommandNames(){
			return ["group","g"]
		}
		
		execute(commandManager, commandName, args){
			
			const result = playerGroupRegEx.exec(args)
			if(!result){
				commandManager.sendErrorMessage("Command 'group' expects: \ngroup groupnumber add/remove playername\ngroup groupnumber clear")
				return
			}
			
			const groupIdx = result[1]
			const task = result[2].toLowerCase()
			
			if((task === "add" || task === "remove") && !result[3] ){
				commandManager.sendErrorMessage("Command 'group' expects: \ngroup groupnumber add/remove playername\ngroup groupnumber clear")
				return
			}
			
			const gobconfig = commandManager._config
			
			const groupsorting = gobconfig.get("userdata.group.sorting")
			if(groupIdx <= 0 || groupsorting.length < groupIdx){
				commandManager.sendErrorMessage(`Command 'group' expects: groupnumber needs to be a number from [0,${groupsorting.length-1}]`)
				return
			}
			
			const groupId = groupsorting[groupIdx-1]
			const group = gobconfig.get(`userdata.group.data.${groupId}`)
			
			if(!("trigger" in group)){
				commandManager.sendErrorMessage(`Command 'group' expects: group ${groupIdx} does not support player names`)
				return
			}
			
			if(task === "clear"){
				gobconfig.set(`userdata.group.data.${groupId}.trigger`,[])
				commandManager.sendInfoMessage(`Removed all players from group ${groupIdx}`)	
				gobconfig.saveToPlugin()				
			}else {
				const playerNameAndServer = result[3].toLowerCase().trim()
				if( playerNameAndServer.length === 0 )
					return
				if(task === "add"){
					if( ! _.includes(group.trigger, playerNameAndServer) ){
						group.trigger.push(playerNameAndServer)
						gobconfig.firePropertyChange(`userdata.group.data.${groupId}.trigger`)
						commandManager.sendInfoMessage(`Added ${playerNameAndServer} to group ${groupIdx}`)
						gobconfig.saveToPlugin()
					}else{
						commandManager.sendInfoMessage(`${playerNameAndServer} already in group ${groupIdx}`)
					}
				}else if(task === "remove"){
					if( _.includes(group.trigger, playerNameAndServer) ){
						_.remove(group.trigger,(i)=>{return i===playerNameAndServer})
						gobconfig.firePropertyChange(`userdata.group.data.${groupId}.trigger`)
						commandManager.sendInfoMessage(`Removed ${playerNameAndServer} to group ${groupIdx}`)
						gobconfig.saveToPlugin()
					}else{
						commandManager.sendInfoMessage(`${playerNameAndServer} is not in group ${groupIdx}`)
					}
				}				
			}						
		}
	}
	
	return Gobchat	
}(Gobchat || {}));