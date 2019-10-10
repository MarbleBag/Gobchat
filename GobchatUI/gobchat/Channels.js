'use strict'

var Gobchat = (function(Gobchat){
	
	Gobchat.ChannelEnum = Object.freeze({
		SAY:			0x000a,
		EMOTE:			0x001c,
		YELL:			0x001e,
		SHOUT:			0x000b,
		TELL_SEND:		0x000c,
		TELL_RECIEVE:	0x000d,
		PARTY:			0x000e,
		GUILD:			0x0018,
		ALLIANCE:		0x000f,
		
		NPC_TALK:		0x0044,
		ANIMATED_EMOTE:	0x001d,
		PARTYFINDER:	0x0048,
		ECHO:			0x0038,
		ERROR:			0x003c,
		
		RANDOM_SELF:	0x084A,
		RANDOM_OTHER:	0x204A,
		
		TELEPORT:		0x001f,
		LOCATION:		0x0039,
		
		WORLD_LINKSHELL_1:	0x0025,
		WORLD_LINKSHELL_2:	0x0065,
		WORLD_LINKSHELL_3:	0x0066,
		WORLD_LINKSHELL_4:	0x0067,
		WORLD_LINKSHELL_5:	0x0068,
		WORLD_LINKSHELL_6:	0x0069,
		WORLD_LINKSHELL_7:	0x006A,
		WORLD_LINKSHELL_8:	0x006B,
		
		LINKSHELL_1:		0x0010,
		LINKSHELL_2:		0x0011,
		LINKSHELL_3:		0x0012,
		LINKSHELL_4:		0x0013,
		LINKSHELL_5:		0x0014,
		LINKSHELL_6:		0x0015,
		LINKSHELL_7:		0x0016,
		LINKSHELL_8:		0x0017,
	})	
	
	Gobchat.MessageSegmentEnum = Object.freeze({
		UNDEFINED: 	0,
		SAY:		1,
		EMOTE:		2,
		OOC:		3,
		MENTION:	4,
	})
	
	//Channel in which a player can write something
	Gobchat.PlayerChannel = Object.freeze([
		Gobchat.ChannelEnum.SAY, Gobchat.ChannelEnum.EMOTE, Gobchat.ChannelEnum.YELL, Gobchat.ChannelEnum.SHOUT, Gobchat.ChannelEnum.TELL_SEND, Gobchat.ChannelEnum.TELL_RECIEVE, Gobchat.ChannelEnum.PARTY, Gobchat.ChannelEnum.GUILD, Gobchat.ChannelEnum.ALLIANCE,
		Gobchat.ChannelEnum.ANIMATED_EMOTE,
		Gobchat.ChannelEnum.WORLD_LINKSHELL_1, Gobchat.ChannelEnum.WORLD_LINKSHELL_2, Gobchat.ChannelEnum.WORLD_LINKSHELL_3, Gobchat.ChannelEnum.WORLD_LINKSHELL_4,
		Gobchat.ChannelEnum.WORLD_LINKSHELL_5, Gobchat.ChannelEnum.WORLD_LINKSHELL_6, Gobchat.ChannelEnum.WORLD_LINKSHELL_7, Gobchat.ChannelEnum.WORLD_LINKSHELL_8,
		Gobchat.ChannelEnum.LINKSHELL_1, Gobchat.ChannelEnum.LINKSHELL_2, Gobchat.ChannelEnum.LINKSHELL_3, Gobchat.ChannelEnum.LINKSHELL_4,
		Gobchat.ChannelEnum.LINKSHELL_5, Gobchat.ChannelEnum.LINKSHELL_6, Gobchat.ChannelEnum.LINKSHELL_7, Gobchat.ChannelEnum.LINKSHELL_8,
	])
		
	return Gobchat
	
}(Gobchat || {}));