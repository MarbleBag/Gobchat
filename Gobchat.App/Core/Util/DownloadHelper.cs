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
using System;
using System.IO;
using System.Net;

namespace Gobchat.Core.Util
{
    internal static class DownloadHelper
    {
        public enum DownloadResult
        {
            CompletedSuccessfully,
            Canceled
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="destinationFile"></param>
        /// <param name="progressMonitor"></param>
        /// <returns></returns>
        /// <exception cref="DownloadFailedException"></exception>
        public static DownloadResult DownloadFileFromGithub(string downloadUrl, string destinationFile, IProgressMonitor progressMonitor)
        {
            var cancellationToken = progressMonitor.GetCancellationToken();
            if (cancellationToken.IsCancellationRequested)
                return DownloadResult.Canceled;

            if (!Directory.Exists(Path.GetDirectoryName(destinationFile)))
                throw new DirectoryNotFoundException(Path.GetDirectoryName(destinationFile));

            Exception downloadException = null;

            using (var webClient = new WebClient())
            {
                var currentVersion = GobchatContext.ApplicationVersion;
                webClient.Headers.Add("User-Agent", $"Gobchat v{currentVersion}");

                progressMonitor.Progress = 0d;
                progressMonitor.StatusText = Resources.Core_Util_DownloadHelper_Waiting;
                progressMonitor.Log(Resources.Core_Util_DownloadHelper_Prepare);

                void OnDownloadProgressChanged(object s, DownloadProgressChangedEventArgs e)
                {
                    progressMonitor.Progress = e.ProgressPercentage / 100d;
                    progressMonitor.StatusText = StringFormat.Format(Resources.Core_Util_DownloadHelper_Download, e.BytesReceived / e.TotalBytesToReceive);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        progressMonitor.Log(Resources.Core_Util_DownloadHelper_Canceled);
                        webClient.CancelAsync();
                    }
                }

                webClient.DownloadProgressChanged += OnDownloadProgressChanged;

                try
                {
                    progressMonitor.Log(StringFormat.Format(Resources.Core_Util_DownloadHelper_Connecting, downloadUrl));
                    var downloadTask = webClient.DownloadFileTaskAsync(downloadUrl, destinationFile);
                    downloadTask.Wait();

                    progressMonitor.Progress = 1d;
                    progressMonitor.StatusText = Resources.Core_Util_DownloadHelper_Complete;
                    progressMonitor.Log(Resources.Core_Util_DownloadHelper_Complete);
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
                throw new DownloadFailedException(downloadException.Message, downloadException);

            return cancellationToken.IsCancellationRequested ? DownloadResult.Canceled : DownloadResult.CompletedSuccessfully;
        }
    }
}