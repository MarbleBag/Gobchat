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
using System.Collections.Generic;

namespace Gobchat.Memory.Actor
{
    internal sealed class PlayerLocationProcessor
    {
        public bool Enabled;

        public bool LocationAvailable { get { return Sharlayan.Reader.CanGetActors(); } }

        public event EventHandler<PlayerEventArgs> PlayerEvent;

        private bool TryProcess(Sharlayan.Core.ActorItem actor, PlayerData.UpdateFlag flag, out PlayerData data)
        {
            data = null;

            if (!(actor.IsValid && actor.IsTargetable))
                return false;

            var name = actor.Name;
            var id = actor.ID;
            var uid = actor.UUID;
            // var pos = new ActorPosition(entry.X, entry.Y, entry.Z);
            var distance = (int)actor.Distance;

            data = new PlayerData(name, id, uid, distance, flag);

            return true;
        }

        private void Process(ICollection<Sharlayan.Core.ActorItem> actors, PlayerData.UpdateFlag flag, ICollection<PlayerData> results)
        {
            foreach (var entry in actors)
                if (TryProcess(entry, flag, out var data))
                    results.Add(data);
        }

        public void Update()
        {
            if (!Enabled && PlayerEvent != null)
                return;

            var result = new List<PlayerData>();
            var memoryResult = Sharlayan.Reader.GetActors();

            Process(memoryResult.CurrentPCs.Values, PlayerData.UpdateFlag.Update, result);
            Process(memoryResult.RemovedPCs.Values, PlayerData.UpdateFlag.Remove, result);
            Process(memoryResult.NewPCs.Values, PlayerData.UpdateFlag.New, result);

            PlayerEvent?.Invoke(this, new PlayerEventArgs(result));
        }
    }
}