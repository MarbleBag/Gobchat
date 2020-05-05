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

using System;
using System.IO;
using Gobchat.Core.Runtime;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Gobchat.Core.Util
{
    internal static class ArchiveUnpackerHelper
    {
        public enum ExtractionResult
        {
            Complete,
            Canceled
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="destinationFolder"></param>
        /// <param name="progressMonitor"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ExtractionFailedException"></exception>
        public static ExtractionResult ExtractArchive(string archivePath, string destinationFolder, IProgressMonitor progressMonitor)
        {
            var cancellationToken = progressMonitor.GetCancellationToken();
            if (cancellationToken.IsCancellationRequested)
                return ExtractionResult.Canceled;

            if (!File.Exists(archivePath))
                throw new FileNotFoundException(archivePath);
            if (!Directory.Exists(destinationFolder))
                throw new DirectoryNotFoundException(destinationFolder);

            progressMonitor.StatusText = "Unpacking";
            progressMonitor.Progress = 0d;
            progressMonitor.Log($"Unpacking {archivePath}");

            try
            {
                using (var archive = SharpCompress.Archives.ArchiveFactory.Open(archivePath))
                {
                    double totalArchiveSize = 0d;
                    double processedBytes = 0d;

                    foreach (var entry in archive.Entries)
                    {
                        totalArchiveSize += entry.Size;
                    }
                    totalArchiveSize = Math.Max(1, totalArchiveSize);

                    using (var reader = archive.ExtractAllEntries())
                    {
                        reader.EntryExtractionProgress += (sender, e) =>
                        {
                            progressMonitor.Progress = (processedBytes + e.ReaderProgress.BytesTransferred) / totalArchiveSize;
                        };

                        while (reader.MoveToNextEntry())
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                progressMonitor.StatusText = "Unpacking cancelled";
                                progressMonitor.Log("Unpacking cancelled");
                                return ExtractionResult.Canceled;
                            }

                            var entry = reader.Entry;
                            if (!entry.IsDirectory)
                            {
                                progressMonitor.StatusText = $"Unpacking: {entry.Key}";
                                progressMonitor.Log($"Unpacking: {entry.Key}");

                                reader.WriteEntryToDirectory(destinationFolder, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                processedBytes += entry.Size;
                                progressMonitor.Progress = processedBytes / totalArchiveSize;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractionFailedException(ex.Message, ex);
            }

            progressMonitor.StatusText = "Unpacking complete";
            progressMonitor.Progress = 1d;

            return ExtractionResult.Complete;
        }
    }
}