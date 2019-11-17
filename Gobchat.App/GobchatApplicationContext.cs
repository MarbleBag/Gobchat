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

using Gobchat.Core;
using Gobchat.UI.Forms;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Gobchat
{
    internal sealed class GobchatApplicationContext : ApplicationContext
    {
        //TODO not nice
        public static string ResourceFolder;

        public static string UserConfigFolder;

        public new Form MainForm { get { return _overlayForm; } private set { } }

        private NotifyIconManager _notifyIconManager;
        private CefOverlayForm _overlayForm;

        private GobchatBackgroundWorker _backgroundWorker;

        public GobchatApplicationContext()
        {
            ResourceFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources");
            UserConfigFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Gobchat\config");

            Application.ApplicationExit += (s, e) => OnApplicationExit();

            //TODO
            // Turn this into the application core
            // Start other parts of the app as components
            // Initialize components on start up
            // Dispose components on shut down
            // Provide a type of UIManager
            // UIManager allows access to UI widgets via ID
            // UIManager allows to run tasks on the UI thread
            // Look for a simple DI framework which supports a context tree for injection

            //   try
            //    {
            OnApplicationStart();
            //    }
            //    catch (Exception e) //TODO
            //    {
            //         OnApplicationExit();
            //         throw;
            //    }
        }

        private void OnApplicationStart()
        {
            InitializeMainForm();
            InitializeNotifyIcon();
            InitializeBackgroundWorker();

            //   Debug.WriteLine(Application.LocalUserAppDataPath);
            //   Debug.WriteLine(Application.UserAppDataPath);
            //   Debug.WriteLine(Application.CommonAppDataPath);
        }

        private void InitializeBackgroundWorker()
        {
            // Initialized in UI thread
            // Maybe add a kind of event queue to the UI thread?
            var _work = new SomeoneWhoDoesSomeWork();
            _work.Initialize(_overlayForm);

            GobchatBackgroundWorker.Job job = (CancellationToken token) =>
            {
                //TODO try-catch the shit out of this
                while (!token.IsCancellationRequested)
                {
                    _work.Update();

                    if (token.IsCancellationRequested)
                        break;

                    Thread.Sleep(1000);
                }
                //TODO maybe hook this up with the application?
                _work.Dispose();
            };

            _backgroundWorker = new GobchatBackgroundWorker();
            _backgroundWorker.Start(job);
        }

        private void InitializeMainForm()
        {
            Gobchat.UI.Web.CEFManager.Initialize();
            _overlayForm = new Gobchat.UI.Forms.CefOverlayForm();
            //_overlayForm.FormClosed
            _overlayForm.Show();
        }

        private void InitializeNotifyIcon()
        {
            _notifyIconManager = new NotifyIconManager();
            _notifyIconManager.TrayIconVisible = true;

            _overlayForm.VisibleChanged += (s, e) =>
            {
                if (_overlayForm == null)
                    return;
                _notifyIconManager.SetHideShowText(_overlayForm.Visible ? NotifyIconManager.HideShowState.Hide : NotifyIconManager.HideShowState.Show);
            };
            _notifyIconManager.SetHideShowText(_overlayForm.Visible ? NotifyIconManager.HideShowState.Hide : NotifyIconManager.HideShowState.Show);

            _notifyIconManager.OnMenuClick += (s, e) =>
                {
                    switch (e.NotifyMenuItem)
                    {
                        case NotifyIconManager.NotifyMenuItem.CloseApplication:
                            Application.Exit();
                            break;

                        case NotifyIconManager.NotifyMenuItem.HideShow:
                            if (_overlayForm != null)
                                _overlayForm.Visible = !_overlayForm.Visible;
                            break;

                        case NotifyIconManager.NotifyMenuItem.ReloadUI:
                            if (_overlayForm != null)
                                _overlayForm.Reload();
                            break;
                    }
                };
        }

        private void OnApplicationExit()
        {
            //TODO try-catch the shit out of this

            System.Diagnostics.Debug.WriteLine("Disposing Context");

            _backgroundWorker?.Stop(true);
            _backgroundWorker = null;

            _notifyIconManager?.Dispose();
            _notifyIconManager = null;

            _overlayForm?.Close();
            _overlayForm?.Dispose();
            _overlayForm = null;

            Gobchat.UI.Web.CEFManager.Dispose();
        }
    }
}