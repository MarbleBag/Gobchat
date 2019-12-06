﻿/*******************************************************************************
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using Gobchat.Core.Chat;
using Gobchat.UI.Web.JavascriptEvents;
using System.Collections.Concurrent;
using Gobchat.Core.Util.Extension.Queue;
using Gobchat.Core.Runtime;
using Gobchat.Core.Config;
using NLog;
using Gobchat.Core.Module;

namespace Gobchat.Core.Module.Chat
{
    // TODO This is chaos
    public sealed class SomeoneWhoDoesSomeWork : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private IDIContext _container;

        private Memory.FFXIVMemoryProcessor _memoryProcessor;
        private CefOverlayForm _overlay;
        private GobchatWebAPI _api;
        private GobchatConfigManager _configManager;
        private ChatMessageToFileLogger _chatLogger;

        private Gobchat.UI.Web.JavascriptBuilder _jsBuilder = new Gobchat.UI.Web.JavascriptBuilder();

        private Gobchat.Core.Chat.ChatlogToMessageConverter _chatlogParser;
        private readonly ConcurrentQueue<ChatMessage> _messageQueue = new ConcurrentQueue<ChatMessage>();
        private DateTime _lastChatMessageTime;

        private string _hotkey;

        internal void Initialize(IDIContext container, CefOverlayForm overlay)
        {
            _container = container;

            _overlay = overlay;

            _configManager = container.Resolve<GobchatConfigManager>();
            _chatLogger = new ChatMessageToFileLogger(_configManager);

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

            UpdateHotkeys();

            //TODO make sure chat is not outside of display
            //TODO make sure chat is not too small
            //TODO make sure chat is not too big
        }

        private JSEvent OnEvent_UIMessage(string eventName, string details)
        {
            if ("LoadGobchatConfig".Equals(eventName, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.Info("Sending config to ui");
                var json = _configManager.UserConfig.ToJson().ToString();
                return new UIEvents.LoadGobchatConfigEvent(json);
            }

            if ("SaveGobchatConfig".Equals(eventName, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.Info("Storing config from ui");
                var configAsJson = _jsBuilder.Deserialize(details);
                _configManager.UserConfig.SetProperties((Newtonsoft.Json.Linq.JObject)configAsJson);

                //TODO fire change events / move config to c#

                UpdateHotkeys();
            }

            return null;
        }

        #region memory parser

        private void LoadMemoryParser()
        {
            logger.Info("Loading memory parser");
            _memoryProcessor = new Memory.FFXIVMemoryProcessor();
            _memoryProcessor.ProcessChangeEvent += OnEvent_MemoryProcessor_ProcessChangeEvent;
            _memoryProcessor.ChatlogEvent += OnEvent_MemoryProcessor_ChatlogEvent;

            var resourceFolder = System.IO.Path.Combine(AbstractGobchatApplicationContext.ResourceLocation, @"sharlayan");
            System.IO.Directory.CreateDirectory(resourceFolder);
            _memoryProcessor.LocalCacheDirectory = resourceFolder;

            _memoryProcessor.Initialize();
        }

        private void OnEvent_MemoryProcessor_ProcessChangeEvent(object sender, Memory.ProcessChangeEventArgs e)
        {
            var uiManager = _container.Resolve<IUIManager>();
            if (uiManager.TryGetUIElement<UI.NotifyIconManager>(AppModuleNotifyIcon.NotifyIconManagerId, out var trayIcon))
            {
                if (e.IsProcessValid)
                    trayIcon.Icon = Gobchat.Resource.GobTrayIconOn;
                else
                    trayIcon.Icon = Gobchat.Resource.GobTrayIconOff;
            }

            if (e.IsProcessValid)
                logger.Info("FFXIV process detected");
            else
                logger.Info("No FFXIV process detected");
        }

        private void OnEvent_MemoryProcessor_ChatlogEvent(object sender, ChatlogEventArgs e)
        {
            var timeStamp = _lastChatMessageTime.Subtract(TimeSpan.FromSeconds(10));
            var chatMessages = e.ChatlogItems
                .Where((item) =>
                {
                    var isNew = timeStamp <= item.TimeStamp;
                    if (!isNew)
                        logger.Debug(() => $"Old message removed: {item.TimeStamp}");
                    return isNew;
                })
                .Select((item) =>
                {
                    try
                    {
                        return _chatlogParser.Convert(item);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in process chat log");
                        logger.Error(() => $"Log: {item}");
                        logger.Error(ex);
                        return null;
                    }
                })
                .Where(item => item != null);

            _lastChatMessageTime = chatMessages.Select(msg => msg.Timestamp).DefaultIfEmpty(_lastChatMessageTime).Max();

            foreach (var msg in chatMessages)
            {
                _messageQueue.Enqueue(msg);
                logger.Debug(() => msg.ToString());
            }
        }

        #endregion memory parser

        private void LoadChatParser()
        {
            logger.Info("Loading chat parser");
            var languagePath = System.IO.Path.Combine(AbstractGobchatApplicationContext.ResourceLocation, @"lang");
            var resourceResolvers = new IResourceLocator[] { new LocalFolderResourceResolver(languagePath) };
            var autotranslateProvider = new AutotranslateProvider(resourceResolvers, "autotranslate", new CultureInfo("en"));

            //TODO not changeable at the moment
            var selectedLanguage = _configManager.UserConfig.GetProperty<string>("behaviour.language");
            autotranslateProvider.LoadCulture(new CultureInfo(selectedLanguage));

            _lastChatMessageTime = DateTime.Now;
            _chatlogParser = new Gobchat.Core.Chat.ChatlogToMessageConverter(autotranslateProvider);
        }

        private void LoadGobchatUI()
        {
            logger.Info("Loading gobchat config");
            var htmlpath = System.IO.Path.Combine(AbstractGobchatApplicationContext.ResourceLocation, @"ui\gobchat.html");
            var ok = System.IO.File.Exists(htmlpath); //TODO
            var uri = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = htmlpath }.Uri.AbsoluteUri;
            _overlay.Browser.Load(uri);
            //_overlay.Browser.Load("about:blank");
            //_overlay.Browser.Load("www.google.com");
        }

        private void LoadGobchatNamespace()
        {
            InjectGobchatJavascript(builder =>
            {
                builder.Append("Gobchat.ChannelEnum = ");
                builder.AppendLine(typeof(Gobchat.Core.Chat.ChannelEnum).EnumToJson());

                builder.Append("Gobchat.MessageSegmentEnum = ");
                builder.AppendLine(typeof(Gobchat.Core.Chat.MessageSegmentEnum).EnumToJson());

                builder.Append("Gobchat.DefaultChatConfig = ");
                builder.AppendLine(_configManager.DefaultConfig.ToJson().ToString());
            });

            InjectGobchatJavascript(builder =>
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

        private void InjectGobchatJavascript(Action<System.Text.StringBuilder> content)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine("'use strict'");
            builder.AppendLine("var Gobchat = (function(Gobchat){");
            builder.AppendLine();
            content(builder);
            builder.AppendLine();
            builder.AppendLine("return Gobchat");
            builder.AppendLine("}(Gobchat || {}));");
            _overlay.Browser.ExecuteScript(builder.ToString());
        }

        private void UpdateHotkeys()
        {
            Keys StringToKeys(string keys)
            {
                if (keys == null || keys.Length == 0)
                    return Keys.None;

                var split = keys.Split(new char[] { '+' }).Select(s => s.Trim().ToLower());

                Keys nKeys = new Keys();
                foreach (var s in split)
                {
                    switch (s)
                    {
                        case "shift":
                            nKeys |= Keys.Shift;
                            break;

                        case "ctrl":
                            nKeys |= Keys.Control;
                            break;

                        case "alt":
                            nKeys |= Keys.Alt;
                            break;

                        default:
                            var result = Util.EnumUtil.ObjectToEnum<Keys>(s);
                            if (result.HasValue)
                                nKeys |= result.Value;
                            break;
                    }
                }

                if ((nKeys & ~Keys.Modifiers) != 0)
                    return nKeys;
                return Keys.None;
            }

            var hotkeyManager = _container.Resolve<IHotkeyManager>();
            var configHotkey = _configManager.UserConfig.GetProperty<string>("behaviour.hotkeys.showhide");

            var currentHotkey = StringToKeys(_hotkey);
            var newHotkey = StringToKeys(configHotkey);

            if (currentHotkey == newHotkey)
                return;

            try
            {
                if (currentHotkey == Keys.None)
                {
                    hotkeyManager.RegisterHotKey(newHotkey, OnEvent_Hotkey);
                }
                else
                {
                    hotkeyManager.UnregisterHotKey(currentHotkey, OnEvent_Hotkey);
                    if (newHotkey != Keys.None)
                        hotkeyManager.RegisterHotKey(newHotkey, OnEvent_Hotkey);
                }

                _hotkey = configHotkey;
            }
            catch (InvalidHotkeyException e)
            {
                _configManager.UserConfig.SetProperty("behaviour.hotkeys.showhide", "");
                logger.Fatal(e, "Invalid Hotkey");

                var userMsg = new ChatMessage(DateTime.Now, "Gobchat", (int)ChannelEnum.ERROR, $"Invalid Hotkey: {e.Message}");
                _messageQueue.Enqueue(userMsg);
            }
        }

        private void OnEvent_Hotkey()
        {
            _overlay.InvokeAsyncOnUI((f) =>
            {
                f.Visible = !f.Visible;
            });
        }

        internal void Update()
        {
            _memoryProcessor.Update();

            _overlay.InvokeSyncOnUI((_) =>
            {
                if (_overlay.Browser.IsBrowserInitialized)
                {
                    foreach (var message in _messageQueue.DequeueMultiple(10))
                    {
                        //TODO maybe this can be done by calling gobchat directly
                        var script = _jsBuilder.BuildCustomEventDispatcher(new ChatMessageWebEvent(message));
                        _overlay.Browser.ExecuteScript(script);

                        //TODO dispatch them also to log
                        try
                        {
                            _chatLogger.Log(message);
                        }
                        catch (Exception e)
                        {
                            logger.Fatal(e);
                        }
                    }
                    try
                    {
                        _chatLogger.Flush();
                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e);
                    }
                }

                return true;
            });
        }

        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls

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

            logger.Debug("Disposing worker");

            if (!_disposedValue)
            {
                _memoryProcessor = null;

                _api = null;

                _overlay = null;

                _chatLogger?.Dispose();
                _chatLogger = null;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}