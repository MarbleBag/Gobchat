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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Gobchat.LogConverter
{
    public partial class LogConverterForm : Form
    {
        private readonly LogConverterManager _manager;
        private LogFormaterContainer _chosenFormater;

        public LogConverterForm()
        {
            InitializeComponent();
            _manager = new LogConverterManager();

            foreach (var id in _manager.GetFormaters())
                cbFormater.Items.Add(id);
            cbFormater.SelectedIndex = cbFormater.Items.Count - 1;

            this.Text = $"Log Converter (v{GobchatContext.InnerApplicationVersion})";
        }

        public event EventHandler OnCancel;

        public double Progress
        {
            get { return pgbProgress.Value / pgbProgress.Maximum; }
            set { pgbProgress.Value = Math.Min(pgbProgress.Maximum, (int)Math.Round(value * pgbProgress.Maximum)); }
        }

        public void AppendLog(string log)
        {
            txtLog.AppendText(log + "\n");
        }

        public void ClearLog()
        {
            txtLog.Text = "";
        }

        private void OnEvent_btnFileSelector_Click(object sender, EventArgs e)
        {
            string selectedFile = null;
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = GobchatContext.UserLogLocation;
                dialog.RestoreDirectory = true;
                dialog.Filter = "Log files (*.log)|*.log";
                if (dialog.ShowDialog() == DialogResult.OK)
                    selectedFile = dialog.FileName;
            }

            if (selectedFile == null)
                return;

            txtFileSelection.Text = selectedFile;
        }

        private void OnEvent_cbFormater_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendLayout();
            settingPanel.Controls.Clear();
            _chosenFormater = _manager.GetFormater(cbFormater.Text);
            if (_chosenFormater.Settings != null)
            {
                //_chosenFormater.Settings.Dock = System.Windows.Forms.DockStyle.Fill;
                settingPanel.Controls.Add(_chosenFormater.Settings);
            }
            this.ResumeLayout(true);
        }

        private void OnEvent_btnCancel_Clicked(object sender, EventArgs e)
        {
            OnCancel?.Invoke(this, new EventArgs());
        }

        private void OnEvent_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                OnCancel?.Invoke(this, new EventArgs());
        }

        private void OnEvent_btnConvert_Click(object sender, EventArgs e)
        {
            Progress = 0;
            ClearLog();
            btnConvert.Enabled = false;

            try
            {
                using (var progressMonitor = new ProgressMonitorAdapter(this))
                {
                    var selectedFile = txtFileSelection.Text ?? "";
                    selectedFile = selectedFile.Trim();
                    if (selectedFile.Length == 0)
                    {
                        AppendLog("No file selected");
                        return;
                    }

                    if (!File.Exists(selectedFile))
                    {
                        AppendLog($"File not found: {selectedFile}");
                        return;
                    }

                    var converter = new LogConverter(_manager);
                    var result = converter.ConvertLog(selectedFile, _chosenFormater, progressMonitor);

                    if (!ckbReplaceOldLog.Checked)
                    {
                        var idx = selectedFile.LastIndexOf(".");
                        if (idx < 0)
                            selectedFile += $".{_chosenFormater.Id}.log";
                        else
                            selectedFile = selectedFile.Substring(0, idx) + $".{_chosenFormater.Id}" + selectedFile.Substring(idx);
                    }

                    progressMonitor.Log($"Saving log: {selectedFile}");
                    File.WriteAllLines(selectedFile, result, System.Text.Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                AppendLog("Error");
                AppendLog($"{ex}");
            }
            finally
            {
                btnConvert.Enabled = true;
                Progress = 1;
            }
        }
    }
}