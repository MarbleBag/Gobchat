using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Core.UI
{
    public partial class UpdateFormDialog : Form
    {
        public enum UpdateType
        {
            Auto,
            Manual,
            Skip
        }

        public UpdateType UpdateRequest { get; private set; } = UpdateType.Skip;

        public UpdateFormDialog()
        {
            InitializeComponent();
        }

        public string UpdateHeadText
        {
            get
            {
                return lblHeader.Text;
            }
            set
            {
                lblHeader.Text = value;
            }
        }

        public string UpdateNotes
        {
            get
            {
                return txtDisplay.Text;
            }
            set
            {
                txtDisplay.Text = value;
            }
        }

        private void OnEvent_ButtonOne_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.UpdateRequest = UpdateType.Auto;
            this.Close();
        }

        private void OnEvent_ButtonTwo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.UpdateRequest = UpdateType.Manual;
            this.Close();
        }

        private void OnEvent_ButtonThree_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.UpdateRequest = UpdateType.Skip;
            this.Close();
        }
    }
}