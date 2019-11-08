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
        public new Form MainForm { get { return _overlayForm; } private set { } }

        private NotifyIcon _notifyIcon;
        private CefOverlayForm _overlayForm;

        private GobchatBackgroundWorker _backgroundWorker;

        public GobchatApplicationContext()
        {
            Application.ApplicationExit += (s, e) => OnApplicationExit();
            OnApplicationStart();
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
            GobchatBackgroundWorker.Job job = (CancellationToken token) =>
            {
                //TODO for now
                var _work = new SomeoneWhoDoesSomeWork();
                //TODO maybe use a kind of UI interface instead?
                //TODO try-catch the shit out of this
                _work.Initialize(_overlayForm);
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

            EventHandler hideShowAction = (s, e) =>
                        {
                            if (_overlayForm == null)
                                return;

                            //TODO
                            //lazy
                            if (_overlayForm.Visible)
                            {
                                _overlayForm.Visible = false;
                                itemHideShowChat.Text = "Show";
                            }
                            else
                            {
                                _overlayForm.Visible = true;
                                itemHideShowChat.Text = "Hide";
                            }
                        };

            itemHideShowChat.Click += hideShowAction;
            _notifyIcon.Click += hideShowAction;
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

            _overlayForm?.Close();
            _overlayForm?.Dispose();
            _overlayForm = null;

            _notifyIcon?.Dispose();
            _notifyIcon = null;

            _backgroundWorker.Stop(true);

            Gobchat.UI.Web.CEFManager.Dispose();
        }
    }
}
