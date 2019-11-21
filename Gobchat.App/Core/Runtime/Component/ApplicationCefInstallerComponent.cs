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
using System.Windows.Forms;
using Gobchat.Core.Util;
using SharpCompress;
using SharpCompress.Archives;

namespace Gobchat.Core.Runtime
{
    public sealed class ApplicationCefInstallerComponent : IApplicationComponent
    {
        private const string CEF_URL = @"https://github.com/MarbleBag/Gobchat/releases/download/v1.0.0/Cef-75.1.14-{ARCH}.7z";

        private sealed class CefInstaller
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
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                _cefPatchArchive = Path.Combine(_cefPatchFolder, filename);
            }

            public void DownloadCef(IProgressMonitor progressMonitor)
            {
                if (!IsCefArchiveAvailable())
                {
                    Directory.CreateDirectory(_cefPatchFolder);
                    var result = DownloadHelper.DownloadFileFromGithub(_cefDownloadUrl, _cefPatchArchive, progressMonitor);
                    switch (result)
                    {
                        case DownloadHelper.DownloadResult.Cancelled:
                            File.Delete(_cefPatchArchive);
                            return;
                    }

                    if (!IsCefArchiveAvailable())
                    {
                        throw new DownloadFailedException(_cefDownloadUrl);
                    }
                }
            }

            //TODO exception handling
            public void ExtractCef(IProgressMonitor progressMonitor)
            {
                if (IsCefAvailable())
                    return; //Done

                Directory.CreateDirectory(_cefLibFolder);
                var unpackingResults = ArchiveUnpackerHelper.ExtractArchive(_cefPatchArchive, _cefLibFolder, progressMonitor);
                switch (unpackingResults)
                {
                    case ArchiveUnpackerHelper.ExtractionResult.Complete:
                        //TODO delete archive
                        break;

                    case ArchiveUnpackerHelper.ExtractionResult.Cancelled:
                        Directory.Delete(_cefLibFolder, true);
                        break;
                }

                if (!IsCefAvailable())
                {
                    //TODO throw exception
                    throw new DirectoryNotFoundException(_cefLibFolder);
                }
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

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var uiSynchronizer = container.Resolve<IUISynchronizer>();
            Updater.ProgressDisplayForm progressDisplay = null;

            var cefFolder = Path.Combine(GobchatApplicationContext.ApplicationLocation, "libs", "cef");
            var patcherFolder = Path.Combine(GobchatApplicationContext.ApplicationLocation, "patch");
            var installer = new CefInstaller(cefFolder, patcherFolder);
            if (installer.IsCefAvailable())
                return;

            //TODO message dialog
            {
                var dialogResult = MessageBox.Show(
                    "CEF not found. Without Gobchat will not work.\nShould it be downloaded and installed for Gobchat?",
                    "Gobchat",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (dialogResult != DialogResult.Yes)
                {
                    handler.StopStartup = true;
                    return;
                }
            }

            try
            {
                uiSynchronizer.RunSync(() =>
                {
                    progressDisplay = new Updater.ProgressDisplayForm();
                    progressDisplay.Show();
                });

                using (var progressMonitor = new global::Gobchat.Updater.ProgressMonitorAdapter(progressDisplay))
                {
                    installer.DownloadCef(progressMonitor);
                    installer.ExtractCef(progressMonitor);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                MessageBox.Show(
                   $"CEF installation failed. Reason:\n{e.Message}\n\nRetry or install CEF manually for gobchat.",
                   "Gobchat",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error
               );
                handler.StopStartup = true;
            }
            finally
            {
                uiSynchronizer.RunSync(() => progressDisplay.Dispose());
            }
        }

        public void Dispose(IDIContext container)
        {
        }
    }
}