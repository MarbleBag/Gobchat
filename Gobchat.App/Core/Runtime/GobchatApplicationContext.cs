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

using Gobchat.Core;
using Gobchat.Core.Config;
using Gobchat.UI.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using TinyMessenger;
using NLog;
using System;
using Gobchat.Core.UI;
using Gobchat.Core.Module;
using Gobchat.Core.Module.CefInstaller;
using Gobchat.Module.Chat;
using Gobchat.Core.Module.Hotkey;
using Gobchat.Core.Module.Updater;

namespace Gobchat.Core.Runtime
{
    public sealed class GobchatApplicationContext : AbstractGobchatApplicationContext
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public new Form MainForm { get { return null; } }

        //TODO later
        // private readonly TinyIoC.TinyIoCContainer _applicationDIContextF = new TinyIoC.TinyIoCContainer();
        //  private readonly TinyMessengerHub _messageHub = new TinyMessenger.TinyMessengerHub();

        private DIContext _applicationDIContext;
        private UIManager _uiManager;
        private List<IApplicationModule> _activeApplicationModules;

        public GobchatApplicationContext()
        {
            //TODO
            // Turn this into the application core
            // Start other parts of the app as components
            // Initialize components on start up
            // Dispose components on shut down
            // Provide a type of UIManager
            // UIManager allows access to UI widgets via ID
            // UIManager allows to run tasks on the UI thread
            // Look for a simple DI framework which supports a context tree for injection
        }

        internal override void ApplicationStartupProcess(CancellationToken token)
        {
            _activeApplicationModules = new List<IApplicationModule>();
            _applicationDIContext = new DIContext();
            _uiManager = new UIManager(GobchatApplicationContext.UISynchronizer);

            _applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.ResourceLocation, nameof(ResourceLocation));
            _applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.UserConfigLocation, nameof(UserConfigLocation));
            _applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.ApplicationLocation, nameof(ApplicationLocation));
            _applicationDIContext.Register<System.Version>((c, _) => GobchatApplicationContext.ApplicationVersion, nameof(ApplicationVersion));

            _applicationDIContext.Register<IUISynchronizer>((c, _) => GobchatApplicationContext.UISynchronizer);
            _applicationDIContext.Register<IUIManager>((c, _) => _uiManager);

            var moduleActivationSequence = new List<IApplicationModule>()
            {
                new AppModuleConfig(),

                new AppModuleUpdater(),
                new AppModuleCefInstaller(),

                new AppModuleNotifyIcon(),
                new AppModuleHotKeyManager(),

                new AppModuleCefManager(),
                new AppModuleChatOverlay(),
                new AppModuleChat()
            };

            logger.Info(() => $"Initialize Gobchat v{GobchatApplicationContext.ApplicationVersion} on {(Environment.Is64BitProcess ? "x64" : "x86")}");

            var startupHandler = new ApplicationStartupHandler();
            foreach (var module in moduleActivationSequence)
            {
                try
                {
                    _activeApplicationModules.Add(module);
                    logger.Info($"Starting: {module}");
                    module.Initialize(startupHandler, _applicationDIContext);
                }
                catch (System.Exception e)
                {
                    logger.Fatal($"Initialization error in {module}");
                    logger.Fatal(e);
                    startupHandler.StopStartup = true;
                }

                if (startupHandler.StopStartup)
                {
                    logger.Fatal("Shutdown in initialization phase");
                    GobchatApplicationContext.ExitGobchat();
                    return;
                }
            }

            logger.Info("Initialization complete");
        }

        internal override async void ApplicationShutdownProcess()
        {
            logger.Info("Gobchat shutdown");

            //components are deactivated in reverse
            var moduleDeactivationSequence = _activeApplicationModules.Reverse<IApplicationModule>().ToList();
            _activeApplicationModules.Clear();

            foreach (var module in moduleDeactivationSequence)
            {
                try
                {
                    logger.Info($"Shutdown: {module}");
                    module.Dispose(_applicationDIContext);
                }
                catch (System.Exception e)
                {
                    //that's the best you get, no one cares for you - for now.
                    logger.Warn($"Shutdown error in {module}");
                    logger.Warn(e);
                }
            }

            _uiManager?.Dispose();
            _applicationDIContext?.Dispose();
        }
    }
}