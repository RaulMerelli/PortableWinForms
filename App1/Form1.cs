using System;
using System.Windows.Forms;

namespace winformtest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Resize += new System.EventHandler(this.Form1_Resize);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form resized to 600x600", "Demo app", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            this.Size = new System.Drawing.Size(600, 600);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("textBox1: " + textBox1.Text, "captiontest", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            button1.Size = new System.Drawing.Size(80, 30);
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
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label1.Text = textBox1.Text;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            textBox2.Text = this.Size.Width.ToString();
        }
    }
}
