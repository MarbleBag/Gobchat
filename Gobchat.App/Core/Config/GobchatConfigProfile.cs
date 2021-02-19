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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using static Gobchat.Core.Config.JsonUtil;
using System.Globalization;

namespace Gobchat.Core.Config
{
    internal sealed class GobchatConfigProfile : IGobchatConfigProfile
    {
        private readonly ICollection<string> UnchangableValues = new HashSet<string>() { "version", "profile.id"/*, "behaviour.frame.chat"*/ };

        private readonly IGobchatConfigProfile _parent;
        private readonly JObject _data;
        private readonly bool _writable;

        private event EventHandler<PropertyChangedEventArgs> _onPropertyChange;

        private readonly bool _canLinkParent;
        private bool _isParentLinked;

        private readonly object _lock = new object();

        public GobchatConfigProfile(JObject data, bool writable) : this(data, writable, null)
        {
        }

        public GobchatConfigProfile(JObject data, bool writable, IGobchatConfigProfile parent)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _writable = writable;
            _parent = parent;

            _canLinkParent = _parent != null && _parent.IsWriteable;
        }

        public string ProfileId => _data["profile"]["id"].Value<string>();

        public int ProfileVersion => _data["version"].Value<int>();

        public bool IsWriteable => _writable;

        public event EventHandler<PropertyChangedEventArgs> OnPropertyChange
        {
            add
            {
                lock (_lock)
                {
                    _onPropertyChange += value;
                    if (_canLinkParent && !_isParentLinked)
                    {
                        _parent.OnPropertyChange += Parent_OnPropertyChange;
                        _isParentLinked = true;
                    }
                }
            }
            remove
            {
                lock (_lock)
                {
                    _onPropertyChange -= value;
                    if (_canLinkParent && _isParentLinked && _onPropertyChange == null)
                    {
                        _parent.OnPropertyChange -= Parent_OnPropertyChange;
                        _isParentLinked = false;
                    }
                }
            }
        }

        private void Parent_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            _onPropertyChange?.Invoke(this, e);
        }

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

            var (changes, _) = JsonUtil.Overwrite(json, _data, (path) => UnchangableValues.Contains(path));
            FirePropertyChange(changes);
        }

        public void Synchronize(JObject root)
        {
            if (!_writable)
                throw new ConfigException("Config is read only");

            var enumTransformer = new JsonValueToEnum();
            root = enumTransformer.Apply(root);

            var (changes, _) = JsonUtil.RemoveUnused(root, _data, (path) => UnchangableValues.Contains(path));
            var (writeChanges, _) = JsonUtil.Overwrite(root, _data, (path) => UnchangableValues.Contains(path));

            changes.UnionWith(writeChanges);
            FirePropertyChange(changes, true);
        }

        public void DeleteProperty(string key)
        {
            if (!_writable)
                throw new ConfigException("Config is read only");
            if (UnchangableValues.Contains(key))
                throw new PropertyReadOnlyException(key);

            bool changed = false;
            WalkJson(key, MissingElementHandling.Stop, (node, propertyName) =>
            {
                changed = node.Remove(propertyName);
            });

            if (changed)
                FirePropertyChange(key);
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

        private void FirePropertyChange(ICollection<string> keys, bool synchronize = false)
        {
            if (_onPropertyChange == null) return;
            foreach (var k in PreparePropertyCallbacks(keys))
                _onPropertyChange?.Invoke(this, new PropertyChangedEventArgs(k, synchronize));
        }

        private ICollection<string> PreparePropertyCallbacks(ICollection<string> keys)
        {
            var completed = new HashSet<string>() { "*" };
            foreach (var key in keys)
            {
                if (!completed.Add(key))
                    continue;

                var split = key.Split('.');
                for (int i = split.Length - 1; i >= 0; --i)
                {
                    var path = string.Join(".", split, 0, i);
                    if (!completed.Add(path))
                        break;
                }
            }

            var sorted = new List<string>(completed);
            sorted.Sort((a, b) =>
            {
                if (a.StartsWith(b, true, CultureInfo.InvariantCulture))
                    return 1;
                if (b.StartsWith(a, true, CultureInfo.InvariantCulture))
                    return -1;
                return string.Compare(a, b, true, CultureInfo.InvariantCulture);
            });

            if (sorted.Count == 1)
                sorted.Clear();

            return sorted;
        }

        public JObject ToJson()
        {
            return (JObject)_data.DeepClone();
        }

        private void WalkJson(string key, JsonUtil.MissingElementHandling missingElementHandling, JsonUtil.WalkAction action)
        {
            JsonUtil.WalkJson(_data, key, missingElementHandling, action);
        }

        private T GetJsonValue<T>(JToken jToken)
        {
            if (jToken is T tValue)
                return tValue;
            return jToken.ToObject<T>(); //works quite well
        }
    }
}