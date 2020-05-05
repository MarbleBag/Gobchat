﻿/*******************************************************************************
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
using Gobchat.UI.Web;
using System;
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
                var task = _browserAPIManager.ChatHandler?.SendChatMessage(channel, source, message);
                if (task == null)
                    return;
                await task.ConfigureAwait(false);
            }

            public async Task SendInfoChatMessage(string message)
            {
                var task = _browserAPIManager.ChatHandler?.SendInfoChatMessage(message);
                if (task == null)
                    return;
                await task.ConfigureAwait(false);
            }

            public async Task SendErrorChatMessage(string message)
            {
                var task = _browserAPIManager.ChatHandler?.SendErrorChatMessage(message);
                if (task == null)
                    return;
                await task.ConfigureAwait(false);
            }

            public async Task<string> GetConfigAsJson()
            {
                var task = _browserAPIManager.ConfigHandler?.GetConfigAsJson();
                if (task == null)
                    return null;
                var result = await task.ConfigureAwait(false);
                return result.ToString();
            }

            public async Task SetConfigActiveProfile(string profileId)
            {
                var task = _browserAPIManager.ConfigHandler?.SetActiveProfile(profileId);
                if (task == null)
                    return;
                await task.ConfigureAwait(false);
            }

            public async Task SynchronizeConfig(string configJson)
            {
                var jToken = _browserAPIManager._jsBuilder.Deserialize(configJson);
                var task = _browserAPIManager.ConfigHandler?.SynchronizeConfig(jToken);
                if (task == null)
                    return;
                await task.ConfigureAwait(false);
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
                dialog.InitialDirectory = GobchatApplicationContext.ResourceLocation;
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
                var file = await OpenFileDialog("Json files (*.json)|*.json");
                if (file == null || file.Trim().Length == 0)
                    return null;
                var task = _browserAPIManager.ConfigHandler?.ParseProfile(file);
                if (task == null)
                    return null;
                var result = await task.ConfigureAwait(false);
                return result?.ToString();
            }
        }
    }
}