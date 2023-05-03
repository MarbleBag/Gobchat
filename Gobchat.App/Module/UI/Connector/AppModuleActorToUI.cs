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

using Gobchat.Core.Runtime;
using Gobchat.Module.Actor;
using Gobchat.Module.MemoryReader;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleActorToUI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IBrowserAPIManager _browserAPIManager;

        /// <summary>
        /// Requires: <see cref="IBrowserAPIManager"/> <br></br>
        /// Requires: <see cref="IMemoryReaderManager"/> <br></br>
        /// Requires: <see cref="IActorManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleActorToUI()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _browserAPIManager = _container.Resolve<IBrowserAPIManager>();
            _browserAPIManager.ActorHandler = new ActorHandler(container);
        }

        public void Dispose()
        {
            _browserAPIManager.ActorHandler = null;
            _browserAPIManager = null;
            _container = null;
        }

        private sealed class ActorHandler : IBrowserActorHandler
        {
            private IActorManager _actorManager;
            private IMemoryReaderManager _memoryManager;

            public ActorHandler(IDIContext container)
            {
                _actorManager = container.Resolve<IActorManager>();
                _memoryManager = container.Resolve<IMemoryReaderManager>();
            }

            public async Task<float> GetDistanceToPlayer(string name)
            {
                return _actorManager.GetDistanceToPlayerWithName(name);
            }

            public async Task<int> GetPlayerNearbyCount()
            {
                return _actorManager.GetPlayerCount();
            }

            public async Task<string[]> GetPlayersNearby()
            {
                return _actorManager.GetPlayersInArea();
            }

            public async Task<bool> IsFeatureAvailable()
            {
                return _memoryManager.PlayerCharactersAvailable;
            }
        }
    }
}