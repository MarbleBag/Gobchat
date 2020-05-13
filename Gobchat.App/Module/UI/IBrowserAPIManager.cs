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

using Gobchat.UI.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public interface IBrowserAPIManager
    {
        bool IsUIReady { get; }
        IBrowserChatHandler ChatHandler { get; set; }
        IBrowserConfigHandler ConfigHandler { get; set; }

        IBrowserActorHandler ActorHandler { get; set; }

        event EventHandler<UIReadChangedEventArgs> OnUIReadyChanged;

        void DispatchEventToBrowser(global::Gobchat.UI.Web.JavascriptEvents.JSEvent jsEvent);

        void ExecuteJavascript(string script);

        Task<IJavascriptResponse> EvaluateJavascript(string script, TimeSpan? timeout);

        void ExecuteGobchatJavascript(Action<System.Text.StringBuilder> content);

        void RegisterAPI(IBrowserAPI api);

        void UnregisterAPI(IBrowserAPI api);
    }

    public interface IBrowserChatHandler
    {
        Task SendChatMessage(int channel, string source, string message);

        Task SendInfoChatMessage(string message);

        Task SendErrorChatMessage(string message);
    }

    public interface IBrowserConfigHandler
    {
        Task<JToken> GetConfigAsJson();

        Task SynchronizeConfig(JToken configJson);

        Task SetActiveProfile(string profileId);

        Task<JToken> ParseProfile(string file);
    }

    public interface IBrowserActorHandler
    {
        Task<bool> IsAvailable();

        Task<int> GetPlayerNearbyCount();

        Task<string[]> GetPlayersNearby();

        Task<float> GetDistanceToPlayer(string name);
    }
}