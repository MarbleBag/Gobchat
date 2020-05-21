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

using Gobchat.Core.Config;
using Gobchat.Module.Chat;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Gobchat.Module.Hotkey
{
    public sealed class ConfigHotkeyUpdater : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IConfigManager _configManager;
        private IHotkeyManager _hotkeyManager;
        private IChatManager _chatManager;

        private readonly string _hotkeyId;
        private readonly string _configKey;
        private readonly string _hotkeyName;

        private Keys _keys = Keys.None;

        public event Action OnHotkey;

        public ConfigHotkeyUpdater(string hotkeyid, string hotkeyName, IConfigManager configManager, string configKey, IHotkeyManager hotkeyManager, IChatManager chatManager)
        {
            _hotkeyId = hotkeyid ?? throw new ArgumentNullException(nameof(hotkeyid));
            _configKey = configKey ?? throw new ArgumentNullException(nameof(configKey));
            _hotkeyName = hotkeyName ?? throw new ArgumentNullException(nameof(hotkeyName));

            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _hotkeyManager = hotkeyManager ?? throw new ArgumentNullException(nameof(hotkeyManager));
            _chatManager = chatManager ?? throw new ArgumentNullException(nameof(chatManager));

            _hotkeyManager.RegisterHotkey(_hotkeyId, Hotkey_Action);
            _configManager.AddPropertyChangeListener(_configKey, true, true, ConfigManager_Update);
        }

        private void ConfigManager_Update(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var keyString = sender.GetProperty<string>(_configKey);
            //var keyString = sender.GetProperty<string>($"{_configKey}.key");
            var keyActive = sender.GetProperty<bool>($"{_configKey}.active", true);
            var _newKeys = StringToKeys(keyString);

            try
            {
                if (_keys != _newKeys)
                {
                    _hotkeyManager.RemoveKey(_hotkeyId, _keys);
                    _hotkeyManager.AddKey(_hotkeyId, _newKeys);
                }

                _hotkeyManager.ToggleHotkey(_hotkeyId, keyActive);

                _keys = _newKeys;
            }
            catch (HotkeyRegisterException e)
            {
                logger.Fatal(e, $"Invalid Hotkey for {_hotkeyId}");
                _chatManager.EnqueueMessage(Core.Chat.SystemMessageType.Error, $"Invalid Hotkey for {_hotkeyName}: {e.Message}");
            }
        }

        private void Hotkey_Action()
        {
            OnHotkey?.Invoke();
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_Update);
            _hotkeyManager.UnregisterHotkey(_hotkeyId);

            _configManager = null;
            _hotkeyManager = null;
            _chatManager = null;
        }

        private static Keys StringToKeys(string keys)
        {
            if (keys == null || keys.Length == 0)
                return Keys.None;

            var split = keys.Split(new char[] { '+' }).Select(s => s.Trim().ToUpper(CultureInfo.InvariantCulture));

            Keys nKeys = new Keys();
            foreach (var s in split)
            {
                switch (s)
                {
                    case "SHIFT":
                        nKeys |= Keys.Shift;
                        break;

                    case "CTRL":
                        nKeys |= Keys.Control;
                        break;

                    case "ALT":
                        nKeys |= Keys.Alt;
                        break;

                    default:
                        var result = Core.Util.EnumUtil.ObjectToEnum<Keys>(s);
                        if (result.HasValue)
                            nKeys |= result.Value;
                        break;
                }
            }

            if ((nKeys & ~Keys.Modifiers) != 0)
                return nKeys;
            return Keys.None;
        }
    }
}