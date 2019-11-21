﻿/*******************************************************************************
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Core.Runtime
{
    public abstract class AbstractGobchatApplicationContext : System.Windows.Forms.ApplicationContext
    {
        private class ContextSpecificSynchronizer : IUISynchronizer
        {
            private class EmptyUnhandledExceptionHandler : IUnhandledExceptionHandler
            {
                public void Handle(Exception exception)
                {
                    //TODO do logging
                }
            }

            private SynchronizationContext _context;

            public ContextSpecificSynchronizer(SynchronizationContext uiContext)
            {
                this._context = uiContext;
            }

            public void RunAsync(Action action) => RunAsync(action, null);

            public void RunAsync(Action action, IUnhandledExceptionHandler handler)
            {
                if (handler != null)
                    _context.Post((s) => { try { action.Invoke(); } catch (Exception e) { handler.Handle(e); } }, null);
                else
                    _context.Post((s) => action.Invoke(), null);
            }

            public void RunSync(Action action) => _context.Send((s) => action.Invoke(), null);
        }

        public static string ResourceLocation
        {
            get { return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources"); }
        }

        public static string UserConfigLocation
        {
            get { return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Gobchat\config"); }
        }

        public static string ApplicationLocation
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static Version ApplicationVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static IUISynchronizer UISynchronizer { get; private set; }

        private Form _hiddenMainForm;
        private IndependendBackgroundWorker _appWorker;

        public AbstractGobchatApplicationContext()
        {
            Application.ApplicationExit += (s, e) => OnApplicationExit();

            _hiddenMainForm = new Form();
            UISynchronizer = new ContextSpecificSynchronizer(WindowsFormsSynchronizationContext.Current);

            System.Diagnostics.Debug.WriteLine("MAIN THREAD: " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            _appWorker = new IndependendBackgroundWorker();
            _appWorker.Start((token) => ApplicationStartupProcess(token));
        }

        private void OnApplicationExit()
        {
            System.Diagnostics.Debug.WriteLine("ON APPLICATION EXIT: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            try
            {
                _appWorker?.Stop(true);
                _appWorker = null;
            }
            catch (Exception e)
            {
                //TODO log
            }

            try
            {
                ApplicationShutdownProcess();
            }
            finally
            {
                _hiddenMainForm?.Dispose();
                _hiddenMainForm = null;
            }
        }

        internal abstract void ApplicationStartupProcess(CancellationToken token);

        internal abstract void ApplicationShutdownProcess();
    }
}