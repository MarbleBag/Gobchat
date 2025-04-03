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

using System;

namespace Gobchat.Memory.Chat.Token
{
    public class AutotranslateToken : IChatlogToken
    {
        public byte[] Code { get; }

        public AutotranslateToken(byte[] code)
        {
            this.Code = code;
        }

        private string GetCompleteKey()
        {
            return BitConverter.ToString(Code).Replace("-", "");
        }

        public string GetKey()
        {
            var key = GetCompleteKey();
            if (key.StartsWith("0"))
                return key.Substring(1, key.Length - 3);
            return key.Substring(0, key.Length - 2); //Code always ends on 0x03. We don't need that.
        }

        public override string ToString()
        {
            return $"{nameof(AutotranslateToken)}[key={GetCompleteKey()}]";
        }
    }
}