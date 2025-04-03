/*******************************************************************************
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

using Gobchat.Core.Runtime;
using Gobchat.UI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Gobchat.Module.UI
{
    internal sealed class UIResourceHandler
    {
        private sealed class ResourcePaths
        {
            public Regex PathMatcher { get; }
            public string NewPath { get; }

            public ResourcePaths(Regex pathMatcher, string newPath)
            {
                PathMatcher = pathMatcher ?? throw new ArgumentNullException(nameof(pathMatcher));
                NewPath = newPath ?? throw new ArgumentNullException(nameof(newPath));
            }
        }

        private readonly List<ResourcePaths> _resourcePaths = new List<ResourcePaths>();
        private readonly Dictionary<string, string> _resolvedPaths = new Dictionary<string, string>();

        public UIResourceHandler()
        {
        }

        public void AddResourceRedirect(Regex uiPathMatcher, string redirectTo)
        {
            _resourcePaths.Add(new ResourcePaths(uiPathMatcher, redirectTo));            
        }

        public bool CheckForResourceRedirection(string originalUri, RedirectResourceRequestEventArgs.Type resourceType, out string newUri)
        {
            var appLocation = GobchatContext.ApplicationLocation;
            newUri = null;

            if (!Uri.TryCreate(appLocation, UriKind.Absolute, out var appUri))
                return false;

            if (!Uri.TryCreate(originalUri, UriKind.RelativeOrAbsolute, out var requestUri))
                return false;

            if (_resolvedPaths.TryGetValue(requestUri.LocalPath, out newUri))
                return true;

            string targetPath = null;

            if (appUri.IsBaseOf(requestUri))
            {
                targetPath = requestUri.LocalPath;
            }
            else
            {
                var relativePath = requestUri.LocalPath.Substring(Path.GetPathRoot(appLocation).Length);

                foreach (var redirect in _resourcePaths)
                {
                    var match = redirect.PathMatcher.Match(relativePath);
                    if (match.Success)
                    {
                        targetPath = redirect.PathMatcher.Replace(relativePath, redirect.NewPath + "\\");
                        break;
                    }
                }
            }

            if (targetPath == null)
            {
                _resolvedPaths.Add(requestUri.LocalPath, null);
                return false;
            }

            if (File.Exists(targetPath))
            {
                newUri = new Uri(targetPath).AbsoluteUri;
                _resolvedPaths.Add(requestUri.LocalPath, newUri);
                return true;
            }

            var possiblePaths = new List<string>();

            switch (resourceType)
            {
                case RedirectResourceRequestEventArgs.Type.Stylesheet:
                    possiblePaths.Add(Path.ChangeExtension(targetPath, "min.css"));
                    possiblePaths.Add(Path.ChangeExtension(targetPath, "css"));
                    break;
                case RedirectResourceRequestEventArgs.Type.Script:
                    possiblePaths.Add(Path.ChangeExtension(targetPath, "min.js"));
                    possiblePaths.Add(Path.ChangeExtension(targetPath, "js"));
                    break;
            }

            foreach (var possiblePath in possiblePaths)
            {
                if (File.Exists(possiblePath))
                {
                    newUri = new Uri(possiblePath).AbsoluteUri;
                    _resolvedPaths.Add(requestUri.LocalPath, newUri);
                    return true;
                }
            }

            return false;
        }
    }


}