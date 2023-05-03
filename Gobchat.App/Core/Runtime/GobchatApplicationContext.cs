/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using NLog;
using System;
using Gobchat.Core.UI;

namespace Gobchat.Core.Runtime
{
    public sealed class GobchatApplicationContext : AbstractGobchatApplicationContext
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

            //_applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.ResourceLocation, nameof(ResourceLocation));
            //_applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.UserConfigLocation, nameof(UserConfigLocation));
            //_applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.ApplicationLocation, nameof(ApplicationLocation));
            //_applicationDIContext.Register<GobVersion>((c, _) => GobchatApplicationContext.ApplicationVersion, nameof(ApplicationVersion));

            _applicationDIContext.Register<IUISynchronizer>((c, _) => GobchatApplicationContext.UISynchronizer);
            _applicationDIContext.Register<IUIManager>((c, _) => _uiManager);

            var moduleActivationSequence = new List<IApplicationModule>()
            {
                //config
                new global::Gobchat.Module.Config.AppModuleConfig(),
                new global::Gobchat.Module.Language.AppModuleLanguage(),

                //updater and downloadable dependencies
                new global::Gobchat.Module.Updater.AppModuleUpdater(),
                new global::Gobchat.Module.Cef.AppModuleCefDependencyChecker(),
                new global::Gobchat.Module.Cef.AppModuleCefInstaller(),

                //base managers
                new global::Gobchat.Module.NotifyIcon.AppModuleNotifyIcon(),
                new global::Gobchat.Module.Hotkey.AppModuleHotkeyManager(),
                new global::Gobchat.Module.MemoryReader.AppModuleMemoryReader(),
                new global::Gobchat.Module.Actor.AppModuleActorManager(),
                new global::Gobchat.Module.Chat.AppModuleChatManager(),

                // CEF overlay and javascript api
                new global::Gobchat.Module.Cef.AppModuleCefManager(),
                new global::Gobchat.Module.Overlay.AppModuleChatOverlay(),
                new global::Gobchat.Module.UI.AppModuleBrowserAPIManager(),

                // Misc
                new global::Gobchat.Module.Misc.AppModuleShowConnectionOnTrayIcon(),
                new global::Gobchat.Module.Misc.AppModuleHideOnMinimize(),
                new global::Gobchat.Module.Misc.Chatlogger.AppModuleChatLogger(),
                new global::Gobchat.Module.Misc.AppModuleInformUserAboutMemoryState(),
                new global::Gobchat.Module.Misc.AppModuleShowHideHotkey(),

                //UI Adapter
                new global::Gobchat.Module.UI.AppModuleChatToUI(),
                new global::Gobchat.Module.UI.AppModuleConfigToUI(),
                new global::Gobchat.Module.UI.AppModuleActorToUI(),
                new global::Gobchat.Module.UI.AppModuleMemoryToUI(),

                //Start UI
                new global::Gobchat.Module.UI.AppModuleLoadUI(),
            };

            logger.Info(() => $"Initialize Gobchat v{GobchatContext.ApplicationVersion} on {(Environment.Is64BitProcess ? "x64" : "x86")}");

            var startupHandler = new ApplicationStartupHandler();
            foreach (var module in moduleActivationSequence)
            {
                try
                {
                    _activeApplicationModules.Add(module);
                    logger.Info($"Starting: {module}");
                    module.Initialize(startupHandler, _applicationDIContext);
                }
                catch (System.Exception ex1)
                {
                    logger.Fatal($"Initialization error in {module}");
                    logger.Fatal(ex1);
                    startupHandler.StopStartup = true;

                    try
                    {
                        MessageBox.Show($"An error prevents Gobchat from starting. For more details please check gobchat_debug.log.\nError:\n{ex1.GetType()}: {ex1.Message}", "Error on start", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (System.Exception)
                    {
                    }
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

        internal override void ApplicationShutdownProcess()
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
                    module.Dispose();
                }
                catch (System.Exception e)
                {
                    //that's the best you get, no one cares for you.
                    logger.Warn(e, $"Shutdown error in {module}");
                }
            }

            _uiManager?.Dispose();
            _applicationDIContext?.Dispose();
        }
    }
}