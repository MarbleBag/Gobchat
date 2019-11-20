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

using Gobchat.Core.Resource;
using Gobchat.Memory.Chat;
using Gobchat.UI.Forms;
using Gobchat.Core.Util.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Gobchat.Core.Chat;
using Gobchat.UI.Web.JavascriptEvents;
using System.Collections.Concurrent;
using Gobchat.Core.Util.Extension.Queue;
using Gobchat.Core.Runtime;
using Gobchat.Core.Config;

namespace Gobchat.Core
{
    public sealed class SomeoneWhoDoesSomeWork : IDisposable
    {
        private Memory.FFXIVMemoryProcessor _memoryProcessor;

        private CefOverlayForm _overlay;
        private GobchatWebAPI _api;

        private global::Gobchat.UI.Web.JavascriptBuilder _jsBuilder = new global::Gobchat.UI.Web.JavascriptBuilder();

        private KeyboardHook _keyboardHook;

        private Chat.ChatlogParser _chatlogParser;
        private readonly ConcurrentQueue<Chat.ChatMessage> _messageQueue = new ConcurrentQueue<Chat.ChatMessage>();
        private DateTime _lastChatMessageTime;

        internal void Initialize(global::Gobchat.Core.Runtime.IDIContext container, global::Gobchat.UI.Forms.CefOverlayForm overlay)
        {
            _overlay = overlay;
            _overlay.Visible = false;

            _configManager = container.Resolve<GobchatConfigManager>();

            //   Application.ApplicationExit += (s, e) => OnEvent_ApplicationExit();

            LoadMemoryParser();
            LoadChatParser();

            _api = new GobchatWebAPI(_overlay.Browser, OnEvent_UIMessage);
            _overlay.Browser.BindBrowserAPI(_api, true);

            //_overlay.InvokeUIThread(true, () => _overlay.Hide());
            _overlay.Browser.BrowserInitialized += (s, e) => LoadGobchatUI();
            _overlay.Browser.BrowserLoadPage += (s, e) => LoadGobchatNamespace();
            _overlay.Browser.BrowserLoadPageDone += (s, e) =>
            {
                if (!_overlay.Visible)
                    _overlay.InvokeAsyncOnUI((_) => _overlay.Visible = true);
            };

            var configManager = _configManager;

            if (configManager.UserConfig.HasProperty("behaviour.frame.chat.position.x") &&
                configManager.UserConfig.HasProperty("behaviour.frame.chat.position.y"))
            {
                var posX = configManager.UserConfig.GetProperty<long>("behaviour.frame.chat.position.x");
                var posY = configManager.UserConfig.GetProperty<long>("behaviour.frame.chat.position.y");
                _overlay.Location = new System.Drawing.Point((int)posX, (int)posY);
            }

            if (configManager.UserConfig.HasProperty("behaviour.frame.chat.size.width") &&
                configManager.UserConfig.HasProperty("behaviour.frame.chat.size.height"))
            {
                var width = configManager.UserConfig.GetProperty<long>("behaviour.frame.chat.size.width");
                var height = configManager.UserConfig.GetProperty<long>("behaviour.frame.chat.size.height");
                _overlay.Size = new System.Drawing.Size((int)width, (int)height);
            }

            //TODO make sure chat is not outside of display
            //TODO make sure chat is not too small
            //TODO make sure chat is not too big

            //not working
            //_keyboardHook = new KeyboardHook();
            // _keyboardHook.RegisterHotKey(ModifierKeys.Control, System.Windows.Forms.Keys.U, () => _overlay.InvokeUIThread(true, () => _overlay.Visible = false));
            // _keyboardHook.RegisterHotKey(ModifierKeys.Control, System.Windows.Forms.Keys.M, () => Debug.WriteLine("Yay!"));
        }

        private JSEvent OnEvent_UIMessage(string eventName, string details)
        {
            if ("LoadGobchatConfig".Equals(eventName, StringComparison.InvariantCultureIgnoreCase))
            {
                var json = _configManager.UserConfig.ToJson().ToString();
                return new UIEvents.LoadGobchatConfigEvent(json);
            }

            if ("SaveGobchatConfig".Equals(eventName, StringComparison.InvariantCultureIgnoreCase))
            {
                var configAsJson = _jsBuilder.Deserialize(details);
                _configManager.UserConfig.SetProperties((Newtonsoft.Json.Linq.JObject)configAsJson);
            }

            return null;
        }

        private void LoadMemoryParser()
        {
            _memoryProcessor = new Memory.FFXIVMemoryProcessor();
            _memoryProcessor.ProcessChangeEvent += MemoryProcessor_ProcessChangeEvent;
            _memoryProcessor.ChatlogEvent += MemoryProcessor_ChatlogEvent;

            var resourceFolder = System.IO.Path.Combine(GobchatApplicationContext.ResourceLocation, @"sharlayan");
            System.IO.Directory.CreateDirectory(resourceFolder);
            _memoryProcessor.LocalCacheDirectory = resourceFolder;

            _memoryProcessor.Initialize();
        }

