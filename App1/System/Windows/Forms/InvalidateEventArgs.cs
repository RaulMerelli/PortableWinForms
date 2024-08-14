using System.Drawing;

namespace System.Windows.Forms
{
    public class InvalidateEventArgs : EventArgs
    {
        private readonly Rectangle invalidRect;

        public Rectangle InvalidRect => invalidRect;

        public InvalidateEventArgs(Rectangle invalidRect)
        {
            this.invalidRect = invalidRect;
        }
    }
}
