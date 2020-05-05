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

using Gobchat.Core.Chat;
using Gobchat.Core.Runtime;
using Gobchat.Module.Chat;
using Gobchat.UI.Web;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleChatToUI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IChatManager _chatManager;
        private IBrowserAPIManager _browserAPIManager;
        private GobchatBrowserChatAPI _providedAPI;

        /// <summary>
        /// Requires: <see cref="IBrowserAPIManager"/> <br></br>
        /// Requires: <see cref="IChatManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleChatToUI()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _chatManager = _container.Resolve<IChatManager>();
            _browserAPIManager = _container.Resolve<IBrowserAPIManager>();

            _browserAPIManager.ChatHandler = new GobchatBrowserChatAPI(_chatManager);

            _browserAPIManager.OnUIReadyChanged += BrowserAPIManager_OnUIReadyChanged;
            _chatManager.OnChatMessage += ChatManager_ChatMessageEvent;
        }

        public void Dispose()
        {
            _browserAPIManager.OnUIReadyChanged -= BrowserAPIManager_OnUIReadyChanged;
            _browserAPIManager.ChatHandler = null;
            _browserAPIManager = null;

            _chatManager.OnChatMessage -= ChatManager_ChatMessageEvent;
            _chatManager = null;
            _container = null;
        }

        private void BrowserAPIManager_OnUIReadyChanged(object sender, UIReadChangedEventArgs e)
        {
            var chatManager = _chatManager;
            if (chatManager != null)
                chatManager.Enable = e.IsUIReady;
        }

        private void ChatManager_ChatMessageEvent(object sender, ChatMessageEventArgs e)
        {
            _browserAPIManager.DispatchEventToBrowser(new ChatMessagesWebEvent(e.Messages));
        }

        private sealed class GobchatBrowserChatAPI : IBrowserChatHandler
        {
            private readonly IChatManager _chatManager;

            public GobchatBrowserChatAPI(IChatManager chatManager)
            {
                _chatManager = chatManager;
            }

            public async Task SendChatMessage(int channel, string source, string message)
            {
                _chatManager.EnqueueMessage(DateTime.Now, (ChatChannel)channel, source, message);
            }

            public async Task SendErrorChatMessage(string message)
            {
                _chatManager.EnqueueMessage(SystemMessageType.Error, message);
            }

            public async Task SendInfoChatMessage(string message)
            {
                _chatManager.EnqueueMessage(SystemMessageType.Info, message);
            }
        }
    }
}