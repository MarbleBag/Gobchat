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

using Gobchat.Module.MemoryReader;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public interface IBrowserMemoryHandler
    {
        Task<int[]> GetAttachableFFXIVProcesses();

        Task<(ConnectionState state, int id)> GetAttachedFFXIVProcess();

        Task<bool> AttachToFFXIVProcess(int id);
    }
}