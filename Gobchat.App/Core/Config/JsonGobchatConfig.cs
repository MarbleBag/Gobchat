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
using System.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class JsonGobchatConfig : GobchatConfig
    {
        private readonly JObject _data;

        public JsonGobchatConfig(JObject data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        private enum MissingElementHandling
        {
            Throw,
            Create,
            Stop
        }

        private delegate void Action(JObject node, string propertyName);

        private void WalkJson(string key, MissingElementHandling missingElementHandling, Action action)
        {
            var path = key.Split(new char[] { '.' });
            var node = _data;

            for (int i = 0; i < path.Length - 1; ++i)
            {
                var step = path[i];
                var nextNode = node[step];

                if (nextNode == null)
                {
                    switch (missingElementHandling)
                    {
                        case MissingElementHandling.Throw:
                            throw new InvalidPropertyPathException("null", String.Join(".", path.Take(i + 1)));

                        case MissingElementHandling.Create:
                            nextNode = new JObject();
                            node[step] = nextNode;
                            break;

                        case MissingElementHandling.Stop:
                            action(null, null);
                            return;
                    }
                }

                node = (JObject)nextNode;
            }

            var lastStep = path[path.Length - 1];
            action(node, lastStep);
        }

        public T GetProperty<T>(string key)
        {
            T result = default;
            WalkJson(key, MissingElementHandling.Throw, (node, propertyName) =>
            {
                var jtoken = node[propertyName];
                if (jtoken == null)
                    throw new MissingPropertyException(key);

                if (jtoken is T tValue)
                {
                    result = tValue;
                    return;
                }

                if (!(jtoken is JValue jValue))
                    throw new InvalidPropertyTypeException(key, typeof(T), jtoken.GetType());

                if (!(jValue.Value is T))
                    throw new InvalidPropertyTypeException(key, typeof(T), jValue.Value.GetType());

                result = (T)jValue.Value;
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

                var jtoken = node[propertyName];
                if (jtoken is T tValue)
                {
                    result = tValue;
                    return;
                }

                if (!(jtoken is JValue jValue))
                    throw new InvalidPropertyTypeException(key, typeof(T), jtoken.GetType());

                if (!(jValue.Value is T))
                    throw new InvalidPropertyTypeException(key, typeof(T), jValue.Value.GetType());

                result = (T)jValue.Value;
            });
            return result;
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