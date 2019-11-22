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
using NLog;
using SharpCompress;
using SharpCompress.Archives;

namespace Gobchat.Core.Runtime
{
    public sealed class ApplicationCefInstallerComponent : IApplicationComponent
    {
        private const string CEF_URL = @"https://github.com/MarbleBag/Gobchat/releases/download/v1.0.0/Cef-75.1.14-{ARCH}.7z";

        private static Logger logger = LogManager.GetCurrentClassLogger();

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
                            case DownloadHelper.DownloadResult.Cancelled:
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
                        //TODO delete archive
                        break;

                    case ArchiveUnpackerHelper.ExtractionResult.Cancelled:
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
                logger.Info("CEF missing");
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
                    try
                    {
                        installer.DownloadCef(progressMonitor);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Fatal, e, () => "CEF download failed");
                        throw;
                    }
                    try
                    {
                        installer.ExtractCef(progressMonitor);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Fatal, e, () => "CEF extraction failed");
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"CEF installation failed. Reason:\n{e.Message}\n\nRetry or install CEF manually for gobchat.";
                logger.Fatal("CEF installation failed");
                logger.Fatal(e);

                MessageBox.Show(
                   errorMessage,
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