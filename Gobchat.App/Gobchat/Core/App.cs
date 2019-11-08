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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Gobchat.Core
{
    //TODO whole .Core needs a proper cleanup.
    public sealed class App : IDisposable
    {

        private bool _isInitialized;
        private volatile bool _running;
        private SomeoneWhoDoesSomeWork _work;

        private Gobchat.UI.Forms.CefOverlayForm _overlay;
        private NotifyIcon _notifyIcon;
        private Task<bool> _workerThread;

        [STAThread]
        internal void Run()
        {
            if (!_isInitialized)
                InitializApp();

            Gobchat.UI.Web.CEFManager.Initialize();
            _overlay = new Gobchat.UI.Forms.CefOverlayForm();

            _workerThread = new System.Threading.Tasks.Task<bool>(() =>
             {
                 _running = true;
                 _work.Initialize(_overlay);
                 while (_running)
                 {
                     _work.Update();

                     Thread.Sleep(1000);
                 }
                 _work.Dispose();
                 return true;
             });
            _workerThread.Start();



            Application.EnableVisualStyles();
            Application.Run(_overlay);
        }

        private void InitializApp()
        {
            _isInitialized = true;

            Application.ApplicationExit += (sender, e) => Dispose();

            _work = new SomeoneWhoDoesSomeWork();


            _notifyIcon = new NotifyIcon();
            //_notifyIcon.Icon = new System.Drawing.Icon(@"resources/testico.ico"); // Eigenes Icon einsetzen
            _notifyIcon.Text = "Doppelklick mich!";   // Eigenen Text einsetzen
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenu = new ContextMenu();
            _notifyIcon.Click += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Test");
            };

            var menuItemCloseApp = new MenuItem();
            menuItemCloseApp.Text = "Shutdown";
            menuItemCloseApp.Click += (s, e) =>
            {
                Application.Exit();
            };
            _notifyIcon.ContextMenu.MenuItems.Add(menuItemCloseApp);

            var menuItemHideShow = new MenuItem();
            menuItemHideShow.Text = "Hide";
            menuItemHideShow.Click += (s, e) =>
            {
               // _overlay.Visible;
            };
            _notifyIcon.ContextMenu.MenuItems.Add(menuItemHideShow);
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            _running = false;

            _notifyIcon?.Dispose();
            _notifyIcon = null;

            _workerThread?.Wait(30000);
            _workerThread = null;

            // thread?.Join();
            // thread = null;

        }
    }
}
