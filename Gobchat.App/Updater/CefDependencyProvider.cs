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
using Gobchat.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Updater
{
    internal class CefDependencyProvider : IDependencyProvider
    {
        private readonly string _location;

        public CefDependencyProvider(string location)
        {
            _location = location;
        }

        public IDependencyData CheckForMissingDependencies()
        {
            var cefLocation = Path.Combine(_location, Environment.Is64BitProcess ? "x64" : "x86");

            var cefArchive = Path.Combine(cefLocation, "cef.7z");
            var targetFile = @"https://github.com/MarbleBag/Gobchat/releases/download/v0.2.2/gobchat-0.2.2.zip";

            using (var webClient = new WebClient())
            {
                var currentVersion = GobchatApplicationContext.ApplicationVersion;
                webClient.Headers.Add("User-Agent", $"Gobchat v{currentVersion.ToString()}");
                Directory.CreateDirectory(cefLocation);
                var result = DownloadHelper.DownloadFile(webClient, targetFile, cefArchive, new XProgressMonitor());
            }

            return null;
        }

        private class XProgressMonitor : DownloadHelper.ProgressMonitor
        {
            public void EndWork()
            {
                Debug.WriteLine("Download finished");
            }

            public void Progress(int progressPercentage, long bytesReceived, long totalBytesToReceive)
            {
                Debug.WriteLine($"Download {progressPercentage}% ({bytesReceived} done of {totalBytesToReceive}");
            }

            public void StartWork()
            {
                Debug.WriteLine("Download started");
            }
        }
    }
}