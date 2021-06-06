using System;
using System.Windows.Forms;

namespace Gobchat.LogConverter.Logs.CCL
{
    public partial class Settings : UserControl
    {
        private readonly CCLv1Formater _formater;

        public Settings(CCLv1Formater formater)
        {
            InitializeComponent();

            _formater = formater ?? throw new ArgumentNullException(nameof(formater));
            textBox1.Text = _formater.LogFormat;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (_formater != null)
                _formater.LogFormat = textBox1.Text;
        }
    }
}
