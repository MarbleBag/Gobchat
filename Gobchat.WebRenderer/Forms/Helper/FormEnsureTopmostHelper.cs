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

using System;
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Helper
{
    internal sealed class FormEnsureTopmostHelper : IDisposable
    {
        public Form TargetForm { get; set; }
        public bool Active { get; private set; } = false;
        public int UpdateInterval { get; set; }

        private System.Threading.Timer timer;

        public FormEnsureTopmostHelper(Form target, int updateInterval)
        {
            TargetForm = target;
            UpdateInterval = updateInterval;
        }

        public void Start()
        {
            Stop();
            Active = true;
            timer = new System.Threading.Timer((state) => RunTask(), null, 0, UpdateInterval);
        }

        public void Stop()
        {
            timer?.Dispose();
            timer = null;
            Active = false;
        }

        private void RunTask()
        {
            Action action = () => EnsureTopMost();
            TargetForm.Invoke(action);
        }

        private void EnsureTopMost()
        {
            NativeMethods.SetWindowPos(TargetForm.Handle, NativeMethods.HWND_TOPMOST,
               0, 0, 0, 0,
               NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE);
        }

        public void Dispose()
        {
            Stop();
            TargetForm = null;
        }
    }
}