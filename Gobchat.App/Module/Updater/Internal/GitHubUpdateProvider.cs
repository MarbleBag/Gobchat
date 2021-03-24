/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gobchat.Core.Runtime;

namespace Gobchat.Module.Updater.Internal
{
    public sealed class GitHubUpdateProvider : IUpdateProvider
    {
        private sealed class GitHubUpdateDescription : IUpdateDescription
        {
            public GobVersion Version { get; } = new GobVersion();
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
                        if (releases[i].Version.IsPreRelease)
                            stringBuilder.Append("Beta: ");
                        stringBuilder.AppendLine(releases[i].Name);
                        stringBuilder.AppendLine(releases[i].Notes);
                    }
                    PatchNotes = stringBuilder.ToString();
                }
            }
        }

        private sealed class TagPackage
        {
            public GobVersion Version { get; }
            public bool IsMarkedPreRelease { get; }
            public string DirectDownloadUrl { get; }
            public string PageUrl { get; }
            public string Name { get; }
            public string Notes { get; }

            public TagPackage(GobVersion version, bool isMarkedPreRelease, string directDownloadUrl, string pageUrl, string name, string notes)
            {
                Version = version ?? throw new ArgumentNullException(nameof(version));
                IsMarkedPreRelease = isMarkedPreRelease;

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

        private readonly GobVersion _currentVersion;
        private readonly string _userName;
        private readonly string _repoName;

        public bool AcceptBetaReleases { get; set; } = false;

        public GitHubUpdateProvider(GobVersion currentVersion, string userName, string repoName)
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
            var jReleases = await FetchGitHubReleases().ConfigureAwait(false);
            if (jReleases == null)
                return new List<TagPackage>();
            var releases = BuildGitHubReleases(jReleases);

            //releases.Sort((a, b) => a.Version.CompareTo(b.Version));

            if (!AcceptBetaReleases)
                return releases.Where(p => !p.Version.IsPreRelease).ToList();

            //only keep the 'last' betas round
            var firstNonBetaRelease = releases.Count - 2;
            for (; 0 <= firstNonBetaRelease; --firstNonBetaRelease)
                if (!releases[firstNonBetaRelease].Version.IsPreRelease)
                    break;

            if (firstNonBetaRelease <= 0)
                return releases;

            var cleanedReleases = releases.Take(firstNonBetaRelease).Where(p => !p.Version.IsPreRelease).ToList();
            cleanedReleases.AddRange(releases.Skip(firstNonBetaRelease));
            return cleanedReleases;
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

        private List<TagPackage> BuildGitHubReleases(JArray jReleases)
        {
            try
            {
                var releases = new List<TagPackage>();
                for (int releaseIndex = 0; releaseIndex < jReleases.Count; ++releaseIndex)
                {
                    var jRelease = jReleases[releaseIndex];

                    //only accept released versions from master or (hotfixes) from production
                    //var branch = (string)jRelease["target_commitish"];
                    //if (!("master".Equals(branch, StringComparison.InvariantCultureIgnoreCase) || "production".Equals(branch, StringComparison.InvariantCultureIgnoreCase)))
                    //    continue;

                    var isMarkedPreRelease = (bool)jRelease["prerelease"];
                    //if (isMarkedPreRelease) //TODO remove comment later
                    //    continue;

                    if (!GobVersion.TryParse(jRelease["tag_name"].ToString(), out var version))
                        continue;

                    // they are sorted from newest to oldest, so it can stop if it reaches a version which is older or equal to this one
                    if (version <= _currentVersion)
                        break;

                    releases.Add(
                        new TagPackage(
                            version,
                            isMarkedPreRelease,
                            directDownloadUrl: GetDirectDownloadUrlFor(version),
                            pageUrl: GetDownloadPageUrlFor(version),
                            name: jRelease["name"].ToString(),
                            notes: jRelease["body"].ToString()
                        ));
                }

                return releases;
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

        private string GetDirectDownloadUrlFor(GobVersion version)
        {
            var sVersion = version.IsPreRelease ? $"{version.Major}.{version.Minor}.{version.Patch}-{version.PreRelease}" : $"{version.Major}.{version.Minor}.{version.Patch}";
            return DIRECT_DOWNLOAD_URL
                .Replace("{USER}", _userName)
                .Replace("{REPO}", _repoName)
                .Replace("{VERSION}", sVersion);
        }

        private string GetDownloadPageUrlFor(GobVersion version)
        {
            var sVersion = version.IsPreRelease ? $"{version.Major}.{version.Minor}.{version.Patch}-{version.PreRelease}" : $"{version.Major}.{version.Minor}.{version.Patch}";
            return DOWNLOAD_PAGE_URL
                .Replace("{USER}", _userName)
                .Replace("{REPO}", _repoName)
                .Replace("{VERSION}", sVersion);
        }
    }
}