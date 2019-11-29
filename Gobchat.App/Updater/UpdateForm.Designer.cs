namespace Gobchat.Updater
{
    partial class UpdateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            this.btnIgnore = new System.Windows.Forms.Button();
            this.txtDisplay = new System.Windows.Forms.TextBox();
            this.btnNo = new System.Windows.Forms.Button();
            this.pnlControls = new System.Windows.Forms.TableLayoutPanel();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.formLayout = new System.Windows.Forms.TableLayoutPanel();
            this.pnlControls.SuspendLayout();
            this.formLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnIgnore
            // 
            this.btnIgnore.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnIgnore, "btnIgnore");
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.UseVisualStyleBackColor = true;
            // 
            // txtDisplay
            // 
            resources.ApplyResources(this.txtDisplay, "txtDisplay");
            this.txtDisplay.Name = "txtDisplay";
            this.txtDisplay.ReadOnly = true;
            // 
            // btnNo
            // 
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnNo, "btnNo");
            this.btnNo.Name = "btnNo";
            this.btnNo.UseVisualStyleBackColor = true;
            // 
            // pnlControls
            // 
            resources.ApplyResources(this.pnlControls, "pnlControls");
            this.pnlControls.Controls.Add(this.btnOk, 0, 0);
            this.pnlControls.Controls.Add(this.btnNo, 1, 0);
            this.pnlControls.Controls.Add(this.btnIgnore, 2, 0);
            this.pnlControls.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.pnlControls.Name = "pnlControls";
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // lblHeader
            // 
            resources.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.Name = "lblHeader";
            // 
            // formLayout
            // 
            resources.ApplyResources(this.formLayout, "formLayout");
            this.formLayout.Controls.Add(this.lblHeader, 0, 0);
            this.formLayout.Controls.Add(this.txtDisplay, 0, 1);
            this.formLayout.Controls.Add(this.pnlControls, 0, 2);
            this.formLayout.Name = "formLayout";
            // 
            // UpdateForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.formLayout);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateForm";
            this.TopMost = true;
            this.pnlControls.ResumeLayout(false);
            this.formLayout.ResumeLayout(false);
            this.formLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnIgnore;
        private System.Windows.Forms.TextBox txtDisplay;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.TableLayoutPanel pnlControls;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TableLayoutPanel formLayout;
    }
}