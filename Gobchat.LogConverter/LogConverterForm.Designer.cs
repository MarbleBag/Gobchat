namespace Gobchat.LogConverter
{
    partial class LogConverterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogConverterForm));
            this.pgbProgress = new System.Windows.Forms.ProgressBar();
            this.ckbReplaceOldLog = new System.Windows.Forms.CheckBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnFileSelector = new System.Windows.Forms.Button();
            this.txtFileSelection = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblSettings = new System.Windows.Forms.Label();
            this.cbFormater = new System.Windows.Forms.ComboBox();
            this.lblFormater = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.settingPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgbProgress
            // 
            this.pgbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgbProgress.Location = new System.Drawing.Point(3, 127);
            this.pgbProgress.Maximum = 1000;
            this.pgbProgress.Name = "pgbProgress";
            this.pgbProgress.Size = new System.Drawing.Size(541, 23);
            this.pgbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgbProgress.TabIndex = 0;
            // 
            // ckbReplaceOldLog
            // 
            this.ckbReplaceOldLog.AutoSize = true;
            this.ckbReplaceOldLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ckbReplaceOldLog.Location = new System.Drawing.Point(167, 16);
            this.ckbReplaceOldLog.Name = "ckbReplaceOldLog";
            this.ckbReplaceOldLog.Size = new System.Drawing.Size(267, 14);
            this.ckbReplaceOldLog.TabIndex = 1;
            this.ckbReplaceOldLog.UseVisualStyleBackColor = true;
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(3, 98);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 3;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.OnEvent_btnConvert_Click);
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 156);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(541, 115);
            this.txtLog.TabIndex = 4;
            // 
            // btnFileSelector
            // 
            this.btnFileSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnFileSelector.Location = new System.Drawing.Point(494, 0);
            this.btnFileSelector.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnFileSelector.Name = "btnFileSelector";
            this.btnFileSelector.Size = new System.Drawing.Size(47, 23);
            this.btnFileSelector.TabIndex = 5;
            this.btnFileSelector.Text = "...";
            this.btnFileSelector.UseVisualStyleBackColor = true;
            this.btnFileSelector.Click += new System.EventHandler(this.OnEvent_btnFileSelector_Click);
            // 
            // txtFileSelection
            // 
            this.txtFileSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFileSelection.Location = new System.Drawing.Point(0, 0);
            this.txtFileSelection.Margin = new System.Windows.Forms.Padding(0);
            this.txtFileSelection.Name = "txtFileSelection";
            this.txtFileSelection.Size = new System.Drawing.Size(491, 20);
            this.txtFileSelection.TabIndex = 6;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnConvert, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.pgbProgress, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.txtLog, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.settingPanel, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(547, 274);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel3.Controls.Add(this.txtFileSelection, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnFileSelector, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(541, 23);
            this.tableLayoutPanel3.TabIndex = 8;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Controls.Add(this.lblSettings, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.ckbReplaceOldLog, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbFormater, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblFormater, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 29);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(547, 60);
            this.tableLayoutPanel2.TabIndex = 7;
            // 
            // lblSettings
            // 
            this.lblSettings.AutoSize = true;
            this.lblSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSettings.Location = new System.Drawing.Point(3, 0);
            this.lblSettings.Name = "lblSettings";
            this.lblSettings.Size = new System.Drawing.Size(158, 13);
            this.lblSettings.TabIndex = 2;
            this.lblSettings.Text = "Settings";
            // 
            // cbFormater
            // 
            this.cbFormater.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbFormater.FormattingEnabled = true;
            this.cbFormater.Location = new System.Drawing.Point(167, 36);
            this.cbFormater.Name = "cbFormater";
            this.cbFormater.Size = new System.Drawing.Size(267, 21);
            this.cbFormater.TabIndex = 8;
            this.cbFormater.SelectedIndexChanged += new System.EventHandler(this.OnEvent_cbFormater_SelectedIndexChanged);
            // 
            // lblFormater
            // 
            this.lblFormater.AutoSize = true;
            this.lblFormater.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFormater.Location = new System.Drawing.Point(3, 33);
            this.lblFormater.Name = "lblFormater";
            this.lblFormater.Size = new System.Drawing.Size(158, 27);
            this.lblFormater.TabIndex = 9;
            this.lblFormater.Text = "Convert to";
            this.lblFormater.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label1.Size = new System.Drawing.Size(158, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "Replace old log";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // settingPanel
            // 
            this.settingPanel.AutoSize = true;
            this.settingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingPanel.Location = new System.Drawing.Point(3, 92);
            this.settingPanel.Name = "settingPanel";
            this.settingPanel.Size = new System.Drawing.Size(541, 1);
            this.settingPanel.TabIndex = 11;
            // 
            // LogConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(547, 274);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogConverterForm";
            this.Text = "Log Converter";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar pgbProgress;
        private System.Windows.Forms.CheckBox ckbReplaceOldLog;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnFileSelector;
        private System.Windows.Forms.TextBox txtFileSelection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblSettings;
        private System.Windows.Forms.ComboBox cbFormater;
        private System.Windows.Forms.Label lblFormater;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel settingPanel;
    }
}

