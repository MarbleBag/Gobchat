namespace Gobchat.Core.UI
{
    partial class UpdateFormDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateFormDialog));
            this.btnThree = new System.Windows.Forms.Button();
            this.txtDisplay = new System.Windows.Forms.TextBox();
            this.btnTwo = new System.Windows.Forms.Button();
            this.pnlControls = new System.Windows.Forms.TableLayoutPanel();
            this.btnOne = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.formLayout = new System.Windows.Forms.TableLayoutPanel();
            this.pnlControls.SuspendLayout();
            this.formLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnThree
            // 
            this.btnThree.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnThree, "btnThree");
            this.btnThree.Name = "btnThree";
            this.btnThree.UseVisualStyleBackColor = true;
            this.btnThree.Click += new System.EventHandler(this.OnEvent_ButtonThree_Click);
            // 
            // txtDisplay
            // 
            resources.ApplyResources(this.txtDisplay, "txtDisplay");
            this.txtDisplay.Name = "txtDisplay";
            this.txtDisplay.ReadOnly = true;
            // 
            // btnTwo
            // 
            this.btnTwo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnTwo, "btnTwo");
            this.btnTwo.Name = "btnTwo";
            this.btnTwo.UseVisualStyleBackColor = true;
            this.btnTwo.Click += new System.EventHandler(this.OnEvent_ButtonTwo_Click);
            // 
            // pnlControls
            // 
            resources.ApplyResources(this.pnlControls, "pnlControls");
            this.pnlControls.Controls.Add(this.btnOne, 0, 0);
            this.pnlControls.Controls.Add(this.btnTwo, 1, 0);
            this.pnlControls.Controls.Add(this.btnThree, 2, 0);
            this.pnlControls.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.pnlControls.Name = "pnlControls";
            // 
            // btnOne
            // 
            resources.ApplyResources(this.btnOne, "btnOne");
            this.btnOne.Name = "btnOne";
            this.btnOne.UseVisualStyleBackColor = true;
            this.btnOne.Click += new System.EventHandler(this.OnEvent_ButtonOne_Click);
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
            // UpdateFormDialog
            // 
            this.AcceptButton = this.btnOne;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnThree;
            this.Controls.Add(this.formLayout);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateFormDialog";
            this.TopMost = true;
            this.pnlControls.ResumeLayout(false);
            this.formLayout.ResumeLayout(false);
            this.formLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnThree;
        private System.Windows.Forms.TextBox txtDisplay;
        private System.Windows.Forms.Button btnTwo;
        private System.Windows.Forms.TableLayoutPanel pnlControls;
        private System.Windows.Forms.Button btnOne;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TableLayoutPanel formLayout;
    }
}