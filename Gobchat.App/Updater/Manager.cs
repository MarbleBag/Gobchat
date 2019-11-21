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
using System.Threading.Tasks;

namespace Gobchat.Updater
{
    public abstract class Manager
    {
        internal sealed class QueryResult<T>
        {
            public T Result { get; }

            public Exception Exception { get; }

            public bool Successful { get { return HasData && !HasException; } }
            public bool HasData { get { return Result != null; } }
            public bool HasException { get { return Exception != null; } }

            public QueryResult(T result, Exception exception)
            {
                if (result == null && exception == null)
                    throw new ArgumentNullException($"{nameof(result)} | {nameof(exception)}", "Either must not be null");
                Result = result;
                Exception = exception;
            }

            public override string ToString()
            {
                if (Successful)
                    return $"UpdateQuery => {Result.ToString()}";
                else
                    return $"UpdateQuery => {Exception.ToString()}";
            }
        }

        internal Task<QueryResult<T>> RunAsync<T>(Func<T> function)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    var result = function.Invoke();
                    return new QueryResult<T>(result, null);
                }
                catch (AggregateException e1)
                {
                    return new QueryResult<T>(default, e1.Flatten());
                }
                catch (Exception e3)
                {
                    return new QueryResult<T>(default, e3);
                }
            });
            return task;
        }
    }
}