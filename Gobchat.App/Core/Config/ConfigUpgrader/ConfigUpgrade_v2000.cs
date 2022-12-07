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
    internal sealed class ConfigUpgrade_v2000 : IConfigUpgrade
    {
        public int MinVersion => 1900;

        public int MaxVersion => 1900;

        public int TargetVersion => 1901;

        public JObject Upgrade(JObject src)
        {
            JObject dst = (JObject)src.DeepClone();

            JsonUtil.MoveIfAvailable(dst, "style.chatbox.background-color", dst, "style.chat.background-color");
            JsonUtil.MoveIfAvailable(dst, "style.channel.base.font-size", dst, "style.chat.font-size");
            JsonUtil.ModifyIfAvailable(dst, "style.chat.font-size", node => {
                if(node.Type != JTokenType.String)
                    return null;
                
                var value = node.ToString().ToLowerInvariant();
                switch (value)
                {
                    case "SMALLER": return "10 px";
                    case "SMALL": return "13.04 px";
                    case "MEDIUM": return "16 px";
                    case "LARGE": return "18 px";
                    case "LARGER": return "19.2 px";
                    case "VERY LARGE": return "24 px";
                    default: return null;
                }
            });

            return dst;
        }
    }
}