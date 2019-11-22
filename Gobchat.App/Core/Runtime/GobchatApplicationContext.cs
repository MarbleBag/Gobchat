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
        private List<IApplicationComponent> _activeApplicationComponents;

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
            _activeApplicationComponents = new List<IApplicationComponent>();
            _applicationDIContext = new DIContext();
            _uiManager = new UIManager();

            _applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.ResourceLocation, nameof(ResourceLocation));
            _applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.UserConfigLocation, nameof(UserConfigLocation));
            _applicationDIContext.Register<string>((c, _) => GobchatApplicationContext.ApplicationLocation, nameof(ApplicationLocation));
            _applicationDIContext.Register<System.Version>((c, _) => GobchatApplicationContext.ApplicationVersion, nameof(ApplicationVersion));

            _applicationDIContext.Register<IUISynchronizer>((c, _) => GobchatApplicationContext.UISynchronizer);
            _applicationDIContext.Register<IUIManager>((c, _) => _uiManager);

            //TODO move all that stuff to a component oriented architecture

            var applicationComponents = new List<IApplicationComponent>()
            {
                new ApplicationConfigComponent(),
                new ApplicationUpdateComponent(),
                new ApplicationCefInstallerComponent(),
                new ApplicationCefOverlayComponent(),
                new ApplicationNotifyIconComponent(),
                new ApplicationChatComponent()
            };

            logger.Info(() => $"Initialize Gobchat v{GobchatApplicationContext.ApplicationVersion} on {(Environment.Is64BitProcess ? "x64" : "x86")}");

            var startupHandler = new ApplicationStartupHandler();
            foreach (var component in applicationComponents)
            {
                try
                {
                    _activeApplicationComponents.Add(component);
                    logger.Info($"Starting: {component}");
                    component.Initialize(startupHandler, _applicationDIContext);
                }
                catch (System.Exception e)
                {
                    logger.Fatal($"Initialization error in {component}");
                    logger.Fatal(e);
                    startupHandler.StopStartup = true;
                }

                if (startupHandler.StopStartup)
                {
                    logger.Fatal("Shutdown in initialization phase");
                    Application.Exit();
                    return;
                }
            }

            logger.Info("Initialization complete");
        }

        internal override async void ApplicationShutdownProcess()
        {
            logger.Info("Gobchat shutdown");

            //components are deactivated in reverse
            var deactivationSequence = _activeApplicationComponents.Reverse<IApplicationComponent>().ToList();
            foreach (var component in deactivationSequence)
            {
                try
                {
                    logger.Info($"Shutdown: {component}");
                    component.Dispose(_applicationDIContext);
                }
                catch (System.Exception e)
                {
                    //that's the best you get, no one cares for you - for now.
                    logger.Warn($"Shutdown error in {component}");
                    logger.Warn(e);
                }
            }
            _activeApplicationComponents.Clear();

            _applicationDIContext?.Dispose();
        }
    }
}