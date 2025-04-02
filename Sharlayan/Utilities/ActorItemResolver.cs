// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorItemResolver.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ActorItemResolver.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Utilities {
    using System;

    using NLog;

    using Sharlayan.Core;
    using Sharlayan.Core.Enums;
    using Sharlayan.Delegates;
    using System.Linq;
    using System.Collections.Generic;

    internal static class ActorItemResolver {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ActorItem ResolveActorFromBytes(byte[] source, bool isCurrentUser = false, ActorItem entry = null)
        {
            entry = entry ?? new ActorItem();
            var defaultBaseOffset = MemoryHandler.Instance.Structures.ActorItem.DefaultBaseOffset;

            try {

                entry.Name = MemoryHandler.Instance.GetStringFromBytes(source, MemoryHandler.Instance.Structures.ActorItem.Name);
                entry.EntityId = BitConverter.TryToUInt32(source, MemoryHandler.Instance.Structures.ActorItem.EntityId);
                entry.UUID = string.IsNullOrEmpty(entry.UUID)
                                 ? Guid.NewGuid().ToString()
                                 : entry.UUID;

                entry.LayoutId = BitConverter.TryToUInt32(source, MemoryHandler.Instance.Structures.ActorItem.LayoutId);
                entry.BaseId = BitConverter.TryToUInt32(source, MemoryHandler.Instance.Structures.ActorItem.BaseId);
                entry.OwnerId = BitConverter.TryToUInt32(source, MemoryHandler.Instance.Structures.ActorItem.OwnerId);

                entry.TypeId = source[MemoryHandler.Instance.Structures.ActorItem.Type];
                entry.Type = (Actor.Type) entry.TypeId;
                entry.SubType = source[MemoryHandler.Instance.Structures.ActorItem.SubType];


                entry.SexId = source[MemoryHandler.Instance.Structures.ActorItem.Gender];
                entry.Sex = (Actor.Sex)entry.SexId;

                entry.YalmFromPlayerX = source[MemoryHandler.Instance.Structures.ActorItem.YalmFromPlayerX];
                entry.YalmFromPlayerZ = source[MemoryHandler.Instance.Structures.ActorItem.YalmFromPlayerZ];


                entry.PositionX = BitConverter.TryToSingle(source, MemoryHandler.Instance.Structures.ActorItem.PositionX + defaultBaseOffset);
                entry.PositionZ = BitConverter.TryToSingle(source, MemoryHandler.Instance.Structures.ActorItem.PositionZ + defaultBaseOffset);
                entry.PositionY = BitConverter.TryToSingle(source, MemoryHandler.Instance.Structures.ActorItem.PositionY + defaultBaseOffset);
                entry.Rotation = BitConverter.TryToSingle(source, MemoryHandler.Instance.Structures.ActorItem.Rotation + defaultBaseOffset);
                entry.HitBoxRadius = BitConverter.TryToSingle(source, MemoryHandler.Instance.Structures.ActorItem.HitBoxRadius + defaultBaseOffset);
                entry.Coordinate = new Coordinate(entry.PositionX, entry.PositionZ, entry.PositionY);
            
            }
            catch (Exception ex)
            {
                MemoryHandler.Instance.RaiseException(Logger, ex, true);
            }

            if (isCurrentUser)
                PCWorkerDelegate.CurrentUser = entry;

            return entry;
        }
    }
}
