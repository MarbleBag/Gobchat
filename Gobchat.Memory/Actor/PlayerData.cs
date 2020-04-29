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

namespace Gobchat.Memory.Actor
{
    public sealed class PlayerCharacter
    {
        public enum UpdateFlag
        {
            New,
            Remove,
            Update
        }

        // public ActorPosition Position { get; }

        public int DistanceToPlayer { get; }

        public string Name { get; }

        public string Id { get; }

        public string UId { get; }

        public UpdateFlag Flag { get; }

        public PlayerCharacter(string name, uint id, string uid, int distance, UpdateFlag flag)
        {
            Name = name;
            Id = id.ToString();
            UId = uid;
            DistanceToPlayer = distance;
            Flag = flag;
        }
    }
}