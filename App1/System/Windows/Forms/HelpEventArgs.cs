using System.Drawing;

namespace System.Windows.Forms
{
    public class HelpEventArgs : EventArgs
    {
        private readonly Point mousePos;

        private bool handled;

        public Point MousePos => mousePos;

        public bool Handled
        {
            get
            {
                return handled;
            }
            set
            {
                handled = value;
            }
        }

        public HelpEventArgs(Point mousePos)
        {
            this.mousePos = mousePos;
        }
    }
}
