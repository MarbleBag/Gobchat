/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

using Gobchat.Core.Chat;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class ConfigUpgrade_v1701 : IConfigUpgrade
    {
        public int MinVersion => 16;

        public int MaxVersion => 1700;

        public int TargetVersion => 1701;

        public JObject Upgrade(JObject src)
        {
            JObject dst = (JObject)src.DeepClone();

            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-1.ffgroup", 0);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-2.ffgroup", 1);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-3.ffgroup", 2);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-4.ffgroup", 3);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-5.ffgroup", 4);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-6.ffgroup", 5);
            JsonUtil.SetIfAvailable(dst, "behaviour.groups.data.group-ff-7.ffgroup", 6);

            var enumTranslationDictionary = Enum.GetValues(typeof(FFXIVChatChannel)).Cast<FFXIVChatChannel>().ToDictionary(
                key => key.ToString().ToUpperInvariant(),
                value => GobchatChannelMapping.GetChannel(value).ChatChannel);

            JArray ReplaceContent(JArray array)
            {
                var result = new JArray();
                foreach (var element in array)
                {
                    var key = element.ToString().ToUpperInvariant();
                    if (enumTranslationDictionary.TryGetValue(key, out var replacement))
                    {
                        if (replacement == ChatChannel.None)
                            continue;
                        var value = replacement.ToString().ToUpperInvariant();
                        if (!result.Contains(value))
                            result.Add(value);
                    }
                }

                return result;
            }

            JsonUtil.MoveIfAvailable(dst, "style.channel.animated-emote", dst, "style.channel.animatedemote");
            JsonUtil.MoveIfAvailable(dst, "style.channel.roll", dst, "style.channel.random");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-1", dst, "style.channel.crossworldlinkshell-1");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-2", dst, "style.channel.crossworldlinkshell-2");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-3", dst, "style.channel.crossworldlinkshell-3");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-4", dst, "style.channel.crossworldlinkshell-4");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-5", dst, "style.channel.crossworldlinkshell-5");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-6", dst, "style.channel.crossworldlinkshell-6");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-7", dst, "style.channel.crossworldlinkshell-7");
            JsonUtil.MoveIfAvailable(dst, "style.channel.worldlinkshell-8", dst, "style.channel.crossworldlinkshell-8");

            JsonUtil.ReplaceArrayIfAvailable(dst, "behaviour.channel.roleplay", ReplaceContent);
            JsonUtil.ReplaceArrayIfAvailable(dst, "behaviour.channel.mention", ReplaceContent);
            JsonUtil.ReplaceArrayIfAvailable(dst, "behaviour.channel.visible", ReplaceContent);
            JsonUtil.ReplaceArrayIfAvailable(dst, "behaviour.channel.rangefilter", ReplaceContent);

            JsonUtil.ReplaceArrayIfAvailable(dst, "behaviour.channel.visible", (array) =>
            {
                array.Add(ChatChannel.GobchatInfo.ToString().ToUpperInvariant());
                array.Add(ChatChannel.GobchatError.ToString().ToUpperInvariant());
                return array;
            });

            return dst;
        }
    }
}