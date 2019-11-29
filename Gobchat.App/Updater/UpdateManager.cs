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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Windows.Forms;
using Gobchat.Core.Runtime;

namespace Gobchat.Updater
{
    public sealed class UpdateManager : Manager
    {
        private IList<IUpdateProvider> _updateProviders = new List<IUpdateProvider>();

        public UpdateManager()
        {
            //TODO look up via reflection?
            _updateProviders.Add(new GitHubUpdateProvider(GobchatApplicationContext.ApplicationVersion, userName: "MarbleBag", repoName: "Gobchat"));
        }

        public bool CheckForUpdates()
        {
            var task = Task.Run<List<QueryResult<IUpdateDescription>>>(async () => await QueryForUpdates().ConfigureAwait(true));
            var queryResult = task.Result;

            if (queryResult.Count == 0)
                return false; //no updates

            //TODO

            var successfulQueries = queryResult.Where(e => e.Successful).Select(e => e.Result).Where(e => e.IsVersionAvailable).OrderByDescending(e => e.Version);
            var newestUpdate = successfulQueries.FirstOrDefault();

            if (newestUpdate == null)
            {
                //TODO check for an unhandled exception
                //TODO check for errors

                //TODO inform user about errors

                //MessageBox.Show(string.Format(Resources.UpdateCheckException, ex.ToString()), Resources.UpdateCheckTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //TODO check for errors

                return false;
            }

            var currentVersion = GobchatApplicationContext.ApplicationVersion;
            if (currentVersion > newestUpdate.Version)
                return false;

            var dialogTest = $"{newestUpdate.UpdateSourceDescription}\n\nPressing Yes will open a webpage with the newest version and stops Gobchat from starting.";
            var dialogResult = MessageBox.Show(dialogTest, "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (DialogResult.Yes == dialogResult)
            {
                var downloadLink = newestUpdate.BrowserDownloadLink;
                Process downloadProcess = System.Diagnostics.Process.Start(downloadLink);
                return true;
            }

            return false;
        }

        private async Task<List<QueryResult<IUpdateDescription>>> QueryForUpdates()
        {
            var tasks = _updateProviders.Select(resolver => RunAsync(() => resolver.CheckForUpdate())).ToList();
            var allTasks = Task.WhenAll(tasks);
            var completedTasks = await allTasks.ConfigureAwait(false);
            return completedTasks.ToList();
        }
    }
}