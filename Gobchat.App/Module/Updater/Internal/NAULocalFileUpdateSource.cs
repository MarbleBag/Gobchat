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

using System;

namespace Gobchat.Module.Updater.Internal
{
    public sealed class NAULocalFileUpdateSource : NAppUpdate.Framework.Sources.IUpdateSource
    {
        private readonly string _updateFolder;

        public NAULocalFileUpdateSource(string updateFolder)
        {
            _updateFolder = updateFolder;
        }

        public bool GetData(string filePath, string basePath, Action<NAppUpdate.Framework.Common.UpdateProgressInfo> onProgress, ref string tempLocation)
        {
            var targetFilePath = System.IO.Path.Combine(_updateFolder, filePath);
            tempLocation = targetFilePath;
            return System.IO.File.Exists(tempLocation);
        }

        public string GetUpdatesFeed()
        {
            return _updateFolder;
        }
    }
}