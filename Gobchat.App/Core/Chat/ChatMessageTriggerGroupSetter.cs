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

using Gobchat.Core.Util.Extension;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageTriggerGroupSetter
    {
        private TriggerGroup[] _groups;

        public TriggerGroup[] Groups
        {
            get => _groups.ToArray();
            set => _groups = value.ToArrayOrEmpty();
        }

        public void SetTriggerGroup(ChatMessage chatMessage)
        {
            chatMessage.Source.TriggerGroupId = FindFirstTriggerGroup(chatMessage);
        }

        private string FindFirstTriggerGroup(ChatMessage message)
        {
            if (message.Source == null || message.Source.Original == null)
                return null;

            switch (message.Channel)
            {
                case ChatChannel.TellRecieve:
                case ChatChannel.TellSend:
                    return null;
            }

            var searchName = message.Source.CharacterName != null ? message.Source.CharacterName : message.Source.Original;
            searchName = searchName.ToLowerInvariant();

            foreach (var group in _groups)
            {
                if (!group.Active)
                    continue;

                if (group.FFGroup.HasValue)
                {
                    if (group.FFGroup.Value == message.Source.FfGroup)
                        return group.Id;
                    continue;
                }

                if (group.Trigger.Contains(searchName))
                    return group.Id;
            }

            return null;
        }
    }

    public sealed class TriggerGroup
    {
        public bool Active { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? FFGroup { get; set; }

        public string Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Trigger { get; set; }
    }
}