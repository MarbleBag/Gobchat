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

namespace Gobchat.Memory.Actor
{
    public sealed class ActorDistance
    {
        public int SimplifiedDistanceToPlayer { get; internal set; }
        public float SquaredDistanceToPlayer { get; internal set; }
        public float DistanceToPlayer { get; internal set; }
    }

    public sealed class PlayerCharacter
    {
        public enum UpdateFlag
        {
            New,
            Remove,
            Update
        }

        // public ActorPosition Position { get; internal set; }

        // public ActorDistance ActorDistance { get; internal set; }

        public int SimplifiedDistanceToPlayer { get; internal set; }

        public float DistanceToPlayer { get; internal set; }

        //public float DistanceToPlayer { get; internal set; }

        public string Name { get; internal set; }

        public uint Id { get; internal set; }

        public string UId { get; internal set; }

        public UpdateFlag Flag { get; internal set; }
        public bool IsUser { get; internal set; }

        public PlayerCharacter()
        {
        }

        public override string ToString()
        {
            return $"{nameof(PlayerCharacter)} => {nameof(Name)}:{Name} | {nameof(Id)}:{Id} | {nameof(UId)}:{UId} | {nameof(Flag)}:{Flag}";
        }
    }
}