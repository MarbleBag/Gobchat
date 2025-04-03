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
using System.Runtime.Serialization;

namespace Gobchat.Core.Util
{
    [Serializable]
    public sealed class DownloadFailedException : Exception
    {
        public DownloadFailedException()
        {
        }

        public DownloadFailedException(string message) : base(message)
        {
        }

        public DownloadFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private DownloadFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}