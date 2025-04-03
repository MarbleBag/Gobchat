/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

using Gobchat.Core.Util;
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
            Throw, // if the element can't be found, throw an exception
            Create, // if the element can't be found, create an object and call action
            Stop //  if the element can't be found, call action and pass null into it
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node">null if node wasn't found</param>
        /// <param name="propertyName">last element of path if node is not null</param>
        public delegate void WalkAction(JObject node, string propertyName);

        public static void WalkJson(JObject root, string jsonPath, MissingElementHandling missingElementHandling, WalkAction action)
        {
            var path = jsonPath.Split(new char[] { '.' });
            var node = root;

            for (int i = 0; i < path.Length - 1; ++i)
            {
                var step = path[i];
                var nextNode = node[step] as JObject;

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

                node = nextNode;
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

        [Obsolete]
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

        [Obsolete]
        public abstract class SwitchError
        {
        }

        [Obsolete]
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

        public static (ISet<string>, bool) Overwrite(JObject source, JObject destination)
        {
            return Overwrite(source, destination, null);
        }

        public static (ISet<string>, bool) Overwrite(JObject source, JObject destination, Func<string, bool> ignorePath)
        {
            var path = new List<string>();
            var changed = new HashSet<string>();
            var callbacks = new JsonUtil.SwitchCallbacks();

            callbacks.OnArray = (arrayA, arrayB) =>
            {
                if (arrayA == null && arrayB == null)
                    return false;

                if (arrayA == null && arrayB != null || arrayA != null && arrayB == null)
                    return true;

                if (arrayA.Count != arrayB.Count)
                    return true;

                var listA = arrayA.ToObject<List<object>>();
                var listB = arrayB.ToObject<List<object>>();

                var overwrite = !listA.SequenceEqual(listB);
                return overwrite;
            };
            callbacks.OnValue = (valueA, valueB) =>
            {
                if (valueA == null && valueB == null)
                    return false;

                if (valueA == null && valueB != null || valueA != null && valueB == null)
                    return true;

                var overwrite = !valueA.Equals(valueB);
                return overwrite;
            };
            callbacks.OnObject = (objectA, objectB) => // move data from A to B
            {
                foreach (var property in objectA.Properties())
                {
                    path.Add(property.Name);

                    if (!ignorePath?.Invoke(string.Join(".", path)) ?? false)
                    {
                        var doOverwrite =
                            objectB[property.Name] == null ||
                            JsonUtil.TypeSwitch(property.Value, objectB[property.Name], callbacks);

                        if (doOverwrite)
                        {
                            objectB[property.Name] = property.Value.DeepClone();
                            changed.Add(string.Join(".", path));
                        }
                    }

                    path.RemoveAt(path.Count - 1);
                }
                return false; //already overwritten
            };

            var needsToBeReplaced = JsonUtil.TypeSwitch(source, destination, callbacks);
            return (changed, needsToBeReplaced);
        }

        public static (ISet<string>, bool) RemoveUnused(JObject source, JObject destination, Func<string, bool> ignorePath)
        {
            var path = new List<string>();
            var changed = new HashSet<string>();
            var callbacks = new JsonUtil.SwitchCallbacks();

            callbacks.OnArray = (arrayA, arrayB) =>
            {
                return false;
            };
            callbacks.OnValue = (valueA, valueB) =>
            {
                return false;
            };
            callbacks.OnObject = (objectA, objectB) => // move data from A to B
            {
                var availableKeys = objectB.Properties().Select(p => p.Name).ToList();
                var allowedKeys = objectA.Properties().Select(p => p.Name).ToList();
                var keysToRemove = availableKeys.Where(p => !allowedKeys.Contains(p)).ToList();

                foreach (var property in keysToRemove)
                {
                    path.Add(property);
                    var fullPath = string.Join(".", path);
                    if (!ignorePath?.Invoke(fullPath) ?? false)
                    {
                        objectB.Remove(property);
                        changed.Add(fullPath);
                    }
                    path.RemoveAt(path.Count - 1);
                }

                foreach (var property in objectB.Properties())
                {
                    path.Add(property.Name);
                    var fullPath = string.Join(".", path);
                    if (!ignorePath?.Invoke(fullPath) ?? false)
                    {
                        JsonUtil.TypeSwitch(objectA[property.Name], objectB[property.Name], callbacks);
                    }
                    path.RemoveAt(path.Count - 1);
                }

                return false;
            };

            var needsToBeReplaced = JsonUtil.TypeSwitch(source, destination, callbacks);
            return (changed, needsToBeReplaced);
        }

        public static bool CopyIfAvailable(JObject src, string srcPath, JObject dst, string dstPath)
        {
            JToken result = null;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;
                result = node[propertyName];
            });

            if (result == null)
                return false;

            JsonUtil.WalkJson(dst, dstPath, JsonUtil.MissingElementHandling.Create, (node, propertyName) =>
            {
                node[propertyName] = result.DeepClone();
            });

            return true;
        }

        public static bool OverwriteIfAvailable(JObject src, string srcPath, JObject dst, string dstPath)
        {
            JToken result = null;
            var found = false;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;
                result = node[propertyName];
                found = true;
            });

            if (found)
                return false;

            JsonUtil.WalkJson(dst, dstPath, JsonUtil.MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;
                node[propertyName] = result?.DeepClone();
                found = true;
            });

            return found;
        }

        public static bool MoveIfAvailable(JObject src, string srcPath, JObject dst, string dstPath)
        {
            if (CopyIfAvailable(src, srcPath, dst, dstPath))
                return DeleteIfAvailable(src, srcPath);
            return false;
        }

        public static bool DeleteIfAvailable(JObject src, string srcPath)
        {
            var result = false;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;
                result = node.Remove(propertyName);
            });
            return result;
        }

        public static bool SetIfAvailable(JObject src, string srcPath, JToken value)
        {
            var found = false;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Stop, (node, propertyName) =>
            {
                if (node == null)
                    return;

                node[propertyName] = value != null ? value.DeepClone() : value;
                found = true;
            });
            return found;
        }

        public static bool SetIfUnavailable(JObject src, string srcPath, JToken value)
        {
            var found = false;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Create, (node, propertyName) =>
            {
                if (node[propertyName] != null)
                    return;

                node[propertyName] = value != null ? value.DeepClone() : value;
                found = true;
            });
            return found;
        }

        public static bool SetIfUnavailable(JObject src, string srcPath, Func<JToken> producer)
        {
            var found = false;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Create, (node, propertyName) =>
            {
                if (node[propertyName] != null)
                    return;

                var value = producer();
                node[propertyName] = value != null ? value.DeepClone() : value;
                found = true;
            });
            return found;
        }



        public enum IterateeResult
        {
            Continue,
            Stop
        }

        public static bool IterateIfAvailable(JObject src, string srcPath, Func<JToken, IterateeResult> iteratee)
        {
            return IterateIfAvailable<JToken>(src, srcPath, iteratee);
        }

        public static bool IterateIfAvailable<T>(JObject src, string srcPath, Func<T, IterateeResult> iteratee) where T : JToken
        {
            var found = false;
            AccessIfAvailable(src, srcPath, (node) =>
            {
                if(node is JObject jObject)
                {
                    found = true;
                    foreach (var propertyName in jObject.Properties())
                    {
                        var item = jObject[propertyName.Name];
                        if(item is T)                        {
                            var result = iteratee((T)item);
                            if (result == IterateeResult.Stop)
                                break;
                        }
                    }
                }
                else if(node is JArray jArray)
                {
                    found = true;
                    foreach (var item in jArray)
                    {
                        if (item is T)
                        {
                            var result = iteratee((T)item);
                            if (result == IterateeResult.Stop)
                                break;
                        }
                    }
                }
            });
            return found;
        }

        public static List<string> GetKeysIfAvailable(JObject src, string srcPath)
        {
            var result = new List<string>();
            AccessIfAvailable(src, srcPath, (node) =>
            {
                if (node is JObject jObject)
                {
                    result.AddRange(jObject.Properties().Select(p => p.Name));
                }
                else if (node is JArray jArray)
                {
                    result.AddRange(Enumerable.Range(0, jArray.Count).Select(x => x.ToString()));
                }
            });
            return result;
        }

        [Obsolete("Use ModifyIfAvailable<T>")]
        public static bool ReplaceArrayIfAvailable(JObject src, string srcPath, Func<JArray, JToken> converter)
        {
            return ModifyIfAvailable<JArray>(src, srcPath, converter);
        }

        public static bool ModifyIfAvailable(JObject src, string srcPath, Func<JToken, JToken> converter) => ModifyIfAvailable<JToken>(src, srcPath, converter);

        public static bool ModifyIfAvailable<T>(JObject src, string srcPath, Func<T, JToken> converter) where T : JToken
        {
            var found = false;
            JsonUtil.WalkJson(src, srcPath, JsonUtil.MissingElementHandling.Stop, (node, key) =>
            {
                if (node == null)
                    return;

                if (!(node[key] is T target))
                    return;

                found = true;

                var newValue = converter(target);

                if (newValue == null)
                    node.Remove(key);
                else
                    node[key] = newValue;
            });
            return found;
        }

        public static bool Remove(JObject src, string srcPath) => DeleteIfAvailable(src, srcPath);

        public static bool AccessIfAvailable(JObject src, string srcPath, Action<JToken> action)
        {
            var token = src.SelectToken(srcPath);
            if (token == null)
                return false;
            action(token);
            return true;
        }

        public static bool TryGet(JObject src, string srcPath, out JToken token)
        {
            token = src.SelectToken(srcPath);
            return token != null;
        }

        public static bool TryConvertValueToEnum<TEnum>(JToken value, out TEnum e) where TEnum : struct, IConvertible
        {
            e = default;

            if (!(value is JValue jValue))
                return false;

            if (jValue.Value == null)
                return false;

            if (jValue.Value is string)
            {
                if (Enum.TryParse<TEnum>((string)jValue.Value, true, out e))
                    return true;
                return false;
            }

            if (MathUtil.IsNumber(jValue.Value))
            {
                //why you do dis c#
                e = (TEnum)(object)(int)(long)jValue.Value;
                return true;
            }

            return false;
        }

        public static JArray ConvertArrayToEnum<TEnum>(JArray array) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            if (!typeT.IsEnum)
                throw new ArgumentException("Not an enum");

            var newArray = new JArray();
            foreach (var element in array)
            {
                if (TryConvertValueToEnum(element, out TEnum enumValue))
                    newArray.Add(enumValue);
            }
            return newArray;
        }

        public static JArray ConvertEnumArrayToString<TEnum>(JArray array) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            if (!typeT.IsEnum)
                throw new ArgumentException("Not an enum");
            var newArray = new JArray();
            foreach (var element in array)
                if (TryConvertEnumToString<TEnum>(element, out var name))
                    newArray.Add(name.ToUpperInvariant());
            return newArray;
        }

        public static bool TryConvertEnumToString<TEnum>(JToken value, out string enumName) where TEnum : struct, IConvertible
        {
            enumName = null;

            if (!(value is JValue jValue))
                return false;

            if (jValue.Value == null)
                return false;

            var eType = typeof(TEnum);

            if (jValue.Type == JTokenType.Integer || jValue.Type == JTokenType.Bytes)
            {
                enumName = Enum.GetName(eType, (int)(long)jValue);
                return true;
            }

            if (Enum.IsDefined(eType, jValue.Value))
            {
                enumName = Enum.GetName(eType, (int)(long)jValue);
                return true;
            }

            if (jValue.Type == JTokenType.String)
            {
                if (int.TryParse((string)jValue, out var iValue))
                {
                    enumName = Enum.GetName(eType, iValue);
                    return true;
                }
            }

            return false;
        }
    }
}