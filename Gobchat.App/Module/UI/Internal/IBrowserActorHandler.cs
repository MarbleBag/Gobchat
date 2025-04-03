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

using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public interface IBrowserActorHandler
    {
        Task<bool> IsFeatureAvailable();

        Task<int> GetPlayerNearbyCount();

        Task<string[]> GetPlayersNearby();

        Task<float> GetDistanceToPlayer(string name);
    }
}