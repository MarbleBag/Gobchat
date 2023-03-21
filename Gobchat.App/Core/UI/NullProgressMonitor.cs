/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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

using NLog;
using System.Threading;

namespace Gobchat.Core.UI
{
    public sealed class NullProgressMonitor : Runtime.IProgressMonitor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CancellationTokenSource _source = new CancellationTokenSource();

        public string StatusText { get; set; }
        public double Progress { get; set; }

        public CancellationToken GetCancellationToken()
        {
            return _source.Token;
        }

        public void Log(string log)
        {
            logger.Info($"ProgressMonitor: {log}");
        }
    }
}