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

namespace Gobchat.Module.Hotkey
{
    [Serializable]
    public sealed class HotkeyRegisterException : HotkeyException
    {
        public int ErrorCode { get; }

        public HotkeyRegisterException(string message) : base(message)
        {
        }

        public HotkeyRegisterException(int errorCode, string hotkey, Exception innerException)
            : base($"Error [{errorCode}] - {hotkey}", innerException)
        {
            ErrorCode = errorCode;
        }

        public HotkeyRegisterException()
        {
        }

        private HotkeyRegisterException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}