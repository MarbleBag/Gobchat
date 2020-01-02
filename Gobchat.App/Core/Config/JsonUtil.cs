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
using System.Linq;

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

        public class SwitchResult
        {
            public bool Value { get; }
            public bool HasError { get; }
            public SwitchError Error { get; }

            public SwitchResult(bool result)
            {
                Value = result;
                HasError = false;
            }

            public SwitchResult(SwitchError error)
            {
                Value = false;
                HasError = true;
                Error = error;
            }
        }

        public abstract class SwitchError
        {
        }

        public sealed class TypeSwitchError : SwitchError
        {
            public Type Expected { get; }
            public Type Actual { get; }

            public TypeSwitchError(Type expected, Type actual)
            {
                Expected = expected;
                Actual = actual;
            }
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
                    //var error = new TypeSwitchError(elementA.GetType(), elementB?.GetType());
                    //return new SwitchResult(error);
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

        public static ISet<string> Write(JObject source, JObject destination)
        {
            return Write(source, destination, null);
        }

        public static ISet<string> Write(JObject source, JObject destination, Func<string, bool> ignorePath)
        {
            var path = new Stack<string>();
            var changed = new HashSet<string>();
            var callbacks = new JsonUtil.SwitchCallbacks();

            callbacks.OnArray = (arrayA, arrayB) =>
            {
                return true; //always overwrite
            };
            callbacks.OnValue = (valueA, valueB) =>
            {
                return true; //always overwrite
            };
            callbacks.OnObject = (objectA, objectB) => // move data from A to B
            {
                foreach (var property in objectA.Properties())
                {
                    path.Push(property.Name);

                    if (ignorePath?.Invoke(string.Join(".", path)) ?? false)
                        continue;

                    var doOverwrite =
                        objectB[property.Name] == null ||
                        JsonUtil.TypeSwitch(objectB[property.Name], property.Value, callbacks);

                    if (doOverwrite)
                    {
                        objectB[property.Name] = property.Value.DeepClone();
                        changed.Add(string.Join(".", path));
                    }

                    path.Pop();
                }
                return false; //already overwritten
            };

            JsonUtil.TypeSwitch(source, destination, callbacks);

            return changed;
        }
    }
}