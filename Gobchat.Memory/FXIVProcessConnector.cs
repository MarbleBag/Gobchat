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

using Sharlayan;
using Sharlayan.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Gobchat.Memory
{
    /// <summary>
    /// Checks for an active FFXIV process and sets the process id for Sharlayan
    /// </summary>
    internal sealed class FFXIVProcessConnector
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool FFXIVProcessValid { get => MemoryHandler.Instance.IsAttached; }
        public int FFXIVProcessId { get; private set; } = 0;

        public event EventHandler OnConnectionLost;

        public FFXIVProcessConnector()
        {
        }

        public bool ConnectToFFXIV()
        {
            if (FFXIVProcessValid)
                return true;

            var process = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            if (process == null)
                return false;

            ConnectTo(process);

            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;

            return FFXIVProcessValid;
        }

        public void Disconnect()
        {
            MemoryHandler.Instance.UnsetProcess();
        }

        private void ConnectTo(Process process)
        {
            var processModel = new ProcessModel
            {
                IsWin64 = System.Environment.Is64BitProcess,
                Process = process
            };

            FFXIVProcessId = process.Id;
            MemoryHandler.Instance.SetProcess(processModel, useLocalCache: true);
            while (Scanner.Instance.IsScanning)
            {
                logger.Debug("Scanning for FFXIV signatures...");
                Thread.Sleep(1000);
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Disconnect();
            OnConnectionLost?.Invoke(this, new EventArgs());
        }
    }
}