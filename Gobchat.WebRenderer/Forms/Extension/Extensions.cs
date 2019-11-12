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

using System;
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Extension
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void AsyncInvoke(this Control control, Action code)
        {
            if (code == null) return;
            if (control.InvokeRequired)
                control.BeginInvoke(code);
            else
                code.Invoke();
            
        }

        /// <summary>
        /// Executes the Action synchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void SyncInvoke(this Control control, Action code)
        {
            if (code == null) return;
            if (control.InvokeRequired)
                control.Invoke(code);
            else
                code.Invoke();
            
        }
    }

    public static class FormExtension
    {
        public static void InvokeAsyncOnUIThread(this Form form, Action action)
        {
            if (action == null) return;
            if (form.InvokeRequired) form.BeginInvoke(action);
            else action.Invoke();
        }

        public static void InvokeSyncOnUIThread(this Form form, Action action)
        {
            if (action == null) return;
            if (form.InvokeRequired) form.Invoke(action);
            else action.Invoke();
        }
    }
}
