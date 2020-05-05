/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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
    public sealed class ExtractionFailedException : Exception
    {
        public ExtractionFailedException()
        {
        }

        public ExtractionFailedException(string message) : base(message)
        {
        }

        public ExtractionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private ExtractionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}