/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
    [Serializable]
    public sealed class InvalidPropertyPathException : PropertyException
    {
        private readonly string _reason;
        private readonly string _propertyPath;

        public InvalidPropertyPathException(string reason, string propertyPath) : base($"{reason} => {propertyPath}")
        {
            _reason = reason;
            _propertyPath = propertyPath;
        }

        private InvalidPropertyPathException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}