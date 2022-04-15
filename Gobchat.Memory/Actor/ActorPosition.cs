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

namespace Gobchat.Memory.Actor
{
    public interface IActorPosition
    {
        float X { get; }
        float Y { get; }
        float Z { get; }

        double DistanceSquared(IActorPosition p);

        float Distance(IActorPosition p);
    }

    public sealed class ActorPosition
    {
        public double X;
        public double Y;
        public double Z;

        public ActorPosition(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public double DistanceSquared(ActorPosition p)
        {
            return (X - p.X) * (X - p.X) + (Y - p.Y) * (Y - p.Y) + (Z - p.Z) * (Z - p.Z);
        }
    }
}