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

using Gobchat.UI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gobchat.Memory;
using Gobchat.Memory.Chat;

namespace Gobchat.Core
{

    public sealed class SomeoneWhoDoesSomeWork : IDisposable
    {
        private Memory.FFXIVMemoryProcessor _memoryProcessor;
        private CefOverlayForm _overlay;
        private GobchatWebAPI _api;
        private Gobchat.UI.Web.JavascriptBuilder _jsBuilder = new Gobchat.UI.Web.JavascriptBuilder();

        private KeyboardHook _keyboardHook;

        private Chat.ChatlogParser _chatlogParser;
        private IList<Chat.ChatMessage> pendingChatMessages = new List<Chat.ChatMessage>();

        private readonly object lockObj = new object();
        private bool browserInitialized = false;

        internal void Initialize(UI.Forms.CefOverlayForm overlay)
        {
            _overlay = overlay;

            _memoryProcessor = new Memory.FFXIVMemoryProcessor();
            _memoryProcessor.ProcessChangeEvent += MemoryProcessor_ProcessChangeEvent;
            _memoryProcessor.ChatlogEvent += MemoryProcessor_ChatlogEvent;
            _memoryProcessor.Initialize();

            _api = new GobchatWebAPI(_overlay.Browser);
            _overlay.Browser.BindBrowserAPI(_api, true);
            //_overlay.InvokeUIThread(true, () => _overlay.Hide());

            _overlay.Browser.BrowserInitialized += OnEvent_Browser_BrowserInitialized;
            if (_overlay.Browser.IsBrowserInitialized)
            {
                InitializeBrowser();
            }

            _chatlogParser = new Chat.ChatlogParser();

            //not working
            //_keyboardHook = new KeyboardHook();
            // _keyboardHook.RegisterHotKey(ModifierKeys.Control, System.Windows.Forms.Keys.U, () => _overlay.InvokeUIThread(true, () => _overlay.Visible = false));
            // _keyboardHook.RegisterHotKey(ModifierKeys.Control, System.Windows.Forms.Keys.M, () => Debug.WriteLine("Yay!"));
        }

        private void OnEvent_Browser_BrowserInitialized(object sender, EventArgs e)
        {
            InitializeBrowser();
        }

        private void InitializeBrowser()
        {
            if (browserInitialized) return;
            lock (lockObj)
            {
                if (browserInitialized) return;
                browserInitialized = true;
            }

            _overlay.Browser.BrowserInitialized -= OnEvent_Browser_BrowserInitialized;

            var htmlpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources\ui\gobchat.html");
            var ok = System.IO.File.Exists(htmlpath);
            var uri = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = htmlpath }.Uri.AbsoluteUri;
            _overlay.Browser.Load(uri);
            //_overlay.Browser.Load("www.google.com");
        }

        private void MemoryProcessor_ProcessChangeEvent(object sender, Memory.ProcessChangeEventArgs e)
        {
            Debug.WriteLine($"FFXIV Process detected! {e.IsProcessValid} {e.ProcessId}");
        }

        private void MemoryProcessor_ChatlogEvent(object sender, ChatlogEventArgs e)
        {
            DateTime timeFilter = DateTime.Now.Subtract(TimeSpan.FromSeconds(10));

            e.ChatlogItems.ForEach(item =>
            {
                if (item.TimeStamp < timeFilter)
                    return;
                var message =  _chatlogParser.Process(item);

                Debug.WriteLine($"Chatlog: {item}");
                Debug.WriteLine($"Chatmessage: {message}");
                //TODO process each message
                // - filter unwanted tokens
                // - extract source
                // - pack players and server
                // - build a useful structure
                // - send to browser

                pendingChatMessages.Add(message);

            });
        }




        private IList<Chat.ChatMessage> GetAndClearPendingRecords()
        {
            if (pendingChatMessages.Count == 0)
                return new List<Chat.ChatMessage>();

            lock (lockObj)
            {
                var tmp = pendingChatMessages;
                pendingChatMessages = new List<Chat.ChatMessage>();
                return tmp;
            }
        }

        internal void Update()
        {
            _memoryProcessor.Update();

            if (_overlay.Browser.IsBrowserInitialized)
            {
                foreach(var message in GetAndClearPendingRecords())
                {
                    //TODO maybe this can be done by directly calling gobchat
                    var script = _jsBuilder.BuildCustomEventDispatcher(new Chat.ChatMessageWebEvent(message));
                    _overlay.Browser.ExecuteScript(script);
                }
                //TODO
            }
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
        #endregion


    }
}
