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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Gobchat.Module.Hotkey.Internal
{
    internal sealed partial class HotkeyManager : IHotkeyManager, IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly OSLevelHotkeyRegister _hotkeyRegister;

        private readonly Dictionary<string, HotkeyData> _hotkeyData = new Dictionary<string, HotkeyData>();
        private readonly object _lock = new object();

        public HotkeyManager(OSLevelHotkeyRegister hotkeyRegister)
        {
            _hotkeyRegister = hotkeyRegister ?? throw new ArgumentNullException(nameof(hotkeyRegister));
        }

        public void Dispose()
        {
            //TODO
            lock (_lock)
            {
                _hotkeyData.Clear();
                _hotkeyRegister.Dispose();
            }
        }

        public void RegisterHotkey(string id, Action callback)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_lock)
            {
                if (_hotkeyData.ContainsKey(id))
                    throw new ArgumentException(); //TODO
                _hotkeyData.Add(id, new HotkeyData(id, callback));
            }
        }

        public void UnregisterHotkey(string id)
        {
            if (id == null)
                return;

            lock (_lock)
            {
                if (!_hotkeyData.ContainsKey(id))
                    return;
                DeactivateHotkey(id);
                _hotkeyData.Remove(id);
            }
        }

        public void ActivateHotkey(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            lock (_lock)
            {
                if (!_hotkeyData.TryGetValue(id, out var data))
                    throw new ArgumentException(nameof(id)); //TODO

                if (data.IsActive)
                    return;

                data.IsActive = true;
                foreach (var key in data.Keys)
                    _hotkeyRegister.RegisterHotKey(key, data.Action);
            }
        }

        public void DeactivateHotkey(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            lock (_lock)
            {
                if (!_hotkeyData.TryGetValue(id, out var data))
                    throw new ArgumentException(nameof(id)); //TODO

                if (!data.IsActive)
                    return;

                data.IsActive = false;
                foreach (var key in data.Keys)
                    _hotkeyRegister.UnregisterHotKey(key, data.Action);
            }
        }

        public void DeactivateAllHotkeys()
        {
            lock (_lock)
            {
                foreach (var data in _hotkeyData.Values)
                {
                    if (!data.IsActive)
                        continue;

                    data.IsActive = false;
                    foreach (var key in data.Keys)
                        _hotkeyRegister.UnregisterHotKey(key, data.Action);
                }
            }
        }

        public void AddKey(string id, Keys key)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (key == Keys.None) throw new ArgumentException(nameof(key)); //TODO

            lock (_lock)
            {
                if (!_hotkeyData.TryGetValue(id, out var data))
                    throw new ArgumentException(nameof(id)); //TODO
                if (data.Keys.Contains(key))
                    return;

                data.Keys.Add(key);
                if (data.IsActive)
                    _hotkeyRegister.RegisterHotKey(key, data.Action);
            }
        }

        public void RemoveAllKeys(string id)
        {
            if (id == null) return;
            lock (_lock)
            {
                if (!_hotkeyData.TryGetValue(id, out var data))
                    return;

                if (data.IsActive)
                    foreach (var key in data.Keys)
                        _hotkeyRegister.UnregisterHotKey(key, data.Action);

                data.Keys.Clear();
            }
        }

        public void RemoveKey(string id, Keys key)
        {
            if (id == null) return;
            lock (_lock)
            {
                if (!_hotkeyData.TryGetValue(id, out var data))
                    return;

                var removed = data.Keys.Remove(key);
                if (removed && data.IsActive)
                    _hotkeyRegister.UnregisterHotKey(key, data.Action);
            }
        }

        public string[] GetActiveHotkeys()
        {
            lock (_lock)
            {
                return _hotkeyData.Values.Where(e => e.IsActive).Select(e => e.Id).ToArray();
            }
        }

        public string[] GetAllHotkeys()
        {
            lock (_lock)
            {
                return _hotkeyData.Keys.ToArray();
            }
        }

        public string[] GetHotkeysForKey(Keys key)
        {
            lock (_lock)
            {
                return _hotkeyData.Values.Where(e => e.Keys.Contains(key)).Select(e => e.Id).ToArray();
            }
        }

        public Keys[] GetKeysForId(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            lock (_lock)
            {
                if (_hotkeyData.TryGetValue(id, out var data))
                    return data.Keys.ToArray();
            }
            return Array.Empty<Keys>();
        }

        public bool IsHotkeyActive(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            lock (_lock)
            {
                if (_hotkeyData.TryGetValue(id, out var data))
                    return data.IsActive;
            }
            return false;
        }

        public void ToggleHotkey(string id, bool active)
        {
            if (active)
                ActivateHotkey(id);
            else
                DeactivateHotkey(id);
        }
    }
}