        private void LoadChatParser()
        {
            var languagePath = System.IO.Path.Combine(GobchatApplicationContext.ResourceLocation, @"lang");
            var resourceResolvers = new IResourceResolver[] { new LocalFolderResourceResolver(languagePath) };
            var autotranslateProvider = new AutotranslateProvider(resourceResolvers, "autotranslate", new CultureInfo("en"));

            //TODO not changeable at the moment
            var selectedLanguage = _configManager.UserConfig.GetProperty<string>("behaviour.language");
            autotranslateProvider.LoadCulture(new CultureInfo(selectedLanguage));

            _lastChatMessageTime = DateTime.Now;
            _chatlogParser = new Chat.ChatlogParser(autotranslateProvider);
        }

        private void OnEvent_ApplicationExit()
        {
            var chatLocation = _overlay.Location;
            _configManager.UserConfig.SetProperty("behaviour.frame.chat.position.x", chatLocation.X);
            _configManager.UserConfig.SetProperty("behaviour.frame.chat.position.y", chatLocation.Y);

            var chatSize = _overlay.Size;
            _configManager.UserConfig.SetProperty("behaviour.frame.chat.size.width", chatSize.Width);
            _configManager.UserConfig.SetProperty("behaviour.frame.chat.size.height", chatSize.Height);
        }

        private void LoadGobchatUI()
        {
            var htmlpath = System.IO.Path.Combine(GobchatApplicationContext.ResourceLocation, @"ui\gobchat.html");
            var ok = System.IO.File.Exists(htmlpath); //TODO
            var uri = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = htmlpath }.Uri.AbsoluteUri;
            _overlay.Browser.Load(uri);
            //_overlay.Browser.Load("about:blank");
            //_overlay.Browser.Load("www.google.com");
        }

        private void LoadGobchatNamespace()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine("'use strict'");
            builder.AppendLine("var Gobchat = (function(Gobchat){");

            builder.Append("Gobchat.ChannelEnum = ");
            builder.AppendLine(typeof(Chat.ChannelEnum).EnumToJson());

            builder.Append("Gobchat.MessageSegmentEnum = ");
            builder.AppendLine(typeof(Chat.MessageSegmentEnum).EnumToJson());

            builder.Append("Gobchat.DefaultChatConfig = ");
            builder.AppendLine(_configManager.DefaultConfig.ToJson().ToString());

            builder.AppendLine("return Gobchat");
            builder.AppendLine("}(Gobchat || {}));");

            var script = builder.ToString();
            _overlay.Browser.ExecuteScript(script);
        }

        private void MemoryProcessor_ProcessChangeEvent(object sender, Memory.ProcessChangeEventArgs e)
        {
            if (e.IsProcessValid)
                Debug.WriteLine($"FFXIV process with id {e.ProcessId} detected!");
            else
                Debug.WriteLine("No FFXIV process detected.");
        }

        private void MemoryProcessor_ChatlogEvent(object sender, ChatlogEventArgs e)
        {
            var chatMessages = e.ChatlogItems
                .Where((item) =>
                {
                    var isNew = _lastChatMessageTime <= item.TimeStamp;
                    return isNew;
                })
                .Select((item) =>
                {
                    try
                    {
                        return _chatlogParser.Process(item);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in process chat log");
                        Debug.WriteLine($"Log: {item}");
                        Debug.WriteLine($"Error: {ex}");
                        return null;
                    }
                })
                .Where(item => item != null);

            _lastChatMessageTime = chatMessages.Select(msg => msg.Timestamp).DefaultIfEmpty(_lastChatMessageTime).Max();

            foreach (var msg in chatMessages)
            {
                _messageQueue.Enqueue(msg);
                Debug.WriteLine($"{msg}");
            }

            // - filter unwanted tokens
            // - extract source
            // - pack players and server
            // - build a useful structure
            // - send to browser

            //TODO process each message
        }

        internal void Update()
        {
            _memoryProcessor.Update();

            _overlay.InvokeAsyncOnUI((_) =>
            {
                if (_overlay.Browser.IsBrowserInitialized)
                {
                    foreach (var message in _messageQueue.DequeueMultiple(10))
                    {
                        //TODO maybe this can be done by calling gobchat directly
                        var script = _jsBuilder.BuildCustomEventDispatcher(new Chat.ChatMessageWebEvent(message));
                        _overlay.Browser.ExecuteScript(script);
                    }
                }
            });
        }

        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls
        private GobchatConfigManager _configManager;

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SomeoneWhoDoesSomeWork()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);

            Debug.WriteLine("Disposing Worker");

            OnEvent_ApplicationExit();

            if (!_disposedValue)
            {
                _keyboardHook?.Dispose();
                _keyboardHook = null;

                _memoryProcessor = null;

                _api = null;

                _overlay = null;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}