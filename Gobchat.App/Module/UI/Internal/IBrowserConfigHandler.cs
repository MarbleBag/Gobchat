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

using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public interface IBrowserConfigHandler
    {
        Task<JToken> GetConfigAsJson();

        Task SynchronizeConfig(JToken configJson);

        Task SetActiveProfile(string profileId);

        Task<JToken> ParseProfile(string file);
    }
}