'use strict'

//requieres Gobchat.ChannelEnum

var Gobchat = (function(Gobchat,undefined){
	
	const ChannelEnum = Gobchat.ChannelEnum
	Gobchat.DefaultChatConfig = Object.freeze({
		version: "0.1.3",
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
					ChannelEnum.ECHO, ChannelEnum.ERROR, ChannelEnum.RANDOM_OTHER, ChannelEnum.RANDOM_SELF,
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
				"ffgroup-1" : {
					"background-color": null,
				},
				"ffgroup-2" : {
					"background-color": null,
				},
				"ffgroup-3" : {
					"background-color": null,
				},
				"ffgroup-4" : {
					"background-color": null,
				},
				"ffgroup-5" : {
					"background-color": null,
				},
				"ffgroup-6" : {
					"background-color": null,
				},
				"ffgroup-7" : {
					"background-color": null,
				},
			},
			sender: {
				"ffgroup-1" : {
					"color": null,
					"background-color": null,
				},
				"ffgroup-2" : {
					"color": null,
					"background-color": null,
				},
				"ffgroup-3" : {
					"color": null,
					"background-color": null,
				},
				"ffgroup-4" : {
					"color": null,
					"background-color": null,
				},
				"ffgroup-5" : {
					"color": null,
					"background-color": null,
				},
				"ffgroup-6" : {
					"color": null,
					"background-color": null,
				},
				"ffgroup-7" : {
					"color": null,
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