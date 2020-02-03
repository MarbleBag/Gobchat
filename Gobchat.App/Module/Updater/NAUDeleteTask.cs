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
using NAppUpdate.Framework.Common;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using NAppUpdate.Framework.Utils;
using System;
using System.IO;

namespace Gobchat.Module.Updater
{
    [Serializable]
    [UpdateTaskAlias("Delete")]
    internal sealed class NAUDeleteTask : NAppUpdate.Framework.Tasks.UpdateTaskBase
    {
        private readonly string _path;
        private string _backupPath;
        private bool isFolder;
        private bool isFile;

        public string TargetPath
        {
            get => _path;
        }

        public NAUDeleteTask(string targetPath)
        {
            _path = targetPath;
        }

        public override void Prepare(IUpdateSource source)
        {
            if (string.IsNullOrEmpty(TargetPath))
            {
                UpdateManager.Instance.Logger.Log(Logger.SeverityLevel.Warning, $"DeleteTask: {nameof(TargetPath)} is empty, task is a noop");
                return;
            }

            UpdateManager.Instance.Logger.Log($"DeleteTask: Checking path '{TargetPath}'");
            isFolder = Directory.Exists(TargetPath);
            isFile = File.Exists(TargetPath);
        }

        public override TaskExecutionStatus Execute(bool coldRun)
        {
            if (!isFolder && !isFile)
                return TaskExecutionStatus.Successful;

            if (!coldRun)
                return TaskExecutionStatus.RequiresAppRestart;

            if (_backupPath == null)
            {
                _backupPath = System.IO.Path.Combine(UpdateManager.Instance.Config.BackupFolder, TargetPath);
                if (isFolder)
                    CopyDirectory(TargetPath, _backupPath);
                else if (isFile)
                    CopyFile(TargetPath, _backupPath);
            }

            if (isFolder)
            {
                return DeleteFolder(coldRun);
            }
            else if (isFile)
            {
                return DeleteFile(coldRun);
            }
            else
            {
                return TaskExecutionStatus.Successful;
            }
        }

        private TaskExecutionStatus DeleteFile(bool coldRun)
        {
            if (File.Exists(TargetPath))
                FileLockWait();

            try
            {
                File.Delete(TargetPath);
            }
            catch (Exception ex)
            {
                if (coldRun)
                {
                    ExecutionStatus = TaskExecutionStatus.Failed;
                    throw new UpdateProcessFailedException($"Unable to delete fike: {TargetPath}", ex);
                }
            }

            if (File.Exists(TargetPath))
                if (PermissionsCheck.HaveWritePermissionsForFileOrFolder(TargetPath))
                    return TaskExecutionStatus.RequiresAppRestart;
                else
                    return TaskExecutionStatus.RequiresPrivilegedAppRestart;
            return TaskExecutionStatus.Successful;
        }

        private TaskExecutionStatus DeleteFolder(bool coldRun)
        {
            try
            {
                FileSystem.DeleteDirectory(TargetPath);
            }
            catch (Exception ex)
            {
                if (coldRun)
                {
                    ExecutionStatus = TaskExecutionStatus.Failed;
                    throw new UpdateProcessFailedException($"Unable to delete folder: {TargetPath}", ex);
                }
            }

            if (Directory.Exists(TargetPath))
                if (PermissionsCheck.HaveWritePermissionsForFolder(TargetPath))
                    return TaskExecutionStatus.RequiresAppRestart;
                else
                    return TaskExecutionStatus.RequiresPrivilegedAppRestart;
            return TaskExecutionStatus.Successful;
        }

        public override bool Rollback()
        {
            if (isFolder)
                CopyDirectory(_backupPath, TargetPath);
            if (isFile)
                CopyFile(_backupPath, TargetPath);
            return true;
        }

        private static void CopyFile(string sourceFile, string destinationFile)
        {
            string destinationDir = Path.GetDirectoryName(destinationFile);
            FileSystem.CreateDirectoryStructure(destinationDir, false);
            File.Move(sourceFile, destinationFile);
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
                fi.CopyTo(System.IO.Path.Combine(dst.FullName, fi.Name), true);

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in src.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = dst.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void FileLockWait()
        {
            int attempt = 0;
            while (FileSystem.IsFileLocked(new FileInfo(TargetPath)))
            {
                System.Threading.Thread.Sleep(500);
                attempt++;
                if (attempt == 10)
                {
                    throw new UpdateProcessFailedException("Failed to update, the file is locked: " + TargetPath);
                }
            }
        }
    }
}