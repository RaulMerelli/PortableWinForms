using System;
using System.Windows.Forms;

namespace winformtest
{
    public static class Program
    {
        [STAThread]
        public static void InternalMain()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
