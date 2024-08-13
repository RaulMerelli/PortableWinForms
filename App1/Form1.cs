using System;
using System.Windows.Forms;

namespace winformtest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("test", "captiontest", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("textBox1: " + textBox1.Text, "captiontest", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Text = textBox2.Text;
            label2.Text = textBox3.Text;
            button3.Text = textBox1.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "test";
        }
    }
}
