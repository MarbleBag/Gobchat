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

using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using NAppUpdate.Framework.Utils;
using System;
using System.IO;

namespace Gobchat.Core.Module.Updater
{
    [Serializable]
    [UpdateTaskAlias("folderDelete")]
    internal sealed class FolderDeleteTask : NAppUpdate.Framework.Tasks.UpdateTaskBase
    {
        private string _folderPath;
        private string _backupFolderPath;

        public FolderDeleteTask(string folderPath)
        {
            FolderPath = folderPath;
        }

        public string FolderPath
        {
            get => _folderPath;
            set => _folderPath = value ?? throw new ArgumentNullException(nameof(FolderPath));
        }

        public override void Prepare(IUpdateSource source)
        {
            UpdateManager.Instance.Logger.Log($"FolderDeleteTask: Deleting Folder {FolderPath}");
        }

        public override TaskExecutionStatus Execute(bool coldRun)
        {
            if (!Directory.Exists(FolderPath))
                return TaskExecutionStatus.Successful;

            if (!coldRun)
                return TaskExecutionStatus.RequiresAppRestart;

            if (_backupFolderPath == null)
            {
                _backupFolderPath = Path.Combine(UpdateManager.Instance.Config.BackupFolder, FolderPath);
                CopyDirectory(FolderPath, _backupFolderPath);
            }

            try
            {
                FileSystem.DeleteDirectory(FolderPath);
            }
            catch (Exception ex)
            {
                if (coldRun)
                {
                    ExecutionStatus = TaskExecutionStatus.Failed;
                    throw new UpdateProcessFailedException($"Unable to delete folder: {FolderPath}", ex);
                }
            }

            if (Directory.Exists(FolderPath))
                if (PermissionsCheck.HaveWritePermissionsForFolder(FolderPath))
                    return TaskExecutionStatus.RequiresAppRestart;
                else
                    return TaskExecutionStatus.RequiresPrivilegedAppRestart;
            return TaskExecutionStatus.Successful;
        }

        public override bool Rollback()
        {
            CopyDirectory(_backupFolderPath, FolderPath);
            return true;
        }

        private static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyDirectory(diSource, diTarget);
        }

        private static void CopyDirectory(DirectoryInfo src, DirectoryInfo dst)
        {
            Directory.CreateDirectory(dst.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in src.GetFiles())
                fi.CopyTo(Path.Combine(dst.FullName, fi.Name), true);

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in src.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = dst.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}