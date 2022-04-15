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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Gobchat.Core.UI
{
    public sealed class NotifyIconManager : INotifyIconManager, IDisposable
    {
        public bool Visible { get => _icon.Visible; set => _icon.Visible = value; }

        public System.Drawing.Icon Icon { get => _icon.Icon; set => _icon.Icon = value; }

        public string Text { get => _icon.Text; set => _icon.Text = value; }

        public string DefaultGroup
        {
            get
            {
                return _defaultGroup;
            }
            set
            {
                if (!_groups.Contains(value))
                    throw new UIElementNotFoundException(value); //TODO
                _defaultGroup = value;
            }
        }

        public event EventHandler OnIconClick;

        public event EventHandler<NotifyIconMenuEventArgs> OnMenuClick;

        public event EventHandler OnDispose;

        private string _defaultGroup;

        private readonly List<string> _groups;
        private readonly IDictionary<string, List<string>> _itemsByGroup;
        private readonly IDictionary<string, ToolStripMenuItem> _itemsById;

        private readonly NotifyIcon _icon;
        private bool _isDisposed;

        public NotifyIconManager() : this(null, null)
        {
        }

        public NotifyIconManager(IList<string> definedGroups) : this(definedGroups, null)
        {
        }

        public NotifyIconManager(IList<string> definedGroups, string defaultGroup)
        {
            _icon = new NotifyIcon();
            _icon.Click += OnEvent_NotifyIcon_Click;

            _groups = definedGroups != null ? new List<string>(definedGroups) : new List<string>();
            if (_groups.Count == 0)
                _groups.Add("default");

            _itemsByGroup = new Dictionary<string, List<string>>(_groups.Count);
            _groups.ForEach(s => _itemsByGroup[s] = new List<string>());

            _itemsById = new Dictionary<string, ToolStripMenuItem>();

            DefaultGroup = defaultGroup ?? _groups[0];

            _icon.ContextMenuStrip = new ContextMenuStrip();
            _icon.ContextMenuStrip.Opening += OnEvent_ContextMenu_Open;
            _icon.ContextMenuStrip.Closed += OnEvent_ContextMenu_Close;
        }

        public void AddMenu(string id, ToolStripMenuItem menu)
        {
            AddMenuToGroup(DefaultGroup, id, menu);
        }

        public void AddMenuToGroup(string groupId, string id, ToolStripMenuItem menu)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(NotifyIconManager));

            if (groupId == null) throw new ArgumentNullException(nameof(groupId));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (menu == null) throw new ArgumentNullException(nameof(menu));

            if (_itemsById.TryGetValue(id, out var storedMenu))
            {
                if (storedMenu != menu)
                    throw new UIElementIdAlreadyInUseException(id);
            }
            else
            {
                _itemsById.Add(id, menu);
                menu.Name = id;
                menu.Disposed += OnEvent_MenuItem_Dispose;
                menu.Click += OnEvent_MenuItem_Click;
            }

            if (!_itemsByGroup.TryGetValue(groupId, out var group))
                throw new UIElementNotFoundException(groupId);

            group.Add(id);
        }

        private void RemoveInnerMenu(string id)
        {
            foreach (var group in _itemsByGroup.Values)
                group.Remove(id);
            _itemsById.Remove(id);
        }

        public ToolStripMenuItem GetMenu(string id)
        {
            if (_itemsById.TryGetValue(id, out var result))
                return result;
            throw new UIElementNotFoundException(id);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;

            try
            {
                OnDispose?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
                //ignored
            }

            foreach (var item in _itemsById.Values.ToList())
            {
                item.Disposed -= OnEvent_MenuItem_Dispose;
                item.Dispose();
            }

            _itemsById.Clear();
            _itemsByGroup.Clear();
            _groups.Clear();

            _icon.Dispose();
        }

        private void OnEvent_MenuItem_Dispose(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item))
                return;
            var id = item.Name;
            RemoveInnerMenu(id);
        }

        private void OnEvent_MenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item))
                return;
            var id = item.Name;
            OnMenuClick?.Invoke(this, new NotifyIconMenuEventArgs(id));
        }

        //runs on UI thread
        private void OnEvent_NotifyIcon_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseEventArgs)
                if (mouseEventArgs.Button == MouseButtons.Left)
                    OnIconClick?.Invoke(this, new EventArgs());
        }

        //runs on UI thread
        private void OnEvent_ContextMenu_Open(object sender, CancelEventArgs evt)
        {
            foreach (var groupId in _groups)
            {
                if (_icon.ContextMenuStrip.Items.Count > 0)
                    _icon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                var itemIds = _itemsByGroup[groupId];
                foreach (var itemId in itemIds)
                {
                    var item = _itemsById[itemId];
                    _icon.ContextMenuStrip.Items.Add(item);
                }
            }

            evt.Cancel = false;
        }

        //runs on UI thread
        private void OnEvent_ContextMenu_Close(object sender, ToolStripDropDownClosedEventArgs e)
        {
            _icon.ContextMenuStrip.Items.Clear();
        }
    }
}