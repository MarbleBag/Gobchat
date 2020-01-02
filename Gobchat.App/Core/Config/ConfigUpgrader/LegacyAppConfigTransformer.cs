/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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

namespace Gobchat.Core.Config
{
    internal sealed class LegacyAppConfigTransformer : IJsonTransformer
    {
        private IGobchatConfigManager _manager;

        public LegacyAppConfigTransformer(IGobchatConfigManager manager)
        {
            _manager = manager;
        }

        public JObject Transform(JObject json)
        {
            var profileId = _manager.CreateNewProfile();
            var profile = _manager.GetProfile(profileId);

            var loader = new JsonConfigLoader();
            var finalizer = new StringToEnumTransformer();
            json = loader.LoadConfig(json);
            json = finalizer.Transform(json);

            profile.SetProperties(json);
            profile.SetProperty("profile.name", "Unnamed");

            var result = new JObject();
            result["version"] = 2;
            result["activeProfile"] = profileId;
            return result;
        }
    }
}