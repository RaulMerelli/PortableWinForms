namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class DrawItemEventArgs : EventArgs
    {
        private Color backColor;
        private Color foreColor;
        private Font font;
        //private readonly System.Drawing.Graphics graphics;
        private readonly int index;
        private readonly Rectangle rect;
        private readonly DrawItemState state;

        //public DrawItemEventArgs(Graphics graphics, Font font, Rectangle rect,
        //                     int index, DrawItemState state)
        //{
        //    this.graphics = graphics;
        //    this.font = font;
        //    this.rect = rect;
        //    this.index = index;
        //    this.state = state;
        //    this.foreColor = SystemColors.WindowText;
        //    this.backColor = SystemColors.Window;
        //}

        //public DrawItemEventArgs(Graphics graphics, Font font, Rectangle rect,
        //                     int index, DrawItemState state, Color foreColor, Color backColor)
        //{
        //    this.graphics = graphics;
        //    this.font = font;
        //    this.rect = rect;
        //    this.index = index;
        //    this.state = state;
        //    this.foreColor = foreColor;
        //    this.backColor = backColor;
        //}

        public Color BackColor
        {
            get
            {
                if ((state & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    return SystemColors.Highlight;
                }
                return backColor;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return rect;
            }
        }

        public Font Font
        {
            get
            {
                return font;
            }
        }

        public Color ForeColor
        {
            get
            {
                if ((state & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    return SystemColors.HighlightText;
                }
                return foreColor;
            }
        }

        //public Graphics Graphics
        //{
        //    get
        //    {
        //        return graphics;
        //    }
        //}

        public int Index
        {
            get
            {
                return index;
            }
        }

        public DrawItemState State
        {
            get
            {
                return state;
            }
        }

        public virtual void DrawBackground()
        {
            //Brush backBrush = new SolidBrush(BackColor);
            //Graphics.FillRectangle(backBrush, rect);
            //backBrush.Dispose();
        }

        public virtual void DrawFocusRectangle()
        {
            //if ((state & DrawItemState.Focus) == DrawItemState.Focus
            //    && (state & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect)
            //    ControlPaint.DrawFocusRectangle(Graphics, rect, ForeColor, BackColor);
        }
    }
}