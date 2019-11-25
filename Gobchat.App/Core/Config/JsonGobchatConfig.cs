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
using static Gobchat.Core.Config.JsonUtil;

namespace Gobchat.Core.Config
{
    internal static class JsonUtil
    {
        public enum MissingElementHandling
        {
            Throw,
            Create,
            Stop
        }

        public delegate void Action(JObject node, string propertyName);

        public static void WalkJson(string key, JObject root, MissingElementHandling missingElementHandling, Action action)
        {
            var path = key.Split(new char[] { '.' });
            var node = root;

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

        public class SwitchCallbacks
        {
            public delegate bool ArrayCallback(JArray arrayA, JArray arrayB);

            public delegate bool ObjectCallback(JObject objectA, JObject objectB);

            public delegate bool ValueCallback(JValue valueA, JValue valueB);

            public ArrayCallback OnArray { get; set; }
            public ObjectCallback OnObject { get; set; }
            public ValueCallback OnValue { get; set; }
        }

        public static bool TypeSwitch(JToken elementA, JToken elementB, SwitchCallbacks callbacks)
        {
            if (elementA is JArray arrayA)
            {
                if (elementB is JArray arrayB)
                {
                    var result = callbacks.OnArray.Invoke(arrayA, arrayB);
                    return result;
                }
                else
                {
                    return false;
                    //TODO A is array, but B is not
                }
            }
            else if (elementB is JArray arrayB)
            {
                return false;
                //TODO B is array, but A is not
            }
            else if (elementA is JObject objectA)
            {
                if (elementB is JObject objectB)
                {
                    var result = callbacks.OnObject.Invoke(objectA, objectB);
                    return result;
                }
                else
                {
                    return false;
                    //TODO A is object, but B is not
                }
            }
            else if (elementB is JObject objectB)
            {
                return false;
                //TODO B is object, but A is not
            }
            else if (elementA is JValue valueA && elementB is JValue valueB)
            {
                var result = callbacks.OnValue.Invoke(valueA, valueB);
                return result;
                //TODO
            }
            else
            {
                return false;
                //TODO something is really wrong
            }
        }

        public static void Write(JObject source, JObject destination)
        {
            var callbacks = new JsonUtil.SwitchCallbacks();
            callbacks.OnArray = (arrayA, arrayB) =>
            {
                return true; //always overwrite
            };
            callbacks.OnValue = (valueA, valueB) =>
            {
                return true; //always overwrite
            };
            callbacks.OnObject = (objectA, objectB) => // move data from B to A
            {
                foreach (var property in objectB.Properties())
                {
                    if (objectA[property.Name] == null)
                    {
                        objectA[property.Name] = property.Value.DeepClone();
                    }
                    else
                    {
                        var doOverwrite = JsonUtil.TypeSwitch(objectA[property.Name], property.Value, callbacks);
                        if (doOverwrite)
                            objectA[property.Name] = property.Value.DeepClone();
                    }
                }
                return false; //already overwritten
            };

            JsonUtil.TypeSwitch(destination, source, callbacks);
        }
    }

    internal sealed class JsonGobchatConfig : GobchatConfig
    {
        private readonly JObject _data;

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

            /*
            if (!(jToken is JValue jValue))
                throw new InvalidPropertyTypeException(key, typeof(T), jToken.GetType());

            if (jValue.Value == null)
                return default;

            if (!(jValue.Value is T))
                throw new InvalidPropertyTypeException(key, typeof(T), jValue.Value.GetType());

            return (T)jValue.Value;
            */
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