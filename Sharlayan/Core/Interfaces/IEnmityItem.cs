﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEnmityItem.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   IEnmityItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Core.Interfaces {
    public interface IEnmityItem {
        uint Enmity { get; set; }

        uint ID { get; set; }

        string Name { get; set; }
    }
}