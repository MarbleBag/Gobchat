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

using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class ConfigUpgrade_v2000 : IConfigUpgrade
    {
        public int MinVersion => 1900;

        public int MaxVersion => 1904;

        public int TargetVersion => 1905;

        public JObject Upgrade(JObject src)
        {
            JObject dst = (JObject)src.DeepClone();

            JsonUtil.MoveIfAvailable(dst, "style.chatbox", dst, "style.chat-history");    
            
            JsonUtil.ModifyIfAvailable(dst, "style.channel.base.font-size", node => {
                if(node.Type != JTokenType.String)
                    return null;
                
                var value = node.ToString().ToLowerInvariant();
                switch (value)
                {
                    case "smaller": return "10px";
                    case "small": return "13px";                    
                    case "medium": return "16px";
                    case "large": return "18px";
                    case "larger": return "20px";
                    case "very large": return "24px";
                    default: return "16px";
                }
            });
            JsonUtil.MoveIfAvailable(dst, "style.channel.base.font-size", dst, "style.chat-history.font-size");

            JsonUtil.DeleteIfAvailable(dst, "behaviour.chattabs.data.default");
            JsonUtil.IterateIfAvailable<JObject>(dst, "behaviour.chattabs.data", node =>
            {
                JsonUtil.SetIfUnavailable(node, "groups", () =>
                {
                    var grp = new JObject();
                    grp["filter"] = new JArray();
                    grp["type"] = "off";
                    return grp;
                });

                return JsonUtil.IterateeResult.Continue;
            });

            JsonUtil.MoveIfAvailable(dst, "behaviour.mentions.data.base.trigger", dst, "behaviour.mentions.trigger");
            JsonUtil.MoveIfAvailable(dst, "behaviour.mentions.data.base.playSound", dst, "behaviour.mentions.playSound");
            JsonUtil.MoveIfAvailable(dst, "behaviour.mentions.data.base.soundPath", dst, "behaviour.mentions.soundPath");
            JsonUtil.MoveIfAvailable(dst, "behaviour.mentions.data.base.soundInterval", dst, "behaviour.mentions.soundInterval");
            JsonUtil.MoveIfAvailable(dst, "behaviour.mentions.data.base.volume", dst, "behaviour.mentions.volume");
            JsonUtil.DeleteIfAvailable(dst, "behaviour.mentions.data");
            JsonUtil.DeleteIfAvailable(dst, "behaviour.mentions.order");

            return dst;
        }
    }
}