'use strict'

var Gobchat = (function (Gobchat) {
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