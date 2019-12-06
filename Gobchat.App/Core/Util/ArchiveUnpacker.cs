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
using Gobchat.Core.Runtime;

namespace Gobchat.Core.Util
{
    public sealed class ArchiveUnpacker
    {
        public string ArchivePath { get; }
        public string OutputFolderPath { get; }

        public bool DeleteArchiveOnCompletion { get; set; } = true;

        public bool DeleteOutputFolderOnFail { get; set; } = false;

        public ArchiveUnpacker(string archivePath, string outputFolderPath)
        {
            ArchivePath = archivePath ?? throw new ArgumentNullException(nameof(archivePath));
            OutputFolderPath = outputFolderPath ?? throw new ArgumentNullException(nameof(outputFolderPath));
        }

        public Result Extract(IProgressMonitor progressMonitor)
        {
            if (progressMonitor == null)
                throw new ArgumentNullException(nameof(progressMonitor));

            Directory.CreateDirectory(OutputFolderPath);
            ArchiveUnpackerHelper.ExtractionResult unpackingResults;

            try
            {
                unpackingResults = ArchiveUnpackerHelper.ExtractArchive(ArchivePath, OutputFolderPath, progressMonitor);
            }
            catch (ExtractionFailedException ex)
            {
                progressMonitor.Log($"An error occured: {ex.Message}");
                DeleteExtractedData(progressMonitor);
                throw;
            }

            switch (unpackingResults)
            {
                case ArchiveUnpackerHelper.ExtractionResult.Complete:
                    progressMonitor.Log("Unpacking complete");
                    if (DeleteArchiveOnCompletion)
                        File.Delete(ArchivePath);
                    return Result.Completed;

                case ArchiveUnpackerHelper.ExtractionResult.Canceled:
                    DeleteExtractedData(progressMonitor);
                    return Result.Canceled;
            }

            return Result.Completed;
        }

        private void DeleteExtractedData(IProgressMonitor progressMonitor)
        {
            progressMonitor.Log("Delete partially unpacked archive");
            if (DeleteOutputFolderOnFail)
                Directory.Delete(OutputFolderPath, true);
        }

        public enum Result
        {
            Completed,
            Canceled,
            Skipped
        }
    }
}