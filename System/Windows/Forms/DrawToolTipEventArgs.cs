namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class DrawToolTipEventArgs : EventArgs
    {

        //private readonly Graphics graphics;
        private readonly IWin32Window associatedWindow;
        private readonly Control associatedControl;
        private readonly Rectangle bounds;
        private readonly string toolTipText;
        private readonly Color backColor;
        private readonly Color foreColor;
        private readonly Font font;

        public DrawToolTipEventArgs(/*Graphics graphics,*/ IWin32Window associatedWindow, Control associatedControl, Rectangle bounds, string toolTipText, Color backColor, Color foreColor, Font font)
        {
            //this.graphics = graphics;
            this.associatedWindow = associatedWindow;
            this.associatedControl = associatedControl;
            this.bounds = bounds;
            this.toolTipText = toolTipText;
            this.backColor = backColor;
            this.foreColor = foreColor;
            this.font = font;
        }

        //public Graphics Graphics
        //{
        //    get
        //    {
        //        return graphics;
        //    }
        //}

        public IWin32Window AssociatedWindow
        {
            get
            {
                return associatedWindow;
            }
        }

        public Control AssociatedControl
        {
            get
            {
                return associatedControl;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
        }


        public string ToolTipText
        {
            get
            {
                return toolTipText;
            }
        }

        public Font Font
        {
            get
            {
                return font;
            }
        }

        public void DrawBackground()
        {
            //Brush backBrush = new SolidBrush(backColor);
            //Graphics.FillRectangle(backBrush, bounds);
            //backBrush.Dispose();
        }

        public void DrawText()
        {
            //Pass in a set of flags to mimic default behavior
            DrawText(TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.HidePrefix);
        }

        public void DrawText(TextFormatFlags flags)
        {
            //TextRenderer.DrawText(graphics, toolTipText, font, bounds, foreColor, flags);
        }

        public void DrawBorder()
        {
            //ControlPaint.DrawBorder(graphics, bounds, SystemColors.WindowFrame, ButtonBorderStyle.Solid);
        }
    }
}