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
using System;
using System.Linq;
using System.Windows.Forms;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleLoadUI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;
        private IBrowserAPIManager _browserAPIManager;
        private CefOverlayForm _cefOverlay;

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

            var uiManager = _container.Resolve<IUIManager>();

            _cefOverlay = uiManager.GetUIElement<CefOverlayForm>(AppModuleChatOverlay.OverlayUIId);

            _cefOverlay.Browser.OnBrowserLoadPage += Browser_BrowserLoadPage;
            _cefOverlay.Browser.OnBrowserLoadPageDone += Browser_BrowserLoadPageDone;
            _cefOverlay.Browser.OnBrowserInitialized += Browser_BrowserInitialized;
        }

        public void Dispose()
        {
            _cefOverlay.Browser.OnBrowserInitialized -= Browser_BrowserInitialized;
            _cefOverlay.Browser.OnBrowserLoadPage -= Browser_BrowserLoadPage;
            _cefOverlay.Browser.OnBrowserLoadPageDone -= Browser_BrowserLoadPageDone;

            _configManager = null;
            _cefOverlay = null;
            _container = null;
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