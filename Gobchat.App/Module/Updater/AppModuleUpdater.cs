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

using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Core.UI;
using Gobchat.Core.Util;
using Gobchat.Module.Updater.Internal;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Module.Updater
{
    public sealed class AppModuleUpdater : IApplicationModule
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string PatchFolder = "patch";
        private const string TempPatchFolder = "temp";

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            if (handler == null) throw new System.ArgumentNullException(nameof(handler));

            DeleteOldPatchData();

            var configManager = container.Resolve<IConfigManager>();
            var doUpdate = configManager.GetProperty<bool>("behaviour.checkForUpdate");

            if (!doUpdate)
                return;

            var allowBetaUpdates = configManager.GetProperty<bool>("behaviour.checkForBetaUpdate");

            var update = GetUpdate(GobchatApplicationContext.ApplicationVersion, allowBetaUpdates);
            if (update == null)
                return;

            var userRequest = AskUser(update);
            if (userRequest == UpdateFormDialog.UpdateType.Skip)
                return;

            if (userRequest == UpdateFormDialog.UpdateType.Auto)
            {
                var needRestart = PerformAutoUpdate(container, update);
                if (needRestart)
                    handler.StopStartup = true;
            }

            if (userRequest == UpdateFormDialog.UpdateType.Manual)
            {
                var dialogText = $"Pressing Yes will close Gobchat and opens a webpage instead, which will provide the newest version for Gobchat.\n\nDownload gobchat-{update.Version}.zip and overwrite your current Gobchat.";
                var dialogResult = System.Windows.Forms.MessageBox.Show(dialogText, "Update available", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);

                //TODO improve - a lot

                if (System.Windows.Forms.DialogResult.Yes == dialogResult)
                {
                    System.Diagnostics.Process downloadProcess = System.Diagnostics.Process.Start(update.PageUrl);
                    handler.StopStartup = true;
                }
            }
        }

        private void DeleteOldPatchData()
        {
            try
            {
                var patchFolder = System.IO.Path.Combine(GobchatApplicationContext.ApplicationLocation, PatchFolder);
                var tmpFolder = System.IO.Path.Combine(patchFolder, TempPatchFolder);

                if (System.IO.Directory.Exists(tmpFolder))
                {
                    logger.Info("Deleting temp update data");
                    System.IO.Directory.Delete(tmpFolder, true);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
            }
        }

        private bool PerformAutoUpdate(IDIContext container, IUpdateDescription update)
        {
            logger.Info("Performing auto update");

            var uiManager = container.Resolve<IUIManager>();
            var displayId = uiManager.CreateUIElement(() =>
            {
                var f = new ProgressDisplayForm();
                f.Show();
                return f;
            });

            try
            {
                var progressDisplay = uiManager.GetUIElement<ProgressDisplayForm>(displayId);
                using (var progressMonitor = new ProgressMonitorAdapter(progressDisplay))
                {
                    var patchFolder = System.IO.Path.Combine(GobchatApplicationContext.ApplicationLocation, PatchFolder);

                    (var downloadResult, var filePath) = PerformAutoUpdateDownload(update, patchFolder, progressMonitor);
                    logger.Info($"Download complete: {downloadResult}");
                    if (!downloadResult)
                        return false;

                    (var extractionResult, var unpackedArchive) = PerformAutoUpdateExtraction(filePath, patchFolder, progressMonitor);
                    logger.Info($"Extraction complete {extractionResult}");
                    if (!extractionResult)
                        return false;

                    PerformAutoUpdateInstall(unpackedArchive, progressMonitor);
                }

                return true;
            }
            finally
            {
                uiManager.DisposeUIElement(displayId);
            }
        }

        private static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = System.IO.Directory.EnumerateFiles(sourcePath, "*", System.IO.SearchOption.AllDirectories)
                                 .GroupBy(s => System.IO.Path.GetDirectoryName(s));

            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                System.IO.Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = System.IO.Path.Combine(targetFolder, System.IO.Path.GetFileName(file));
                    if (System.IO.File.Exists(targetFile)) System.IO.File.Delete(targetFile);
                    System.IO.File.Move(file, targetFile);
                }
            }

            System.IO.Directory.Delete(source, true);
        }

        private (bool, string) PerformAutoUpdateDownload(IUpdateDescription update, string targetFolder, IProgressMonitor progressMonitor)
        {
            var fileDownloader = new GitHubFileDownloader(update.DirectDownloadUrl, targetFolder);
            fileDownloader.FileName = $"gobchat-{update.Version.Major}.{update.Version.Minor}.{update.Version.Patch}-{update.Version.PreRelease}.zip";

            try
            {
                var downloadResult = fileDownloader.Download(progressMonitor);
                switch (downloadResult)
                {
                    case GitHubFileDownloader.Result.Canceled:
                        return (false, null);
                }
            }
            catch (DownloadFailedException ex)
            {
                logger.Error(ex);

                System.Windows.Forms.MessageBox.Show(
                        string.Format("Unable to download newest version.\nReason: {0}", ex.ToString()),
                        "Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );

                return (false, null);
            }

            return (true, fileDownloader.FilePath);
        }

        private (bool, string) PerformAutoUpdateExtraction(string archivePath, string patchFolder, IProgressMonitor progressMonitor)
        {
            var outputFolder = System.IO.Path.Combine(patchFolder, TempPatchFolder);
            var unpacker = new ArchiveUnpacker(archivePath, outputFolder);
#if DEBUG
            unpacker.DeleteArchiveOnCompletion = false;
#else
            unpacker.DeleteArchiveOnCompletion = true;
#endif
            unpacker.DeleteOutputFolderOnFail = true;

            try
            {
                var result = unpacker.Extract(progressMonitor);
                switch (result)
                {
                    case ArchiveUnpacker.Result.Canceled:
                        return (false, null);
                }
            }
            catch (ExtractionFailedException ex)
            {
                logger.Error(ex);

                System.Windows.Forms.MessageBox.Show(
                        string.Format("Unable to extract archive.\nReason: {0}", ex.ToString()),
                        "Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );

                return (false, null);
            }

            return (true, outputFolder);
        }

        private void PerformAutoUpdateInstall(string patchFolder, IProgressMonitor progressMonitor)
        {
            logger.Info("Prepare updates");

            progressMonitor.StatusText = "Prepare installation of update";
            progressMonitor.Progress = 0d;

            var tmp = System.IO.Path.Combine(patchFolder, "Gobchat");
            if (System.IO.Directory.Exists(tmp))
                patchFolder = tmp;

            var manager = NAppUpdate.Framework.UpdateManager.Instance;
            manager.UpdateSource = new NAULocalFileUpdateSource(patchFolder);
            manager.UpdateFeedReader = new NAULocalFileFeedReader();
            manager.Config.UpdateExecutableName = System.AppDomain.CurrentDomain.FriendlyName;

            manager.ReinstateIfRestarted();
            manager.CheckForUpdates();

            progressMonitor.Log($"{manager.UpdatesAvailable} updates for installation found.");

            if (manager.UpdatesAvailable > 0)
                manager.PrepareUpdates();

            progressMonitor.StatusText = "Waiting for shutdown";
            progressMonitor.Progress = 1d;

            logger.Info($"{manager.UpdatesAvailable} updates prepared.");
        }

        private UpdateFormDialog.UpdateType AskUser(IUpdateDescription update)
        {
            using (var notes = new UpdateFormDialog())
            {
                notes.UpdateHeadText = $"An update to version {update.Version} is available.\nCurrent version is {GobchatApplicationContext.ApplicationVersion}\nUpdate and restart?";
                notes.UpdateNotes = update.PatchNotes;
                notes.ShowDialog();
                return notes.UpdateRequest;
            }
        }

        private IUpdateDescription GetUpdate(GobVersion appVersion, bool allowBetaUpdates = false)
        {
            var provider = new GitHubUpdateProvider(appVersion, userName: "MarbleBag", repoName: "Gobchat");
            provider.AcceptBetaReleases = allowBetaUpdates;

            try
            {
                var updateDescription = provider.CheckForUpdate();
                if (!updateDescription.IsVersionAvailable || updateDescription.Version <= appVersion)
                    return null;
                return updateDescription;
            }
            catch (Exception)
            {
                //TODO
                return null;
            }
        }

        public void Dispose()
        {
        }
    }
}