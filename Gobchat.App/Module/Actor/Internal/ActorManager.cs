﻿/*******************************************************************************
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
using System.Collections.Generic;
using System.Linq;
using Gobchat.Core.Chat;
using Gobchat.Memory.Actor;

namespace Gobchat.Module.Actor.Internal
{
    internal sealed class ActorManager : IActorManager
    {
        public TimeSpan OutdatedTimelimit { get; set; } = TimeSpan.FromSeconds(3);

        private sealed class Data
        {
            public DateTime LastUpdateTime;
            public PlayerCharacter Actor;
        }

        private readonly Dictionary<string, Data> _realm = new Dictionary<string, Data>();
        private readonly Queue<Data> _pendingUpdates = new Queue<Data>();

        public bool IsEnabled { get; set; }

        public int GetPlayerCount()
        {
            lock (_realm)
            {
                return _realm.Count;
            }
        }

        public string[] GetPlayersInArea()
        {
            lock (_realm)
            {
                return _realm.Keys.ToArray();
            }
        }

        public float GetFastDistanceToPlayerWithName(string name)
        {
            if (name == null)
                return 0;

            lock (_realm)
            {
                if (_realm.TryGetValue(name, out var storedData))
                    return storedData.Actor.SquaredDistanceToPlayer;
                return 0;
            }
        }

        public float GetDistanceToPlayerWithName(string name)
        {
            var sqrtDistance = GetFastDistanceToPlayerWithName(name);
            if (sqrtDistance <= 0)
                return sqrtDistance;
            return (float)Math.Sqrt(sqrtDistance);
        }

        internal void AddUpdate(IEnumerable<PlayerCharacter> actors)
        {
            var updateTime = DateTime.Now;

            var updates = actors
                .Where(e => e.Flag != PlayerCharacter.UpdateFlag.Remove)
                .Select(e => new Data() { LastUpdateTime = updateTime, Actor = e });

            lock (_realm)
            {
                foreach (var update in updates)
                    _pendingUpdates.Enqueue(update);
            }
        }

        internal void UpdateManager()
        {
            lock (_realm)
            {
                _realm.Clear();
                foreach (var newData in _pendingUpdates)
                {
                    if (_realm.TryGetValue(newData.Actor.Name, out var oldData))
                    {
                        if (newData.Actor.SquaredDistanceToPlayer < oldData.Actor.SquaredDistanceToPlayer)
                            _realm[newData.Actor.Name] = newData;
                    }
                    else
                    {
                        _realm[newData.Actor.Name] = newData;
                    }
                }
                _pendingUpdates.Clear();
            }
        }
    }
}