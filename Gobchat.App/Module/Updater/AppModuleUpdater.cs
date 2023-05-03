/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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
using System.IO;
using System.Linq;

namespace Gobchat.Module.Updater
{
    public sealed class AppModuleUpdater : IApplicationModule
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string PatchStorageFolder => System.IO.Path.Combine(GobchatContext.ApplicationLocation, "patch");
        private string PatchArchiveExtractionFolder => System.IO.Path.Combine(PatchStorageFolder, "extracted");

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            if (handler == null) throw new System.ArgumentNullException(nameof(handler));

            DeleteOldPatchData();

#if DEBUG
            handler.StopStartup = DoLocaleUpdate();
            if (handler.StopStartup)
                return;
#endif

            var configManager = container.Resolve<IConfigManager>();
            var doUpdate = configManager.GetProperty<bool>("behaviour.appUpdate.checkOnline");

            if (!doUpdate)
                return;

            var allowBetaUpdates = configManager.GetProperty<bool>("behaviour.appUpdate.acceptBeta");

            var update = GetUpdate(GobchatContext.ApplicationVersion, allowBetaUpdates);
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
                var dialogText = StringFormat.Format(Resources.Module_Updater_Dialog_ManualInstall_Text, update.Version);
                var dialogResult = System.Windows.Forms.MessageBox.Show(dialogText, Resources.Module_Updater_Dialog_ManualInstall_Title, System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);

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
                if (System.IO.Directory.Exists(PatchArchiveExtractionFolder))
                {
                    logger.Info("Deleting temp update data");
                    System.IO.Directory.Delete(PatchArchiveExtractionFolder, true);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
            }
        }

        private bool DoLocaleUpdate()
        {
            if (!Directory.Exists(PatchStorageFolder))
                return false;

            var archives = Directory.EnumerateFiles(PatchStorageFolder, "*.zip", SearchOption.TopDirectoryOnly);

            GobVersion lastVersion = null;
            string archivePath = null;

            foreach( var archive in archives)
            {
                var fileName = Path.GetFileNameWithoutExtension(archive);
                var hasVersion = fileName.IndexOf("-");
                if (hasVersion <= 0)
                    continue;

                if(!GobVersion.TryParse(fileName.Substring(hasVersion+1), out var gobVersion))
                    continue;

                if(gobVersion <= GobchatContext.ApplicationVersion)
                    continue;

                if(lastVersion == null || lastVersion <= gobVersion)
                {
                    lastVersion = gobVersion;
                    archivePath = archive;
                }
            }

            if (archivePath == null)
                return false;

            if (!PerformExtractionAndUpdate(archivePath, new NullProgressMonitor()))
                return false;

            return true;
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
                    var patchFolder = PatchStorageFolder;
                    (var downloadResult, var archivePath) = PerformAutoUpdateDownload(update, PatchStorageFolder, progressMonitor);
                    logger.Info($"Download complete: {downloadResult}");
                    if (!downloadResult)
                        return false;

                    if(!PerformExtractionAndUpdate(archivePath, progressMonitor))
                        return false;
                }

                return true;
            }
            finally
            {
                uiManager.DisposeUIElement(displayId);
            }
        }

        private bool PerformExtractionAndUpdate(string archivePath, IProgressMonitor progressMonitor)
        {
            (var extractionResult, var unpackedPatchDir) = PerformAutoUpdateExtraction(archivePath, PatchArchiveExtractionFolder, progressMonitor);
            logger.Info($"Extraction complete {extractionResult}");
            if (!extractionResult)
                return false;

            PerformAutoUpdateInstall(unpackedPatchDir, progressMonitor);
            return true;
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
#if DEBUG
            fileDownloader.DeleteFileOnCancelOrError = false;
#endif

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
                        StringFormat.Format(Resources.Module_Updater_Dialog_DownloadFailed_Text, ex.ToString()),
                        Resources.Module_Updater_Dialog_DownloadFailed_Title,
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );

                return (false, null);
            }

            return (true, fileDownloader.FilePath);
        }

        private (bool, string) PerformAutoUpdateExtraction(string archivePath, string extractTo, IProgressMonitor progressMonitor)
        {
            var unpacker = new ArchiveUnpacker(archivePath, extractTo);
#if DEBUG
            unpacker.DeleteArchiveOnCompletion = false;
            unpacker.DeleteOutputFolderOnFail = false;
#endif


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
                        StringFormat.Format(Resources.Module_Updater_Dialog_ExtractionFailed_Text, ex.ToString()),
                        Resources.Module_Updater_Dialog_ExtractionFailed_Title,
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );

                return (false, null);
            }

            return (true, extractTo);
        }

        private void PerformAutoUpdateInstall(string unpackedArchive, IProgressMonitor progressMonitor)
        {
            logger.Info("Prepare updates");

            progressMonitor.StatusText = Resources.Module_Updater_UI_Log_PrepareUpdates;
            progressMonitor.Progress = 0d;

            var innerPath = System.IO.Path.Combine(unpackedArchive, "Gobchat");
            if (System.IO.Directory.Exists(innerPath)) // happens, if it wasn't packed with a parent folder
                unpackedArchive = innerPath;

            var manager = NAppUpdate.Framework.UpdateManager.Instance;
            manager.UpdateSource = new NAULocalFileUpdateSource(unpackedArchive);
            manager.UpdateFeedReader = new NAULocalFileFeedReader();
            manager.Config.UpdateExecutableName = System.AppDomain.CurrentDomain.FriendlyName;

            manager.ReinstateIfRestarted();
            manager.CheckForUpdates();

            progressMonitor.Log(StringFormat.Format(Resources.Module_Updater_UI_Log_UpdateCount, manager.UpdatesAvailable));

            if (manager.UpdatesAvailable > 0)
                manager.PrepareUpdates();

            progressMonitor.StatusText = Resources.Module_Updater_UI_Log_Done;
            progressMonitor.Progress = 1d;

            logger.Info($"{manager.UpdatesAvailable} updates prepared.");
        }

        private UpdateFormDialog.UpdateType AskUser(IUpdateDescription update)
        {
            using (var notes = new UpdateFormDialog())
            {
                notes.UpdateHeadText = StringFormat.Format(notes.UpdateHeadText, update.Version, GobchatContext.ApplicationVersion);
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
            catch (Exception e)
            {
                logger.Error(e);
                return null;
            }
        }

        public void Dispose()
        {
        }
    }
}