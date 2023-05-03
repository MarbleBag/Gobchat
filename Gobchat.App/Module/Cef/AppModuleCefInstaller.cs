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
using System.IO;
using System.Windows.Forms;
using Gobchat.Core.Runtime;
using Gobchat.Core.UI;
using Gobchat.Core.Util;
using Gobchat.Module.Cef.Internal;
using NLog;

namespace Gobchat.Module.Cef
{
    public sealed partial class AppModuleCefInstaller : IApplicationModule
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///
        /// Requires: <see cref="IUISynchronizer"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleCefInstaller()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var uiSynchronizer = container.Resolve<IUISynchronizer>();
            ProgressDisplayForm progressDisplay = null;

            var cefFolder = Path.Combine(GobchatContext.ApplicationLocation, "libs", "cef");
            var patcherFolder = Path.Combine(GobchatContext.ApplicationLocation, "patch");
            var installer = new CefInstaller(cefFolder, patcherFolder);
            if (installer.IsCorrectCefVersionAvailable())
                return;

            var needUpdate = installer.DoesCefNeedAnUpdate();

            logger.Info(needUpdate ? "CEF needs to be updated" : "CEF not available");
            var dialogResult = MessageBox.Show(
                needUpdate ? Resources.Module_Cef_Dialog_CefUpdate_Text : Resources.Module_Cef_Dialog_CefMissing_Text,
                "Gobchat",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (dialogResult != DialogResult.OK)
            {
                handler.StopStartup = true;
                return;
            }

            try
            {
                uiSynchronizer.RunSync(() =>
                {
                    progressDisplay = new ProgressDisplayForm();
                    progressDisplay.Show();
                });

                using (var progressMonitor = new ProgressMonitorAdapter(progressDisplay))
                {
                    if (needUpdate)
                    {
                        try
                        {
                            installer.RemoveCef(progressMonitor);
                        }catch(Exception e)
                        {
                            logger.Log(LogLevel.Fatal, e, () => "CEF uninstall failed");
                            throw;
                        }
                    }

                    try
                    {
                        installer.DownloadCef(progressMonitor);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Fatal, e, () => "CEF download failed");
                        throw;
                    }
                    try
                    {
                        installer.ExtractCef(progressMonitor);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.Fatal, e, () => "CEF extraction failed");
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Fatal("CEF installation failed");
                logger.Fatal(e);

                MessageBox.Show(
                    StringFormat.Format(Resources.Module_Cef_Dialog_InstallFailed_Text, e.Message),
                   Resources.Module_Cef_Dialog_InstallFailed_Title,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error
               );

                handler.StopStartup = true;
            }
            finally
            {
                uiSynchronizer.RunSync(() => progressDisplay.Dispose());
            }
        }

        public void Dispose()
        {
        }
    }
}