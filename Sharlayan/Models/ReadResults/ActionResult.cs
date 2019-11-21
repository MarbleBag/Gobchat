﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionResult.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   ActionResult.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Models.ReadResults {
    using System.Collections.Generic;

    using Sharlayan.Core;

    public class ActionResult {
        public List<ActionContainer> ActionContainers { get; } = new List<ActionContainer>();
    }
}