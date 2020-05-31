namespace Gobchat.Core.UI
{
    partial class ProgressDisplayForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDisplayForm));
            this.pgbProgressBar = new System.Windows.Forms.ProgressBar();
            this.btnSingle = new System.Windows.Forms.Button();
            this.lblStatusText = new System.Windows.Forms.Label();
            this.pnlProgressBar = new System.Windows.Forms.Panel();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.pnlProgressBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgbProgressBar
            // 
            resources.ApplyResources(this.pgbProgressBar, "pgbProgressBar");
            this.pgbProgressBar.Maximum = 1000;
            this.pgbProgressBar.Name = "pgbProgressBar";
            this.pgbProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // btnSingle
            // 
            resources.ApplyResources(this.btnSingle, "btnSingle");
            this.btnSingle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnSingle.Name = "btnSingle";
            this.btnSingle.UseVisualStyleBackColor = true;
            this.btnSingle.Click += new System.EventHandler(this.BtnSingle_Click);
            // 
            // lblStatusText
            // 
            resources.ApplyResources(this.lblStatusText, "lblStatusText");
            this.lblStatusText.Name = "lblStatusText";
            // 
            // pnlProgressBar
            // 
            resources.ApplyResources(this.pnlProgressBar, "pnlProgressBar");
            this.pnlProgressBar.Controls.Add(this.pgbProgressBar);
            this.pnlProgressBar.Controls.Add(this.btnSingle);
            this.pnlProgressBar.Controls.Add(this.lblStatusText);
            this.pnlProgressBar.Name = "pnlProgressBar";
            // 
            // txtLog
            // 
            resources.ApplyResources(this.txtLog, "txtLog");
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            // 
            // ProgressDisplayForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnSingle;
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.pnlProgressBar);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDisplayForm";
            this.TopMost = true;
            this.pnlProgressBar.ResumeLayout(false);
            this.pnlProgressBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pgbProgressBar;
        private System.Windows.Forms.Button btnSingle;
        private System.Windows.Forms.Label lblStatusText;
        private System.Windows.Forms.Panel pnlProgressBar;
        private System.Windows.Forms.TextBox txtLog;
    }
}