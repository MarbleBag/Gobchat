// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructuresContainer.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   StructuresContainer.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Models.Structures {
    public class StructuresContainer {
        public ActorItem ActorItem { get; set; } = new ActorItem();

        public ChatLogPointers ChatLogPointers { get; set; } = new ChatLogPointers();

        public CurrentPlayer CurrentPlayer { get; set; } = new CurrentPlayer();

        public StatusItem StatusItem { get; set; } = new StatusItem();

        public TargetInfo TargetInfo { get; set; } = new TargetInfo();
    }
}