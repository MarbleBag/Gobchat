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

using Gobchat.Core.Runtime;
using Gobchat.UI.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public interface IBrowserAPIManager
    {
        bool IsUIReady { get; }
        IBrowserChatHandler ChatHandler { get; set; }

        IBrowserConfigHandler ConfigHandler { get; set; }

        IBrowserActorHandler ActorHandler { get; set; }

        IBrowserMemoryHandler MemoryHandler { get; set; }

        event EventHandler<UIReadyChangedEventArgs> OnUIReadyChanged;

        void DispatchEventToBrowser(global::Gobchat.UI.Web.JavascriptEvents.JSEvent jsEvent);

        void ExecuteJavascript(string script);

        Task<IJavascriptResponse> EvaluateJavascript(string script, TimeSpan? timeout);

        void ExecuteGobchatJavascript(Action<System.Text.StringBuilder> content);

        void RegisterAPI(IBrowserAPI api);

        void UnregisterAPI(IBrowserAPI api);

        IUISynchronizer UISynchronizer { get; }
    }
}