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
    public partial class ProgressDisplayForm : Form
    {
        public event EventHandler Cancel;

        public ProgressDisplayForm()
        {
            InitializeComponent();
        }

        public string StatusText
        {
            get { return lblStatusText.Text; }
            set { lblStatusText.Text = value; }
        }

        public double Progress
        {
            get { return pgbProgressBar.Value / pgbProgressBar.Maximum; }
            set { pgbProgressBar.Value = Math.Min(pgbProgressBar.Maximum, (int)Math.Round(value * pgbProgressBar.Maximum)); }
        }

        public void AppendLog(string log)
        {
            txtLog.AppendText(log + "\n");
        }

        public void ClearLog()
        {
            txtLog.Text = "";
        }

        private void BtnSingle_Click(object sender, EventArgs e)
        {
            Cancel?.Invoke(this, new EventArgs());
        }
    }
}