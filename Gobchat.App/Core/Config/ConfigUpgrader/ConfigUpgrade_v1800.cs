/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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

using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class ConfigUpgrade_v1800 : IConfigUpgrade
    {
        public int MinVersion => 1701;

        public int MaxVersion => 1799;

        public int TargetVersion => 1800;

        public JObject Upgrade(JObject src)
        {
            JObject dst = (JObject)src.DeepClone();

            JsonUtil.MoveIfAvailable(dst, "behaviour.autodetectEmoteInSay", dst, "behaviour.chat.autodetectEmoteInSay");

            JsonUtil.MoveIfAvailable(dst, "behaviour.channel.visible", dst, "behaviour.chattabs.data.chat.channel.visible");
            JsonUtil.MoveIfAvailable(dst, "behaviour.showTimestamp", dst, "behaviour.chattabs.data.chat.formatting.timestamps");
            JsonUtil.MoveIfAvailable(dst, "behaviour.rangefilter.active", dst, "behaviour.chattabs.data.chat.formatting.rangefilter");

            return dst;
        }
    }
}