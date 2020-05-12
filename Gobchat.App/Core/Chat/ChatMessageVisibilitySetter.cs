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

using System;
using System.Globalization;
using System.Linq;
using Gobchat.Core.Util.Extension;
using Gobchat.Module.Actor;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageVisibilitySetter
    {
        private const int MaxVisibility = 100;

        private float _cutOffDistance;
        private float _fadeOutDistance;

        private ChatChannel[] _channels = Array.Empty<ChatChannel>();

        private readonly IActorManager _actorManager;

        public ChatMessageVisibilitySetter(IActorManager actorManager)
        {
            _actorManager = actorManager ?? throw new ArgumentNullException(nameof(actorManager));
        }

        public float CutOffDistance
        {
            get => _cutOffDistance;
            set
            {
                if (value < 0f) throw new ArgumentException("Must be a value greater than or equal to 0", nameof(CutOffDistance));
                _cutOffDistance = value * value; //use the squared value
            }
        }

        public float FadeOutDistance
        {
            get => _fadeOutDistance;
            set
            {
                if (value < 0f) throw new ArgumentException("Must be a value greater than or equal to 0", nameof(FadeOutDistance));
                _fadeOutDistance = value * value; //use the squared value
            }
        }

        public ChatChannel[] CutOffChannels
        {
            get => _channels.ToArray();
            set => _channels = value.ToArrayOrEmpty();
        }

        private int CalculateVisibility(float distance)
        {
            if (distance > _cutOffDistance) return 0;
            if (distance < _fadeOutDistance) return MaxVisibility;
            var percentage = 1 - (distance - _fadeOutDistance) / (_cutOffDistance - _fadeOutDistance);
            return (int)Math.Round(MaxVisibility * percentage);
        }

        public void SetVisibility(ChatMessage message)
        {
            if (!_channels.Contains(message.Channel))
                return;

            var characterName = message.Source.CharacterName;
            if (characterName == null)
                return;

            var serverIdx = characterName.IndexOf("[", StringComparison.InvariantCultureIgnoreCase);
            if (serverIdx >= 0) //strip server name
                characterName = characterName.Substring(0, serverIdx).Trim();

            var distance = _actorManager.GetFastDistanceToPlayerWithName(characterName);
            if (distance > 0)
                message.Source.Visibility = CalculateVisibility(distance);
        }
    }
}