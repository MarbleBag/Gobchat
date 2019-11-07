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

using Gobchat.Core.Chat;
using Gobchat.UI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gobchat.Core
{
    /// <summary>
    /// Each implemented method in this class is accessible from the web ui
    /// </summary>
    internal class GobchatWebAPI : IBrowserAPI
    {
        private IManagedWebBrowser _browser;
        private Gobchat.UI.Web.JavascriptBuilder _jsBuilder = new Gobchat.UI.Web.JavascriptBuilder();

        public GobchatWebAPI(IManagedWebBrowser browser)
        {
            this._browser = browser;
        }

        public string APIName => "GobchatAPI";

        public void Message(string message)
        {
            Debug.WriteLine("JSMSG: " + message?.Replace("{", "{{")?.Replace("}", "}}"));

            var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(message));
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var obj = serializer.Deserialize<Dictionary<string, string>>(reader);

            if (obj.ContainsKey("event"))
            {
                var eventName = obj["event"];
                if ("LoadGobchatConfig".Equals(eventName))
                {
                    var script = _jsBuilder.BuildCustomEventDispatcher(new Gobchat.UI.Web.JavascriptEvents.LoadGobchatConfigEvent(null));
                    _browser.ExecuteScript(script);
                }
            }
        }


    }
}
