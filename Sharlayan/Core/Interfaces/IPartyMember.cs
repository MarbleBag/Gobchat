﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPartyMember.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   IPartyMember.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Core.Interfaces {
    using System.Collections.Generic;

    using Sharlayan.Core.Enums;

    public interface IPartyMember {
        int HPCurrent { get; set; }

        int HPMax { get; set; }

        uint ID { get; set; }

        Actor.Job Job { get; set; }

        byte JobID { get; set; }

        byte Level { get; set; }

        int MPCurrent { get; set; }

        int MPMax { get; set; }

        string Name { get; set; }

        List<StatusItem> StatusItems { get; }

        string UUID { get; set; }

        float X { get; set; }

        float Y { get; set; }

        float Z { get; set; }

        PartyMember Clone();
    }
}