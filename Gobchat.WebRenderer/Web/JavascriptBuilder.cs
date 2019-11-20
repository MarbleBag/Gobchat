/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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

namespace Gobchat.UI.Web
{
    public class JavascriptBuilder
    {
        private readonly System.Text.StringBuilder stringbuilder;
        private readonly Newtonsoft.Json.JsonSerializer jsonSerializer;
        private readonly Newtonsoft.Json.JsonTextWriter jsonWriter;

        public JavascriptBuilder()
        {
            stringbuilder = new System.Text.StringBuilder(1000);
            jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            jsonWriter = new Newtonsoft.Json.JsonTextWriter(new System.IO.StringWriter(stringbuilder));
        }

        public string BuildCustomEventDispatcher(JavascriptEvents.JSEvent evt)
        {
            stringbuilder.Append("document.dispatchEvent(new CustomEvent('");
            stringbuilder.Append(evt.EventName);
            stringbuilder.Append("', { detail: ");
            jsonSerializer.Serialize(jsonWriter, evt);
            stringbuilder.Append(" }));");
            string result = stringbuilder.ToString();
            stringbuilder.Clear();
            return result;
        }

        public JToken Deserialize(string json)
        {
            return JToken.Parse(json);
        }

        public T Deserialize<T>(string json)
        {
            using (var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(json)))
            {
                var obj = jsonSerializer.Deserialize<T>(reader);
                return obj;
            }
        }
    }
}