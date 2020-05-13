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
using Gobchat.Memory;
using System.Threading;
using Gobchat.Module.Actor.Internal;

namespace Gobchat.Module.Actor
{
    public sealed class AppModuleActorManager : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;

        private FFXIVMemoryReader _memoryReader;
        private ActorManager _actorManager;

        private IndependendBackgroundWorker _updater;

        private long _updateInterval;

        /// <summary>
        ///
        /// Requires: <see cref="IGobchatConfig"/> <br></br>
        /// Requires: <see cref="FFXIVMemoryReader"/> <br></br>
        /// Provides: <see cref="IActorManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleActorManager()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _configManager = _container.Resolve<IConfigManager>();
            _memoryReader = _container.Resolve<FFXIVMemoryReader>();

            _actorManager = new ActorManager();
            _updater = new IndependendBackgroundWorker();

            _memoryReader.OnProcessChanged += MemoryReader_OnProcessChanged;

            _configManager.AddPropertyChangeListener("behaviour.chatUpdateInterval", true, true, ConfigManager_UpdateChatInterval);
            _configManager.AddPropertyChangeListener("behaviour.fadeout.active", true, true, ConfigManager_UpdateRangeFilter);

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
            _memoryReader = null;
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
                logger.Info("Actor updates concluded");
            }
        }

        private void UpdateManager()
        {
            if (_memoryReader.FFXIVProcessValid)
            {
                var characterData = _memoryReader.GetPlayerCharacters();
                _actorManager.AddUpdate(characterData);
            }

            _actorManager.UpdateManager();
        }

        private void MemoryReader_OnProcessChanged(object sender, ProcessChangeEventArgs e)
        {
            _actorManager.IsAvailable = _memoryReader.PlayerCharactersAvailable;
        }

        private void ConfigManager_UpdateChatInterval(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _updateInterval = config.GetProperty<long>("behaviour.chatUpdateInterval");
        }

        private void ConfigManager_UpdateRangeFilter(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var runManager = config.GetProperty<bool>("behaviour.fadeout.active");
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