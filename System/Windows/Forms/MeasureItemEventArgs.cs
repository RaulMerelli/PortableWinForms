namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    public class MeasureItemEventArgs : EventArgs
    {

        private int itemHeight;
        private int itemWidth;
        private int index;

        private readonly System.Drawing.Graphics graphics;

        public MeasureItemEventArgs(Graphics graphics, int index, int itemHeight)
        {
            this.graphics = graphics;
            this.index = index;
            this.itemHeight = itemHeight;
            this.itemWidth = 0;
        }

        public MeasureItemEventArgs(Graphics graphics, int index)
        {
            this.graphics = graphics;
            this.index = index;
            this.itemHeight = 0;
            this.itemWidth = 0;
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return graphics;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public int ItemHeight
        {
            get
            {
                return itemHeight;
            }
            set
            {
                itemHeight = value;
            }
        }

        public int ItemWidth
        {
            get
            {
                return itemWidth;
            }
            set
            {
                itemWidth = value;
            }
        }
    }
}