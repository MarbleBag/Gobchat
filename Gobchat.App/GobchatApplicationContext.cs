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
    internal class GobchatApplicationContext : ApplicationContext
    {
        //TODO not nice
        public static string ResourceFolder;

        public static string UserConfigFolder;

        public new Form MainForm { get { return _overlayForm; } private set { } }

        private NotifyIcon _notifyIcon;
        private CefOverlayForm _overlayForm;

        private GobchatBackgroundWorker _backgroundWorker;

        public GobchatApplicationContext()
        {
            ResourceFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources");
            UserConfigFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Gobchat\config");

            Application.ApplicationExit += (s, e) => OnApplicationExit();
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
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon(@"resources/gobtray.ico");
            _notifyIcon.Text = "Gobchat";
            _notifyIcon.Visible = true;

            _notifyIcon.ContextMenu = new ContextMenu();

            var itemHideShowChat = new MenuItem("Hide");
            _overlayForm.VisibleChanged += (s, e) =>
            {
                if (_overlayForm == null)
                    return;

                if (_overlayForm.Visible)
                    itemHideShowChat.Text = "Hide";
                else
                    itemHideShowChat.Text = "Show";
            };

            EventHandler hideShowAction = (s, e) =>
                        {
                            if (_overlayForm == null)
                                return;
                            System.Diagnostics.Debug.WriteLine("clicked me!");
                            _overlayForm.Visible = !_overlayForm.Visible;
                        };

            itemHideShowChat.Click += hideShowAction;
            //   _notifyIcon.Click += hideShowAction;
            _notifyIcon.ContextMenu.MenuItems.Add(itemHideShowChat);

            var itemUnPauseChat = new MenuItem("Pause");
            itemUnPauseChat.Click += (s, e) =>
            {
                //TODO while the chat parsing continues, no new chat messages are sent to the ui
            };
            _notifyIcon.ContextMenu.MenuItems.Add(itemUnPauseChat);

            _notifyIcon.ContextMenu.MenuItems.Add("-");

            var itemCloseApplication = new MenuItem("Close");
            itemCloseApplication.Click += (s, e) => Application.Exit(); //TODO
            _notifyIcon.ContextMenu.MenuItems.Add(itemCloseApplication);
        }

        private void OnApplicationExit()
        {
            //TODO try-catch the shit out of this

            System.Diagnostics.Debug.WriteLine("Disposing Context");

            _backgroundWorker?.Stop(true);
            _backgroundWorker = null;

            _notifyIcon?.Dispose();
            _notifyIcon = null;

            _overlayForm?.Close();
            _overlayForm?.Dispose();
            _overlayForm = null;

            Gobchat.UI.Web.CEFManager.Dispose();
        }
    }
}