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
