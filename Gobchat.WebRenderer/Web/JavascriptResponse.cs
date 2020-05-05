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

namespace Gobchat.UI.Web
{
    internal sealed class JavascriptResponse : IJavascriptResponse
    {
        private CefSharp.JavascriptResponse _response;

        internal JavascriptResponse(global::CefSharp.JavascriptResponse response)
        {
            _response = response;
        }

        public string Message { get => _response.Message; }

        public bool Success { get => _response.Success; }

        public object Result { get => _response.Result; }
    }
}