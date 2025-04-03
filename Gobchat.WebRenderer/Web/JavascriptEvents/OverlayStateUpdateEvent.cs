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

namespace Gobchat.UI.Web
{
    namespace JavascriptEvents
    {
        public sealed class OverlayStateUpdateEvent : JSEvent
        {
            public bool isLocked;

            public OverlayStateUpdateEvent(bool isLocked) : base("OverlayStateUpdate")
            {
                this.isLocked = isLocked;
            }
        }
    }
}