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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gobchat.Core.Runtime;

namespace Gobchat.Module.Updater.Internal
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

            var updateContent = GetContent(feed);
            var appContent = GetContent(GobchatContext.ApplicationLocation);

            // delete stuff if and only if both lists are not empty, otherwise stuff can really go sideway            
            /*if (appContent.Count > 0 && updateContent.Count > 0)
            {
                foreach (var file in appContent)
                    if (!updateContent.Contains(file))
                        // For some reason this task can crash the update process
                        tasks.Add(new NAUDeleteTask()
                        {
                            LocalPath = file.Replace(feed, "").TrimStart('\\', ' ') // Path.Combine(GobchatContext.ApplicationLocation, file)
                        });
            }*/
            

            var files = System.IO.Directory.EnumerateFiles(feed, "*", System.IO.SearchOption.AllDirectories)
             .GroupBy(s => System.IO.Path.GetDirectoryName(s));

            foreach (var folder in files)
                foreach (var file in folder)
                    tasks.Add(
                        new NAppUpdate.Framework.Tasks.FileUpdateTask()
                        {
                            LocalPath = file.Replace(feed, "").TrimStart('\\', ' ')
                        }
                    );

            return tasks;
        }

        private IList<string> GetContent(string location)
        {
            var content = new List<string>();

            var xmlFile = Path.Combine(location, "GobFileContent.xml");
            if (!File.Exists(xmlFile))
                return content;

            using (var xmlReader = System.Xml.XmlReader.Create(xmlFile))
            {
                bool readFile = false;
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == System.Xml.XmlNodeType.Element && xmlReader.Name == "File")
                        readFile = true;

                    if (readFile && xmlReader.NodeType == System.Xml.XmlNodeType.Text)
                        content.Add(xmlReader.Value);

                    if (xmlReader.NodeType == System.Xml.XmlNodeType.EndElement && xmlReader.Name == "File")
                        readFile = false;
                }
            }

            return content;
        }
    }
}