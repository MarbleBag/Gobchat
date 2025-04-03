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

namespace Gobchat.Core.Runtime
{
    public interface IApplicationModule : System.IDisposable
    {
        //TODO replace methods with attributes and make dependency injection via attributes possible

        /// <summary>
        /// It is not guranteed that "Initialize" and "Dispose" will be executed on the same thread
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="container"></param>
        void Initialize(ApplicationStartupHandler handler, IDIContext container);

        /// <summary>
        /// It is not guranteed that "Initialize" and "Dispose" will be executed on the same thread
        /// </summary>
        new void Dispose();
    }
}