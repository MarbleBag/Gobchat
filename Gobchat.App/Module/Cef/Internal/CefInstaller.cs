﻿/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
using System.Diagnostics;
using System.IO;
using Gobchat.Core.Runtime;
using Gobchat.Core.Util;
using NLog;

namespace Gobchat.Module.Cef.Internal
{
    internal sealed class CefInstaller
    {
        private const string CURRENT_VERSION = "107.1.12";
        private const string CEF_URL = @"https://github.com/MarbleBag/Gobchat/releases/download/v1.12.0-cef/Cef-107.1.12-{ARCH}.7z";
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string cefLibDll { get { return Path.Combine(_cefLibFolder, "libcef.dll"); } }

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
            if (!IsArchiveAvailable())
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

                if (!IsArchiveAvailable())
                {

                    throw new DirectoryNotFoundException(_cefPatchArchive);
                }
            }

            return _cefPatchArchive;
        }

        //TODO exception handling
        public string ExtractCef(IProgressMonitor progressMonitor)
        {
            if (IsCorrectCefVersionAvailable())
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

            if (!Directory.Exists(_cefLibFolder))          
                throw new DirectoryNotFoundException(_cefLibFolder);

            if (!IsCorrectCefVersionAvailable())
            {
                throw new Exception("Version missmatch.");
            }

            return _cefLibFolder;
        }
        
        public void RemoveCef(IProgressMonitor progressMonitor)
        {
            progressMonitor.Log(StringFormat.Format(Resources.Module_Cef_Installer_Delete, _cefLibFolder));
            Directory.Delete(_cefLibFolder, true);
        }

        public bool IsCorrectCefVersionAvailable()
        {
            return File.Exists(cefLibDll) && FileVersionInfo.GetVersionInfo(cefLibDll).FileVersion.StartsWith(CURRENT_VERSION);
        }

        public bool DoesCefNeedAnUpdate()
        {
            return File.Exists(cefLibDll) && !FileVersionInfo.GetVersionInfo(cefLibDll).FileVersion.StartsWith(CURRENT_VERSION);
        }

        private bool IsArchiveAvailable()
        {
            return File.Exists(_cefPatchArchive);
        }
    }
}