﻿/*******************************************************************************
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
    public class ConfigException : System.Exception
    {
        public ConfigException()
        {
        }

        public ConfigException(String message) : base(message)
        {
        }

        public ConfigException(String message, System.Exception innerException) : base(message, innerException)
        {
        }

        public ConfigException(System.Exception innerException) : base(String.Empty, innerException)
        {
        }

        protected ConfigException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}