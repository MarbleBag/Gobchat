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
using Gobchat.Core.Util;
using Gobchat.Module.MemoryReader;
using Gobchat.UI.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Module.UI.Internal
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

        #region chat

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

        #endregion chat

        #region config

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
            var jToken = JToken.Parse(configJson);
            await _browserAPIManager.ConfigHandler.SynchronizeConfig(jToken).ConfigureAwait(false);
        }

        public async Task<string> ImportProfile()
        {
            var file = await OpenFileDialog("Json files (*.json)|*.json").ConfigureAwait(false);
            if (file == null || file.Trim().Length == 0)
                return null;
            var result = await _browserAPIManager.ConfigHandler.ParseProfile(file).ConfigureAwait(false);
            return result?.ToString();
        }

        #endregion config

        #region files

        public async Task<string> OpenDirectoryDialog(string path = null)
        {
            string selectedElement = "";
            _browserAPIManager.UISynchronizer.RunSync(() =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.SelectedPath = path == null || path.Length == 0 ? GobchatContext.ResourceLocation : path;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        selectedElement = dialog.SelectedPath;
                }
            });
            return selectedElement;
        }

        public async Task<string> OpenFileDialog(string filter)
        {
            string selectedFileName = "";
            _browserAPIManager.UISynchronizer.RunSync(() =>
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
            _browserAPIManager.UISynchronizer.RunSync(() =>
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
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
                return dialog.FileName;
            return null;
        }

        public async Task WriteTextToFile(string file, string content)
        {
            System.IO.File.WriteAllText(file, content);
        }

        public async Task<string> ReadTextFromFile(string file)
        {
            if (String.IsNullOrEmpty(file))
                throw new ArgumentNullException(nameof(file));
            if (!System.IO.Path.IsPathRooted(file))
                file = System.IO.Path.Combine(GobchatContext.ResourceLocation, file);
            return System.IO.File.ReadAllText(file);
        }
        public async Task<string> GetAbsoluteChatLogPath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(GobchatContext.AppDataLocation, path);
        }

        public async Task<string> GetRelativeChatLogPath(string path)
        {
            if (Path.IsPathRooted(path))
                if (path.StartsWith(GobchatContext.AppDataLocation))
                    path = path.Substring(GobchatContext.AppDataLocation.Length);

            while (path.StartsWith("" + Path.DirectorySeparatorChar))
                path = path.Substring(1);

            return path;
        }

        #endregion files

        #region player data

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

        #endregion player data

        #region process functions

        public async Task<int[]> GetAttachableFFXIVProcesses()
        {
            return await _browserAPIManager.MemoryHandler.GetAttachableFFXIVProcesses().ConfigureAwait(false);
        }

        public async Task<(ConnectionState state, int id)> GetAttachedFFXIVProcess()
        {
            return await _browserAPIManager.MemoryHandler.GetAttachedFFXIVProcess().ConfigureAwait(false);
        }

        public async Task<bool> AttachToFFXIVProcess(int id)
        {
            return await _browserAPIManager.MemoryHandler.AttachToFFXIVProcess(id).ConfigureAwait(false);
        }

        #endregion process functions

        #region localization

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
                    result.Add(requestedId, StringFormat.Format(WebUIResources.localization_key_missing, requestedId));
                else
                    result.Add(requestedId, translation);
            }

            return result;
        }

        #endregion

        public async Task<string> GetAppVersion()
        {
            return GobchatContext.ApplicationVersion.ToString();
        }

        public async Task<(int width, int height)> GetScreenDimensions()
        {
            var bounds = Screen.PrimaryScreen.Bounds;
            return (bounds.Width, bounds.Height);
        }

        public async Task CloseGobchat()
        {
            logger.Info("User requests shutdown");
            GobchatApplicationContext.ExitGobchat();
        }
    }
}
