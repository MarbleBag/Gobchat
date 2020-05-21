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
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using System.Threading;
using Gobchat.Module.Actor.Internal;
using Gobchat.Module.MemoryReader;

namespace Gobchat.Module.Actor
{
    public sealed class AppModuleActorManager : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;
        private IMemoryReaderManager _memoryManager;
        private ActorManager _actorManager;
        private IndependendBackgroundWorker _updater;
        private long _updateInterval;

        /// <summary>
        /// Adds an <see cref="IActorManager"/> to the app context and supplies it with constant updates by querying a <see cref="IMemoryReaderManager"/>
        /// <br></br>
        /// <br></br>
        /// Requires: <see cref="IGobchatConfig"/> <br></br>
        /// Requires: <see cref="IMemoryReaderManager"/> <br></br>
        /// <br></br>
        /// Provides: <see cref="IActorManager"/> <br></br>
        /// </summary>
        public AppModuleActorManager()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _configManager = _container.Resolve<IConfigManager>();
            _memoryManager = _container.Resolve<IMemoryReaderManager>();

            _actorManager = new ActorManager();
            _updater = new IndependendBackgroundWorker();

            _configManager.AddPropertyChangeListener("behaviour.actor.updateInterval", true, true, ConfigManager_UpdateChatInterval);
            _configManager.AddPropertyChangeListener("behaviour.actor.active", true, true, ConfigManager_UpdateRangeFilter);

            _container.Register<IActorManager>((c, p) => _actorManager);
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateChatInterval);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateRangeFilter);

            _updater.Dispose();

            _updater = null;
            _actorManager = null;
            _container = null;
            _configManager = null;
            _memoryManager = null;
        }

        private void UpdateJob(CancellationToken cancellationToken)
        {
            //TODO some start up logging
            try
            {
                var timer = new System.Diagnostics.Stopwatch();
                while (!cancellationToken.IsCancellationRequested)
                {
                    timer.Restart();

                    UpdateManager();

                    timer.Stop();
                    var timeSpend = timer.Elapsed;

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    int waitTime = (int)Math.Max(0, _updateInterval - timeSpend.Milliseconds);
                    if (waitTime > 0)
                        Thread.Sleep(waitTime);
                }
            }
            finally
            {
                _actorManager.UpdateManager();
                _actorManager.IsAvailable = false;
                logger.Info("Actor updates concluded");
            }
        }

        private void UpdateManager()
        {
            _actorManager.IsAvailable = _memoryManager.PlayerCharactersAvailable;

            if (_memoryManager.IsConnected)
            {
                var characterData = _memoryManager.GetPlayerCharacters();
                _actorManager.AddUpdate(characterData);
            }

            _actorManager.UpdateManager();
        }

        private void ConfigManager_UpdateChatInterval(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _updateInterval = config.GetProperty<long>("behaviour.actor.updateInterval");
        }

        private void ConfigManager_UpdateRangeFilter(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var runManager = config.GetProperty<bool>("behaviour.actor.active");
            if (runManager)
            {
                if (_updater.IsRunning)
                    return;
                else
                    _updater.Start(UpdateJob);
            }
            else
            {
                _updater.Stop(false);
            }
        }
    }
}