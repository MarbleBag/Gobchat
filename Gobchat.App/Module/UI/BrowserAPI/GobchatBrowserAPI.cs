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

using Gobchat.Core.Runtime;
using Gobchat.Core.Util;
using Gobchat.UI.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Module.UI.Internal
{
    internal partial class BrowserAPIManager
    {
        internal sealed class GobchatBrowserAPI : IBrowserAPI
        {
            private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            public string APIName => "GobchatAPI";

            private BrowserAPIManager _browserAPIManager;

            public GobchatBrowserAPI(BrowserAPIManager browserAPIManager)
            {
                _browserAPIManager = browserAPIManager ?? throw new ArgumentNullException(nameof(browserAPIManager));
            }

            public async Task SetUIReady(bool ready)
            {
                _browserAPIManager.IsUIReady = ready;
            }

            private async Task DoTask<I>(I source, Func<I, Task> action)
            {
                if (source == null)
                    return;
                var task = action(source);
                if (task == null)
                    return;
                await task.ConfigureAwait(false);
            }

            //TODO not good enough
            private async Task<O> DoTask<I, O>(I source, Func<I, Task<O>> action)
            {
                if (source == null)
                    return default;
                var task = action(source);
                if (task == null)
                    return default;
                return await task.ConfigureAwait(false);
            }

            public async Task SendChatMessage(int channel, string source, string message)
            {
                await _browserAPIManager.ChatHandler.SendChatMessage(channel, source, message).ConfigureAwait(false);
            }

            public async Task SendInfoChatMessage(string message)
            {
                await _browserAPIManager.ChatHandler.SendInfoChatMessage(message).ConfigureAwait(false);
            }

            public async Task SendErrorChatMessage(string message)
            {
                await _browserAPIManager.ChatHandler.SendErrorChatMessage(message).ConfigureAwait(false);
            }

            public async Task<string> GetConfigAsJson()
            {
                var result = await _browserAPIManager.ConfigHandler.GetConfigAsJson().ConfigureAwait(false);
                return result.ToString();
            }

            public async Task SetConfigActiveProfile(string profileId)
            {
                await _browserAPIManager.ConfigHandler.SetActiveProfile(profileId).ConfigureAwait(false);
            }

            public async Task SynchronizeConfig(string configJson)
            {
                var jToken = _browserAPIManager._jsBuilder.Deserialize(configJson);
                await _browserAPIManager.ConfigHandler.SynchronizeConfig(jToken).ConfigureAwait(false);
            }

            public async Task<string> GetAppVersion()
            {
                return GobchatContext.ApplicationVersion.ToString();
            }

            public async Task CloseGobchat()
            {
                logger.Info("User requests shutdown");
                GobchatApplicationContext.ExitGobchat();
            }

            public async Task<string> OpenFileDialog(string filter)
            {
                string selectedFileName = "";
                _browserAPIManager._synchronizer.RunSync(() =>
                {
                    using (var dialog = new OpenFileDialog())
                    {
                        selectedFileName = RunFileDialog(dialog, filter, null);
                    }
                });
                return selectedFileName;
            }

            public async Task<string> SaveFileDialog(string filter, string fileName)
            {
                var selectedFileName = "";
                _browserAPIManager._synchronizer.RunSync(() =>
                {
                    using (var dialog = new SaveFileDialog())
                    {
                        selectedFileName = RunFileDialog(dialog, filter, fileName);
                    }
                });
                return selectedFileName;
            }

            private string RunFileDialog(FileDialog dialog, string filter, string fileName)
            {
                dialog.InitialDirectory = GobchatContext.ResourceLocation;
                dialog.RestoreDirectory = true;
                dialog.Filter = filter ?? "Json files (*.json)|*.json";
                dialog.FileName = fileName ?? "";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    return dialog.FileName;
                return null;
            }

            public async Task WriteTextToFile(string file, string content)
            {
                System.IO.File.WriteAllText(file, content);
            }

            public async Task<string> ImportProfile()
            {
                var file = await OpenFileDialog("Json files (*.json)|*.json").ConfigureAwait(false);
                if (file == null || file.Trim().Length == 0)
                    return null;
                var result = await _browserAPIManager.ConfigHandler.ParseProfile(file).ConfigureAwait(false);
                return result.ToString();
            }

            public async Task<bool> IsFeaturePlayerLocationAvailable()
            {
                return await _browserAPIManager.ActorHandler.IsFeatureAvailable().ConfigureAwait(false);
            }

            public async Task<int> GetPlayerCount()
            {
                return await _browserAPIManager.ActorHandler.GetPlayerNearbyCount().ConfigureAwait(false);
            }

            public async Task<string[]> GetPlayersNearby()
            {
                return await _browserAPIManager.ActorHandler.GetPlayersNearby().ConfigureAwait(false);
            }

            public async Task<float> GetPlayerDistance(string playerName)
            {
                var distance = await _browserAPIManager.ActorHandler.GetDistanceToPlayer(playerName).ConfigureAwait(false);
                return distance;
            }

            public async Task<string[]> GetPlayersAndDistance()
            {
                var players = await _browserAPIManager.ActorHandler.GetPlayersNearby().ConfigureAwait(false);
                if (players.Length == 0)
                    return Array.Empty<string>();

                var result = new List<(float Distance, string Name)>(players.Length);
                for (var i = 0; i < players.Length; ++i)
                {
                    var distance = await _browserAPIManager.ActorHandler.GetDistanceToPlayer(players[i]).ConfigureAwait(false);
                    result.Add((distance, players[i]));
                }

                result.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                return result.Select(e => $"{e.Name}: {e.Distance.ToString("0.00", CultureInfo.InvariantCulture)}").ToArray();
            }

            public async Task<Dictionary<string, string>> GetLocalizedStrings(string locale, string[] requestedIds)
            {
                if (requestedIds == null)
                    requestedIds = Array.Empty<string>();

                if (locale == null)
                    throw new ArgumentNullException(nameof(locale));

                var selectedCulture = CultureInfo.GetCultureInfo(locale);
                var manager = WebUIResources.ResourceManager;

                var result = new Dictionary<string, string>();
                foreach (var requestedId in requestedIds)
                {
                    if (result.ContainsKey(requestedId))
                        continue;

                    var translation = manager.GetString(requestedId, selectedCulture);
                    if (translation == null)
                        result.Add(requestedId, StringFormat.Format(WebUIResources.config_missing, requestedId));
                    else
                        result.Add(requestedId, translation);
                }

                return result;
            }
        }
    }
}