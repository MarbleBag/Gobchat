namespace Gobchat
{
    public enum ChatChannelEnum
    {
        UNKNOWN = 0x0000,
        SAY = 0x000a,
        EMOTE = 0x001c,
        YELL = 0x001e,
        PARTY = 0x000e,
        ALLIANCE = 0x000f,
        GUILD = 0x0018,
        TELL_SEND = 0x000c,
        TELL_RECIEVE = 0x000d,

        ANIMATED_EMOTE = 0x001d,
        ECHO = 0x0038,
        SYSTEM_ERROR = 0x003c,
        NPC_TALK = 0x0044,
        PARTYFINDER = 0x0048,
        TELEPORT = 0x001f,
        LOCATION = 0x0039,

        WORLD_LINKSHELL_1 = 0x0025,
        WORLD_LINKSHELL_2 = 0x0065,
        WORLD_LINKSHELL_3 = 0x0066,
        WORLD_LINKSHELL_4 = 0x0067,
        WORLD_LINKSHELL_5 = 0x0068,
        WORLD_LINKSHELL_6 = 0x0069,
        WORLD_LINKSHELL_7 = 0x006A,
        WORLD_LINKSHELL_8 = 0x006B,

        LINKSHELL_1 = 0x0010,
        LINKSHELL_2 = 0x0011,
        LINKSHELL_3 = 0x0012,
        LINKSHELL_4 = 0x0013,
        LINKSHELL_5 = 0x0014,
        LINKSHELL_6 = 0x0015,
        LINKSHELL_7 = 0x0016,
        LINKSHELL_8 = 0x0017,        
    }
}
