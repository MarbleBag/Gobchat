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
    public interface IJavascriptResponse
    {
        //
        // Summary:
        //     Error message
        string Message { get; }

        //
        // Summary:
        //     Was the javascript executed successfully
        bool Success { get; }

        //
        // Summary:
        //     Javascript response
        object Result { get; }
    }
}