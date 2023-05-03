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
        public event EventHandler OnCancel;

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
            if (txtLog.Text.Length == 0)
                txtLog.Text = log;
            else
                txtLog.AppendText($"{Environment.NewLine}{log}");
        }

        public void ClearLog()
        {
            txtLog.Text = "";
        }

        private void BtnSingle_Click(object sender, EventArgs e)
        {
            OnCancel?.Invoke(this, new EventArgs());
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                OnCancel?.Invoke(this, new EventArgs());
        }
    }
}