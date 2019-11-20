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

namespace Gobchat.Core.Runtime
{
    public sealed class ApplicationUpdateComponent : IApplicationComponent
    {
        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            if (handler == null) throw new System.ArgumentNullException(nameof(handler));

            var updateManager = new Updater.UpdateManager();
            if (updateManager.CheckForUpdates())
                handler.StopStartup = true;
        }

        public void Dispose(IDIContext container)
        {
        }
    }
}