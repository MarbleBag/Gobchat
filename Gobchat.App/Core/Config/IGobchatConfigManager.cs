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

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Gobchat.Core.Config
{
    public delegate void PropertyChangedListener(IGobchatConfigManager sender, ProfilePropertyChangedEventArgs evt);

    public interface IGobchatConfigManager
    {
        event EventHandler<ActiveProfileChangedEventArgs> OnActiveProfileChange;

        event EventHandler<ProfileChangedEventArgs> OnProfileChange;

        void AddPropertyChangeListener(string path, PropertyChangedListener listener);

        void RemovePropertyChangeListener(string path, PropertyChangedListener listener);

        void RemovePropertyChangeListener(PropertyChangedListener listener);

        IGobchatConfigProfile DefaultProfile { get; }

        IGobchatConfigProfile GetProfile(string profileId);

        IGobchatConfigProfile ActiveProfile { get; }

        string ActiveProfileId { get; set; }

        IList<string> Profiles { get; }

        void DeleteProfile(string profileId);

        string CreateNewProfile();

        void CopyProfile(string srcProfileId, string dstProfileId);

        JToken AsJson();

        void Synchronize(JToken configJson);

        T GetProperty<T>(string key);

        T GetProperty<T>(string key, T defaultValue);

        bool HasProperty(string key);

        void SetProperties(JObject json);

        void SetProperty(string key, object value);

        void SaveProfiles();
    }
}