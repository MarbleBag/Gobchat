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

using Gobchat.Memory;
using Gobchat.Memory.Actor;
using Gobchat.Memory.Chat;
using System;
using System.Collections.Generic;

namespace Gobchat.Module.MemoryReader
{
    internal interface IMemoryReaderManager
    {
        ConnectionState ConnectionState { get; }

        bool IsConnected { get; }

        int ConnectedProcessId { get; }

        bool ChatLogAvailable { get; }

        bool PlayerCharactersAvailable { get; }

        bool ObserveGameWindow { get; set; }

        event EventHandler<WindowFocusChangedEventArgs> OnWindowFocusChanged;

        event EventHandler<ConnectionEventArgs> OnConnectionStateChanged;

        List<int> GetFFXIVProcesses();

        List<PlayerCharacter> GetPlayerCharacters();

        List<ChatlogItem> GetNewestChatlog();
    }

    public enum ConnectionState
    {
        NotInitialized,
        Connected,
        NotFound,
        Searching
    }

    public sealed class ConnectionEventArgs : EventArgs
    {
        public ConnectionState State { get; }

        public bool IsConnected { get => State == ConnectionState.Connected; }

        public ConnectionEventArgs(ConnectionState state)
        {
            State = state;
        }
    }
}