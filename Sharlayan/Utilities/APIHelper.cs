﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="APIHelper.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   APIHelper.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sharlayan.Utilities
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

    using Newtonsoft.Json;

    using Sharlayan.Models;
    using Sharlayan.Models.Structures;
    using Sharlayan.Models.XIVDatabase;

    using StatusItem = Sharlayan.Models.XIVDatabase.StatusItem;

    internal static class APIHelper
    {
        private static WebClient _webClient = new WebClient
        {
            Encoding = Encoding.UTF8
        };

        private const string RESOURCE_URL = "https://raw.githubusercontent.com/FFXIVAPP/sharlayan-resources/master/{0}/global/{1}.json";


        public static IEnumerable<Signature> GetSignatures(ProcessModel processModel, string patchVersion = "latest")
        {
            var file = Path.Combine(MemoryHandler.Instance.LocalCacheDirectory, $"signatures-{patchVersion}.json");
            if (File.Exists(file) && MemoryHandler.Instance.UseLocalCache)
            {
                var json = FileResponseToJSON(file);
                return JsonConvert.DeserializeObject<IEnumerable<Signature>>(json, Constants.SerializerSettings);
            }
            else
            {
                var json = APIResponseToJSON(string.Format(RESOURCE_URL, "signatures", patchVersion));
                IEnumerable<Signature> resolved = JsonConvert.DeserializeObject<IEnumerable<Signature>>(json, Constants.SerializerSettings);
                File.WriteAllText(file, JsonConvert.SerializeObject(resolved, Formatting.Indented, Constants.SerializerSettings), Encoding.GetEncoding(932));
                return resolved;
            }
        }

        public static StructuresContainer GetStructures(ProcessModel processModel, string patchVersion = "latest")
        {
            var file = Path.Combine(MemoryHandler.Instance.LocalCacheDirectory, $"structures-{patchVersion}.json");
            if (File.Exists(file) && MemoryHandler.Instance.UseLocalCache)            
                return EnsureClassValues<StructuresContainer>(file);            
            return APIResponseToClass<StructuresContainer>(file, string.Format(RESOURCE_URL, "structures", patchVersion));
        }

        public static void GetStatusEffects(ConcurrentDictionary<uint, StatusItem> statusEffects, string patchVersion = "latest")
        {
            var file = Path.Combine(MemoryHandler.Instance.LocalCacheDirectory, "statuses.json");
            if (File.Exists(file) && MemoryHandler.Instance.UseLocalCache)
            {
                EnsureDictionaryValues(statusEffects, file);
            }
            else
            {
                // It will be either there or not
                // APIResponseToDictionary(statusEffects, file, $"https://raw.githubusercontent.com/FFXIVAPP/sharlayan-resources/master/xivdatabase/{patchVersion}/statuses.json");
            }
        }

        public static void GetActions(ConcurrentDictionary<uint, ActionItem> actions, string patchVersion = "latest")
        {
            var file = Path.Combine(MemoryHandler.Instance.LocalCacheDirectory, "actions.json");
            if (File.Exists(file) && MemoryHandler.Instance.UseLocalCache)
            {
                EnsureDictionaryValues(actions, file);
            }
            else
            {
                // It will be either there or not
                // APIResponseToDictionary(actions, file, $"https://raw.githubusercontent.com/FFXIVAPP/sharlayan-resources/master/xivdatabase/{patchVersion}/actions.json");
            }
        }


        public static void GetZones(ConcurrentDictionary<uint, MapItem> mapInfos, string patchVersion = "latest")
        {
            // These ID's link to offset 7 in the old JSON values.
            // eg: "map id = 4" would be 148 in offset 7.
            // This is known as the TerritoryType value
            // - It maps directly to SaintCoins map.csv against TerritoryType ID
            var file = Path.Combine(MemoryHandler.Instance.LocalCacheDirectory, "zones.json");
            if (File.Exists(file) && MemoryHandler.Instance.UseLocalCache)
            {
                EnsureDictionaryValues(mapInfos, file);
            }
            else
            {
                // It will be either there or not
                // APIResponseToDictionary(mapInfos, file, $"https://raw.githubusercontent.com/FFXIVAPP/sharlayan-resources/master/xivdatabase/{patchVersion}/zones.json");
            }
        }

        private static T APIResponseToClass<T>(string file, string uri)
        {
            var json = APIResponseToJSON(uri);
            var resolved = JsonConvert.DeserializeObject<T>(json, Constants.SerializerSettings);

            File.WriteAllText(file, JsonConvert.SerializeObject(resolved, Formatting.Indented, Constants.SerializerSettings), Encoding.UTF8);

            return resolved;
        }

        private static void APIResponseToDictionary<T>(ConcurrentDictionary<uint, T> dictionary, string file, string uri)
        {
            var json = APIResponseToJSON(uri);
            ConcurrentDictionary<uint, T> resolved = JsonConvert.DeserializeObject<ConcurrentDictionary<uint, T>>(json, Constants.SerializerSettings);

            foreach (KeyValuePair<uint, T> kvp in resolved)
            {
                dictionary.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
            }

            File.WriteAllText(file, JsonConvert.SerializeObject(dictionary, Formatting.Indented, Constants.SerializerSettings), Encoding.UTF8);
        }

        private static string APIResponseToJSON(string uri)
        {
            return _webClient.DownloadString(uri);
        }

        private static T EnsureClassValues<T>(string file)
        {
            var json = FileResponseToJSON(file);
            return JsonConvert.DeserializeObject<T>(json, Constants.SerializerSettings);
        }

        private static void EnsureDictionaryValues<T>(ConcurrentDictionary<uint, T> dictionary, string file)
        {
            var json = FileResponseToJSON(file);
            ConcurrentDictionary<uint, T> resolved = JsonConvert.DeserializeObject<ConcurrentDictionary<uint, T>>(json, Constants.SerializerSettings);

            foreach (KeyValuePair<uint, T> kvp in resolved)
            {
                dictionary.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
            }
        }

        private static string FileResponseToJSON(string file)
        {
            using (var streamReader = new StreamReader(file))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}