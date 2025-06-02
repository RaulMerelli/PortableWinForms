using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
    // TODO:
    // This is heavly incompled due the Graphics class missing.

    public class PaintEventArgs : EventArgs, IDisposable
    {
        private Graphics graphics;
        private GraphicsState savedGraphicsState;
        private readonly IntPtr dc = IntPtr.Zero;
        private IntPtr oldPal = IntPtr.Zero;
        private readonly Rectangle clipRect;

        public Rectangle ClipRectangle => clipRect;

        internal IntPtr HDC
        {
            get
            {
                if (graphics == null)
                {
                    return dc;
                }
                return IntPtr.Zero;
            }
        }

        public Graphics Graphics
        {
            get
            {
                if (graphics == null && dc != IntPtr.Zero)
                {
                    //oldPal = Control.SetUpPalette(dc, force: false, realizePalette: false);
                    //graphics = Graphics.FromHdcInternal(dc);
                    //graphics.PageUnit = GraphicsUnit.Pixel;
                    //savedGraphicsState = graphics.Save();
                }

                return graphics;
            }
        }

        public PaintEventArgs(Graphics graphics, Rectangle clipRect)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }

            this.graphics = graphics;
            this.clipRect = clipRect;
        }

        internal PaintEventArgs(IntPtr dc, Rectangle clipRect)
        {
            this.dc = dc;
            this.clipRect = clipRect;
        }

        ~PaintEventArgs()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && graphics != null && dc != IntPtr.Zero)
            {
                //graphics.Dispose();
            }

            if (oldPal != IntPtr.Zero && dc != IntPtr.Zero)
            {
                //SafeNativeMethods.SelectPalette(new HandleRef(this, dc), new HandleRef(this, oldPal), 0);
                oldPal = IntPtr.Zero;
            }
        }

        internal void ResetGraphics()
        {
            if (graphics != null && savedGraphicsState != null)
            {
                //graphics.Restore(savedGraphicsState);
                savedGraphicsState = null;
            }
        }
        
    }
}
