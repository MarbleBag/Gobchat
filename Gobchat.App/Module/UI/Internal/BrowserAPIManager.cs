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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gobchat.Core.Runtime;
using Gobchat.UI.Forms;
using Gobchat.UI.Web;
using Gobchat.UI.Web.JavascriptEvents;
using Newtonsoft.Json.Linq;

namespace Gobchat.Module.UI.Internal
{
    internal sealed partial class BrowserAPIManager : IBrowserAPIManager, IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private event EventHandler<UIReadyChangedEventArgs> _onUIReadyChanged;

        private readonly JavascriptAndJsonBuilder _jsBuilder = new JavascriptAndJsonBuilder();
        private readonly List<IBrowserAPI> _apis = new List<IBrowserAPI>();
        private IUISynchronizer _synchronizer;
        private CefOverlayForm _overlay;
        private bool _isUIReady;

        public BrowserAPIManager(
                CefOverlayForm overlay,
                IUISynchronizer uiSynchronizer
            )
        {
            _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
            _synchronizer = uiSynchronizer ?? throw new ArgumentNullException(nameof(uiSynchronizer));

            RegisterAPI(new GobchatBrowserAPI(this));
        }

        public bool IsUIReady
        {
            get => _isUIReady;
            set
            {
                if (_isUIReady == value)
                    return;
                _isUIReady = value;
                _onUIReadyChanged?.Invoke(this, new UIReadyChangedEventArgs(IsUIReady));
            }
        }
        public IUISynchronizer UISynchronizer { get { return _synchronizer; } }
        public IBrowserChatHandler ChatHandler { get; set; }
        public IBrowserConfigHandler ConfigHandler { get; set; }
        public IBrowserActorHandler ActorHandler { get; set; }
        public IBrowserMemoryHandler MemoryHandler { get; set; }

        public event EventHandler<UIReadyChangedEventArgs> OnUIReadyChanged
        {
            add
            {
                _onUIReadyChanged += value;
                _onUIReadyChanged?.Invoke(this, new UIReadyChangedEventArgs(IsUIReady));
            }
            remove => _onUIReadyChanged -= value;
        }

        public void Dispose()
        {
            lock (_apis)
            {
                foreach (var api in _apis)
                {
                    try
                    {
                        _overlay.Browser.UnbindBrowserAPI(api);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }
                }

                _overlay = null;
            }

            _synchronizer = null;
            _onUIReadyChanged = null;

            ChatHandler = null;
            ConfigHandler = null;
        }

        public void DispatchEventToBrowser(JSEvent jsEvent)
        {
            if (jsEvent == null)
                return;
            var script = _jsBuilder.BuildCustomEventDispatcher(jsEvent);
            ExecuteJavascript(script);
        }

        public void ExecuteGobchatJavascript(Action<System.Text.StringBuilder> content)
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("'use strict'");
            builder.AppendLine("var Gobchat = function(Gobchat){");
            builder.AppendLine();
            content(builder);
            builder.AppendLine();
            builder.AppendLine("return Gobchat");
            builder.AppendLine("}(Gobchat || {});");
            ExecuteJavascript(builder.ToString());
        }

        public void ExecuteJavascript(string script)
        {
            _synchronizer.RunSync(() =>
            {
                try
                {
                    _overlay.Browser.ExecuteScript(script);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex, $"On script execution: {script}");
                }
            });
        }

        public async Task<IJavascriptResponse> EvaluateJavascript(string script, TimeSpan? timeout = null)
        {
            return await _synchronizer.RunSync(() =>
             {
                 try
                 {
                     return _overlay.Browser.EvaluateScript(script: script, timeout: timeout);
                 }
                 catch (Exception ex)
                 {
                     logger.Fatal(ex, $"On script execution: {script}");
                     return null;
                 }
             }).ConfigureAwait(false);
        }

        public void RegisterAPI(IBrowserAPI api)
        {
            lock (_apis)
            {
                if (_overlay.Browser.BindBrowserAPI(api, true))
                    _apis.Add(api);
            }
        }

        public void UnregisterAPI(IBrowserAPI api)
        {
            lock (_apis)
            {
                if (_overlay.Browser.UnbindBrowserAPI(api))
                    _apis.Remove(api);
            }
        }
    }
}