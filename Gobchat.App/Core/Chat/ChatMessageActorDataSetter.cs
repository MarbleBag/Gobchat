﻿/*******************************************************************************
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

using Gobchat.Core.Util.Extension;
using Gobchat.Module.Actor;
using System;
using System.Linq;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageActorDataSetter
    {
        private const int MaxVisibility = 100;

        private float _cutOffDistance;
        private float _fadeOutDistance;
        private float _cutOffDistanceSquared;
        private float _fadeOutDistanceSquared;

        private ChatChannel[] _channels = Array.Empty<ChatChannel>();

        private readonly IActorManager _actorManager;

        public ChatMessageActorDataSetter(IActorManager actorManager)
        {
            _actorManager = actorManager ?? throw new ArgumentNullException(nameof(actorManager));
        }

        public float CutOffDistance
        {
            get => _cutOffDistance;
            set
            {
                if (value < 0f) throw new ArgumentException("Must be a value greater than or equal to 0", nameof(CutOffDistance));
                _cutOffDistance = value;
                _cutOffDistanceSquared = value * value;
            }
        }

        public float FadeOutDistance
        {
            get => _fadeOutDistance;
            set
            {
                if (value < 0f) throw new ArgumentException("Must be a value greater than or equal to 0", nameof(FadeOutDistance));
                _fadeOutDistance = value;
                _fadeOutDistanceSquared = value * value;
            }
        }

        public ChatChannel[] CutOffChannels
        {
            get => _channels.ToArray();
            set => _channels = value.ToArrayOrEmpty();
        }

        public bool SetVisibility { get; set; }

        public void SetActorData(ChatMessage message)
        {
            if (!_actorManager.IsAvailable)
                return;

            if (SetVisibility)
                SetVisibilityOnMessage(message);

            var currentUser = _actorManager.GetActivePlayerName();
            if (currentUser != null && message.Source.IsAPlayer)
                message.Source.IsUser = currentUser.Equals(message.Source.CharacterName, StringComparison.InvariantCultureIgnoreCase);
        }

        private void SetVisibilityOnMessage(ChatMessage message)
        {
            if (!_channels.Contains(message.Channel))
                return;

            var characterName = message.Source.CharacterName;
            if (characterName == null)
                return;

            var distance = _actorManager.GetFastDistanceToPlayerWithName(characterName);
            if (distance > 0)
                message.Source.Visibility = CalculateVisibility(distance);
        }

        /// <summary>
        /// Calculates the visibility in a range of [0,100] based on the squared distance
        /// </summary>
        /// <param name="distance">Squared distance</param>
        /// <returns>A value from [0,100]</returns>
        private int CalculateVisibility(float distance)
        {
            if (distance > _cutOffDistanceSquared) return 0;
            if (distance < _fadeOutDistanceSquared) return MaxVisibility;
            var percentage = 1 - (distance - _fadeOutDistanceSquared) / (_cutOffDistanceSquared - _fadeOutDistanceSquared);
            return (int)Math.Round(MaxVisibility * percentage);
        }
    }
}