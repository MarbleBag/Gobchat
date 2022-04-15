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
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Gobchat.UI.Web
{
    public static class CEFManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static bool isInitialized = false;
        private static bool isDisposed = false;

        public static string CefAssemblyLocation { get; set; } = string.Empty;

        public static void Initialize()
        {
            if (isInitialized)
                return;
            if (isDisposed)
                throw new NotImplementedException(); //TODO ERROR, was already initialized

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            InitializeCefSharp();

            isInitialized = true;
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!args.Name.StartsWith("CefSharp", true, CultureInfo.InvariantCulture))
                return null;

            string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";

            var archSpecificPath = AppDomain.CurrentDomain.BaseDirectory; //AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            if (CefAssemblyLocation != null && CefAssemblyLocation.Length > 0)
            {
                if (Path.IsPathRooted(CefAssemblyLocation))
                {
                    archSpecificPath = Path.Combine(CefAssemblyLocation, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);
                }
                else
                {
                    archSpecificPath = Path.Combine(archSpecificPath, CefAssemblyLocation, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);
                }
            }
            else
            {
                archSpecificPath = Path.Combine(archSpecificPath, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);
            }

            //logger.Trace(() => $"Assembly CefSharp Lookup: {archSpecificPath}");

            /* string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                        Environment.Is64BitProcess ? "x64" : "x86",
                                        assemblyName);*/

            return File.Exists(archSpecificPath)
                       ? System.Reflection.Assembly.LoadFile(archSpecificPath)
                       : null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp()
        {
            CefSharp.Cef.EnableHighDPISupport();

            var cefSettings = new CefSharp.OffScreen.CefSettings()
            {
                MultiThreadedMessageLoop = true,
                WindowlessRenderingEnabled = true,
            };

            CefSharp.CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            CefSharp.CefSharpSettings.ConcurrentTaskExecution = true;

            cefSettings.CefCommandLineArgs["enable-begin-frame-scheduling"] = "1";
            cefSettings.CefCommandLineArgs["allow-file-access-from-files"] = "1";
            cefSettings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";

            cefSettings.EnableAudio();

            cefSettings.LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cef_debug.log");
            cefSettings.LogSeverity = CefSharp.LogSeverity.Error;

            cefSettings.SetOffScreenRenderingBestPerformanceArgs();

            CefSharp.Cef.Initialize(cefSettings, performDependencyCheck: true, browserProcessHandler: null);
        }

        //TODO implementd for ui thread integration
        private sealed class BrowserProcessHandler : CefSharp.IBrowserProcessHandler
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void OnContextInitialized()
            {
                throw new NotImplementedException();
            }

            public void OnScheduleMessagePumpWork(long delay)
            {
                throw new NotImplementedException();

                if (delay <= 0)
                {
                    //DO NOW
                }
                else
                {
                    //CHECK IF WORK IS ALREADY DELAYED
                    //IF SO, CANCEL
                    //SCHEDULE NEW WORK IN DELAY TIME
                }
            }
        }

        public static void Dispose()
        {
            if (isDisposed)
                return;
            if (!isInitialized)
                return;

            CefSharp.Cef.Shutdown();

            isDisposed = true;
        }
    }
}