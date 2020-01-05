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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using static Gobchat.Core.Config.JsonUtil;

namespace Gobchat.Core.Config
{
    internal sealed class JsonGobchatConfigProfile : IGobchatConfigProfile
    {
        private readonly ICollection<string> UnchangableValues = new HashSet<string>() { "version", "profile.id", "behaviour.frame.chat" };

        private readonly IGobchatConfigProfile _parent;
        private readonly JObject _data;
        private readonly bool _writable;

        public JsonGobchatConfigProfile(JObject data, bool writable) : this(data, writable, null)
        {
        }

        public JsonGobchatConfigProfile(JObject data, bool writable, IGobchatConfigProfile parent)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _writable = writable;
            _parent = parent;
        }

        public string ProfileId => _data["profile"]["id"].Value<string>();

        public event EventHandler<PropertyChangedEventArgs> OnPropertyChange;

        public T GetProperty<T>(string key)
        {
            T result = default;
            bool found = false;

            WalkJson(key, MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;

                var jToken = node[propertyName];
                if (jToken == null)
                    return;

                result = GetJsonValue<T>(jToken);
                found = true;
            });

            if (!found)
                if (_parent != null)
                    result = _parent.GetProperty<T>(key);
                else
                    throw new MissingPropertyException(key);

            return result;
        }

        public T GetProperty<T>(string key, T defaultValue)
        {
            T result = defaultValue;
            bool found = false;

            WalkJson(key, MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;

                var jToken = node[propertyName];
                if (jToken == null)
                    return;

                result = GetJsonValue<T>(jToken);
                found = true;
            });

            if (!found && _parent != null)
                result = _parent.GetProperty<T>(key, defaultValue);

            return result;
        }

        public bool HasProperty(string key)
        {
            bool result = false;
            WalkJson(key, MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;
                result = node[propertyName] != null;
            });

            if (!result && _parent != null)
                result = _parent.HasProperty(key);

            return result;
        }

        public void SetProperties(JObject json)
        {
            if (!_writable)
                throw new ConfigException("Config is read only");

            var (changes, _) = JsonUtil.Write(json, _data, (path) => UnchangableValues.Contains(path));
            FirePropertyChange(changes);
        }

        public void SetProperty(string key, object value)
        {
            if (!_writable)
                throw new ConfigException("Config is read only");
            if (UnchangableValues.Contains(key))
                throw new PropertyReadOnlyException(key);

            bool changed = false;

            WalkJson(key, MissingElementHandling.Create, (node, propertyName) =>
            {
                if (value is JToken jToken)
                {
                    changed = !jToken.Equals(node[propertyName]);
                    node[propertyName] = jToken;
                }
                else
                {
                    var newValue = JToken.FromObject(value);
                    changed = !newValue.Equals(node[propertyName]);
                    node[propertyName] = newValue;
                }
            });

            if (changed)
                FirePropertyChange(key);
        }

        private void FirePropertyChange(string key)
        {
            FirePropertyChange(new List<string>() { key });
        }

        private void FirePropertyChange(ICollection<string> keys)
        {
            if (OnPropertyChange == null) return;
            foreach (var k in PreparePropertyCallbacks(keys))
                OnPropertyChange?.Invoke(this, new PropertyChangedEventArgs(k));
        }

        private ICollection<string> PreparePropertyCallbacks(ICollection<string> keys)
        {
            var completed = new HashSet<string>() { "*" };
            foreach (var key in keys)
            {
                if (!completed.Add(key))
                    continue;

                var split = key.Split('.');
                for (int i = split.Length - 2; i >= 0; --i)
                {
                    var path = string.Join(".", split, 0, i);
                    if (!completed.Add(path))
                        break;
                }
            }

            var sorted = new List<string>(completed);
            sorted.Sort((a, b) =>
            {
                if (a.StartsWith(b))
                    return 1;
                if (b.StartsWith(a))
                    return -1;
                return a.CompareTo(b);
            });

            return sorted;
        }

        public JObject ToJson()
        {
            return (JObject)_data.DeepClone();
        }

        private void WalkJson(string key, JsonUtil.MissingElementHandling missingElementHandling, JsonUtil.Action action)
        {
            JsonUtil.WalkJson(key, _data, missingElementHandling, action);
        }

        private T GetJsonValue<T>(JToken jToken)
        {
            if (jToken is T tValue)
                return tValue;
            return jToken.ToObject<T>(); //works quite well
        }
    }
}