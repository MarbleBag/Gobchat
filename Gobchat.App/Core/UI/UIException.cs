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

namespace Gobchat.Core.UI
{
    [Serializable]
    public class UIException : System.Exception
    {
        public UIException()
        {
        }

        public UIException(string message) : base(message)
        {
        }

        public UIException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UIException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
