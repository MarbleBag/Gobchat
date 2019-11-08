/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.UI.Web
{
    public static class CEFManager
    {
        private static bool isInitialized = false;
        private static bool isDisposed = false;

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
            if (!args.Name.StartsWith("CefSharp"))
                return null;

            string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
            string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                       Environment.Is64BitProcess ? "x64" : "x86",
                                       assemblyName);


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

            // Necessary to avoid input lag with a framerate limit below 60.
            cefSettings.CefCommandLineArgs["enable-begin-frame-scheduling"] = "1";

            // Allow websites to play sound even if the user never interacted with that site (pretty common for our overlays)
            cefSettings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";

            cefSettings.EnableAudio();

            // Enables software compositing instead of GPU compositing -> less CPU load but no WebGL
            cefSettings.SetOffScreenRenderingBestPerformanceArgs();

            CefSharp.Cef.Initialize(cefSettings, performDependencyCheck: true, browserProcessHandler: null);
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
