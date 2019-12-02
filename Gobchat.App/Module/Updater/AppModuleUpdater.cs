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

using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Core.UI;
using Gobchat.Updater;
using System;

namespace Gobchat.Core.Module.Updater
{
    public sealed class AppModuleUpdater2 : IApplicationModule
    {
        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            if (handler == null) throw new System.ArgumentNullException(nameof(handler));

            var configManager = container.Resolve<GobchatConfigManager>();
            var doUpdate = configManager.UserConfig.GetProperty<bool>("behaviour.checkForUpdate");

            if (doUpdate)
            {
                var updateManager = new UpdateManager();
                if (updateManager.CheckForUpdates())
                    handler.StopStartup = true;
            }
        }

        public void Dispose(IDIContext container)
        {
        }
    }

    public sealed class AppModuleUpdater : IApplicationModule
    {
        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            if (handler == null) throw new System.ArgumentNullException(nameof(handler));

            var configManager = container.Resolve<GobchatConfigManager>();
            var doUpdate = configManager.UserConfig.GetProperty<bool>("behaviour.checkForUpdate");

            if (!doUpdate)
                return;

            var update = GetUpdate(/*GobchatApplicationContext.ApplicationVersion*/ new Version(0, 0));
            if (update == null)
                return;

            var userRequest = AskUser(update);
            if (userRequest == UpdateFormDialog.UpdateType.Skip)
                return;

            if (userRequest == UpdateFormDialog.UpdateType.Auto)
            {
                //Try to download
                //In case this fails, fall back to manual

                //Try to unpack
                //Try to overwrite

                //In case any of these steps fail, fall back to manual (?) Provide user with instructions for a manual update? Link to readme?
            }

            if (userRequest == UpdateFormDialog.UpdateType.Manual)
            {
                //Show another form
                //Link to download page
                //Button with FileOpen for downloaded zip

                //Try to unpack
                //Try to overwrite

                //In case any of these steps fail, fall back to manual (?) Provide user with instructions for a manual update? Link to readme?
            }

            //    var synchronizer = container.Resolve<IUISynchronizer>();
            //    synchronizer.RunSync(() =>
            //     {
            //
            //     });

            //TODO
        }

        private UpdateFormDialog.UpdateType AskUser(IUpdateDescription update)
        {
            using (var notes = new UpdateFormDialog())
            {
                notes.UpdateHeadText = $"An update is available. Upgrade to version {update.Version}?";
                notes.UpdateNotes = update.PatchNotes;
                notes.ShowDialog();
                return notes.UpdateRequest;
            }
        }

        private IUpdateDescription GetUpdate(Version appVersion)
        {
            var provider = new GitHubUpdateProvider(appVersion, userName: "MarbleBag", repoName: "Gobchat");

            try
            {
                var updateDescription = provider.CheckForUpdate();
                if (!updateDescription.IsVersionAvailable || updateDescription.Version <= appVersion)
                    return null;
                return updateDescription;
            }
            catch (Exception e)
            {
                //TODO
                return null;
            }
        }

        public void Dispose(IDIContext container)
        {
        }
    }
}