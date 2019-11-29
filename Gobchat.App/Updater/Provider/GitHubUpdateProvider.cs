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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gobchat.Updater
{
    public sealed class GitHubUpdateProvider : IUpdateProvider
    {
        private sealed class GitHubUpdateDescription : IUpdateDescription
        {
            private const string DOWNLOAD_DESCRIPTION = "Version {VERSION} is now available on github!\nhttps://github.com/MarbleBag/Gobchat/releases";

            public Version Version { get; } = new Version(0, 0);
            public string BrowserDownloadLink { get; } = string.Empty;
            public string UpdateSourceDescription { get; } = string.Empty;
            public string PatchNotes { get; } = string.Empty;
            public bool IsVersionAvailable { get; } = false;

            public GitHubUpdateDescription(List<TagPackage> releases)
            {
                if (releases == null)
                    throw new ArgumentNullException(nameof(releases));

                IsVersionAvailable = releases.Count != 0;

                if (IsVersionAvailable)
                {
                    Version = releases[0].Version;
                    BrowserDownloadLink = releases[0].DownloadLink;
                    UpdateSourceDescription = DOWNLOAD_DESCRIPTION.Replace("{VERSION}", Version.ToString());

                    var stringBuilder = new System.Text.StringBuilder();
                    for (int i = 0; i < releases.Count; ++i)
                    {
                        if (i != 0)
                            stringBuilder.AppendLine("");
                        stringBuilder.AppendLine(releases[i].Name);
                        stringBuilder.AppendLine(releases[i].Notes);
                    }
                }
            }
        }

        private sealed class TagPackage
        {
            public Version Version { get; }
            public string DownloadLink { get; }
            public string Name { get; }
            public string Notes { get; }

            public TagPackage(Version version, string downloadLink, string name, string notes)
            {
                Version = version ?? throw new ArgumentNullException(nameof(version));
                DownloadLink = downloadLink ?? throw new ArgumentNullException(nameof(downloadLink));
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Notes = notes ?? throw new ArgumentNullException(nameof(notes));
            }
        }

        //     private const string GITHUB_URL = @"https://api.github.com/repos/marblebag/gobchat/releases";
        //     private const string DOWNLOAD_LINK = "https://github.com/MarbleBag/Gobchat/releases/tag/v{VERSION}";

        private const string PROJECT_RELEASES = @"https://api.github.com/repos/{USER}/{REPO}/releases";
        private const string PROJECT_PAGE = @"https://github.com/{USER}/{REPO}";
        private const string DOWNLOAD_LINK = @"https://github.com/{USER}/{REPO}/releases/tag/v{VERSION}";

        private Version _currentVersion;
        private string _userName;
        private string _repoName;

        private string ProjectReleasesLink
        {
            get { return PROJECT_RELEASES.Replace("{USER}", _userName).Replace("{REPO}", _repoName); }
        }

        private string ProjectPageLink
        {
            get { return PROJECT_PAGE.Replace("{USER}", _userName).Replace("{REPO}", _repoName); }
        }

        private string DownloadLink
        {
            get { return DOWNLOAD_LINK.Replace("{USER}", _userName).Replace("{REPO}", _repoName); }
        }

        public GitHubUpdateProvider(Version currentVersion, string userName, string repoName)
        {
            _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            _userName = userName ?? throw new ArgumentNullException(nameof(userName));
            _repoName = repoName ?? throw new ArgumentNullException(nameof(repoName));
        }

        public IUpdateDescription CheckForUpdate()
        {
            var availableVersions = GetRelevantGitHubReleases().Result;
            return new GitHubUpdateDescription(availableVersions);
        }

        private async Task<List<TagPackage>> GetRelevantGitHubReleases()
        {
            var releases = await FetchGitHubReleases().ConfigureAwait(false);
            if (releases == null)
                return new List<TagPackage>();
            return FilterGitHubReleases(releases);
        }

        private async Task<JArray> FetchGitHubReleases()
        {
            //needed for some reasons, this should be a default for .net 4.6+
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            using (var httpClient = new HttpClient())
            {
                //needed to use githubs api. It's used to identify who is calling.
                // https://developer.github.com/v3/#user-agent-required
                httpClient.DefaultRequestHeaders.Add("User-Agent", $"{_repoName} v{_currentVersion.ToString()}");

                string json = null;
                try
                {
                    //returns a json array with all releases. 0 is the newest release
                    json = await httpClient.GetStringAsync(ProjectReleasesLink).ConfigureAwait(false);
                }
                catch (HttpRequestException ex)
                {
                    throw new UpdateException($@"Unable to fetch most recent release from github '{ProjectPageLink}'", ex);
                }

                var releases = JToken.Parse(json);
                if (releases is JArray allReleases)
                    return allReleases;

                return new JArray(releases);
            }
        }

        private List<TagPackage> FilterGitHubReleases(JArray releases)
        {
            try
            {
                var relevantReleases = new List<TagPackage>();
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

                    // they are sorted from newest to oldest, so it can stop if it reaches a version which is older or equal to this one
                    if (releaseVersion <= _currentVersion)
                        break;

                    var downloadLink = DownloadLink.Replace("{VERSION}", releaseVersion.ToString());
                    relevantReleases.Add(new TagPackage(releaseVersion, downloadLink: downloadLink, name: release["name"].ToString(), notes: release["body"].ToString()));
                }

                return relevantReleases;
            }
            catch (Exception ex)
            {
                throw new UpdateException(ex.Message, ex);
            }
        }
    }
}