// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorItem.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ActorItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Core {
    using Sharlayan.Core.Enums;
    using Sharlayan.Core.Interfaces;
    using Sharlayan.Delegates;

    public class ActorItem : ActorItemBase {
        public static ActorItem CurrentUser => PCWorkerDelegate.CurrentUser;

        public byte YalmFromPlayerX { get; set; }
        public byte YalmFromPlayerZ { get; set; }
        public float Rotation { get; set; }

        public uint LayoutId { get; set; }

        public uint BaseId { get; set; }

        public uint OwnerId { get; set; }

        public byte Race { get; set; }

        public Actor.Sex Sex { get; set; }

        public byte SexId { get; set; }

        public Actor.Type Type { get; set; }

        public byte TypeId { get; set; }

        public byte SubType { get; set; }

        public bool IsValid
        {
            get
            {
                switch (this.Type)
                {
                    case Actor.Type.NPC:
                        return this.EntityId != 0 && (this.LayoutId != 0 || this.BaseId != 0);
                    default:
                        return this.EntityId != 0;
                }
            }
        }

        public ActorItem Clone() {
            var cloned = (ActorItem) this.MemberwiseClone();

            cloned.Coordinate = new Coordinate(this.Coordinate.X, this.Coordinate.Z, this.Coordinate.Y);
            cloned.EnmityItems = new System.Collections.Generic.List<EnmityItem>();
            cloned.StatusItems = new System.Collections.Generic.List<StatusItem>();

            foreach (EnmityItem item in this.EnmityItems) {
                cloned.EnmityItems.Add(
                    new EnmityItem {
                        Enmity = item.Enmity,
                        ID = item.ID,
                        Name = item.Name
                    });
            }

            foreach (StatusItem item in this.StatusItems) {
                cloned.StatusItems.Add(
                    new StatusItem {
                        CasterID = item.CasterID,
                        Duration = item.Duration,
                        IsCompanyAction = item.IsCompanyAction,
                        Stacks = item.Stacks,
                        StatusID = item.StatusID,
                        StatusName = item.StatusName,
                        TargetName = item.TargetName
                    });
            }

            return cloned;
        }

    }
}
