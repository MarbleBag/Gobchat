namespace Gobchat
{
    partial class GobchatOverlayControlPanel
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GobchatOverlayControlPanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelOverlayVisible = new System.Windows.Forms.Label();
            this.checkOverlayVisible = new System.Windows.Forms.CheckBox();
            this.labelClickThrough = new System.Windows.Forms.Label();
            this.checkOverlayClickThrough = new System.Windows.Forms.CheckBox();
            this.labelOverlayLocked = new System.Windows.Forms.Label();
            this.checkOverlayLocked = new System.Windows.Forms.CheckBox();
            this.labelChatURL = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.textUrl = new System.Windows.Forms.TextBox();
            this.btnUrl = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnReloadOverlay = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkShowDebug = new System.Windows.Forms.CheckBox();
            this.labelPluginActive = new System.Windows.Forms.Label();
            this.checkPluginActive = new System.Windows.Forms.CheckBox();
            this.labelGlobalHotkey = new System.Windows.Forms.Label();
            this.textGlobalHotkey = new System.Windows.Forms.TextBox();
            this.labelInfo1 = new System.Windows.Forms.Label();
            this.labelEnableGlobalHotkey = new System.Windows.Forms.Label();
            this.checkEnableGlobalHotkey = new System.Windows.Forms.CheckBox();
            this.labelMentions = new System.Windows.Forms.Label();
            this.textMentions = new System.Windows.Forms.TextBox();
            this.labelMentionInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.labelOverlayVisible, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkOverlayVisible, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelClickThrough, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkOverlayClickThrough, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelOverlayLocked, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkOverlayLocked, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelChatURL, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 13);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this.checkShowDebug, 1, 12);
            this.tableLayoutPanel1.Controls.Add(this.labelPluginActive, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkPluginActive, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelGlobalHotkey, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.textGlobalHotkey, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.labelInfo1, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.labelEnableGlobalHotkey, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.checkEnableGlobalHotkey, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.labelMentions, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.textMentions, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.labelMentionInfo, 1, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // labelOverlayVisible
            // 
            this.labelOverlayVisible.AllowDrop = true;
            resources.ApplyResources(this.labelOverlayVisible, "labelOverlayVisible");
            this.labelOverlayVisible.Name = "labelOverlayVisible";
            // 
            // checkOverlayVisible
            // 
            resources.ApplyResources(this.checkOverlayVisible, "checkOverlayVisible");
            this.checkOverlayVisible.Name = "checkOverlayVisible";
            this.checkOverlayVisible.CheckedChanged += new System.EventHandler(this.checkOverlayVisible_CheckedChanged);
            // 
            // labelClickThrough
            // 
            resources.ApplyResources(this.labelClickThrough, "labelClickThrough");
            this.labelClickThrough.Name = "labelClickThrough";
            // 
            // checkOverlayClickThrough
            // 
            resources.ApplyResources(this.checkOverlayClickThrough, "checkOverlayClickThrough");
            this.checkOverlayClickThrough.Name = "checkOverlayClickThrough";
            this.checkOverlayClickThrough.CheckedChanged += new System.EventHandler(this.checkOverlayClickThrough_CheckedChanged);
            // 
            // labelOverlayLocked
            // 
            resources.ApplyResources(this.labelOverlayLocked, "labelOverlayLocked");
            this.labelOverlayLocked.Name = "labelOverlayLocked";
            // 
            // checkOverlayLocked
            // 
            resources.ApplyResources(this.checkOverlayLocked, "checkOverlayLocked");
            this.checkOverlayLocked.Name = "checkOverlayLocked";
            this.checkOverlayLocked.CheckedChanged += new System.EventHandler(this.checkOverlayLocked_CheckedChanged);
            // 
            // labelChatURL
            // 
            resources.ApplyResources(this.labelChatURL, "labelChatURL");
            this.labelChatURL.Name = "labelChatURL";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.textUrl, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnUrl, 1, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // textUrl
            // 
            resources.ApplyResources(this.textUrl, "textUrl");
            this.textUrl.Name = "textUrl";
            this.textUrl.Leave += new System.EventHandler(this.textUrl_Leave);
            // 
            // btnUrl
            // 
            resources.ApplyResources(this.btnUrl, "btnUrl");
            this.btnUrl.Name = "btnUrl";
            this.btnUrl.UseVisualStyleBackColor = true;
            this.btnUrl.Click += new System.EventHandler(this.btnUrl_Click);
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.btnReloadOverlay, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // btnReloadOverlay
            // 
            resources.ApplyResources(this.btnReloadOverlay, "btnReloadOverlay");
            this.btnReloadOverlay.Name = "btnReloadOverlay";
            this.btnReloadOverlay.UseVisualStyleBackColor = true;
            this.btnReloadOverlay.Click += new System.EventHandler(this.btnReloadOverlay_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // checkShowDebug
            // 
            resources.ApplyResources(this.checkShowDebug, "checkShowDebug");
            this.checkShowDebug.Name = "checkShowDebug";
            this.checkShowDebug.UseVisualStyleBackColor = true;
            this.checkShowDebug.CheckedChanged += new System.EventHandler(this.checkShowDebug_CheckedChanged);
            // 
            // labelPluginActive
            // 
            resources.ApplyResources(this.labelPluginActive, "labelPluginActive");
            this.labelPluginActive.Name = "labelPluginActive";
            // 
            // checkPluginActive
            // 
            resources.ApplyResources(this.checkPluginActive, "checkPluginActive");
            this.checkPluginActive.Name = "checkPluginActive";
            this.checkPluginActive.UseVisualStyleBackColor = true;
            this.checkPluginActive.CheckedChanged += new System.EventHandler(this.checkPluginActive_CheckedChanged);
            // 
            // labelGlobalHotkey
            // 
            resources.ApplyResources(this.labelGlobalHotkey, "labelGlobalHotkey");
            this.labelGlobalHotkey.Name = "labelGlobalHotkey";
            // 
            // textGlobalHotkey
            // 
            resources.ApplyResources(this.textGlobalHotkey, "textGlobalHotkey");
            this.textGlobalHotkey.Name = "textGlobalHotkey";
            this.textGlobalHotkey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textGlobalHotkey_KeyDown);
            // 
            // labelInfo1
            // 
            resources.ApplyResources(this.labelInfo1, "labelInfo1");
            this.labelInfo1.Name = "labelInfo1";
            // 
            // labelEnableGlobalHotkey
            // 
            resources.ApplyResources(this.labelEnableGlobalHotkey, "labelEnableGlobalHotkey");
            this.labelEnableGlobalHotkey.Name = "labelEnableGlobalHotkey";
            // 
            // checkEnableGlobalHotkey
            // 
            resources.ApplyResources(this.checkEnableGlobalHotkey, "checkEnableGlobalHotkey");
            this.checkEnableGlobalHotkey.Name = "checkEnableGlobalHotkey";
            this.checkEnableGlobalHotkey.UseVisualStyleBackColor = true;
            this.checkEnableGlobalHotkey.CheckedChanged += new System.EventHandler(this.checkEnableGlobalHotkey_CheckedChanged);
            // 
            // labelMentions
            // 
            resources.ApplyResources(this.labelMentions, "labelMentions");
            this.labelMentions.Name = "labelMentions";
            // 
            // textMentions
            // 
            resources.ApplyResources(this.textMentions, "textMentions");
            this.textMentions.Name = "textMentions";
            this.textMentions.Enter += new System.EventHandler(this.textMentions_Leave);
            this.textMentions.Leave += new System.EventHandler(this.textMentions_Leave);
            // 
            // labelMentionInfo
            // 
            resources.ApplyResources(this.labelMentionInfo, "labelMentionInfo");
            this.labelMentionInfo.Name = "labelMentionInfo";
            // 
            // GobchatOverlayControlPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GobchatOverlayControlPanel";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelOverlayVisible;
        private System.Windows.Forms.CheckBox checkOverlayVisible;
        private System.Windows.Forms.Label labelClickThrough;
        private System.Windows.Forms.CheckBox checkOverlayClickThrough;
        private System.Windows.Forms.Label labelOverlayLocked;
        private System.Windows.Forms.CheckBox checkOverlayLocked;
        private System.Windows.Forms.Label labelChatURL;
        private System.Windows.Forms.TextBox textUrl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnUrl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btnReloadOverlay;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkShowDebug;
        private System.Windows.Forms.Label labelPluginActive;
        private System.Windows.Forms.CheckBox checkPluginActive;
        private System.Windows.Forms.Label labelGlobalHotkey;
        private System.Windows.Forms.TextBox textGlobalHotkey;
        private System.Windows.Forms.Label labelInfo1;
        private System.Windows.Forms.Label labelEnableGlobalHotkey;
        private System.Windows.Forms.CheckBox checkEnableGlobalHotkey;
        private System.Windows.Forms.Label labelMentions;
        private System.Windows.Forms.TextBox textMentions;
        private System.Windows.Forms.Label labelMentionInfo;
    }
}
