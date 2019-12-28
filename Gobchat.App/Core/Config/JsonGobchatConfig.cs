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
using static Gobchat.Core.Config.JsonUtil;

namespace Gobchat.Core.Config
{
    internal sealed class JsonGobchatConfig : IGobchatConfig
    {
        private readonly JObject _data;

        public event EventHandler<PropertyChangedEventArgs> OnPropertyChange;

        public JsonGobchatConfig(JObject data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        private void WalkJson(string key, JsonUtil.MissingElementHandling missingElementHandling, JsonUtil.Action action)
        {
            JsonUtil.WalkJson(key, _data, missingElementHandling, action);
        }

        public T GetProperty<T>(string key)
        {
            T result = default;
            WalkJson(key, MissingElementHandling.Throw, (node, propertyName) =>
            {
                var jToken = node[propertyName];
                if (jToken == null)
                    throw new MissingPropertyException(key);
                result = GetJsonValue<T>(key, jToken);
            });
            return result;
        }

        public T GetProperty<T>(string key, T defaultValue)
        {
            T result = defaultValue;
            WalkJson(key, MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;
                var jToken = node[propertyName];
                result = GetJsonValue<T>(key, jToken);
            });
            return result;
        }

        private T GetJsonValue<T>(string key, JToken jToken)
        {
            if (jToken is T tValue)
                return tValue;
            return jToken.ToObject<T>(); //works quite well
        }

        public void SetProperty(string key, object value)
        {
            WalkJson(key, MissingElementHandling.Create, (node, propertyName) =>
            {
                if (value is JToken)
                {
                    node[propertyName] = (JToken)value;
                    return;
                }

                node[propertyName] = JToken.FromObject(value);
            });
        }

        public void SetProperties(JObject json)
        {
            JsonUtil.Write(json, _data);
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
            return result;
        }

        public JObject ToJson()
        {
            return (JObject)_data.DeepClone();
        }
    }
}