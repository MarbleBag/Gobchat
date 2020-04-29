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
    internal sealed class PlayerLocationMemoryReader
    {
        public bool LocationAvailable { get { return Sharlayan.Reader.CanGetActors(); } }

        private bool TryProcess(Sharlayan.Core.ActorItem actor, PlayerCharacter.UpdateFlag flag, out PlayerCharacter data)
        {
            data = null;

            if (!(actor.IsValid && actor.IsTargetable))
                return false;

            var name = actor.Name;
            var id = actor.ID;
            var uid = actor.UUID;
            // var pos = new ActorPosition(entry.X, entry.Y, entry.Z);
            var distance = (int)actor.Distance;

            data = new PlayerCharacter(name, id, uid, distance, flag);

            return true;
        }

        private void Process(ICollection<Sharlayan.Core.ActorItem> actors, PlayerCharacter.UpdateFlag flag, ICollection<PlayerCharacter> results)
        {
            foreach (var entry in actors)
                if (TryProcess(entry, flag, out var data))
                    results.Add(data);
        }

        public List<PlayerCharacter> GetPlayerData()
        {
            var result = new List<PlayerCharacter>();
            var memoryResult = Sharlayan.Reader.GetActors();

            Process(memoryResult.CurrentPCs.Values, PlayerCharacter.UpdateFlag.Update, result);
            Process(memoryResult.RemovedPCs.Values, PlayerCharacter.UpdateFlag.Remove, result);
            Process(memoryResult.NewPCs.Values, PlayerCharacter.UpdateFlag.New, result);

            return result;
        }
    }
}