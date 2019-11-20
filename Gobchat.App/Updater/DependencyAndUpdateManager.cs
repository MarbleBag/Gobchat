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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq;

using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Gobchat.Core.Runtime;

namespace Gobchat.Updater
{
    public abstract class Manager
    {
        internal sealed class QueryResult<T>
        {
            public T Result { get; }

            public Exception Exception { get; }

            public bool Successful { get { return HasData && !HasException; } }
            public bool HasData { get { return Result != null; } }
            public bool HasException { get { return Exception != null; } }

            public QueryResult(T result, Exception exception)
            {
                if (result == null && exception == null)
                    throw new ArgumentNullException($"{nameof(result)} | {nameof(exception)}", "Either must not be null");
                Result = result;
                Exception = exception;
            }

            public override string ToString()
            {
                if (Successful)
                    return $"UpdateQuery => {Result.ToString()}";
                else
                    return $"UpdateQuery => {Exception.ToString()}";
            }
        }

        internal Task<QueryResult<T>> RunAsync<T>(Func<T> function)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    var result = function.Invoke();
                    return new QueryResult<T>(result, null);
                }
                catch (AggregateException e1)
                {
                    return new QueryResult<T>(default, e1.Flatten());
                }
                catch (Exception e3)
                {
                    return new QueryResult<T>(default, e3);
                }
            });
            return task;
        }
    }

    public class UpdateManager : Manager
    {
        private IList<IUpdateProvider> _updateProviders = new List<IUpdateProvider>();

        public UpdateManager()
        {
            //TODO look up via reflection?
            _updateProviders.Add(new GitHubUpdateProvider());
        }

        public bool CheckForUpdates()
        {
            var task = Task.Run<List<QueryResult<IUpdateData>>>(async () => await QueryForUpdates().ConfigureAwait(true));
            var queryResult = task.Result;

            if (queryResult.Count == 0)
                return false; //no updates

            //TODO

            var successfulQueries = queryResult.Where(e => e.Successful).Select(e => e.Result).Where(e => e.IsUpdateAvailable).OrderByDescending(e => e.Version);
            var newestUpdate = successfulQueries.FirstOrDefault();

            if (newestUpdate == null)
            {
                //TODO check for an unhandled exception
                //TODO check for errors

                //TODO inform user about errors

                //MessageBox.Show(string.Format(Resources.UpdateCheckException, ex.ToString()), Resources.UpdateCheckTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //TODO check for errors

                return false;
            }

            var currentVersion = GobchatApplicationContext.ApplicationVersion;
            if (currentVersion >= newestUpdate.Version)
                return false;

            var dialogTest = $"{newestUpdate.DownloadDescription}\n\nPressing Yes will open a webpage with the newest version and stops Gobchat from starting.";
            var dialogResult = MessageBox.Show(dialogTest, "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (DialogResult.Yes == dialogResult)
            {
                var downloadLink = newestUpdate.UserDownloadLink;
                Process downloadProcess = System.Diagnostics.Process.Start(downloadLink);
                return true;
            }

            return false;
        }

        private async Task<List<QueryResult<IUpdateData>>> QueryForUpdates()
        {
            var tasks = _updateProviders.Select(resolver => RunAsync(() => resolver.CheckForUpdate())).ToList();
            var allTasks = Task.WhenAll(tasks);
            var completedTasks = await allTasks.ConfigureAwait(false);
            return completedTasks.ToList();
        }
    }

    public class DependencyManager : Manager
    {
        private IList<IDependencyProvider> _dependencyProviders = new List<IDependencyProvider>();

        public DependencyManager()
        {
            _dependencyProviders.Add(new CefDependencyProvider(""));
        }

        public bool CheckForMissingDependencies()
        {
            foreach (var dependencyProvider in _dependencyProviders)
            {
                var data = dependencyProvider.CheckForMissingDependencies();
                if (data == null || !data.HasMissingDependency)
                    continue;
            }

            //TODO

            return true;
        }

        public static DialogResult DialogBox(string title, string promptText, ref string value, string button1 = "OK", string button2 = "Cancel", string button3 = null)
        {
            using (Form form = new Form())
            {
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button button_1 = new Button();
                Button button_2 = new Button();
                Button button_3 = new Button();

                int buttonStartPos = 228; //Standard two button position

                if (button3 != null)
                    buttonStartPos = 228 - 81;
                else
                {
                    button_3.Visible = false;
                    button_3.Enabled = false;
                }

                form.Text = title;

                // Label
                label.Text = promptText;
                label.SetBounds(9, 20, 372, 13);
                label.Font = new Font("Microsoft Tai Le", 10, FontStyle.Regular);

                // TextBox
                if (value == null)
                {
                }
                else
                {
                    textBox.Text = value;
                    textBox.SetBounds(12, 36, 372, 20);
                    textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                }

                button_1.Text = button1;
                button_2.Text = button2;
                button_3.Text = button3 ?? string.Empty;
                button_1.DialogResult = DialogResult.OK;
                button_2.DialogResult = DialogResult.Cancel;
                button_3.DialogResult = DialogResult.Yes;

                button_1.SetBounds(buttonStartPos, 72, 75, 23);
                button_2.SetBounds(buttonStartPos + 81, 72, 75, 23);
                button_3.SetBounds(buttonStartPos + (2 * 81), 72, 75, 23);

                label.AutoSize = true;
                button_1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                button_2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                button_3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, button_1, button_2 });
                if (button3 != null)
                    form.Controls.Add(button_3);
                if (value != null)
                    form.Controls.Add(textBox);

                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = button_1;
                form.CancelButton = button_2;

                DialogResult dialogResult = form.ShowDialog();
                value = textBox.Text;
                return dialogResult;
            }
        }
    }
}