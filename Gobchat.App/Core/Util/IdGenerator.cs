/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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

using System.Collections.Generic;

namespace Gobchat.Core.Util
{
    internal sealed class IdGenerator
    {
        public static string GenerateId(int lenght)
        {
            var builder = new System.Text.StringBuilder();
            while (lenght > 0)
            {
                var path = System.IO.Path.GetRandomFileName().Replace(".", "");
                if (path.Length > lenght)
                    builder.Append(path.Substring(0, lenght));
                else
                    builder.Append(path);
                lenght -= path.Length;
            }
            return builder.ToString();
        }

        public static string GenerateNewId(int length, ICollection<string> usedIds)
        {
            do
            {
                var id = GenerateId(length);
                if (!usedIds.Contains(id))
                    return id;
            } while (true);
        }
    }
}