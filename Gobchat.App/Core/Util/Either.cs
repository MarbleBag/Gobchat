/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

namespace Gobchat.Core.Util
{
    public class Either<L, R>
    {
        public bool IsLeft { get; }

        public L Left { get; } = default;

        public R Right { get; } = default;

        public Either(L left)
        {
            IsLeft = true;
            Left = left;
        }

        public Either(R right)
        {
            IsLeft = false;
            Right = right;
        }

        public override string ToString()
        {
            if (IsLeft)
                return $"Either[Left={(Left != null ? Left : default)}]";
            else
                return $"Either[Right={(Right != null ? Right : default)}]";
        }
    }
}