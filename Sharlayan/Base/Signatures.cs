// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Signatures.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   Signatures.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan {
    using System.Collections.Generic;

    using Sharlayan.Models;
    using Sharlayan.Utilities;

    public static class Signatures {

        public const string CharacterMapKey = "CHARMAP";

        public const string ChatLogKey = "CHATLOG";

        public static IEnumerable<Signature> Resolve(ProcessModel processModel, string patchVersion = "latest") {
            return APIHelper.GetSignatures(processModel, patchVersion);
        }
    }
}