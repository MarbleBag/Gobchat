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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Gobchat.Memory
{
    /// <summary>
    /// Provides functionality to search, connect and monitor FFXIV processes for the Sharlayan framework.
    /// While this class itself is not a singleton, Sharlayan is and thus this class itself becomes one because it's not possible to connect to more than one FFXIV process per app process.
    /// </summary>
    internal sealed class ProcessConnector
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool FFXIVProcessValid { get; private set; }

        public int FFXIVProcessId { get; private set; } = 0;

        public event EventHandler OnConnectionLost;

        public ProcessConnector()
        {
        }

        public List<int> GetFFXIVProcesses()
        {
            var processes = Process.GetProcessesByName("ffxiv_dx11");
            return processes.Select(p => p.Id).ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="processId"></param>
        /// <returns>true if the connection to the given process id is still valid or was successful created</returns>
        public bool ConnectToProcess(int processId)
        {
            if (FFXIVProcessValid)
            {
                if (FFXIVProcessId == processId)
                    return true;
                else
                    Disconnect();
            }

            var process = Process.GetProcessById(processId);
            if (process == null || !process.ProcessName.Equals("ffxiv_dx11"))
                return false;

            ConnectTo(process);

            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;
            FFXIVProcessValid = true;

            return FFXIVProcessValid;
        }

        public void Disconnect()
        {
            MemoryHandler.Instance.UnsetProcess();
            FFXIVProcessValid = false;
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