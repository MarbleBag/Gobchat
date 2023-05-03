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

using Gobchat.Core.Config;
using Gobchat.Core.Util;
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
            var keyActive = true; //sender.GetProperty<bool>($"{_configKey}.active");
            var _newKeys = StringToKeys(keyString);

            try
            {
                if (_newKeys != _keys)
                {
                    _hotkeyManager.RemoveKey(_hotkeyId, _keys);
                    _keys = _newKeys;
                    if (_keys != Keys.None)
                        _hotkeyManager.AddKey(_hotkeyId, _newKeys);
                }

                //  if (_keys != Keys.None)
                _hotkeyManager.ToggleHotkey(_hotkeyId, keyActive);
            }
            catch (HotkeyRegisterException ex1)
            {
                logger.Fatal(ex1, $"Invalid Hotkey for {_hotkeyId}");
                _chatManager.EnqueueMessage(Core.Chat.SystemMessageType.Error, StringFormat.Format(Resources.Module_Hotkey_ConfigHotkeyUpdater_InvalidHotkey, _hotkeyName, ex1.Message));
            }
            catch (Exception ex2)
            {
                logger.Fatal(ex2, $"Unknown Hotkey exception {_hotkeyId}");
                _chatManager.EnqueueMessage(Core.Chat.SystemMessageType.Error, StringFormat.Format(Resources.Module_Hotkey_ConfigHotkeyUpdater_InvalidHotkey, _hotkeyName, ex2.Message));
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
