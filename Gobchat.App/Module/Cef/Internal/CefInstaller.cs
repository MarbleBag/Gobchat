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

using System;
using System.IO;
using Gobchat.Core.Runtime;
using Gobchat.Core.Util;

namespace Gobchat.Module.Cef.Internal
{
    internal sealed class CefInstaller
    {
        private const string CEF_URL = @"https://github.com/MarbleBag/Gobchat/releases/download/v1.0.0/Cef-75.1.14-{ARCH}.7z";

        private string _cefDownloadUrl;

        private string _cefLibFolder;
        private string _cefPatchFolder;
        private string _cefPatchArchive;

        public CefInstaller(string cefLocation, string patchFolder)
        {
            var architecture = Environment.Is64BitProcess ? "x64" : "x86";
            _cefDownloadUrl = CEF_URL.Replace("{ARCH}", architecture);

            _cefLibFolder = Path.Combine(cefLocation, architecture);
            _cefPatchFolder = patchFolder;

            Uri uri = new Uri(_cefDownloadUrl);
            string filename = Path.GetFileName(uri.LocalPath);
            _cefPatchArchive = Path.Combine(_cefPatchFolder, filename);
        }

        public string DownloadCef(IProgressMonitor progressMonitor)
        {
            if (!IsCefArchiveAvailable())
            {
                try
                {
                    Directory.CreateDirectory(_cefPatchFolder);
                    var result = DownloadHelper.DownloadFileFromGithub(_cefDownloadUrl, _cefPatchArchive, progressMonitor);
                    switch (result)
                    {
                        case DownloadHelper.DownloadResult.Canceled:
                            progressMonitor.Log(StringFormat.Format(Resources.Module_Cef_Installer_Download_DeleteIncomplete, _cefPatchArchive));
                            File.Delete(_cefPatchArchive);
                            throw new OperationCanceledException("Download canceled");
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e) //TODO not good
                {
                    progressMonitor.Log(StringFormat.Format(Resources.GeneralErrorOccured, e.Message));
                    File.Delete(_cefPatchArchive);
                    throw;
                }

                if (!IsCefArchiveAvailable())
                {
                    throw new DirectoryNotFoundException(_cefPatchArchive);
                }
            }

            return _cefPatchArchive;
        }

        //TODO exception handling
        public string ExtractCef(IProgressMonitor progressMonitor)
        {
            if (IsCefAvailable())
                return _cefLibFolder; //Done

            Directory.CreateDirectory(_cefLibFolder);
            var unpackingResults = ArchiveUnpackerHelper.ExtractArchive(_cefPatchArchive, _cefLibFolder, progressMonitor);
            switch (unpackingResults)
            {
                case ArchiveUnpackerHelper.ExtractionResult.Complete:
                    progressMonitor.Log(Resources.Module_Cef_Installer_Unpack_Complete);
                    File.Delete(_cefPatchArchive);
                    break;

                case ArchiveUnpackerHelper.ExtractionResult.Canceled:
                    progressMonitor.Log(StringFormat.Format(Resources.Module_Cef_Installer_Unpack_DeleteIncomplete, _cefLibFolder));
                    Directory.Delete(_cefLibFolder, true);
                    throw new OperationCanceledException("Unpacking canceled");
            }

            if (!IsCefAvailable())
            {
                //TODO throw exception
                throw new DirectoryNotFoundException(_cefLibFolder);
            }

            return _cefLibFolder;
        }

        public bool IsCefAvailable()
        {
            return File.Exists(Path.Combine(_cefLibFolder, "libcef.dll"));
        }

        public bool IsCefArchiveAvailable()
        {
            return File.Exists(_cefPatchArchive);
        }
    }
}