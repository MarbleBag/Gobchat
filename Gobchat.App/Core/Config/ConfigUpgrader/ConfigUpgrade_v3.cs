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

using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    // FINALIZED DO NOT CHANGE
    internal sealed class ConfigUpgrade_v3 : IConfigUpgrade
    {
        public int MinVersion => 2;

        public int MaxVersion => 2;

        public int TargetVersion => 3;

        public JObject Upgrade(JObject json)
        {
            var result = json.DeepClone();
            result["version"] = 3;

            if (result["behaviour"] == null)
                result["behaviour"] = new JObject();

            var jsonTemplate = //default values
$@"{{
    ""data"": {{
        ""base"": {{
            ""id"": ""base"",
            ""trigger"": [],
            ""playSound"": false,
            ""soundPath"": ""../sounds/FFXIV_Linkshell_Transmission.mp3"",
            ""soundInterval"": 5000,
            ""volume"": 0.2
        }}
    }},
    ""order"": [ ""base"" ]
}}";

            result["behaviour"]["mentions"] = JToken.Parse(jsonTemplate);

            if (json["behaviour"]["mentions"] is JArray)
                result["behaviour"]["mentions"]["data"]["base"]["trigger"] = json["behaviour"]["mentions"].DeepClone();

            if (result["behaviour"]["playSoundOnMention"] != null) //remove old values
                result["behaviour"]["playSoundOnMention"].Parent.Remove();

            //  var jMentions = json["behaviour"]["mentions"];
            //   if (jMentions != null && jMentions is JArray)
            //  {
            // result["behaviour"]["mentions"]["data"]["base"]["style"] = json["style"]["segment"]["mention"].DeepClone();

            /*
            var id = Util.IdGenerator.GenerateNewId(8, new List<string>());

            var jsonTemplate =
$@"{{
""id"": ""{id}"",
""trigger"": [],
""playSound"": false,
""soundPath"": ""../sounds/Alarm.ogg"",
""volume"": 1.0,
""style"": {{
    ""color"": ""#9358E4""
}}
}}";

            var newJMentions = JToken.Parse(jsonTemplate);
            newJMentions["trigger"] = json["behaviour"]["mentions"].DeepClone();
            newJMentions["style"] = json["style"]["segment"]["mention"].DeepClone();

            result["behaviour"]["mentions"]["data"][id] = newJMentions;
            (result["behaviour"]["mentions"]["order"] as JArray).Add(id);
            */
            //       }

            return (JObject)result;
        }
    }
}