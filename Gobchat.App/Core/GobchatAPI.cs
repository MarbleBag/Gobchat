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
using Gobchat.Core.Runtime;
using Gobchat.UI.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gobchat.Core
{
    /// <summary>
    /// Each implemented method in this class is accessible from the web ui
    /// </summary>
    internal class GobchatWebAPI : IBrowserAPI
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        private IManagedWebBrowser _browser;

        private MessageHandler _eventHandler;
        private RequestHandler _requestHandler;

        private JavascriptBuilder _jsBuilder = new JavascriptBuilder();

        public delegate global::Gobchat.UI.Web.JavascriptEvents.JSEvent MessageHandler(string eventName, string details);

        public delegate Task<string> RequestHandler(string request, string data);

        public GobchatWebAPI(IManagedWebBrowser browser, MessageHandler eventHandler, RequestHandler requestHandler)
        {
            _browser = browser;
            _eventHandler = eventHandler;
            _requestHandler = requestHandler;
        }

        public string APIName => "GobchatAPI";

        public async Task<string> Request(string message, string data)
        {
            var answere = _requestHandler(message, data);
            return await answere;
        }

        public async Task<string> GetConfig()
        {
            return await Request(nameof(GetConfig), null);
        }

        public async Task<string> OpenFileDialog()
        {
            return await Request(nameof(OpenFileDialog), "");
        }

        public async void SetConfig(string json)
        {
            Request(nameof(SetConfig), json);
        }

        public async void SetActiveProfile(string profileId)
        {
            Request(nameof(SetActiveProfile), profileId);
        }

        public void Message(string message)
        {
            logger.Debug(() => "UI Event: " + message?.Replace("{", "{{")?.Replace("}", "}}"));

            var obj = _jsBuilder.Deserialize<Dictionary<string, string>>(message);
            if (!obj.ContainsKey("event"))
            {
                logger.Warn("Recieved UI event without event name");
                return;
            }

            var eventName = obj["event"];
            var detail = obj.ContainsKey("detail") ? obj["detail"] : null;

            var response = _eventHandler(eventName, detail);
            if (response != null)
            {
                var script = _jsBuilder.BuildCustomEventDispatcher(response);
                _browser.ExecuteScript(script);
            }
        }
    }
}