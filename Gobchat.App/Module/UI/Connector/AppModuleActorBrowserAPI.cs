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

using Gobchat.Core.Runtime;
using Gobchat.Module.Actor;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleActorBrowserAPI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IBrowserAPIManager _browserAPIManager;
        private IActorManager _actorManager;

        /// <summary>
        /// Requires: <see cref="IBrowserAPIManager"/> <br></br>
        /// Requires: <see cref="IActorManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleActorBrowserAPI()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _browserAPIManager = _container.Resolve<IBrowserAPIManager>();
            _actorManager = _container.Resolve<IActorManager>();

            _browserAPIManager.ActorHandler = new ActorHandler(_actorManager);
        }

        public void Dispose()
        {
            _browserAPIManager.ActorHandler = null;
            _browserAPIManager = null;
            _actorManager = null;
            _container = null;
        }

        private sealed class ActorHandler : IBrowserActorHandler
        {
            private IActorManager _actorManager;

            public ActorHandler(IActorManager actorManager)
            {
                _actorManager = actorManager;
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

            public async Task<bool> IsAvailable()
            {
                return _actorManager.IsAvailable;
            }
        }
    }
}