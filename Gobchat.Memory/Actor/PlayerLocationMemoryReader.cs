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

using Sharlayan;
using Sharlayan.Core;
using System;
using System.Collections.Generic;

namespace Gobchat.Memory.Actor
{
    internal sealed class PlayerLocationMemoryReader
    {
        public bool LocationAvailable { get { return Sharlayan.Reader.CanGetActors() && MemoryHandler.Instance.IsAttached; } }

        private void Process(ICollection<Sharlayan.Core.ActorItem> actors, PlayerCharacter.UpdateFlag flag, Coordinate mainActorPosition, ICollection<PlayerCharacter> results)
        {
            foreach (var actor in actors)
            {
                if (!(actor.IsValid && actor.Type == Sharlayan.Core.Enums.Actor.Type.PC))
                    continue;

                var data = new PlayerCharacter()
                {
                    Name = actor.Name,
                    Id = actor.EntityId,
                    UId = actor.UUID,
                    Flag = flag,
                    SimplifiedDistanceToPlayer = actor.YalmFromPlayerX,
                };
                
                if (mainActorPosition != null)
                    data.DistanceToPlayer = mainActorPosition.DistanceTo(actor.Coordinate);

                results.Add(data);
            }
        }

        private Sharlayan.Core.Coordinate GetActivePlayerPosition()
        {
            var currentUser = Sharlayan.Core.ActorItem.CurrentUser;
            if (currentUser != null && currentUser.IsValid)
                return currentUser.Coordinate; // new Vector3(currentUser.PositionX, currentUser.PositionY, currentUser.PositionZ);
            return null;
        }

        private void MarkActivePlayer(List<PlayerCharacter> characters)
        {
            var currentUser = Sharlayan.Core.ActorItem.CurrentUser;
            if (currentUser == null)
                return;

            foreach (var character in characters)
            {
                if (character.Id == currentUser.EntityId)
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