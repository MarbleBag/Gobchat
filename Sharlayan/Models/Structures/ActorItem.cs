// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorItem.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ActorItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Sharlayan.Models.Structures {

    public class ActorItem {
        public int SourceSize { get; set; }
        public int EntityCount { get; set; }
        public int DefaultBaseOffset { get; set; }
        public int DefaultStatOffset { get; set; }


        public int Name { get; set; }
        public int EntityId { get; set; }
        public int LayoutId { get; set; }
        public int BaseId { get; set; }
        public int OwnerId { get; set; }

        public int Type { get; set; }
        public int SubType { get; set; }
        public int Gender { get; set; }

        public int YalmFromPlayerX { get; set; } 
        public int YalmFromPlayerZ { get; set; }


        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int PositionZ { get; set; }
        public int Rotation { get; set; }
        public int Scale { get; set; }
        public int Height { get; set; }
        public int VfxScale { get; set; }
        public int HitBoxRadius { get; set; }

    }
}