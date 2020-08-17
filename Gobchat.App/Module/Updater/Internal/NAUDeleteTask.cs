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

using Gobchat.Core.Runtime;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using NAppUpdate.Framework.Utils;
using System;
using System.IO;

namespace Gobchat.Module.Updater.Internal
{
    [Serializable]
    [UpdateTaskAlias("Delete")]
    internal sealed class NAUDeleteTask : NAppUpdate.Framework.Tasks.UpdateTaskBase
    {
        private string _backupPath;
        private string _targetPath;

        [NauField("localPath", "The local path of the file to delete", true)]
        public string LocalPath { get; set; }

        public NAUDeleteTask()
        {
        }

        public override void Prepare(IUpdateSource source)
        {
            if (string.IsNullOrEmpty(LocalPath))
                UpdateManager.Instance.Logger.Log(Logger.SeverityLevel.Warning, $"DeleteTask: {nameof(LocalPath)} is empty, task is a noop");
            else
                UpdateManager.Instance.Logger.Log($"DeleteTask: Checking path '{LocalPath}'");
        }

        public override TaskExecutionStatus Execute(bool coldRun)
        {
            if (string.IsNullOrEmpty(LocalPath))
                return TaskExecutionStatus.Successful;

            //  if (!coldRun)
            //     return TaskExecutionStatus.RequiresAppRestart;

            _backupPath = System.IO.Path.Combine(UpdateManager.Instance.Config.BackupFolder, LocalPath);
            _targetPath = System.IO.Path.Combine(GobchatContext.ApplicationLocation, LocalPath);

            if (Directory.Exists(_targetPath))
                CopyDirectory(_targetPath, _backupPath);
            else if (File.Exists(_targetPath))
                CopyFile(_targetPath, _backupPath);

            if (Directory.Exists(_targetPath))
                return DeleteFolder(_targetPath, coldRun);
            else if (File.Exists(_targetPath))
                return DeleteFile(_targetPath, coldRun);
            else
                return TaskExecutionStatus.Successful;
        }

        private TaskExecutionStatus DeleteFile(string path, bool coldRun)
        {
            if (File.Exists(path))
                FileLockWait(path);

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                if (coldRun)
                {
                    ExecutionStatus = TaskExecutionStatus.Failed;
                    throw new UpdateProcessFailedException($"Unable to delete file: {path}", ex);
                }
            }

            if (File.Exists(path))
                if (PermissionsCheck.HaveWritePermissionsForFileOrFolder(path))
                    return TaskExecutionStatus.RequiresAppRestart;
                else
                    return TaskExecutionStatus.RequiresPrivilegedAppRestart;
            return TaskExecutionStatus.Successful;
        }

        private TaskExecutionStatus DeleteFolder(string path, bool coldRun)
        {
            try
            {
                FileSystem.DeleteDirectory(path);
            }
            catch (Exception ex)
            {
                if (coldRun)
                {
                    ExecutionStatus = TaskExecutionStatus.Failed;
                    throw new UpdateProcessFailedException($"Unable to delete folder: {path}", ex);
                }
            }

            if (Directory.Exists(path))
                if (PermissionsCheck.HaveWritePermissionsForFolder(path))
                    return TaskExecutionStatus.RequiresAppRestart;
                else
                    return TaskExecutionStatus.RequiresPrivilegedAppRestart;
            return TaskExecutionStatus.Successful;
        }

        public override bool Rollback()
        {
            if (_backupPath == null)
                return true;

            try
            {
                if (Directory.Exists(_backupPath))
                {
                    CopyDirectory(_backupPath, _targetPath);
                    if (Directory.Exists(_backupPath))
                        FileSystem.DeleteDirectory(_backupPath);
                }
                else if (File.Exists(_backupPath))
                {
                    CopyFile(_backupPath, _targetPath);
                    if (File.Exists(_backupPath))
                        File.Delete(_backupPath);
                }
            }
            catch (Exception)
            {
                throw; //TODO
            }

            return true;
        }

        private static void CopyFile(string sourceFile, string destinationFile)
        {
            string destinationDir = Path.GetDirectoryName(destinationFile);
            FileSystem.CreateDirectoryStructure(destinationDir, false);
            File.Copy(sourceFile, destinationFile, true);
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

        private static void FileLockWait(string path)
        {
            int attempt = 0;
            while (FileSystem.IsFileLocked(new FileInfo(path)))
            {
                System.Threading.Thread.Sleep(500);
                attempt++;
                if (attempt == 10)
                    throw new UpdateProcessFailedException("Failed to update, the file is locked: " + path);
            }
        }
    }
}