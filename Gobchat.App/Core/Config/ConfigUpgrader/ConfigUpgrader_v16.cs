﻿/*******************************************************************************
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    // FINALIZED DO NOT CHANGE
    internal sealed class ConfigUpgrader_v16 : IJsonTransformer
    {
        public JObject Transform(JObject src)
        {
            var dst = new JObject();
            dst["version"] = 16;

            // clean up old profiles
            JsonUtil.CopyIfAvailable(src, "profile", dst, "profile"); // keep around

            JsonUtil.CopyIfAvailable(src, "behaviour.channel.roleplay", dst, "behaviour.channel.roleplay");
            JsonUtil.CopyIfAvailable(src, "behaviour.channel.mention", dst, "behaviour.channel.mention");
            JsonUtil.CopyIfAvailable(src, "behaviour.channel.visible", dst, "behaviour.channel.visible");
            JsonUtil.CopyIfAvailable(src, "behaviour.channel.fadeout", dst, "behaviour.channel.rangefilter");

            JsonUtil.CopyIfAvailable(src, "behaviour.segment", dst, "behaviour.segment"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.frame", dst, "behaviour.frame"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.language", dst, "behaviour.language"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.mentions", dst, "behaviour.mentions"); // keep around

            JsonUtil.CopyIfAvailable(src, "behaviour.fadeout.active", dst, "behaviour.rangefilter.active");
            JsonUtil.CopyIfAvailable(src, "behaviour.fadeout.cutoff", dst, "behaviour.rangefilter.cutoff");
            JsonUtil.CopyIfAvailable(src, "behaviour.fadeout.falloff", dst, "behaviour.rangefilter.fadeout");
            JsonUtil.CopyIfAvailable(src, "behaviour.fadeout.endopacity", dst, "behaviour.rangefilter.endopacity");
            JsonUtil.CopyIfAvailable(src, "behaviour.fadeout.startopacity", dst, "behaviour.rangefilter.startopacity");
            JsonUtil.CopyIfAvailable(src, "behaviour.fadeout.mention", dst, "behaviour.rangefilter.ignoreMention");

            JsonUtil.CopyIfAvailable(src, "behaviour.chatUpdateInterval", dst, "behaviour.chat.updateInterval");

            JsonUtil.CopyIfAvailable(src, "behaviour.checkForUpdate", dst, "behaviour.appUpdate.checkOnline");
            JsonUtil.CopyIfAvailable(src, "behaviour.checkForBetaUpdate", dst, "behaviour.appUpdate.acceptBeta");

            JsonUtil.CopyIfAvailable(src, "behaviour.autodetectEmoteInSay", dst, "behaviour.autodetectEmoteInSay"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.hideOnMinimize", dst, "behaviour.hideOnMinimize"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.writeChatLog", dst, "behaviour.writeChatLog"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.showTimestamp", dst, "behaviour.showTimestamp"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.hotkeys", dst, "behaviour.hotkeys"); // keep around
            JsonUtil.CopyIfAvailable(src, "behaviour.groups", dst, "behaviour.groups"); // keep around
            JsonUtil.CopyIfAvailable(src, "style", dst, "style"); // keep around

            return dst;
        }
    }
}