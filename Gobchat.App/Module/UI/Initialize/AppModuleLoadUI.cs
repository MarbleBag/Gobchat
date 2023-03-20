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

using Gobchat.Core.Chat;
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Core.Util.Extension;
using Gobchat.Module.Overlay;
using Gobchat.UI.Forms;
using Gobchat.UI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

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

        private ResourcePaths[] _resourcePaths;
        private Dictionary<string, string> _resolvedPaths = new Dictionary<string, string>();

        public UIResourceHandler()
        {
            _resourcePaths = new ResourcePaths[]{
                new ResourcePaths(new Regex(@"^lib\\"), System.IO.Path.Combine(GobchatContext.ResourceLocation, "ui", "lib")),
                new ResourcePaths(new Regex(@"^module\\"), System.IO.Path.Combine(GobchatContext.ResourceLocation, "ui", "modules")),
                new ResourcePaths(new Regex(@"^styles\\"), System.IO.Path.Combine(GobchatContext.ResourceLocation, "ui", "styles")),
                new ResourcePaths(new Regex(@"^graphics\\"), System.IO.Path.Combine(GobchatContext.ResourceLocation, "ui", "graphics"))
            };
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

    public sealed class AppModuleLoadUI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;
        private IBrowserAPIManager _browserAPIManager;
        private CefOverlayForm _cefOverlay;
        private UIResourceHandler _resourceHandler;


        /// <summary>
        /// Requires: <see cref="IBrowserAPIManager"/> <br></br>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// Requires: <see cref="IConfigManager"/> <br></br>
        /// <br></br>
        /// Adds to UI element: <see cref="CefOverlayForm"/> <br></br>
        /// </summary>
        public AppModuleLoadUI()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _configManager = _container.Resolve<IConfigManager>();
            _browserAPIManager = _container.Resolve<IBrowserAPIManager>();

            _resourceHandler = new UIResourceHandler();

            var uiManager = _container.Resolve<IUIManager>();

            _cefOverlay = uiManager.GetUIElement<CefOverlayForm>(AppModuleChatOverlay.OverlayUIId);

            _cefOverlay.Browser.OnRedirectableResourceRequests += Browser_RedirectResources;
            _cefOverlay.Browser.OnBrowserLoadPage += Browser_BrowserLoadPage;
            _cefOverlay.Browser.OnBrowserLoadPageDone += Browser_BrowserLoadPageDone;
            _cefOverlay.Browser.OnBrowserInitialized += Browser_BrowserInitialized;
        }


        public void Dispose()
        {
            _cefOverlay.Browser.OnRedirectableResourceRequests -= Browser_RedirectResources;
            _cefOverlay.Browser.OnBrowserInitialized -= Browser_BrowserInitialized;
            _cefOverlay.Browser.OnBrowserLoadPage -= Browser_BrowserLoadPage;
            _cefOverlay.Browser.OnBrowserLoadPageDone -= Browser_BrowserLoadPageDone;

            _resourceHandler = null;
            _configManager = null;
            _cefOverlay = null;
            _container = null;
        }

        private void Browser_RedirectResources(object sender, RedirectResourceRequestEventArgs e)
        {
            if (_resourceHandler.CheckForResourceRedirection(e.OriginalUri, e.ResourceType, out var newUri))
                e.RedirectUri = newUri;
        }

        private void Browser_BrowserInitialized(object sender, Gobchat.UI.Web.BrowserInitializedEventArgs e)
        { //browser is ready
            Browser_OnBrowserInitialized_LoadIndex();
        }

        private void Browser_OnBrowserInitialized_LoadIndex()
        {
            logger.Info("Loading gobchat ui");
            var htmlpath = System.IO.Path.Combine(GobchatContext.ResourceLocation, @"ui\gobchat.html");
            var uri = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = htmlpath }.Uri.AbsoluteUri;
            _cefOverlay.Browser.Load(uri);
        }

        private void Browser_BrowserLoadPage(object sender, Gobchat.UI.Web.BrowserLoadPageEventArgs e)
        {
            try
            {
                Browser_OnLoadPage_InjectEnums();
                Browser_OnLoadPage_InjectDefaultConfig();
                Browser_OnloadPage_InjectKeyCodes();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Error in browser load page");
            }
        }

        private void Browser_OnloadPage_InjectKeyCodes()
        {
            _browserAPIManager.ExecuteGobchatJavascript(builder =>
            {
                builder.Append("Gobchat.KeyCodeToKeyEnum = ");
                builder.AppendLine("function(keyCode){");
                {
                    var lookupTable = Enum.GetValues(typeof(Keys)).Cast<object>()
                                    .Where(enumValue => ((Keys)enumValue & Keys.Modifiers) == 0)
                                    .Distinct()
                                    .ToDictionary(enumValue => (int)enumValue, enumValue => enumValue.ToString());
                    var jsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(lookupTable);

                    builder.Append("const lookup = ").AppendLine(jsonObject);
                    builder.AppendLine("const result = lookup[keyCode]");
                    builder.AppendLine("return result === undefined ? null : result");
                }
                builder.AppendLine("}");
            });
        }

        private void Browser_OnLoadPage_InjectDefaultConfig()
        {
            _browserAPIManager.ExecuteGobchatJavascript(builder =>
            {
                builder.Append("Gobchat.DefaultProfileConfig = ");
                builder.AppendLine(_configManager.DefaultProfile.ToJson().ToString());
            });
        }

        private void Browser_OnLoadPage_InjectEnums()
        {
            _browserAPIManager.ExecuteGobchatJavascript(builder =>
            {
                builder.Append("Gobchat.MessageSegmentEnum = ");
                builder.AppendLine(typeof(MessageSegmentType).EnumToJson(s => s.ToUpperInvariant()));
            });

            _browserAPIManager.ExecuteGobchatJavascript(builder =>
            {
                builder.Append("Gobchat.ChannelEnum = ");
                builder.AppendLine(typeof(ChatChannel).EnumToJson(s => s.ToUpperInvariant()));
            });

            _browserAPIManager.ExecuteGobchatJavascript(builder =>
            {
                var channels = GobchatChannelMapping.GetAllChannels();

                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

                builder.AppendLine("Gobchat.Channels = {");

                for (var i = 0; i < channels.Count; ++i)
                {
                    var channel = channels[i];
                    var name = channel.ChatChannel.ToString().ToUpperInvariant();
                    var jsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(channel, settings);
                    builder.Append("\"").Append(name).Append("\": ").Append(jsonObject);
                    if (i + 1 < channels.Count)
                        builder.AppendLine(",");
                    else
                        builder.AppendLine();
                }

                builder.AppendLine("}");
            });
        }

        private void Browser_BrowserLoadPageDone(object sender, Gobchat.UI.Web.BrowserLoadPageEventArgs e)
        {
            Browser_OnPageLoaded_MakeOverlayVisible();
        }

        private void Browser_OnPageLoaded_MakeOverlayVisible()
        {
            if (!_cefOverlay.Visible)
                _cefOverlay.InvokeSyncOnUI((overlay) => overlay.Visible = true);
            //_synchronizer.RunSync();
        }


    }


}