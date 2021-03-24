/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
using Gobchat.Core.Runtime;
using Gobchat.Module.Language.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gobchat.Module.Language
{
    public sealed class AppModuleLanguage : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IDIContext _container;
        private IConfigManager _configManager;

        private LocaleManager _localeManager;

        /// <summary>
        /// Provides: <see cref="ILocaleManager"/> <br></br>
        /// </summary>
        public AppModuleLanguage()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _configManager = _container.Resolve<IConfigManager>();

            _localeManager = new LocaleManager();
            _configManager.AddPropertyChangeListener("behaviour.language", true, true, ConfigManager_UpdateLanguage);

            //update ui thread
            _localeManager.OnLocaleChange += (s, e) =>
            {
                _container.Resolve<IUISynchronizer>().RunSync(() =>
                {
                    CultureInfo.DefaultThreadCurrentCulture = e.Locale;
                    CultureInfo.DefaultThreadCurrentUICulture = e.Locale;
                    Thread.CurrentThread.CurrentCulture = e.Locale;
                    Thread.CurrentThread.CurrentUICulture = e.Locale;
                    Resources.Culture = e.Locale;
                });
            };

            _container.Register<ILocaleManager>((c, p) => _localeManager);
        }

        public void Dispose()
        {
            _container.Unregister<ILocaleManager>();
            _container = null;

            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateLanguage);
            _configManager = null;

            _localeManager = null;
        }

        private void ConfigManager_UpdateLanguage(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var selectedLanguage = config.GetProperty<string>("behaviour.language");
            var culture = CultureInfo.GetCultureInfo(selectedLanguage);
            _localeManager.SwitchCulture(culture);
        }
    }
}