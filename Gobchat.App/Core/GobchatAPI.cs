﻿/*******************************************************************************
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
using Newtonsoft.Json;
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
        private OnUIEvent _eventHandler;
        private JavascriptBuilder _jsBuilder = new JavascriptBuilder();

        public delegate global::Gobchat.UI.Web.JavascriptEvents.JSEvent OnUIEvent(string eventName, string details);

        public GobchatWebAPI(IManagedWebBrowser browser, OnUIEvent eventHandler)
        {
            this._browser = browser;
            this._eventHandler = eventHandler;
        }

        public string APIName => "GobchatAPI";

        public void Message(string message)
        {
            Debug.WriteLine("JSMSG: " + message?.Replace("{", "{{")?.Replace("}", "}}"));

            var obj = _jsBuilder.Deserialize<Dictionary<string, string>>(message);
            if (!obj.ContainsKey("event"))
            {
                Debug.WriteLine("Error. UI event needs event name"); //TODO
                return;
            }

            var eventName = obj["event"];
            var detail = obj.ContainsKey("detail") ? obj["detail"] : null;

            var response = _eventHandler.Invoke(eventName, detail);
            if (response != null)
            {
                var script = _jsBuilder.BuildCustomEventDispatcher(response);
                _browser.ExecuteScript(script);
            }
        }
    }
}