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

namespace Gobchat.Core.Module.Updater
{
    public sealed class GitHubUpdateProvider : IUpdateProvider
    {
        private sealed class GitHubUpdateDescription : IUpdateDescription
        {
            public Version Version { get; } = new Version(0, 0);
            public string DirectDownloadUrl { get; } = string.Empty;
            public string PageUrl { get; } = string.Empty;
            public bool IsVersionAvailable { get; } = false;
            public string PatchNotes { get; } = string.Empty;

            public GitHubUpdateDescription(List<TagPackage> releases)
            {
                if (releases == null)
                    throw new ArgumentNullException(nameof(releases));
                IsVersionAvailable = releases.Count != 0;

                if (IsVersionAvailable)
                {
                    Version = releases[0].Version;

                    DirectDownloadUrl = releases[0].DirectDownloadUrl;
                    PageUrl = releases[0].PageUrl;

                    var stringBuilder = new System.Text.StringBuilder();
                    for (int i = 0; i < releases.Count; ++i)
                    {
                        if (i != 0)
                            stringBuilder.AppendLine("");
                        stringBuilder.AppendLine(releases[i].Name);
                        stringBuilder.AppendLine(releases[i].Notes);
                    }
                    PatchNotes = stringBuilder.ToString();
                }
            }
        }

        private sealed class TagPackage
        {
            public Version Version { get; }
            public bool IsPreRelease { get; }
            public string DirectDownloadUrl { get; }
            public string PageUrl { get; }
            public string Name { get; }
            public string Notes { get; }

            public TagPackage(Version version, bool isPreRelease, string directDownloadUrl, string pageUrl, string name, string notes)
            {
                Version = version ?? throw new ArgumentNullException(nameof(version));
                IsPreRelease = isPreRelease;

                DirectDownloadUrl = directDownloadUrl ?? throw new ArgumentNullException(nameof(directDownloadUrl));
                PageUrl = pageUrl ?? throw new ArgumentNullException(nameof(pageUrl));

                Name = name ?? throw new ArgumentNullException(nameof(name));
                Notes = notes ?? throw new ArgumentNullException(nameof(notes));
            }
        }

        //     private const string GITHUB_URL = @"https://api.github.com/repos/marblebag/gobchat/releases";
        //     private const string DOWNLOAD_LINK = "https://github.com/MarbleBag/Gobchat/releases/tag/v{VERSION}";

        private const string GITHUB_API_RELEASES = @"https://api.github.com/repos/{USER}/{REPO}/releases";

        private const string PROJECT_PAGE_URL = @"https://github.com/{USER}/{REPO}";
        private const string DOWNLOAD_PAGE_URL = @"https://github.com/{USER}/{REPO}/releases/tag/v{VERSION}";
        private const string DIRECT_DOWNLOAD_URL = @"https://github.com/{USER}/{REPO}/releases/download/v{VERSION}/gobchat-{VERSION}.zip";

        private readonly Version _currentVersion;
        private readonly string _userName;
        private readonly string _repoName;

        public bool AcceptBetaReleases { get; set; } = false;

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
                    json = await httpClient.GetStringAsync(GetGithubReleaseAPI()).ConfigureAwait(false);
                }
                catch (HttpRequestException ex)
                {
                    throw new UpdateException($@"Unable to fetch most recent release from github '{GetProjectPageLink()}'", ex);
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

                    var isPreRelease = (bool)release["prerelease"];
                    if (isPreRelease && !AcceptBetaReleases)
                        continue;

                    if (!Version.TryParse(release["tag_name"].ToString().Substring(1), out var releaseVersion))
                        continue;

                    // they are sorted from newest to oldest, so it can stop if it reaches a version which is older or equal to this one
                    if (releaseVersion <= _currentVersion)
                        break;

                    relevantReleases.Add(
                        new TagPackage(
                            releaseVersion,
                            isPreRelease,
                            directDownloadUrl: GetDirectDownloadUrlFor(releaseVersion),
                            pageUrl: GetDownloadPageUrlFor(releaseVersion),
                            name: release["name"].ToString(),
                            notes: release["body"].ToString()
                        ));
                }

                return relevantReleases;
            }
            catch (Exception ex)
            {
                throw new UpdateException(ex.Message, ex);
            }
        }

        private string GetGithubReleaseAPI()
        {
            return GITHUB_API_RELEASES.Replace("{USER}", _userName).Replace("{REPO}", _repoName);
        }

        private string GetProjectPageLink()
        {
            return PROJECT_PAGE_URL.Replace("{USER}", _userName).Replace("{REPO}", _repoName);
        }

        private string GetDirectDownloadUrlFor(Version version)
        {
            return DIRECT_DOWNLOAD_URL
                .Replace("{USER}", _userName)
                .Replace("{REPO}", _repoName)
                .Replace("{VERSION}", version.ToString());
        }

        private string GetDownloadPageUrlFor(Version version)
        {
            return DOWNLOAD_PAGE_URL
                .Replace("{USER}", _userName)
                .Replace("{REPO}", _repoName)
                .Replace("{VERSION}", version.ToString());
        }
    }
}