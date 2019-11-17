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

using Gobchat.Updater;
using System;
using System.Windows.Forms;

namespace Gobchat
{
    public static class App
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (CheckVersion())
                return;

            if (CheckDependencies())
                return;

            StartApp();
        }

        private static bool CheckVersion()
        {
            var versionChecker = new GobchatVersionChecker();
            var stopStart = versionChecker.Check();
            return stopStart;
        }

        private static bool CheckDependencies()
        {
            return false; //TODO
        }

        public static void StartApp()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GobchatApplicationContext());
        }
    }
}