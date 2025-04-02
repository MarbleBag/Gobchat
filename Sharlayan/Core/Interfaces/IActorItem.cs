// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActorItem.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   IActorItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Core.Interfaces {
    using System.Collections.Generic;

    using Sharlayan.Core.Enums;

    public interface IActorItem {
        Actor.ActionStatus ActionStatus { get; set; }

        byte ActionStatusID { get; set; }

        byte AgroFlags { get; set; }

        short CastingID { get; set; }

        float CastingProgress { get; set; }

        uint CastingTargetID { get; set; }

        float CastingTime { get; set; }

        uint ClaimedByID { get; set; }

        byte CombatFlags { get; set; }

        short CPCurrent { get; set; }

        short CPMax { get; set; }

        byte DifficultyRank { get; set; }

        byte YalmFromPlayerX { get; set; }

        byte TargetStatus { get; set; }

        byte YalmFromPlayerZ { get; set; }

        Actor.EventObjectType EventObjectType { get; set; }

        ushort EventObjectTypeID { get; set; }

        uint Fate { get; set; }

        byte GatheringInvisible { get; set; }

        byte GatheringStatus { get; set; }

        short GPCurrent { get; set; }

        short GPMax { get; set; }

        byte GrandCompany { get; set; }

        byte GrandCompanyRank { get; set; }

        float Rotation { get; set; }

        float HitBoxRadius { get; set; }

        int HPCurrent { get; set; }

        int HPMax { get; set; }

        Actor.Icon Icon { get; set; }

        byte IconID { get; set; }

        uint EntityId { get; set; }

        bool InCombat { get; }

        bool IsAggressive { get; }

        bool IsCasting { get; }

        bool IsGM { get; set; }

        Actor.Job Job { get; set; }

        byte JobID { get; set; }

        byte Level { get; set; }

        uint MapID { get; set; }

        uint MapIndex { get; set; }

        uint MapTerritory { get; set; }

        uint ModelID { get; set; }

        int MPCurrent { get; set; }

        int MPMax { get; set; }

        string Name { get; set; }

        uint LayoutId { get; set; }

        uint BaseId { get; set; }

        uint OwnerId { get; set; }

        byte Race { get; set; }

        Actor.Sex Sex { get; set; }

        byte SexID { get; set; }

        Actor.Status Status { get; set; }

        byte StatusID { get; set; }

        List<StatusItem> StatusItems { get; }

        byte TargetFlags { get; set; }

        int TargetID { get; set; }

        Actor.TargetType TargetType { get; set; }

        byte TargetTypeID { get; set; }

        byte Title { get; set; }

        int TPCurrent { get; set; }

        Actor.Type Type { get; set; }

        byte TypeId { get; set; }

        string UUID { get; set; }

        bool WeaponUnsheathed { get; }

        float PositionX { get; set; }

        float PositionY { get; set; }

        float PositionZ { get; set; }

        ActorItem Clone();
    }
}