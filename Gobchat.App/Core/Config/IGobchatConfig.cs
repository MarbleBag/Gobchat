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

namespace Gobchat.Core.Config
{
    public interface IGobchatConfig
    {
        event EventHandler<PropertyChangedEventArgs> OnPropertyChange;

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidPropertyTypeException"></exception>
        /// <exception cref="InvalidPropertyPathException"></exception>
        T GetProperty<T>(string key);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidPropertyTypeException"></exception>
        /// <exception cref="InvalidPropertyPathException"></exception>
        T GetProperty<T>(string key, T defaultValue);

        bool HasProperty(string key);

        void SetProperty(string key, object value);

        JObject ToJson();

        void SetProperties(JObject json);

        void Synchronize(JObject root);
    }
}