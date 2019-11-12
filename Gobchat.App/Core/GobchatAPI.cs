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
using Gobchat.UI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gobchat.Core
{
    //TODO needs to go
    public class LoadGobchatConfigEvent : UI.Web.JavascriptEvents.JSEvent
    {
        public string data;

        public LoadGobchatConfigEvent(string data) : base("LoadGobchatConfig")
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Each implemented method in this class is accessible from the web ui
    /// </summary>
    internal class GobchatWebAPI : IBrowserAPI
    {
        private IManagedWebBrowser _browser;
        private UI.Web.JavascriptBuilder _jsBuilder = new UI.Web.JavascriptBuilder();

        public GobchatWebAPI(IManagedWebBrowser browser)
        {
            this._browser = browser;
        }

        public string APIName => "GobchatAPI";

        public void Message(string message)
        {
            Debug.WriteLine("JSMSG: " + message?.Replace("{", "{{")?.Replace("}", "}}"));

            var obj = _jsBuilder.Deserialize<Dictionary<string, string>>(message);
            if (obj.ContainsKey("event"))
            {
                var eventName = obj["event"];
                if ("LoadGobchatConfig".Equals(eventName))
                {
                    var script = _jsBuilder.BuildCustomEventDispatcher(new LoadGobchatConfigEvent(null));
                    _browser.ExecuteScript(script);
                }
            }
        }
    }
}