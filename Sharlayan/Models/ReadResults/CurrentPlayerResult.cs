﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentPlayerResult.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   CurrentPlayerResult.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Models.ReadResults {
    using Sharlayan.Core;

    public class CurrentPlayerResult {
        public CurrentPlayer CurrentPlayer { get; set; } = new CurrentPlayer();
    }
}