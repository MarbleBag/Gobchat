'use strict'

//requieres Gobchat.ChannelEnum

var Gobchat = (function(Gobchat){
	
	const ChannelEnum = Gobchat.ChannelEnum
	const DefaultChatConfig = Object.freeze({
		behaviour: {
			channel: {
				roleplay: [ //which channels are formatted for roleplay
					ChannelEnum.SAY, ChannelEnum.EMOTE, ChannelEnum.YELL, ChannelEnum.SHOUT, ChannelEnum.PARTY, ChannelEnum.GUILD, ChannelEnum.ALLIANCE
				],
				mention: [ //which channels can trigger a mention
					ChannelEnum.SAY, ChannelEnum.EMOTE, ChannelEnum.YELL, ChannelEnum.SHOUT, ChannelEnum.PARTY, ChannelEnum.GUILD, ChannelEnum.ALLIANCE,
					ChannelEnum.LINKSHELL_1, ChannelEnum.LINKSHELL_2, ChannelEnum.LINKSHELL_3, ChannelEnum.LINKSHELL_4, ChannelEnum.LINKSHELL_5, ChannelEnum.LINKSHELL_6, ChannelEnum.LINKSHELL_7, ChannelEnum.LINKSHELL_8,
					ChannelEnum.WORLD_LINKSHELL_1, ChannelEnum.WORLD_LINKSHELL_2, ChannelEnum.WORLD_LINKSHELL_3, ChannelEnum.WORLD_LINKSHELL_4, ChannelEnum.WORLD_LINKSHELL_5, ChannelEnum.WORLD_LINKSHELL_6, ChannelEnum.WORLD_LINKSHELL_7, ChannelEnum.WORLD_LINKSHELL_8
				],
			},
		},
		style: {
			ui: { //will be used to generate css
				chatbox: {
					"background-color": "rgba(20, 20, 20, 0.95)",
				},
				channel: {
					base: { // base style for every message/channel
						color: "#DEDEDE", 
						"font-family": "'Times New Roman', Times, serif",
						"font-size": "medium",
					},
					say: {
							color: "#FFFFFF",
							"background-color": null,
					},
					emote: {
							color: "#F19212",
							"background-color": null,
					},
					yell: {
							color: "#D1DE09",
							"background-color": null,
					},
					shout: {
							color: "#D1DE09",
							"background-color": null,
					},
					party: {
							color: "#05f7ff",
							"background-color": null,
					},
					guild: {
							color: "#50DE09",
							"background-color": null,
					},
					alliance: {
							color: "#ff5005",
							"background-color": null,
					},
					linkshell: {
							color: "#03fc73",
							"background-color": null,
					},
					worldlinkshell: {
							color: "#03fc73",
							"background-color": null,
					},
					tellrecieve : {
						color: "#A118BC",
					},
					tellsend : {
						color: "#AA3DC0",
					},
				},
				segment: {
					say: {
						color: null, //will fallback to channel.say
					},
					emote: {
						color: null, //will fallback to channel.emote
					},
					ooc: {
						color: "#FF5920",
					},
					mention: {
						color: "#9358E4",
					},
				},
			}
		},
	})
	
	function getConfigKey(key){
		if( key == undefined || key == null ) return []
		const parts = key.split(".")
		return parts
	}
	
	function getConfigValue(key,config){
		const steps = getConfigKey(key)
		let c = config
		for(let step of steps){
			if(step in c){
				c = c[step]
			}else{
				return undefined
			}
		}
		return c
	}
	
	function getConfigValueWithFallback(key,config,fallback){
		const v = getConfigValue(key,config)
		return v === undefined ? getConfigValue(key,fallback) : v
	}
	
	class GobchatConfig{
		constructor(){
			this._propertyListener = []
			this._config = {}
		}
		
		get(key){
			const steps = getConfigKey(key)
			//TODO
		}
		
		set(key, value){
			const steps = getConfigKey(key)
		}
		
		addPropertyListener(topic,callback){
			
		}
		
		firePropertyChange(topic){
			
		}
	}
	Gobchat.GobchatConfig = GobchatConfig

	

	

	
	return Gobchat	
}(Gobchat || {}));