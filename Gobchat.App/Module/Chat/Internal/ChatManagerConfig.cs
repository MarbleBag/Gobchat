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

using Gobchat.Core.Chat;
using Gobchat.Core.Util.Extension;
using System;
using System.Linq;

namespace Gobchat.Module.Chat.Internal
{
    internal interface IChatManagerConfig
    {
        IAutotranslateProvider AutotranslateProvider { get; set; }
        ChatChannel[] VisibleChannels { get; set; }
        ChatChannel[] FormatChannels { get; set; }
        ChatChannel[] MentionChannels { get; set; }
        string[] Mentions { get; set; }
        FormatConfig[] Formats { get; set; }
        TriggerGroup[] TriggerGroups { get; set; }
        bool DetecteEmoteInSayChannel { get; set; }
        bool ExcludeUserMention { get; set; }
        ChatChannel[] CutOffChannels { get; set; }
        bool EnableCutOff { get; set; }
        float CutOffDistance { get; set; }
        float FadeOutDistance { get; set; }
    }

    internal sealed partial class ChatManager
    {
        private sealed class ChatManagerConfig : IChatManagerConfig
        {
            private readonly ChatManager _parent;

            public ChatManagerConfig(ChatManager parent)
            {
                _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }

            public IAutotranslateProvider AutotranslateProvider
            {
                get => _parent._chatlogCleaner.AutotranslateProvider;
                set => _parent._chatlogCleaner.AutotranslateProvider = value ?? throw new ArgumentNullException(nameof(AutotranslateProvider));
            }

            public ChatChannel[] VisibleChannels
            {
                get => _parent._visibleChannels.ToArray();
                set => _parent._visibleChannels = value.ToArrayOrEmpty();
            }

            public ChatChannel[] FormatChannels
            {
                get => _parent._chatMessageBuilder.FormatChannels;
                set => _parent._chatMessageBuilder.FormatChannels = value;
            }

            public FormatConfig[] Formats
            {
                get => _parent._chatMessageBuilder.Formats;
                set => _parent._chatMessageBuilder.Formats = value;
            }

            public TriggerGroup[] TriggerGroups
            {
                get => _parent._chatMessageTriggerGroups.Groups;
                set => _parent._chatMessageTriggerGroups.Groups = value;
            }

            public ChatChannel[] MentionChannels
            {
                get => _parent._chatMessageBuilder.MentionChannels;
                set => _parent._chatMessageBuilder.MentionChannels = value;
            }

            public string[] Mentions
            {
                get => _parent._chatMessageBuilder.Mentions;
                set => _parent._chatMessageBuilder.Mentions = value;
            }

            public bool DetecteEmoteInSayChannel
            {
                get => _parent._chatMessageBuilder.DetecteEmoteInSayChannel;
                set => _parent._chatMessageBuilder.DetecteEmoteInSayChannel = value;
            }

            public bool ExcludeUserMention
            {
                get => _parent._chatMessageBuilder.ExcludeUserMention;
                set => _parent._chatMessageBuilder.ExcludeUserMention = value;
            }

            public bool EnableCutOff
            {
                get => _parent._chatMessageActorData.SetVisibility;
                set => _parent._chatMessageActorData.SetVisibility = value;
            }

            public float CutOffDistance
            {
                get => _parent._chatMessageActorData.CutOffDistance;
                set => _parent._chatMessageActorData.CutOffDistance = value;
            }

            public float FadeOutDistance
            {
                get => _parent._chatMessageActorData.FadeOutDistance;
                set => _parent._chatMessageActorData.FadeOutDistance = value;
            }

            public ChatChannel[] CutOffChannels
            {
                get => _parent._chatMessageActorData.CutOffChannels;
                set => _parent._chatMessageActorData.CutOffChannels = value;
            }
        }
    }
}