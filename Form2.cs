using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winformtest
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            checkBox1.Click += CheckBox_Click;
            checkBox2.Click += CheckBox_Click;
            checkBox3.Click += CheckBox_Click;
            checkBox4.Click += CheckBox_Click;
            checkBox5.Click += CheckBox_Click;
            checkBox6.Click += CheckBox_Click;
            radioButton1.Click += RadioButton_Click;
            radioButton2.Click += RadioButton_Click;
            radioButton3.Click += RadioButton_Click;
            radioButton4.Click += RadioButton_Click;
            radioButton5.Click += RadioButton_Click;
            radioButton6.Click += RadioButton_Click;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            button3.Text = "Battery (%)";
            MessageBox.Show("Battery percent: " + new PowerStatus().BatteryLifePercent.ToString(), "Battery percent", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void RadioButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("What is clicked: " + (sender as RadioButton).Text);
        }

        private void CheckBox_Click(object sender, EventArgs e)
        {
            MessageBox.Show("What is clicked: " + (sender as CheckBox).Text);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (groupBox1.Text == "Title #1")
            {
                groupBox1.Text = "Title #2";
            }
            else
            {
                groupBox1.Text = "Title #1";
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (panel1.BorderStyle == BorderStyle.Fixed3D)
            {
                panel1.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (panel1.BorderStyle == BorderStyle.FixedSingle)
            {
                panel1.BorderStyle = BorderStyle.Fixed3D;
            }
        }
    }
}
