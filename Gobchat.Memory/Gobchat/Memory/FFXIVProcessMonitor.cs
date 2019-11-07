/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Gobchat.Memory
{
    /// <summary>
    /// Checks for an active FFXIV process and sets the process id for Sharlayan
    /// </summary>
    internal class FFXIVProcessFinder
    {
        public bool FFXIVProcessValid { get; private set; } = false;
        public int FFXIVProcessId { get; private set; } = 0;

        public bool CheckProcess()
        {
            Process[] processes = Process.GetProcessesByName("ffxiv_dx11");
            if (IsActiveProcessValid(processes))
                return false;

            var processChanged = FFXIVProcessValid;

            Process process = processes.FirstOrDefault();

            FFXIVProcessValid = false;
            FFXIVProcessId = 0;

            if (process == null)
                return processChanged;

            FFXIVProcessId = process.Id;

            var processModel = new ProcessModel
            {
                IsWin64 = System.Environment.Is64BitProcess,
                Process = process
            };

            MemoryHandler.Instance.SetProcess(processModel);

            while (Scanner.Instance.IsScanning)
            {
                Thread.Sleep(1000);
                Debug.WriteLine("Scanning...");
            }

            FFXIVProcessValid = true;

            return true;
        }

        private bool IsActiveProcessValid(Process[] processes)
        {
            if (FFXIVProcessValid)
                if (processes.Any(p => p.Id == FFXIVProcessId))
                    return true;
            return false;
        }
    }

}
