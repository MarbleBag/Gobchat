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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Gobchat.Core.Config
{
    public sealed class GobchatConfigManager : IGobchatConfigManager
    {
        public string ConfigFolderPath { get; set; }

        public event EventHandler<ProfileChangedEventArgs> OnProfileChange;

        public event EventHandler<ActiveProfileChangedEventArgs> OnActiveProfileChange;

        public IGobchatConfigProfile ActiveProfile => GetProfile(ActiveProfileId);

        public IList<string> Profiles => _profiles.Keys.ToArray();

        public string ActiveProfileId
        {
            get => _activeProfileId;
            set
            {
                if (_activeProfileId == value)
                    return;
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(ActiveProfileId));
                if (!_profiles.ContainsKey(value))
                    throw new InvalidProfileIdException(value);
                OnActiveProfileChange?.Invoke(this, new ActiveProfileChangedEventArgs(_activeProfileId, _activeProfileId = value));
            }
        }

        private string _defaultProfilePath;

        private Dictionary<string, IGobchatConfigProfile> _profiles;
        private JsonGobchatConfigProfile _defaultConfig;

        private string _activeProfileId;
        private Dictionary<string, IList<PropertyChangedListener>> _propertyChangedListener;

        public GobchatConfigManager(string defaultProfilePath, string configFolderPath)
        {
            _defaultProfilePath = defaultProfilePath ?? throw new ArgumentNullException(nameof(defaultProfilePath));
            ConfigFolderPath = configFolderPath ?? throw new ArgumentNullException(nameof(configFolderPath));

            _profiles = new Dictionary<string, IGobchatConfigProfile>();
            _propertyChangedListener = new Dictionary<string, IList<PropertyChangedListener>>();
        }

        public void InitializeManager()
        {
            LoadDefaultProfile();
            LoadUserProfiles();
            LoadAppConfig();
        }

        private JsonConfigLoader GetConfigLoader()
        {
            var loader = new JsonConfigLoader();
            //TODO add converters
            return loader;
        }

        private void LoadDefaultProfile()
        {
            var loader = GetConfigLoader();
            var defaultConfig = loader.LoadConfig(_defaultProfilePath);

            var finalizer = new StringToEnumTransformer();
            defaultConfig = finalizer.Transform(defaultConfig);
            defaultConfig["profile"]["id"] = null;

            _defaultConfig = new JsonGobchatConfigProfile(defaultConfig, false);
        }

        private void LoadUserProfiles()
        {
            var userProfileFolderPath = Path.Combine(ConfigFolderPath, "profiles");
            if (!Directory.Exists(userProfileFolderPath))
                return;

            var userProfileFiles = Directory.EnumerateFiles(userProfileFolderPath, "profile_*.json", SearchOption.TopDirectoryOnly);

            var loader = GetConfigLoader();
            var finalizer = new StringToEnumTransformer();

            foreach (var userProfileFile in userProfileFiles)
            {
                var userProfile = loader.LoadConfig(userProfileFile);
                userProfile = finalizer.Transform(userProfile);

                var profileId = userProfile["profile"]["id"].Value<string>();

                if (profileId == null || profileId.Length == 0)
                {
                    profileId = GenerateProfileId();
                    userProfile["profile"]["id"] = profileId;
                }

                var config = new JsonGobchatConfigProfile(userProfile, true, _defaultConfig);
                config.OnPropertyChange += OnEvent_Config_OnPropertyChange;
                _profiles.Add(profileId, config);
            }
        }

        private void LoadAppConfig()
        {
            var appConfigPath = Path.Combine(ConfigFolderPath, "gobconfig.json");

            string activeProfile;
            if (File.Exists(appConfigPath))
            {
                var loader = new JsonConfigLoader();
                loader.AddConverter(1, new LegacyAppConfigTransformer(this));

                var appConfig = loader.LoadConfig(appConfigPath);

                if (appConfig["activeProfile"] != null)
                {
                    activeProfile = appConfig["activeProfile"].Value<string>();
                    if (activeProfile != null && activeProfile.Length > 0 && _profiles.ContainsKey(activeProfile))
                    {
                        ActiveProfileId = activeProfile;
                        return;
                    }
                }
            }

            if (ActiveProfileId != null)
                return;

            activeProfile = _profiles.Keys.FirstOrDefault();
            if (activeProfile != null)
            {
                ActiveProfileId = activeProfile;
                return;
            }
            ActiveProfileId = CreateNewProfile();
        }

        private void OnEvent_Config_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            var propertyKey = e.PropertyKey;
            if (!_propertyChangedListener.TryGetValue(propertyKey, out var listeners))
                return;

            foreach (var listener in listeners.ToArray())
                listener.Invoke(this, e);
        }

        private string GenerateProfileId()
        {
            string GenerateId(int lenght)
            {
                var builder = new System.Text.StringBuilder();
                while (lenght > 0)
                {
                    var path = Path.GetRandomFileName().Replace(".", "");
                    if (path.Length > lenght)
                        builder.Append(path.Substring(0, lenght));
                    else
                        builder.Append(path);
                    lenght -= path.Length;
                }
                return builder.ToString();
            }

            do
            {
                var id = GenerateId(8);
                if (!_profiles.ContainsKey(id))
                    return id;
            } while (true);
        }

        public IGobchatConfigProfile GetProfile(string profileId)
        {
            if (_profiles.TryGetValue(profileId, out var profile))
                return profile;
            throw new InvalidProfileIdException(profileId);
        }

        public void DeleteProfile(string profileId)
        {
            if (!_profiles.ContainsKey(profileId))
                return;

            if (_profiles.Count == 1)
                throw new ConfigException("Unable to delete last profile");

            var config = _profiles[profileId];
            config.OnPropertyChange -= OnEvent_Config_OnPropertyChange;

            _profiles.Remove(profileId);
            if (ActiveProfileId == profileId)
                ActiveProfileId = _profiles.Keys.First();

            OnProfileChange?.Invoke(this, new ProfileChangedEventArgs(profileId, ProfileChangedEventArgs.Type.Delete));
        }

        public string CreateNewProfile()
        {
            var profileId = GenerateProfileId();

            var newConfig = new JObject();
            newConfig["profile"] = new JObject();
            newConfig["profile"]["id"] = profileId;
            newConfig["profile"]["name"] = "";

            var config = new JsonGobchatConfigProfile(newConfig, true, _defaultConfig);
            config.OnPropertyChange += OnEvent_Config_OnPropertyChange;

            _profiles.Add(profileId, config);
            OnProfileChange?.Invoke(this, new ProfileChangedEventArgs(profileId, ProfileChangedEventArgs.Type.New));
            return profileId;
        }

        public void CopyProfile(string srcProfileId, string dstProfileId)
        {
            throw new NotImplementedException();
        }

        public void SaveProfiles()
        {
            SaveUserProfiles();
            SaveAppConfig();
        }

        private void SaveUserProfiles()
        {
            var outputFolder = Path.Combine(ConfigFolderPath, "profiles");
            Directory.CreateDirectory(outputFolder);

            var finalizer = new EnumToStringTransformer();

            foreach (var profile in _profiles.Values)
            {
                var profilePath = Path.Combine(outputFolder, $"profile_{profile.ProfileId}.json");

                var json = profile.ToJson();
                json = finalizer.Transform(json);

                try
                {
                    using (StreamWriter file = File.CreateText(profilePath))
                    using (JsonTextWriter writer = new JsonTextWriter(file))
                    {
                        writer.Formatting = Formatting.Indented;
                        json.WriteTo(writer);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    //TODO
                }
            }
        }

        private void SaveAppConfig()
        {
            var appConfigPath = Path.Combine(ConfigFolderPath, "gobconfig.json");
            Directory.CreateDirectory(ConfigFolderPath);

            var appConfigJson = new JObject();
            appConfigJson["version"] = 2;
            appConfigJson["activeProfile"] = ActiveProfileId;

            try
            {
                using (StreamWriter file = File.CreateText(appConfigPath))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    appConfigJson.WriteTo(writer);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                //TODO
            }
        }

        public void AddPropertyChangeListener(string path, PropertyChangedListener listener)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            if (_propertyChangedListener.TryGetValue(path, out var listeners))
            {
                if (!listeners.Contains(listener))
                    listeners.Add(listener);
            }
            else
            {
                listeners = new List<PropertyChangedListener>() { listener };
                _propertyChangedListener.Add(path, listeners);
            }
        }

        public void RemovePropertyChangeListener(string path, PropertyChangedListener listener)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (listener == null) return;

            if (_propertyChangedListener.TryGetValue(path, out var listeners))
            {
                listeners.Remove(listener);
                if (listeners.Count == 0)
                    _propertyChangedListener.Remove(path);
            }
        }

        public void RemovePropertyChangeListener(PropertyChangedListener listener)
        {
            if (listener == null) return;
            foreach (var key in _propertyChangedListener.Keys.ToArray())
            {
                var listeners = _propertyChangedListener[key];
                listeners.Remove(listener);
                if (listeners.Count == 0)
                    _propertyChangedListener.Remove(key);
            }
        }
    }

    public class GobchatConfigManager2
    {
        public string DefaultConfigPath { get; set; }
        public string UserConfigPath { get; set; }

        public IGobchatConfig DefaultConfig { get; private set; }

        public IGobchatConfig UserConfig { get; private set; }

        public GobchatConfigManager2(string defaultConfigPath, string userConfigPath)
        {
            DefaultConfigPath = defaultConfigPath;
            UserConfigPath = userConfigPath;
        }

        public void LoadConfig()
        {
            LoadDefaultConfig();
            LoadUserConfig();
        }

        public void LoadDefaultConfig()
        {
            var loader = new JsonConfigLoader();
            var defaultConfig = loader.LoadConfig(DefaultConfigPath);

            var finalizer = new StringToEnumTransformer();
            defaultConfig = finalizer.Transform(defaultConfig);
            DefaultConfig = new JsonGobchatConfig(defaultConfig);
        }

        public void LoadUserConfig()
        {
            JObject userConfig = null;
            if (File.Exists(UserConfigPath))
            {
                var loader = new JsonConfigLoader();
                userConfig = loader.LoadConfig(UserConfigPath);
            }

            JObject clonedDefaultConfig = DefaultConfig.ToJson();

            if (userConfig == null) //easy to solve
            {
                userConfig = clonedDefaultConfig;
            }
            else
            {
                var finalizer = new StringToEnumTransformer();
                userConfig = finalizer.Transform(userConfig);

                clonedDefaultConfig.Merge(userConfig, new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Merge,
                    PropertyNameComparison = StringComparison.OrdinalIgnoreCase,
                });

                userConfig = clonedDefaultConfig;
            }

            UserConfig = new JsonGobchatConfig(userConfig);
        }

        public void SaveConfig()
        {
            if (UserConfigPath == null)
                return;

            var directoryPath = Path.GetDirectoryName(UserConfigPath);
            Directory.CreateDirectory(directoryPath);

            //TODO calculate diff to default config
            //TODO only save diff
            var jsonDiff = UserConfig.ToJson();

            var finalizer = new EnumToStringTransformer();
            jsonDiff = finalizer.Transform(jsonDiff);

            try
            {
                using (StreamWriter file = File.CreateText(UserConfigPath))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    jsonDiff.WriteTo(writer);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                //TODO
                throw;
            }
        }
    }
}