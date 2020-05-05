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

using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Core.Runtime
{
    public abstract class AbstractGobchatApplicationContext : System.Windows.Forms.ApplicationContext
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region static functions

        public static string ResourceLocation
        {
            get { return System.IO.Path.Combine(ApplicationLocation, @"resources"); }
        }

        public static string UserDataLocation
        {
#if DEBUG
            get { return System.IO.Path.Combine(ApplicationLocation, "DebugConfig"); }
#else
            get { return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gobchat"); }
#endif
        }

        public static string UserLogLocation
        {
            get { return System.IO.Path.Combine(UserDataLocation, "log"); }
        }

        public static string UserConfigLocation
        {
            get { return System.IO.Path.Combine(UserDataLocation, "config"); }
        }

        public static string ApplicationLocation
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static GobVersion ApplicationVersion
        {
            get { return new GobVersion(InnerApplicationVersion); }
        }

        private static Version InnerApplicationVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static IUISynchronizer UISynchronizer { get; private set; }

        public static event EventHandler<GobchatExitEventArgs> OnGobchatExit;

        public static void ExitGobchat()
        {
            //make sure this does not run on the UI-thread, otherwise we may run into deadlocks, while we 'wait' for the shutdown, but stuff needs to be done on the ui-thread to do said shutdown
            Task.Run((Action)(() =>
            {
                OnGobchatExit?.Invoke((object)null, new GobchatExitEventArgs());
                Application.Exit();
            }));
        }

        #endregion static functions

        private Form _hiddenMainForm;
        private IndependendBackgroundWorker _appWorker;

        public AbstractGobchatApplicationContext()
        {
            AbstractGobchatApplicationContext.OnGobchatExit += (s, e) => Context_OnGobchatExit();
            Application.ApplicationExit += (s, e) => OnApplicationExit();

            _hiddenMainForm = new Form();
            UISynchronizer = new ContextSpecificUISynchronizer(WindowsFormsSynchronizationContext.Current);

            _appWorker = new IndependendBackgroundWorker();
            _appWorker.Start((token) => ApplicationStartupProcess(token));
        }

        private void Context_OnGobchatExit()
        {
            logger.Info("Start application shutdown");

            try
            {
                _appWorker?.Stop(true);
                _appWorker = null;
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error in appWorker");
            }

            try
            {
                ApplicationShutdownProcess();
            }
            finally
            {
                _hiddenMainForm?.Dispose();
                _hiddenMainForm = null;
                logger.Info("Shutdown complete");
            }
        }

        private void OnApplicationExit()
        {
            PerformApplicationUpdate();
        }

        private static void PerformApplicationUpdate()
        {
            var manager = NAppUpdate.Framework.UpdateManager.Instance;
            if (manager.UpdatesAvailable == 0)
                return; //nothing to do

            logger.Info("Install updates app");
            try
            {
                manager.ApplyUpdates(true, true, false);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, ex.Message);

                var dialogText = $"Unable to perform update. A backup folder was created and can be used to restore any damaged files.\nError:\n{ex.Message}";
                System.Windows.Forms.MessageBox.Show(dialogText, "Update error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        internal abstract void ApplicationStartupProcess(CancellationToken token);

        internal abstract void ApplicationShutdownProcess();
    }

    public sealed class GobchatExitEventArgs : EventArgs
    {
    }
}