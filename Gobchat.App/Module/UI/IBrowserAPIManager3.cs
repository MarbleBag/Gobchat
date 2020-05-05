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

using Gobchat.UI.Web;
using System;

namespace Gobchat.Module.UI
{
    public interface IBrowserAPIManager3
    {
        bool IsUIReady { get; }

        event EventHandler<UIReadChangedEventArgs> OnUIReadyChanged;

        void DispatchEventToBrowser(global::Gobchat.UI.Web.JavascriptEvents.JSEvent jsEvent);

        void RegisterAPI(IBrowserAPI api);

        void UnregisterAPI(IBrowserAPI api);
    }
}