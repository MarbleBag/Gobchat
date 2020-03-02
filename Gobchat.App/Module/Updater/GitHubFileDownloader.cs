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

using System;
using System.IO;
using System.Runtime.Serialization;
using Gobchat.Core.Runtime;
using Gobchat.Core.Util;

namespace Gobchat.Module.Updater
{
    public sealed class GitHubFileDownloader
    {
        public string URL { get; private set; }
        public string OutputFolder { get; private set; }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (value == null || value.Length == 0)
                    _fileName = Path.GetRandomFileName();
                else
                    _fileName = value;
            }
        }

        public string FilePath { get { return Path.Combine(OutputFolder, FileName); } }

        private string _fileName;

        public GitHubFileDownloader(string url, string outputFolder)
        {
            URL = url ?? throw new ArgumentNullException(nameof(url));
            OutputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));

            try
            {
                var uri = new Uri(url);
                FileName = Path.GetFileName(uri.LocalPath);
            }
            catch (Exception)
            {
                FileName = null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="progressMonitor"></param>
        /// <returns></returns>
        /// <exception cref="DownloadFailedException"></exception>
        public Result Download(IProgressMonitor progressMonitor)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException(nameof(progressMonitor));

            if (IsFileAlreadyDownloaded())
                return Result.Skipped;

            try
            {
                Directory.CreateDirectory(OutputFolder);
                var result = DownloadHelper.DownloadFileFromGithub(downloadUrl: URL, destinationFile: FilePath, progressMonitor);
                switch (result)
                {
                    case DownloadHelper.DownloadResult.Canceled:
                        DeleteFile(progressMonitor);
                        return Result.Canceled;
                }
            }
            catch (DownloadFailedException ex)
            {
                progressMonitor.Log($"An error occured: {ex.Message}");
                DeleteFile(progressMonitor);
                throw;
            }

            return Result.Completed;
        }

        private void DeleteFile(IProgressMonitor progressMonitor)
        {
            progressMonitor.Log($"Delete partially downloaded file:\n{FilePath}");
            File.Delete(FilePath);
        }

        public bool IsFileAlreadyDownloaded()
        {
            return File.Exists(FilePath);
        }

        public enum Result
        {
            Completed,
            Canceled,
            Skipped
        }
    }
}