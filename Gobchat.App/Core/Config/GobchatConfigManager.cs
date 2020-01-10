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
using NLog;

namespace Gobchat.Core.Config
{
    public sealed class GobchatConfigManager : IGobchatConfigManager
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

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
                var oldProfileId = _activeProfileId;
                _activeProfileId = value;
                OnActiveProfileChange?.Invoke(this, new ActiveProfileChangedEventArgs(oldProfileId, _activeProfileId));
            }
        }

        public IGobchatConfigProfile DefaultProfile => _defaultConfig;

        private string _defaultProfilePath;

        private Dictionary<string, IGobchatConfigProfile> _profiles;
        private IList<string> _profilesLoadedFromFile;
        private JsonGobchatConfigProfile _defaultConfig;

        private string _activeProfileId;
        private Dictionary<string, IList<PropertyChangedListener>> _propertyChangedListener;

        public GobchatConfigManager(string defaultProfilePath, string configFolderPath)
        {
            _defaultProfilePath = defaultProfilePath ?? throw new ArgumentNullException(nameof(defaultProfilePath));
            ConfigFolderPath = configFolderPath ?? throw new ArgumentNullException(nameof(configFolderPath));

            _profiles = new Dictionary<string, IGobchatConfigProfile>();
            _profilesLoadedFromFile = new List<string>();
            _propertyChangedListener = new Dictionary<string, IList<PropertyChangedListener>>();
        }

        public void InitializeManager()
        {
            LoadDefaultProfile();
            LoadUserProfiles();
            LoadAppConfig();
        }

        private JsonConfigLoader GetProfileLoader()
        {
            var loader = new JsonConfigLoader();
            loader.AddConverter(2, new Transforme_v2_to_v3(_defaultConfig));
            //TODO add converters
            return loader;
        }

        private void LoadDefaultProfile()
        {
            var loader = new JsonConfigLoader();
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

            var loader = GetProfileLoader();
            var finalizer = new StringToEnumTransformer();

            foreach (var userProfileFile in userProfileFiles)
            {
                JObject userProfile;
                try
                {
                    userProfile = loader.LoadConfig(userProfileFile);
                    userProfile = finalizer.Transform(userProfile);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Unable to load profile");
                    continue;
                }

                var profileId = userProfile["profile"]["id"].Value<string>();

                if (profileId == null || profileId.Length == 0)
                {
                    profileId = GenerateProfileId();
                    userProfile["profile"]["id"] = profileId;
                }

                var config = new JsonGobchatConfigProfile(userProfile, true, _defaultConfig);

                var configVersion = config.ProfileVersion;
                var defaultVersion = _defaultConfig.ProfileVersion;

                if (configVersion < defaultVersion)
                {
                    logger.Warn($"Profile {config.ProfileId} is outdated with version {configVersion}. Expected is version {defaultVersion}");
                    return;
                }

                config.OnPropertyChange += OnEvent_Config_OnPropertyChange;
                _profiles.Add(profileId, config);
                _profilesLoadedFromFile.Add(profileId);
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
                    if (activeProfile != null && _profiles.ContainsKey(activeProfile))
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
            if (!_propertyChangedListener.TryGetValue(e.PropertyKey, out var listeners))
                return;

            var profileId = (sender as IGobchatConfigProfile).ProfileId;
            var evt = new ProfilePropertyChangedEventArgs(e.PropertyKey, profileId, profileId == ActiveProfileId);

            foreach (var listener in listeners.ToArray())
                listener.Invoke(this, evt);
        }

        private string GenerateProfileId()
        {
            return Util.IdGenerator.GenerateNewId(8, _profiles.Keys);
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

            //TODO that's not good. Versioning needs to be done automatically.
            var newConfig = new JObject();
            newConfig["version"] = 3;
            newConfig["profile"] = new JObject();
            newConfig["profile"]["id"] = profileId;
            newConfig["profile"]["name"] = $"Profile {this.Profiles.Count() + 1}";

            StoreNewProfile(newConfig);
            return profileId;
        }

        private void StoreNewProfile(JObject profile)
        {
            var config = new JsonGobchatConfigProfile(profile, true, _defaultConfig);
            config.OnPropertyChange += OnEvent_Config_OnPropertyChange;
            _profiles.Add(config.ProfileId, config);
            OnProfileChange?.Invoke(this, new ProfileChangedEventArgs(config.ProfileId, ProfileChangedEventArgs.Type.New));
        }

        public void CopyProfile(string srcProfileId, string dstProfileId)
        {
            var srcProfile = GetProfile(srcProfileId);
            var dstProfile = GetProfile(dstProfileId);
            dstProfile.SetProperties(srcProfile.ToJson());
        }

        public JToken AsJson()
        {
            var root = new JObject();
            root["activeProfile"] = this.ActiveProfileId;
            root["profiles"] = new JObject();

            var profiles = this.Profiles;
            var profileStore = root["profiles"];
            foreach (var profileId in profiles)
            {
                profileStore[profileId] = this.GetProfile(profileId).ToJson();
            }

            return root;
        }

        public void Synchronize(JToken configJson)
        {
            var activeProfile = configJson["activeProfile"].ToString();
            var profileIds = (configJson["profiles"] as JObject).Properties().Select(p => p.Name);

            var storedProfiles = this.Profiles;
            var availableProfiles = profileIds.Where(p => storedProfiles.Contains(p));
            var newProfiles = profileIds.Where(p => !storedProfiles.Contains(p));
            var removedProfiles = storedProfiles.Where(p => !profileIds.Contains(p));

            foreach (var profileId in newProfiles)
                StoreNewProfile(configJson["profiles"][profileId] as JObject);

            this.ActiveProfileId = activeProfile;

            foreach (var profileId in removedProfiles)
                DeleteProfile(profileId);

            foreach (var profileId in availableProfiles)
                GetProfile(profileId).SetProperties(configJson["profiles"][profileId] as JObject);
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
                    logger.Fatal(ex);
                }
            }

            foreach (var profileId in _profilesLoadedFromFile)
            {
                if (_profiles.ContainsKey(profileId))
                    continue;

                try
                {
                    var file = Path.Combine(outputFolder, $"profile_{profileId}.json");
                    File.Delete(file);
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Warn(ex);
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
                logger.Fatal(ex);
            }
        }

        public T GetProperty<T>(string key)
        {
            return ActiveProfile.GetProperty<T>(key);
        }

        public T GetProperty<T>(string key, T defaultValue)
        {
            return ActiveProfile.GetProperty<T>(key, defaultValue);
        }

        public bool HasProperty(string key)
        {
            return ActiveProfile.HasProperty(key);
        }

        public void SetProperties(JObject json)
        {
            ActiveProfile.SetProperties(json);
        }

        public void SetProperty(string key, object value)
        {
            ActiveProfile.SetProperty(key, value);
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
}