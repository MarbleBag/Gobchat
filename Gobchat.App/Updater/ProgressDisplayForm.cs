using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Updater
{
    public partial class ProgressDisplayForm : Form
    {
        public event EventHandler Cancel;

        public ProgressDisplayForm()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("FORM THREAD: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
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

        private void btnSingle_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("BUTTON THREAD: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            System.Diagnostics.Debug.WriteLine("Cancel Clicked!");
            Cancel?.Invoke(this, new EventArgs());
        }
    }
}