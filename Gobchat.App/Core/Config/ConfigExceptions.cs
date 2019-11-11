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

using System;

namespace Gobchat.Core.Config
{
    public class ConfigException : System.Exception
    {
        public ConfigException()
        {
        }

        public ConfigException(String message) : base(message)
        {
        }

        public ConfigException(String message, System.Exception innerException) : base(message, innerException)
        {
        }

        public ConfigException(System.Exception innerException) : base(String.Empty, innerException)
        {
        }
    }

    public class ConfigLoadException : ConfigException
    {
        public ConfigLoadException(String message) : base(message)
        {
        }

        public ConfigLoadException(String message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

    public class PropertyException : ConfigException
    {
        public PropertyException()
        {
        }

        public PropertyException(String message) : base(message)
        {
        }

        public PropertyException(String message, System.Exception innerException) : base(message, innerException)
        {
        }

        public PropertyException(System.Exception innerException) : base(String.Empty, innerException)
        {
        }
    }

    public class MissingPropertyException : PropertyException
    {
        public MissingPropertyException(string propertyPath) : base(propertyPath)
        {
        }
    }

    public class InvalidPropertyPathException : PropertyException
    {
        private readonly string _reason;
        private readonly string _propertyPath;

        public InvalidPropertyPathException(string reason, string propertyPath) : base($"{reason} => {propertyPath}")
        {
            _reason = reason;
            _propertyPath = propertyPath;
        }
    }

    public class InvalidPropertyTypeException : PropertyException
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
    }
}