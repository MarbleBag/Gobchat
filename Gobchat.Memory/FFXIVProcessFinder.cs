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

using NLog;
using Sharlayan;
using Sharlayan.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Gobchat.Memory
{
    public enum GameLanguage
    {
        English, French, Japanese, German, Chinese, Korean
    }

    /// <summary>
    /// Checks for an active FFXIV process and sets the process id for Sharlayan
    /// </summary>
    internal sealed class FFXIVProcessFinder
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool FFXIVProcessValid { get; private set; } = false;
        public int FFXIVProcessId { get; private set; } = 0;

        public GameLanguage GameLanguage { get; set; } = GameLanguage.English;

        public FFXIVProcessFinder()
        {
        }

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

            //var gameLanguage = System.Enum.GetName(typeof(GameLanguage), GameLanguage);
            //MemoryHandler.Instance.SetProcess(processModel, useLocalCache: true, gameLanguage: gameLanguage);
            MemoryHandler.Instance.SignaturesFoundEvent += OnEvent_SignatureFound;
            MemoryHandler.Instance.SetProcess(processModel, useLocalCache: true);

            while (Scanner.Instance.IsScanning)
            {
                logger.Debug("Scanning for FFXIV signatures...");
                Thread.Sleep(1000);
            }

            FFXIVProcessValid = true;
            MemoryHandler.Instance.SignaturesFoundEvent -= OnEvent_SignatureFound;
            return true;
        }

        private void OnEvent_SignatureFound(object sender, Sharlayan.Events.SignaturesFoundEvent e)
        {
            if (e.Signatures.ContainsKey(Signatures.ChatLogKey))
                logger.Info("Chatlog signature found");
            else
                logger.Info("Chatlog signature not found");

            if (e.Signatures.ContainsKey(Signatures.CharacterMapKey))
                logger.Info("Actors signature found");
            else
                logger.Info("Actors signature not found");
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