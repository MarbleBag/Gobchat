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

using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Gobchat.Memory;
using Gobchat.Core;
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Core.Resource;
using Gobchat.Core.Chat;
using Gobchat.Core.Util.Extension;
using Gobchat.Core.Util.Extension.Queue;
using Gobchat.Memory.Chat;
using Gobchat.UI.Forms;
using Gobchat.UI.Web.JavascriptEvents;
using Gobchat.Module.NotifyIcon;
using Gobchat.Module.Hotkey;

namespace Gobchat.Module.Chat
{
    // TODO This is chaos
    public sealed class SomeoneWhoDoesSomeWork : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IUISynchronizer _synchronizer;

        private Memory.FFXIVMemoryProcessor _memoryProcessor;
        private CefOverlayForm _overlay;
        private GobchatWebAPI _api;
        private IGobchatConfigManager _configManager;
        private ChatMessageToFileLogger _chatLogger;

        private volatile bool _gobchatReady;

        //TODO move that stuff to its own class

        private bool _errorReportChatLogAvailable = false;
        private bool _errorReportRegistered = false;
        private bool _errorReportFFNotFound = false;

        private readonly Gobchat.UI.Web.JavascriptBuilder _jsBuilder = new Gobchat.UI.Web.JavascriptBuilder();

        private Gobchat.Core.Chat.ChatlogToMessageConverter _chatlogParser;
        private readonly ConcurrentQueue<ChatMessage> _messageQueue = new ConcurrentQueue<ChatMessage>();
        private DateTime _lastChatMessageTime;

        private sealed class HotkeyData
        {
            public string Name { get; }
            public string Hotkey { get; set; }
            public Action Callback { get; }

