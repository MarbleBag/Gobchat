/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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

using System;

namespace Gobchat.Core.Config
{
    [Serializable]
    public sealed class InvalidPropertyTypeException : PropertyException
    {
        private readonly Type _expectedType;
        private readonly Type _actualType;
        private readonly string _propertyPath;

        public InvalidPropertyTypeException(string propertyPath, Type expectedType, Type actualType) : base($"{expectedType} | {actualType} => {propertyPath}")
        {
            _expectedType = expectedType;
            _actualType = actualType;
            _propertyPath = propertyPath;
        }

        public InvalidPropertyTypeException(string propertyPath, string expectedType, string actualType) : base($"{expectedType} | {actualType} => {propertyPath}")
        {
            _expectedType = null;
            _actualType = null;
            _propertyPath = propertyPath;
        }

        private InvalidPropertyTypeException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}