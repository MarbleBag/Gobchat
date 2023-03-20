/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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

using Sharlayan;
using System;
using System.Collections.Generic;

namespace Gobchat.Memory.Actor
{
    internal sealed class PlayerLocationMemoryReader
    {
        public bool LocationAvailable { get { return Sharlayan.Reader.CanGetActors() && MemoryHandler.Instance.IsAttached; } }

        private void Process(ICollection<Sharlayan.Core.ActorItem> actors, PlayerCharacter.UpdateFlag flag, ActorPosition mainActor, ICollection<PlayerCharacter> results)
        {
            foreach (var actor in actors)
            {
                if (!(actor.IsValid && actor.IsTargetable))
                    continue;

                var data = new PlayerCharacter()
                {
                    Name = actor.Name,
                    Id = actor.ID,
                    UId = actor.UUID,
                    Flag = flag,
                    SimplifiedDistanceToPlayer = actor.DistanceOnXAxisToPlayer,
                };

                if (mainActor != null)
                    data.SquaredDistanceToPlayer = (float)mainActor.DistanceSquared(new ActorPosition(actor.X, actor.Y, actor.Z));

                results.Add(data);
            }
        }

        private ActorPosition GetActivePlayerPosition()
        {
            var currentUser = Sharlayan.Core.ActorItem.CurrentUser;
            if (currentUser != null && currentUser.IsValid)
                return new ActorPosition(currentUser.X, currentUser.Y, currentUser.Z);
            return null;
        }

        private void MarkActivePlayer(List<PlayerCharacter> characters)
        {
            var currentUser = Sharlayan.Core.ActorItem.CurrentUser;
            if (currentUser == null)
                return;

            foreach (var character in characters)
            {
                if (character.Id == currentUser.ID)
                {
                    character.IsUser = true;
                    break;
                }
            }
        }

        public List<PlayerCharacter> GetPlayerCharacters()
        {
            var result = new List<PlayerCharacter>();
            var memoryResult = Sharlayan.Reader.GetActors();

            var activePlayerPosition = GetActivePlayerPosition();
            Process(memoryResult.CurrentPCs.Values, PlayerCharacter.UpdateFlag.Update, activePlayerPosition, result);
            Process(memoryResult.RemovedPCs.Values, PlayerCharacter.UpdateFlag.Remove, activePlayerPosition, result);
            Process(memoryResult.NewPCs.Values, PlayerCharacter.UpdateFlag.New, activePlayerPosition, result);
            MarkActivePlayer(result);

            return result;
        }
    }
}