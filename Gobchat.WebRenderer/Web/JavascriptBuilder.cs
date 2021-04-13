/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
using Newtonsoft.Json.Serialization;

namespace Gobchat.UI.Web
{
    public sealed class JavascriptBuilder
    {
        private readonly System.Text.StringBuilder _stringbuilder;
        public Newtonsoft.Json.JsonSerializer JsonSerializer { get; }
        public Newtonsoft.Json.JsonTextWriter JsonWriter { get; }

        public JavascriptBuilder()
        {
            _stringbuilder = new System.Text.StringBuilder(1000);
            JsonSerializer = new Newtonsoft.Json.JsonSerializer();
            JsonSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

            JsonWriter = new Newtonsoft.Json.JsonTextWriter(new System.IO.StringWriter(_stringbuilder));
            JsonWriter.QuoteName = true;
        }

        public string BuildCustomEventDispatcher(JavascriptEvents.JSEvent evt)
        {
            lock (_stringbuilder)
            {
                _stringbuilder.Append("document.dispatchEvent(new CustomEvent('");
                _stringbuilder.Append(evt.EventName);
                _stringbuilder.Append("', { \"detail\": ");
                JsonSerializer.Serialize(JsonWriter, evt);
                _stringbuilder.Append(" }));");
                string result = _stringbuilder.ToString();
                _stringbuilder.Clear();
                return result;
            }
        }

        public JToken Deserialize(string json)
        {
            return JToken.Parse(json);
        }

        public string SerializeObject(object obj)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
        }

        public T Deserialize<T>(string json)
        {
            using (var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(json)))
            {
                var obj = JsonSerializer.Deserialize<T>(reader);
                return obj;
            }
        }
    }
}