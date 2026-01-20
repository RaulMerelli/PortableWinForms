using System;
using System.Windows.Forms;

namespace winformtest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Resize += new System.EventHandler(this.Form1_Resize);
            ResizeBegin += new System.EventHandler(this.Form1_ResizeBegin);
            ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            label1.Click += Label1_Click;
            label2.Click += Label2_Click;
            textBox2.Click += TextBox2_Click;
            button1.MouseMove += Button1_MouseMove;
            this.Move += Form1_Move;
        }

        private void Button1_MouseMove(object sender, MouseEventArgs e)
        {
            textBox3.Text = $"e.X: {e.X}, e.Y: {e.Y}";
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            textBox2.Text = this.Location.ToString();
        }

        private void TextBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "You clicked me!";
            Form2 form = new Form2();
            form.Show();
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Click on label1", "Demo app", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(100, 100);
            MessageBox.Show("Click on label2", "Demo app", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            label2.Click -= Label2_Click;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form resized to 600x600", "Demo app", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            this.Size = new System.Drawing.Size(600, 600);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Button1 should now be bigger! Also this is the content of textBox1: " + textBox1.Text, "captiontest", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            button1.Size = new System.Drawing.Size(80, 30);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Text = textBox2.Text;
            label2.Text = textBox3.Text;
            button3.Text = textBox1.Text;
            progressBar1.Value = new Random().Next(progressBar1.Minimum, progressBar1.Maximum);
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
        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            textBox3.Text = this.Size.Width.ToString();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            textBox3.Text = this.Size.Width.ToString();
        }
    }
}
