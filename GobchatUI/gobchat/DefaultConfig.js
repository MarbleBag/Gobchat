'use strict'

//requieres Gobchat.ChannelEnum

var Gobchat = (function(Gobchat,undefined){
	
	const ChannelEnum = Gobchat.ChannelEnum
	const FFUnicode = Gobchat.FFUnicode
	
	Gobchat.DefaultChatConfig = Object.freeze({
		version: "0.2.0",
		behaviour: {
			channel: {
				roleplay: [ //which channels are formatted for roleplay
					ChannelEnum.SAY, ChannelEnum.EMOTE, ChannelEnum.YELL, ChannelEnum.SHOUT, ChannelEnum.PARTY, ChannelEnum.GUILD, ChannelEnum.ALLIANCE, ChannelEnum.TELL_SEND, ChannelEnum.TELL_RECIEVE,
				],
				mention: [ //which channels can trigger a mention
					ChannelEnum.SAY, ChannelEnum.EMOTE, ChannelEnum.YELL, ChannelEnum.SHOUT, ChannelEnum.PARTY, ChannelEnum.GUILD, ChannelEnum.ALLIANCE,
					ChannelEnum.LINKSHELL_1, ChannelEnum.LINKSHELL_2, ChannelEnum.LINKSHELL_3, ChannelEnum.LINKSHELL_4, ChannelEnum.LINKSHELL_5, ChannelEnum.LINKSHELL_6, ChannelEnum.LINKSHELL_7, ChannelEnum.LINKSHELL_8,
					ChannelEnum.WORLD_LINKSHELL_1, ChannelEnum.WORLD_LINKSHELL_2, ChannelEnum.WORLD_LINKSHELL_3, ChannelEnum.WORLD_LINKSHELL_4, ChannelEnum.WORLD_LINKSHELL_5, ChannelEnum.WORLD_LINKSHELL_6, ChannelEnum.WORLD_LINKSHELL_7, ChannelEnum.WORLD_LINKSHELL_8
				],
				visible: [
					ChannelEnum.SAY, ChannelEnum.EMOTE, ChannelEnum.YELL, ChannelEnum.SHOUT, ChannelEnum.TELL_SEND, ChannelEnum.TELL_RECIEVE, ChannelEnum.PARTY, ChannelEnum.GUILD, ChannelEnum.ALLIANCE,
					ChannelEnum.NPC_TALK, ChannelEnum.ANIMATED_EMOTE,
					ChannelEnum.ECHO, ChannelEnum.ERROR,
					ChannelEnum.RANDOM_OTHER, ChannelEnum.RANDOM_PARTY, ChannelEnum.RANDOM_SELF,
					ChannelEnum.WORLD_LINKSHELL_1, ChannelEnum.WORLD_LINKSHELL_2, ChannelEnum.WORLD_LINKSHELL_3, ChannelEnum.WORLD_LINKSHELL_4,
					ChannelEnum.WORLD_LINKSHELL_5, ChannelEnum.WORLD_LINKSHELL_6, ChannelEnum.WORLD_LINKSHELL_7, ChannelEnum.WORLD_LINKSHELL_8,
					ChannelEnum.LINKSHELL_1, ChannelEnum.LINKSHELL_2, ChannelEnum.LINKSHELL_3, ChannelEnum.LINKSHELL_4,
					ChannelEnum.LINKSHELL_5, ChannelEnum.LINKSHELL_6, ChannelEnum.LINKSHELL_7, ChannelEnum.LINKSHELL_8,
				],
			},			
			mentions: [],
			language: "en",
			autodetectEmoteInSay: true,
			playSoundOnMention: false,
			datacenter: null,
			displayServerType: 0,
		},
		userdata: { //will be copied whole on save
			group: {
				sorting: ["group-ff-1","group-ff-2","group-ff-3","group-ff-4","group-ff-5","group-ff-6","group-ff-7"],
				data: {
					/*{ //Template
						name: //group name
						active: true/false //if false, group can't be triggered
						ffgroup: //if set, group reflects one of the ffxiv friendlist groups. Not available for userAgent
					}*/
					"group-ff-1": {
						name: FFUnicode.GROUP_1.char,
						active: true,					
						ffgroup: 1,
						id: "group-ff-1",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},
					},
					"group-ff-2": {
						name: FFUnicode.GROUP_2.char,
						active: true,
						ffgroup: 2,
						id: "group-ff-2",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},
					},
					"group-ff-3": {
						name: FFUnicode.GROUP_3.char,
						active: true,
						ffgroup: 3,
						id: "group-ff-3",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},						
					},
					"group-ff-4": {
						name: FFUnicode.GROUP_4.char,
						active: true,
						ffgroup: 4,
						id: "group-ff-4",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},						
					},
					"group-ff-5": {
						name: FFUnicode.GROUP_5.char,
						active: true,
						ffgroup: 5,
						id: "group-ff-5",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},						
					},
					"group-ff-6": {
						name: FFUnicode.GROUP_6.char,
						active: true,
						ffgroup: 6,
						id: "group-ff-6",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},						
					},
					"group-ff-7": {
						name: FFUnicode.GROUP_7.char,
						active: true,
						ffgroup: 7,
						id: "group-ff-7",
						style: {
							body:{ "background-color": null, },
							header: { "background-color": null, "color": null, },
						},						
					}				
				},
			},
		},
		style: { //will be used to generate css
			chatbox: {
				"background-color": "rgba(20, 20, 20, 0.95)",
			},
			channel: {
				base: { // base style for every message/channel
					"color": "#DEDEDE", 
					"font-family": "'Times New Roman', Times, sans-serif",
					"font-size": "medium",
					"white-space": "pre-wrap",
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
				tellrecieve : {
					color: "#A118BC",
				},
				tellsend : {
					color: "#AA3DC0",
				},
				"linkshell-1": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-2": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-3": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-4": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-5": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-6": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-7": {
						"color": "#03fc73",
						"background-color": null,
				},
				"linkshell-8": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-1": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-2": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-3": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-4": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-5": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-6": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-7": {
						"color": "#03fc73",
						"background-color": null,
				},
				"worldlinkshell-8": {
						"color": "#03fc73",
						"background-color": null,
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
		},
	})
		
	return Gobchat	
}(Gobchat || {}));