            public HotkeyData(string name, Action callback)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }
        }

        private Dictionary<string, HotkeyData> _hotkeys = new Dictionary<string, HotkeyData>();

        internal void Initialize(IDIContext container, CefOverlayForm overlay)
        {
            _container = container;
            _synchronizer = container.Resolve<IUISynchronizer>();

            _overlay = overlay;

            _configManager = container.Resolve<IGobchatConfigManager>();
            _chatLogger = new ChatMessageToFileLogger(_configManager);

            LoadMemoryParser();
            LoadChatParser();

            _api = new GobchatWebAPI(_overlay.Browser, OnEvent_UIMessage, OnEvent_UIRequest);
            _overlay.Browser.BindBrowserAPI(_api, true);

            //_overlay.InvokeUIThread(true, () => _overlay.Hide());
            _overlay.Browser.BrowserInitialized += (s, e) => LoadGobchatUI();
            _overlay.Browser.BrowserLoadPage += (s, e) => LoadGobchatNamespace();
            _overlay.Browser.BrowserLoadPageDone += (s, e) =>
            {
                if (!_overlay.Visible)
                    _synchronizer.RunSync(() => _overlay.Visible = true);
            };

            _hotkeys.Add("behaviour.hotkeys.showhide", new HotkeyData("Show & Hide", OnEvent_Hotkey_ShowHide));
            _hotkeys.Add("behaviour.hotkeys.search", new HotkeyData("Search", OnEvent_Hotkey_Search));

            _configManager.OnActiveProfileChange += Event_Config_ProfileChanged;
            _configManager.AddPropertyChangeListener("behaviour.hotkeys", Event_Config_HotkeysChanged);
            UpdateHotkeys();
        }

        private void Event_Config_ProfileChanged(object sender, ActiveProfileChangedEventArgs e)
        {
            //TODO trigger all stuff that needs to be updated on a profile change

            UpdateHotkeys();
        }

        private void Event_Config_HotkeysChanged(IGobchatConfigManager sender, ProfilePropertyChangedEventArgs evt)
        {
            UpdateHotkeys();
        }

        private async Task<string> OnEvent_UIRequest(string request, string data)
        {
            if (request == "GetConfig")
            {
                logger.Info("Sending config to ui");
                var configJson = _configManager.AsJson();
                return configJson.ToString();
            }
            else if (request == "SetConfig")
            {
                logger.Info("Storing config from ui");
                var configJson = _jsBuilder.Deserialize(data);
                _configManager.Synchronize(configJson);

                try
                { //try to do a early save
                    _configManager.SaveProfiles();
                    this.SendInfoMessageToUI("Profiles saved");
                }
                catch (Exception e)
                {
                    logger.Warn(e, "Error on profile save");
                    this.SendErrorMessageToUI("Unable to save profiles to disk. See debug log for more informations");
                }
            }
            else if (request == "SetActiveProfile")
            {
                _configManager.ActiveProfileId = data;
            }
            else if (request == "CloseGobchat")
            {
                // async request to kill the app immediately. This means other stuff may be happening at the same time that depends on the ui. Maybe not the best way.
                logger.Info("User requests shutdown");
                GobchatApplicationContext.ExitGobchat();
            }
            else if (request == "OpenFileDialog")
            {
                var uiManager = _container.Resolve<IUIManager>();

                string fileName = "";

                uiManager.UISynchronizer.RunSync(() =>
                {
                    using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
                    {
                        openFileDialog.InitialDirectory = GobchatApplicationContext.ResourceLocation;
                        openFileDialog.RestoreDirectory = true;
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            fileName = openFileDialog.FileName;
                    }
                });

                return fileName;
            }
            return "";
        }

        private JSEvent OnEvent_UIMessage(string eventName, string details)
        {
            if ("GobchatReady".Equals(eventName, StringComparison.InvariantCultureIgnoreCase))
            {
                _gobchatReady = true;
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
            _memoryProcessor.WindowFocusChangedEvent += OnEvent_MemoryProcessor_WindowFocusChanged;

            var resourceFolder = System.IO.Path.Combine(AbstractGobchatApplicationContext.ResourceLocation, @"sharlayan");
            System.IO.Directory.CreateDirectory(resourceFolder);
            _memoryProcessor.LocalCacheDirectory = resourceFolder;

            _memoryProcessor.Initialize();

            void updateObserveGameWindow()
            {
                var synchronizer = _container.Resolve<IUISynchronizer>();
                synchronizer.RunSync(() =>
                {
                    var hideOnMinimize = _configManager.GetProperty<bool>("behaviour.hideOnMinimize");
                    _memoryProcessor.ObserveGameWindow = hideOnMinimize;
                });
            }

            _configManager.AddPropertyChangeListener("behaviour.hideOnMinimize", (s, e) => { if (e.IsActiveProfile) updateObserveGameWindow(); });
            _configManager.OnActiveProfileChange += (s, e) => updateObserveGameWindow();
            updateObserveGameWindow();
        }

        private void OnEvent_MemoryProcessor_ProcessChangeEvent(object sender, Memory.ProcessChangeEventArgs e)
        {
            var uiManager = _container.Resolve<IUIManager>();
            if (uiManager.TryGetUIElement<Gobchat.Core.UI.NotifyIconManager>(AppModuleNotifyIcon.NotifyIconManagerId, out var trayIcon))
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
            var chatMessages = new List<ChatMessage>();

            var removeOutdatedMessages = true; // _configManager.GetProperty<bool>("behaviour.outdatedMessages.ignore");
            var outdatedTimelimit = 10; // _configManager.GetProperty<int>("behaviour.outdatedMessages.timelimit");
            var showOutdatedToUser = true; // _configManager.GetProperty<bool>("behaviour.outdatedMessages.showUser");
            var timeStamp = _lastChatMessageTime.Subtract(TimeSpan.FromSeconds(outdatedTimelimit));

            var numberOfOutdatedMessages = 0;

            foreach (var item in e.ChatlogItems)
            {
                if (removeOutdatedMessages)
                {
                    if (item.TimeStamp < timeStamp)
                    {
                        numberOfOutdatedMessages += 1;
                        logger.Debug(() => $"Outdated message removed: {item.TimeStamp}");
                        continue;
                    }
                }

                try
                {
                    logger.Debug(() => "Raw Message: " + item.ToString());
                    var message = _chatlogParser.Convert(item);
                    if (message != null)
                        chatMessages.Add(message);
                    else
                        logger.Info(() => "Invalid Message: " + item.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error("Error in process chat log");
                    logger.Error(() => $"Log: {item}");
                    logger.Error(ex);
                    continue;
                }
            }

            _lastChatMessageTime = chatMessages.Select(msg => msg.Timestamp).DefaultIfEmpty(_lastChatMessageTime).Max();

            foreach (var msg in chatMessages)
            {
                _messageQueue.Enqueue(msg);
                logger.Debug(() => msg.ToString());
            }

            if (showOutdatedToUser && numberOfOutdatedMessages > 0)
            {
                this.SendInfoMessageToUI($"Ignored {numberOfOutdatedMessages} outdated messages.");
            }
        }

        private void OnEvent_MemoryProcessor_WindowFocusChanged(object sender, WindowFocusChangedEventArgs e)
        {
            //   var hideOnMinimize = _configManager.ActiveProfile.GetProperty<bool>("behaviour.hideOnMinimize");
            //   if (!hideOnMinimize)
            //        return;
            _synchronizer.RunSync(() => _overlay.Visible = e.IsInForeground);
        }

        #endregion memory parser

        private void LoadChatParser()
        {
            logger.Info("Loading chat parser");
            var languagePath = System.IO.Path.Combine(GobchatApplicationContext.ResourceLocation, @"lang");
            var resourceResolvers = new IResourceLocator[] { new LocalFolderResourceResolver(languagePath) };
            var autotranslateProvider = new AutotranslateProvider(resourceResolvers, "autotranslate", new CultureInfo("en"));

            //TODO not changeable at the moment
            var selectedLanguage = _configManager.ActiveProfile.GetProperty<string>("behaviour.language");
            autotranslateProvider.LoadCulture(new CultureInfo(selectedLanguage));

            _lastChatMessageTime = DateTime.Now;
            _chatlogParser = new Gobchat.Core.Chat.ChatlogToMessageConverter(autotranslateProvider);
        }

        private void LoadGobchatUI()
        {
            logger.Info("Loading gobchat ui");
            var htmlpath = System.IO.Path.Combine(AbstractGobchatApplicationContext.ResourceLocation, @"ui\gobchat.html");
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

                builder.Append("Gobchat.DefaultProfileConfig = ");
                builder.AppendLine(_configManager.DefaultProfile.ToJson().ToString());
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

                var split = keys.Split(new char[] { '+' }).Select(s => s.Trim().ToUpper(CultureInfo.InvariantCulture));

                Keys nKeys = new Keys();
                foreach (var s in split)
                {
                    switch (s)
                    {
                        case "SHIFT":
                            nKeys |= Keys.Shift;
                            break;

                        case "CTRL":
                            nKeys |= Keys.Control;
                            break;

                        case "ALT":
                            nKeys |= Keys.Alt;
                            break;

                        default:
                            var result = Core.Util.EnumUtil.ObjectToEnum<Keys>(s);
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

            void UpdateHotkey(string propertyKey)
            {
                var hotkeyData = _hotkeys[propertyKey];

                var configHotkey = _configManager.ActiveProfile.GetProperty<string>(propertyKey);
                var currentHotkey = StringToKeys(hotkeyData.Hotkey);
                var newHotkey = StringToKeys(configHotkey);
                if (currentHotkey == newHotkey)
                    return;

                try
                {
                    var hotkeyAction = hotkeyData.Callback;

                    if (currentHotkey == Keys.None)
                    {
                        hotkeyManager.RegisterHotKey(newHotkey, hotkeyAction);
                    }
                    else
                    {
                        hotkeyManager.UnregisterHotKey(currentHotkey, hotkeyAction);
                        if (newHotkey != Keys.None)
                            hotkeyManager.RegisterHotKey(newHotkey, hotkeyAction);
                    }

                    hotkeyData.Hotkey = configHotkey;
                }
                catch (InvalidHotkeyException e)
                {
                    _configManager.ActiveProfile.SetProperty(propertyKey, "");
                    logger.Fatal(e, $"Invalid Hotkey for {hotkeyData.Name}");
                    var userMsg = new ChatMessage(DateTime.Now, "Gobchat", (int)ChannelEnum.ERROR, $"Invalid Hotkey for {hotkeyData.Name}: {e.Message}");
                    _messageQueue.Enqueue(userMsg);
                }
            }

            foreach (var hotkey in _hotkeys.Keys.ToList())
            {
                UpdateHotkey(hotkey);
            }
        }

        private void OnEvent_Hotkey_ShowHide()
        {
            _synchronizer.RunSync(() =>
            {
                _overlay.Visible = !_overlay.Visible;
            });
        }

        private void OnEvent_Hotkey_Search()
        {
            logger.Info("NOICE");
        }

        private void ValidateProcessAndInformUser()
        {
            if (!_memoryProcessor.FFXIVProcessValid)
            {
                if (!_errorReportFFNotFound)
                {
                    SendErrorMessageToUI("Can't find a running instance of FFXIV.");
                    _errorReportFFNotFound = true;
                }
            }
            else
            {
                SendInfoMessageToUI("FFXIV detected.");
                _errorReportFFNotFound = false;
            }

            if (_memoryProcessor.FFXIVProcessValid)
            {
                if (_memoryProcessor.ChatLogAvailable)
                {
                    _errorReportChatLogAvailable = false;
                }
                else if (!_errorReportChatLogAvailable)
                {
                    SendErrorMessageToUI("Can't access FFXIV chatlog. Restart Gobchat with admin rights.");
                    _errorReportChatLogAvailable = true;
                }
            }
        }

        private void SendInfoMessageToUI(string msg)
        {
            _messageQueue.Enqueue(new ChatMessage(DateTime.Now, "Gobchat", (int)ChannelEnum.GOBCHAT_INFO, msg));
        }

        private void SendErrorMessageToUI(string msg)
        {
            _messageQueue.Enqueue(new ChatMessage(DateTime.Now, "Gobchat", (int)ChannelEnum.GOBCHAT_ERROR, msg));
        }

        internal void Update()
        {
            _memoryProcessor.Update();

            if (!_errorReportRegistered)
            {
                _errorReportRegistered = true;
                _memoryProcessor.ProcessChangeEvent += (s, e) => ValidateProcessAndInformUser();
                ValidateProcessAndInformUser();
            }

            _synchronizer.RunSync(() =>
            {
                if (/*_overlay.Browser.IsBrowserInitialized*/ _gobchatReady)
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
                _synchronizer?.RunSync(() => _memoryProcessor?.Dispose());
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