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

namespace Gobchat.Core.Config
{
    [Serializable]
    public sealed class SynchronizationException : ConfigException
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(String message) : base(message)
        {
        }

        public SynchronizationException(String message, System.Exception innerException) : base(message, innerException)
        {
        }

        public SynchronizationException(System.Exception innerException) : base(String.Empty, innerException)
        {
        }

        protected SynchronizationException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}