/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using System;
using System.Linq;

namespace Gobchat.Core.Config
{
    public sealed class ProfilePropertyChangedEventArgs : EventArgs
    {
        public string PropertyKey { get; }
        public string ProfileId { get; }
        public bool IsActiveProfile { get; }
        public bool Synchronizing { get; }

        public ProfilePropertyChangedEventArgs(string propertyKey, string profileId, bool isActiveProfile, bool synchronizing)
        {
            PropertyKey = propertyKey;
            ProfileId = profileId;
            IsActiveProfile = isActiveProfile;
            Synchronizing = synchronizing;
        }
    }

    public sealed class ProfilePropertyChangedCollectionEventArgs : EventArgs
    {
        public System.Collections.Generic.IEnumerable<ProfilePropertyChangedEventArgs> Changed { get; }

        public bool Synchronizing { get; }

        public bool IsAnyActiveProfile { get; }

        public ProfilePropertyChangedCollectionEventArgs(System.Collections.Generic.IEnumerable<ProfilePropertyChangedEventArgs> changed)
        {
            Changed = changed ?? throw new ArgumentNullException(nameof(changed));
            foreach (var evt in changed)
            {
                if (Synchronizing && IsAnyActiveProfile)
                    break;

                IsAnyActiveProfile |= evt.IsActiveProfile;
                Synchronizing |= evt.Synchronizing;
            }
        }
    }
}