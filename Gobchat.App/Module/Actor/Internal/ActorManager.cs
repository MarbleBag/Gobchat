﻿/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
        private sealed class Data
        {
            public DateTime LastUpdateTime;
            public PlayerCharacter Actor;
        }

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Data> _realm = new Dictionary<string, Data>();
        private readonly Queue<Data> _pendingUpdates = new Queue<Data>();
        private PlayerCharacter User { get; set; }

        public bool IsAvailable { get; internal set; }
        public TimeSpan OutdatedTimelimit { get; set; } = TimeSpan.FromSeconds(3);

        public int GetPlayerCount()
        {
            lock (_realm)
            {
                return _realm.Count;
            }
        }

        public string GetActivePlayerName()
        {
            lock (_realm)
            {
                if (User == null)
                    return null;
                return User.Name;
            }
        }

        public string[] GetPlayersInArea()
        {
            lock (_realm)
            {
                return _realm.Values.Select(data => data.Actor.Name).ToArray();
            }
        }

        public float GetDistanceToPlayerWithName(string name)
        {
            if (name == null)
                return 0;

            name = ChatUtil.StripServerName(name);

            lock (_realm)
            {
                if (_realm.TryGetValue(name.ToUpperInvariant(), out var storedData))
                    return storedData.Actor.DistanceToPlayer;
                return 0;
            }
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
                PlayerCharacter newUser = null;


                foreach (var newData in _pendingUpdates)
                {
                    var key = newData.Actor.Name.ToUpperInvariant(); // names can have a different capitalization than seen in the chat window!

                    if (_realm.TryGetValue(key, out var oldData))
                    {
                        //if (newData.LastUpdateTime > oldData.LastUpdateTime)
                        if (newData.Actor.DistanceToPlayer < oldData.Actor.DistanceToPlayer)
                            _realm[key] = newData;
                    }
                    else
                    {
                        _realm[key] = newData;
                    }

                    if (newData.Actor.IsUser)
                        newUser = newData.Actor;
                }

                if(newUser != null && (User == null || User.Id != newUser.Id))
                {
                    User = newUser;
                    logger.Debug($"Set active player to '{newUser?.Name}'");
                }

                _pendingUpdates.Clear();
            }
        }
    }
}