﻿/*******************************************************************************
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
using Gobchat.Core.Util;

namespace Gobchat.Core.Module.CefInstaller
{
    public sealed partial class AppModuleCefInstaller
    {
        internal sealed class CefInstaller
        {
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
                                progressMonitor.Log($"Delete partially downloaded cef archive\n{_cefPatchArchive}");
                                File.Delete(_cefPatchArchive);
                                throw new OperationCanceledException("Download cancelled");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e) //TODO not good
                    {
                        progressMonitor.Log($"An error occured: {e.Message}");
                        File.Delete(_cefPatchArchive);
                        throw;
                    }

                    if (!IsCefArchiveAvailable())
                    {
                        throw new DirectoryNotFoundException("Downloaded CEF archive not found");
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
                        progressMonitor.Log("Unpacking complete");
                        File.Delete(_cefPatchArchive);
                        break;

                    case ArchiveUnpackerHelper.ExtractionResult.Canceled:
                        progressMonitor.Log("Delete partially unpacked CEF archive");
                        Directory.Delete(_cefLibFolder, true);
                        throw new OperationCanceledException("Unpacking cancelled");
                }

                if (!IsCefAvailable())
                {
                    //TODO throw exception
                    throw new DirectoryNotFoundException("Unpacked CEF folder not found");
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
}