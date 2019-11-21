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
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Gobchat.Core.Util
{
    internal static class DownloadHelper
    {
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
                progressMonitor.StatusText = $"Start download";

                void OnDownloadDataCompleted(object s, DownloadDataCompletedEventArgs e)
                {
                    downloadException = e.Error;
                    downloadResult = e.Cancelled ? DownloadResult.Cancelled : DownloadResult.CompletedSuccessfuly;

                    progressMonitor.Progress = 1d;
                    progressMonitor.StatusText = "Downloading finished";
                }

                void OnDownloadProgressChanged(object s, DownloadProgressChangedEventArgs e)
                {
                    progressMonitor.Progress = e.ProgressPercentage / 100;
                    progressMonitor.StatusText = $"Downloading {e.BytesReceived} / {e.TotalBytesToReceive}";
                    if (cancellationToken.IsCancellationRequested)
                        webClient.CancelAsync();
                }

                webClient.DownloadDataCompleted += OnDownloadDataCompleted;
                webClient.DownloadProgressChanged += OnDownloadProgressChanged;

                try
                {
                    var downloadTask = webClient.DownloadFileTaskAsync(downloadUrl, destinationFile);
                    downloadTask.Wait();
                    //  cancellationToken.Register(webClient.CancelAsync);
                    //  await downloadTask.ConfigureAwait(false);
                }
                catch (WebException ex) when (ex.Message == "The request was aborted: The request was canceled.")
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    //throw new OperationCanceledException();
                }
                catch (AggregateException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                catch (TaskCanceledException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    //throw new OperationCanceledException();
                }
            }

            if (downloadException != null)
                throw downloadException;

            return downloadResult;
        }
    }
}