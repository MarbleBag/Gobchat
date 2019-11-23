using System;
using System.Windows.Forms;

namespace Gobchat.UI.Forms
{
    partial class CefOverlayForm
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
            this.DisposeForm(disposing);
            base.Dispose(disposing);
        }



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CefOverlayForm));
            this.SuspendLayout();
            // 
            // CefOverlayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Magenta;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(50, 50);
            this.Name = "CefOverlayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OverlayForm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.OnEvent_Form_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnEvent_Form_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnEvent_Form_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnEvent_Form_MouseDown);
            this.MouseEnter += new System.EventHandler(this.OverlayForm_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.OverlayForm_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnEvent_Form_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnEvent_Form_MouseUp);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.OnEvent_Form_MouseWheel);
            this.ResumeLayout(false);

        }




        #endregion
    }
}