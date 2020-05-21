/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

using Gobchat.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Module.Updater.Internal
{
    public interface IUpdateProvider
    {
        IUpdateDescription CheckForUpdate();
    }
}

namespace Gobchat.Module.Updater.Internal
{
    public interface IUpdateDescription
    {
        GobVersion Version { get; }

        bool IsVersionAvailable { get; }

        string DirectDownloadUrl { get; }

        string PageUrl { get; }

        string PatchNotes { get; }
    }
}