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

using System;
using System.Windows.Forms;

namespace Gobchat.Module.Hotkey
{
    public interface IHotkeyManager
    {
        void RegisterHotkey(string id, Action callback);

        void UnregisterHotkey(string id);

        void AddKey(string id, Keys key);

        void RemoveKey(string id, Keys key);

        void RemoveAllKeys(string id);

        void ActivateHotkey(string id);

        void DeactivateHotkey(string id);

        void ToggleHotkey(string id, bool active);

        bool IsHotkeyActive(string id);

        void DeactivateAllHotkeys();

        string[] GetActiveHotkeys();

        string[] GetHotkeysForKey(Keys key);

        string[] GetAllHotkeys();

        Keys[] GetKeysForId(string id);
    }
}