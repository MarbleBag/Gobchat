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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Gobchat.Core.Runtime;
using Microsoft.Win32;
using System.Linq;

namespace Gobchat.Module.Cef.Internal
{
}

namespace Gobchat.Module.Cef
{
    public sealed class AppModuleCefDependencyChecker : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string name);

        [DllImport("kernel32.dll")]
        private static extern void FreeLibrary(IntPtr handle);

        /// <summary>
        ///
        /// Requires: <see cref="IUISynchronizer"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleCefDependencyChecker()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            try
            {
                if (AnyRedistributableLibraryLoadable())
                    return;

                var registrySearcher = new RedistributableRegistryChecker();
                if (registrySearcher.HasRedistributables())
                    return;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Check on redistributables failed");
            }

            logger.Error("Chance that 2015+ redistributables are not installed");
        }

        public void Dispose()
        {
        }

        private bool AnyRedistributableLibraryLoadable()
        {
            return AnyRedistributableLibraryLoadable(new string[] { "msvcp140.dll", "msvcr140.dll", "msvcp170.dll", "msvcr170.dll" });
        }

        private bool AnyRedistributableLibraryLoadable(IEnumerable<string> libNames)
        {
            foreach (var libName in libNames)
            {
                var lib = LoadLibrary(libName);
                if (lib != IntPtr.Zero)
                {
                    FreeLibrary(lib);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// <see cref="//https://stackoverflow.com/questions/12206314/detect-if-visual-c-redistributable-for-visual-studio-2012-is-installed"/>
        /// </summary>
        internal sealed class RedistributableRegistryChecker
        {
            private interface ISearcher
            {
                bool Search();
            }

            private sealed class DoesKeyExist : ISearcher
            {
                private Func<RegistryKey> _search;

                public DoesKeyExist(Func<RegistryKey> search)
                {
                    _search = search;
                }

                public bool Search()
                {
                    return _search() != null;
                }
            }

            private sealed class HasSubKeyDisplayName : ISearcher
            {
                private static readonly string[] displayNames = new string[]
                    {
                        @"Microsoft Visual C++ 2015-2022 Redistributable",
                        @"Microsoft Visual C++ 2015-2019 Redistributable",
                        @"Microsoft Visual C++ 2017 Redistributable",
                        @"Microsoft Visual C++ 2015 Redistributable"
                    };

                private Func<RegistryKey> _search;

                public HasSubKeyDisplayName(Func<RegistryKey> search)
                {
                    _search = search;
                }

                public bool Search()
                {
                    var registryKey = _search();
                    if (registryKey == null)
                        return false;

                    foreach (var subKey in registryKey.GetSubKeyNames())
                    {
                        var dependency = registryKey.OpenSubKey(subKey);
                        var displayName = dependency.GetValue("DisplayName") as string;
                        if (displayName != null)
                            foreach (var expectedDisplayName in displayNames)
                                if (displayName.StartsWith(expectedDisplayName, false, CultureInfo.InvariantCulture))
                                    return true;
                    }

                    return false;
                }
            }

            public bool HasRedistributables()
            {
                var searchers = new ISearcher[]
                {
                new DoesKeyExist(() => Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\VC\Servicing\14.0\RuntimeMinimum")),
                new HasSubKeyDisplayName(() => Registry.ClassesRoot.OpenSubKey(@"Installer\Dependencies")),
                new HasSubKeyDisplayName(() => Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Dependencies")),
                };

                foreach (var searcher in searchers)
                    if (searcher.Search())
                        return true;
                return false;
            }
        }
    }
}
