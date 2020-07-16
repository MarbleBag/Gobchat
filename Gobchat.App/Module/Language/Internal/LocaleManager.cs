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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gobchat.Core.Resource;
using Gobchat.Core.Runtime;

namespace Gobchat.Module.Language.Internal
{
    internal sealed class LocaleManager : ILocaleManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private event EventHandler<LocaleEventArgs> _localeChange;

        private LocalFolderResourceResolver _resourceResolver;
        private HjsonResourceLoader _resourceLoader;

        private object _lock = new object();

        public LocaleManager()
        {
            var languagePath = System.IO.Path.Combine(GobchatContext.ResourceLocation, @"lang");
            _resourceResolver = new LocalFolderResourceResolver(languagePath);
            _resourceLoader = new HjsonResourceLoader();

            ActiveCulture = DefaultCulture;
        }

        public CultureInfo DefaultCulture { get => CultureInfo.GetCultureInfo("en"); }

        public CultureInfo ActiveCulture { get; private set; }

        public event EventHandler<LocaleEventArgs> OnLocaleChange
        {
            add
            {
                lock (_lock)
                {
                    _localeChange += value;
                    value(this, new LocaleEventArgs(ActiveCulture));
                }
            }
            remove
            {
                _localeChange -= value;
            }
        }

        public string GetLocale(CultureInfo cultureInfo)
        {
            return cultureInfo.Name;
        }

        public void SwitchCulture(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                cultureInfo = DefaultCulture;

            lock (_lock)
            {
                if (ActiveCulture.Equals(cultureInfo))
                    return;
                ActiveCulture = cultureInfo;
            }

            logger.Info($"Set locale to {ActiveCulture}");
            _localeChange?.Invoke(this, new LocaleEventArgs(ActiveCulture));
        }

        public IResourceBundle GetResourceBundle(string filename)
        {
            //TODO cache, auto update, selective resource loader depending on resource type
            var bundle = new CachedResourceBundle(_resourceResolver, _resourceLoader, filename, GetLocale(DefaultCulture));
            bundle.SetLocale(GetLocale(ActiveCulture));
            return bundle;
        }

        public R Build<R, M>() where R : MessageRegistry<M>
        {
            var registryType = typeof(R);
            var emptyConstructor2 = registryType.GetConstructor(Type.EmptyTypes);
            var registry = (R)emptyConstructor2.Invoke(Array.Empty<object>());

            Update(registry);
            return registry;
        }

        public IMessageRegistry<M> BuildMessageRegistry<M>()
        {
            var registry = new MessageRegistry<M>();
            Update(registry);
            return registry;
        }

        private void Update<M>(MessageRegistry<M> registry)
        {
            var messageType = typeof(M);
            var emptyConstructor = messageType.GetConstructor(Type.EmptyTypes);
            var message = (M)emptyConstructor.Invoke(Array.Empty<object>());
            registry.UpdateMessages(message);
        }
    }
}