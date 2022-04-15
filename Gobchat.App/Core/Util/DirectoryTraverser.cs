/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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
using System.Collections.Generic;

namespace Gobchat.Core.Util
{
    public static class DirectoryTraverser
    {
        public interface IFileVisitor
        {
            VisitResult PreDirectoryVisit(string currentDirectory);

            VisitResult PostDirectoryVisit(string currentDirectory);

            VisitResult VisitFile(string file);
        }

        public abstract class FileVisitorAdapter : IFileVisitor
        {
            virtual public VisitResult PostDirectoryVisit(string currentDirectory)
            {
                return VisitResult.Continue;
            }

            virtual public VisitResult PreDirectoryVisit(string currentDirectory)
            {
                return VisitResult.Continue;
            }

            virtual public VisitResult VisitFile(string file)
            {
                return VisitResult.Continue;
            }
        }

        public enum VisitResult
        {
            Continue,
            Terminate,
            SkipDirectories,
            SkipFiles,
            Return
        }

        internal class Node
        {
            public string Directory { get; }
            public bool IsFinished;

            public Node(string directory)
            {
                Directory = directory;
            }
        }

        public static IFileVisitor Walk(string root, IFileVisitor visitor)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));
            if (!System.IO.Directory.Exists(root))
                throw new ArgumentException($"");

            var nodes = new Stack<Node>(20);
            nodes.Push(new Node(root));

            VisitResult result;
            while (nodes.Count > 0)
            {
                Node currentNode = nodes.Peek();
                if (currentNode.IsFinished)
                {
                    nodes.Pop();
                    result = visitor.PostDirectoryVisit(currentNode.Directory);
                    if (VisitResult.Terminate == result)
                        return visitor;
                }

                currentNode.IsFinished = true;
                bool skipSubDirectories = false;
                bool skipFiles = false;

                result = visitor.PreDirectoryVisit(currentNode.Directory);

                switch (result)
                {
                    case VisitResult.Terminate:
                        visitor.PostDirectoryVisit(currentNode.Directory);
                        return visitor;

                    case VisitResult.Return:
                        skipSubDirectories = true;
                        skipFiles = true;
                        break;

                    case VisitResult.SkipDirectories:
                        skipSubDirectories = true;
                        break;

                    case VisitResult.SkipFiles:
                        skipFiles = true;
                        break;
                }

                if (!skipFiles)
                {
                    string[] files = System.IO.Directory.GetFiles(currentNode.Directory);
                    for (int i = 0; i < files.Length && !skipFiles; ++i)
                    {
                        result = visitor.VisitFile(files[i]);

                        switch (result)
                        {
                            case VisitResult.Terminate:
                                visitor.PostDirectoryVisit(currentNode.Directory);
                                return visitor;

                            case VisitResult.Return:
                                skipSubDirectories = true;
                                skipFiles = true;
                                break;

                            case VisitResult.SkipDirectories:
                                skipSubDirectories = true;
                                break;

                            case VisitResult.SkipFiles:
                                skipFiles = true;
                                break;
                        }
                    }
                }

                if (!skipSubDirectories)
                {
                    string[] subDirs = System.IO.Directory.GetDirectories(currentNode.Directory);
                    foreach (var subDir in subDirs)
                        nodes.Push(new Node(subDir));
                }
            }

            return visitor;
        }
    }
}