/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

namespace Gobchat.Core.Chat
{
    public enum FFXIVChatChannel : int
    {
        NONE = 0x0000,
        WORLD = 0x0003,
        SAY = 0x000a,
        EMOTE = 0x001c,
        YELL = 0x001e,
        SHOUT = 0x000b,
        TELL_SEND = 0x000c,
        TELL_RECIEVE = 0x000d,
        PARTY = 0x000e,
        GUILD = 0x0018,
        ALLIANCE = 0x000f,

        NPC_TALK = 0x0044,
        NPC_DIALOGUE = 0x003d,
        ANIMATED_EMOTE = 0x001d,
        PARTYFINDER = 0x0048,
        ECHO = 0x0038,
        ERROR = 0x003c,

        RANDOM_SELF = 0x084A,
        RANDOM_PARTY = 0x104A,
        RANDOM_OTHER = 0x204A,

        TELEPORT = 0x001f,
        SYSTEM = 0x0039, // online status, gear set equipped, triple triad allowed/disallowed, buy from npc, custom deliveries

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

        LOGOUT = 0x2246,

        LINKSHELL_JOIN = 0x2239,

        // 57 - Engage!

        // 313 - Battle commencing in 15 seconds!

        //All around YOU
        // 2091 - You use Summon III / You cast Ruin III
        // 2092 - You uses a serving of stuffed highland cabbage
        // 2105 - The Epic of Alexander (Ultimate) has begun. / Your item level has been synced to 475 / Topaz Carbuncle withdraws from the battlefield. / Class change / recieve gil
        // 2110 - You obtained XYZ
        // 2106 - You give Topaz Carbuncle the order “Place.”
        // 2218 - The attack misses
        // 2219 - You ready Teleport / You begin casting
        // 2221 - You recover X HP
        // 2222 - You gain the effect of / Meal benefit duration extended
        // 2224 - You lose the effect of Nocturnal Balance
        // 2731 - You begin casting Ruin III
        // 2729 - Critical direct hit! The living liquid takes 22483 damage
        // 2859 - You begin casting Ruin III
        // 2874 - You are defeated by the jagd doll

        //All around Party Members
        // 4139 - Gota'to Nhabo Zodiark uses Diurnal Sect. (Ability?)
        // 4140 - Kaito Asagi Odin uses a serving of stuffed highland cabbage
        // 4269 - You recover 8514 HP
        // 4270 - You gain the effect of Diurnal Opposition
        // 4397 - Gota'to Nhabo Zodiark recovers 800 MP
        // 4398 - Kaito Asagi Odin gains the effect of
        // 4400 - Gota'to Nhabo Zodiark loses the effect of
        // 4777 - Direct Hit (from other player)
        // 4777 - Critical direct hit! (from other player)
        // 4777 - The living liquid takes 8135 (+66%) damage
        // 4783 - The living liquid suffers the effect of Dia
        // 4922 - is defeated by the living liquid

        // 10283 - The living liquid uses Fluid Swing
        // 10409 - You take 75601 damage
        // 10537 - The living liquid hits Dave Tribale Odin for 18702 damage
        // 10543 - Dave Tribale Odin suffers the effect of Water Resistance Down II
        // 10929 - The living liquid recovers from the effect of Reprisal

        // 12585 - Artemis Junior Odin takes 0 damage.
        // 12591 - Artemis Junior Odin suffers the effect of Water Resistance Down II

        // 16427 - Your pet uses an action - Topaz Carbuncle casts Burning Strike
        // 16558 - Your pet buffs you - You gain the effect of Devotion
        // 16686 - Your pet buffs players - Dave Tribale Odin gains the effect of Devotion
        // 17065 - Your pet does damage - Critical direct hit! The living liquid takes 7404 damage
        // 17071 - Your pet debuffs a mob - The living liquid suffers the effect of Inferno

        // 18475 - Other player pet uses an action - The bunshin uses Aeolian Edge.
        // 19113 - Other player's pet does damage - The living liquid takes 6546 damage.
        // 19632 - Other player's pet loses buff - The Earthly Star loses the effect of Devotion
    }
}