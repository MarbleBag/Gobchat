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
using System.Collections.Generic;
using System.Threading.Tasks;
using Gobchat.Core.Runtime;
using Gobchat.UI.Forms;
using Gobchat.UI.Web;
using Gobchat.UI.Web.JavascriptEvents;

namespace Gobchat.Module.UI.Internal
{
    internal sealed partial class BrowserAPIManager : IBrowserAPIManager, IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private event EventHandler<UIReadChangedEventArgs> _onUIReadyChanged;

        private readonly JavascriptBuilder _jsBuilder = new JavascriptBuilder();
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
                _onUIReadyChanged?.Invoke(this, new UIReadChangedEventArgs(IsUIReady));
            }
        }

        public IBrowserChatHandler ChatHandler { get; set; }
        public IBrowserConfigHandler ConfigHandler { get; set; }

        public event EventHandler<UIReadChangedEventArgs> OnUIReadyChanged
        {
            add
            {
                _onUIReadyChanged += value;
                _onUIReadyChanged?.Invoke(this, new UIReadChangedEventArgs(IsUIReady));
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
                    logger.Fatal(ex);
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
                     logger.Fatal(ex);
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