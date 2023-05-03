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

using Gobchat.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gobchat.Core.UI
{
    public interface INotifyIconManager : IDisposable
    {
        bool Visible { get; set; }

        System.Drawing.Icon Icon { get; set; }

        string Text { get; set; }

        string DefaultGroup { get; set; }

        event EventHandler OnIconClick;

        event EventHandler<NotifyIconMenuEventArgs> OnMenuClick;

        event EventHandler OnDispose;

        void AddMenu(string id, ToolStripMenuItem menu);

        void AddMenuToGroup(string groupId, string id, ToolStripMenuItem menu);

        ToolStripMenuItem GetMenu(string id);
    }

    public sealed class NotifyIconMenuEventArgs : EventArgs
    {
        public string Id { get; }

        public NotifyIconMenuEventArgs(string id)
        {
            Id = id;
        }
    }
}