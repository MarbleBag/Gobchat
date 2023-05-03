/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

namespace Gobchat.Core.Config
{
    public sealed class ProfileChangedEventArgs : EventArgs
    {
        public enum Type
        {
            New,
            Delete
        }

        public string ProfileId { get; }
        public Type Action { get; }

        public bool Synchronizing { get; }

        public ProfileChangedEventArgs(string profileId, Type action, bool synchronizing)
        {
            ProfileId = profileId;
            Action = action;
            Synchronizing = synchronizing;
        }
    }
}