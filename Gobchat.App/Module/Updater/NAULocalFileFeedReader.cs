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

using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Core.Module.Updater
{
    public sealed class NAULocalFileFeedReader : NAppUpdate.Framework.FeedReaders.IUpdateFeedReader
    {
        public NAULocalFileFeedReader()
        {
        }

        public IList<NAppUpdate.Framework.Tasks.IUpdateTask> Read(string feed)
        {
            var tasks = new List<NAppUpdate.Framework.Tasks.IUpdateTask>();

            if (!System.IO.Directory.Exists(feed))
                return tasks;

            var files = System.IO.Directory.EnumerateFiles(feed, "*", System.IO.SearchOption.AllDirectories)
                 .GroupBy(s => System.IO.Path.GetDirectoryName(s));

            foreach (var folder in files)
            {
                foreach (var file in folder)
                {
                    var fileToPatch = file.Replace(feed, "").TrimStart('\\', ' ');
                    var updateTask = new NAppUpdate.Framework.Tasks.FileUpdateTask();
                    updateTask.LocalPath = fileToPatch;
                    tasks.Add(updateTask);
                }
            }

            return tasks;
        }
    }
}