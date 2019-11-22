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
using NLog;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Gobchat.Core.Util
{
    internal static class DownloadHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public enum DownloadResult
        {
            CompletedSuccessfuly,
            Cancelled
        }

        public static DownloadResult DownloadFileFromGithub(string downloadUrl, string destinationFile, IProgressMonitor progressMonitor)
        {
            var cancellationToken = progressMonitor.GetCancellationToken();
            if (cancellationToken.IsCancellationRequested)
                return DownloadResult.Cancelled;

            if (!Directory.Exists(Path.GetDirectoryName(destinationFile)))
                throw new DirectoryNotFoundException(Path.GetDirectoryName(destinationFile));

            DownloadResult downloadResult = DownloadResult.CompletedSuccessfuly;
            Exception downloadException = null;

            using (var webClient = new WebClient())
            {
                var currentVersion = AbstractGobchatApplicationContext.ApplicationVersion;
                webClient.Headers.Add("User-Agent", $"Gobchat v{currentVersion.ToString()}");

                progressMonitor.Progress = 0d;
                progressMonitor.StatusText = $"Waiting...";
                progressMonitor.Log("Prepare CEF download");

                void OnDownloadProgressChanged(object s, DownloadProgressChangedEventArgs e)
                {
                    progressMonitor.Progress = e.ProgressPercentage / 100d;
                    progressMonitor.StatusText = $"Downloading: {e.BytesReceived} / {e.TotalBytesToReceive}";
                    if (cancellationToken.IsCancellationRequested)
                    {
                        progressMonitor.Log("Download cancelled");
                        webClient.CancelAsync();
                    }
                }

                webClient.DownloadProgressChanged += OnDownloadProgressChanged;

                try
                {
                    progressMonitor.Log($"Connecting to {downloadUrl}");
                    var downloadTask = webClient.DownloadFileTaskAsync(downloadUrl, destinationFile);
                    downloadTask.Wait();

                    progressMonitor.Progress = 1d;
                    progressMonitor.StatusText = "Download complete";
                    progressMonitor.Log("Download complete");
                }
                catch (AggregateException ex)
                {
                    downloadException = ex.Flatten();
                }
                catch (Exception ex)
                {
                    downloadException = ex;
                }
            }

            if (downloadException != null && !cancellationToken.IsCancellationRequested)
            {
                logger.Fatal(downloadException);
                throw downloadException;
            }

            return cancellationToken.IsCancellationRequested ? DownloadResult.Cancelled : DownloadResult.CompletedSuccessfuly;
        }
    }
}