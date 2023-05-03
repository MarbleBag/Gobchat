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

namespace Gobchat.Core.Config
{
    internal sealed class ConfigUpgrade_1_9_0 : IConfigUpgrade
    {
        public int MinVersion => 1800;

        public int MaxVersion => 1899;

        public int TargetVersion => 1900;

        public JObject Upgrade(JObject src)
        {
            JObject dst = (JObject)src.DeepClone();

            JsonUtil.MoveIfAvailable(dst, "behaviour.writeChatLog", dst, "behaviour.chatlog.active");

            return dst;
        }
    }
}