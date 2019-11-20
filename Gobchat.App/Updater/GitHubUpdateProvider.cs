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

using Gobchat.Core.Runtime;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gobchat.Updater
{
    public class GitHubUpdateProvider : IUpdateProvider
    {
        internal class GitHubUpdateData : IUpdateData
        {
            private const string DOWNLOAD_DESCRIPTION = "Version {VERSION} is now available on github!\nhttps://github.com/MarbleBag/Gobchat/releases";
            private const string DOWNLOAD_LINK = "https://github.com/MarbleBag/Gobchat/releases/tag/v{VERSION}";

            private readonly List<Version> _availableVersions;

            public Version Version => _availableVersions[0];
            public bool IsUpdateAvailable => _availableVersions.Count > 0;
            public string DownloadDescription { get; } = string.Empty;
            public string UserDownloadLink { get; } = string.Empty;

            public GitHubUpdateData(List<Version> availableVersions)
            {
                if (availableVersions == null)
                    throw new ArgumentNullException(nameof(availableVersions));
                if (availableVersions.Count == 0)
                    throw new ArgumentException("Is empty", nameof(availableVersions));

                this._availableVersions = new List<Version>(availableVersions);
                this._availableVersions.Sort();
                this._availableVersions.Reverse();

                if (IsUpdateAvailable)
                    DownloadDescription = DOWNLOAD_DESCRIPTION.Replace("{VERSION}", Version.ToString());
                if (IsUpdateAvailable)
                    UserDownloadLink = DOWNLOAD_LINK.Replace("{VERSION}", Version.ToString());
            }
        }

        private const string GITHUB_URL = @"https://api.github.com/repos/marblebag/gobchat/releases";

        public GitHubUpdateProvider()
        {
        }

        private async Task<JArray> FetchGitHubReleases()
        {
            var currentVersion = GobchatApplicationContext.ApplicationVersion;

            //needed for some reasons, this should be a default for .net 4.6+
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            using (var httpClient = new HttpClient())
            {
                //needed to use githubs api. It's used to identify who is calling.
                // https://developer.github.com/v3/#user-agent-required
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"Gobchat v{currentVersion.ToString()}");

                string json = null;
                try
                {
                    //returns a json array with all releases. 0 is the newest release
                    json = await httpClient.GetStringAsync(GITHUB_URL).ConfigureAwait(false);
                }
                catch (HttpRequestException e)
                {
                    throw new GobchatUpdateException("Unable to fetch most recent release from github 'https://github.com/MarbleBag/Gobchat'", e);
                }

                var releases = JToken.Parse(json);
                if (releases is JArray allReleases)
                    return allReleases;

                return new JArray(releases);
            }
        }

        private List<Version> FilterGitHubReleases(JArray releases)
        {
            var currentVersion = GobchatApplicationContext.ApplicationVersion;
            try
            {
                var versions = new List<Version>();
                for (int releaseIndex = 0; releaseIndex < releases.Count; ++releaseIndex)
                {
                    var release = releases[releaseIndex];

                    //only accept released versions on master
                    var branch = (string)release["target_commitish"];
                    if (!"master".Equals(branch, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    //do not accept pre releases
                    var isPreRelease = (bool)release["prerelease"];
                    if (isPreRelease)
                        continue;

                    if (!Version.TryParse(release["tag_name"].ToString().Substring(1), out var releaseVersion))
                        continue;

                    if (releaseVersion <= currentVersion)
                        break; //no new versions available

                    versions.Add(releaseVersion);
                }

                //github does not allow duplicate tag names, even for different branches in the same repository. So this is somewhat safe.
                return versions;
            }
            catch (Exception e2)
            {
                throw new GobchatUpdateException(e2.Message, e2);
            }
        }

        private async Task<List<Version>> GetRelevantGitHubReleases()
        {
            var releases = await FetchGitHubReleases().ConfigureAwait(false);
            if (releases == null)
                return new List<Version>();
            return FilterGitHubReleases(releases);
        }

        public IUpdateData CheckForUpdate()
        {
            var availableVersions = GetRelevantGitHubReleases().Result;
            if (availableVersions == null)
                return null;
            return new GitHubUpdateData(availableVersions);
        }
    }
}