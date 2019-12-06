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

namespace Gobchat.Core.Util
{
    public sealed class Exceptional<T>
    {
        public bool HasException { get; private set; }
        public Exception Exception { get; private set; }
        public T Value { get; private set; }

        public Exceptional(T value)
        {
            HasException = false;
            Value = value;
        }

        public Exceptional(Exception exception)
        {
            HasException = true;
            Exception = exception;
        }

        public Exceptional(Func<T> getValue)
        {
            try
            {
                Value = getValue();
                HasException = false;
            }
            catch (Exception exc)
            {
                Exception = exc;
                HasException = true;
            }
        }

        public override string ToString()
        {
            return (this.HasException ? Exception.GetType().Name : ((Value != null) ? Value.ToString() : "null"));
        }
    }
}