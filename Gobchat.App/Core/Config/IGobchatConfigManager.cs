/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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
    public delegate void PropertyChangedListener(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt);

    public interface IConfigManager
    {
        #region event handling

        event EventHandler<ActiveProfileChangedEventArgs> OnActiveProfileChange;

        event EventHandler<ProfileChangedEventArgs> OnProfileChange;

        bool AddPropertyChangeListener(string path, PropertyChangedListener listener);

        bool AddPropertyChangeListener(string path, bool onActiveProfile, PropertyChangedListener listener);

        bool AddPropertyChangeListener(string path, bool onActiveProfile, bool initialize, PropertyChangedListener listener);

        void RemovePropertyChangeListener(string path, PropertyChangedListener listener);

        void RemovePropertyChangeListener(PropertyChangedListener listener);

        #endregion event handling

        IGobchatConfigProfile DefaultProfile { get; }

        IGobchatConfigProfile GetProfile(string profileId);

        // IGobchatConfigProfile ActiveProfile { get; }

        string ActiveProfileId { get; set; }

        IList<string> Profiles { get; }

        void DeleteProfile(string profileId);

        string CreateNewProfile();

        void SaveProfiles();

        void CopyProfile(string srcProfileId, string dstProfileId);

        JObject ParseProfile(string path);

        #region property handling

        T GetProperty<T>(string key);

        T GetProperty<T>(string key, T defaultValue);

        bool HasProperty(string key);

        void SetProperty(string key, object value);

        void DeleteProperty(string key);

        void SetGlobalProperty(string key, object value);

        void DispatchChangeEvents();

        #endregion property handling

        JObject AsJson();

        void Synchronize(JToken json);
    }
}