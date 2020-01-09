﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class Transforme_v2_to_v3 : IJsonTransformer
    {
        private JsonGobchatConfigProfile defaultConfig;

        public Transforme_v2_to_v3(JsonGobchatConfigProfile defaultConfig)
        {
            this.defaultConfig = defaultConfig;
        }

        public JObject Transform(JObject json)
        {
            var result = json.DeepClone();
            result["version"] = 3;

            result["behaviour"]["mentions"] = defaultConfig.GetProperty<JObject>("behaviour.mentions").DeepClone();

            result["behaviour"]["playSoundOnMention"].Parent.Remove();
            //  result["style"]["segment"]["mention"].Parent.Remove();

            result["behaviour"]["mentions"]["data"]["base"]["trigger"] = json["behaviour"]["mentions"].DeepClone();

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