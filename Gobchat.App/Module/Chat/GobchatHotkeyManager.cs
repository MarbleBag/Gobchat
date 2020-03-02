/*******************************************************************************
 * Copyright (C) 2020 MarbleBag
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

using Gobchat.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Module.Chat
{
    internal sealed class GobchatHotkeyManager
    {
        private sealed class HotkeyData
        {
            public string Id { get; }
            public string Label { get; set; }
            public string Hotkey { get; set; }
            public bool IsActive { get; set; }
            public Action Callback { get; }

            public HotkeyData(string id, string label, Action callback)
            {
                Id = id ?? throw new ArgumentNullException(nameof(id));
                Label = label ?? throw new ArgumentNullException(nameof(label));
                Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }
        }

        private Dictionary<string, HotkeyData> _hotkeys = new Dictionary<string, HotkeyData>();
        private IGobchatConfigManager _configManager;

        public GobchatHotkeyManager(IGobchatConfigManager configManager)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _configManager.OnActiveProfileChange += OnEvent_ProfileChanged;
        }

        public void RegisterHotkey(string id, string name, Action callback, bool activate = true)
        {
            //TODO
            _hotkeys.Add(id, new HotkeyData(id, name, callback));
            _configManager.AddPropertyChangeListener(id, OnEvent_HotkeyChanged);
            if (activate)
                ActivateHotkey(id);
        }

        private void OnEvent_HotkeyChanged(IGobchatConfigManager sender, ProfilePropertyChangedCollectionEventArgs e)
        {
            //TODO
        }

        private void OnEvent_ProfileChanged(object sender, ActiveProfileChangedEventArgs e)
        {
            //TODO
        }

        public void UnregisterHotkey(string id)
        {
        }

        public void DeactivateHotkey(string id)
        {
        }

        public void ActivateHotkey(string id)
        {
        }

        private void UpdateHotkeys()
        {
        }
    }
}