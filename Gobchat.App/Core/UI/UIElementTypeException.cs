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

namespace Gobchat.Core.UI
{
    [System.Serializable]
    public sealed class UIElementTypeException : UIException
    {
        private const string ERROR_MESSAGE = "Expected type {0} but was {1}";

        public UIElementTypeException(System.Type expected, System.Type actual)
            : base(string.Format(System.Globalization.CultureInfo.InvariantCulture, ERROR_MESSAGE, expected.FullName, actual.FullName))
        {
        }

        private UIElementTypeException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
