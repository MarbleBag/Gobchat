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

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gobchat.Core.Config
{
    internal sealed class LegacyAppConfigTransformer : IJsonFunction
    {
        private IConfigManager _manager;

        public LegacyAppConfigTransformer(IConfigManager manager)
        {
            _manager = manager;
        }

        public JObject Apply(JObject json)
        {
            var profileId = _manager.CreateNewProfile();
            var profile = _manager.GetProfile(profileId);

            var loader = new JsonConfigLoader();
            var finalizer = new JsonValueToEnum();
            json = loader.LoadConfig(json);
            json = finalizer.Apply(json);

            profile.SetProperties(json);
            profile.SetProperty("profile.name", "Profile 1");

            var visibleChannels = profile.GetProperty<List<int>>("behaviour.channel.visible");
            visibleChannels.Add((int)0x01FFFF /*GOBCHAT_INFO*/);
            visibleChannels.Add((int)0x02FFFF /*GOBCHAT_ERROR*/);
            profile.SetProperty("behaviour.channel.visible", visibleChannels);

            var result = new JObject();
            result["version"] = 2;
            result["activeProfile"] = profileId;
            return result;
        }
    }
}