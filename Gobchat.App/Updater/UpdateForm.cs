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
    public partial class UpdateForm : Form
    {
        public event EventHandler Cancel;

        public UpdateForm()
        {
            InitializeComponent();
        }
    }
}