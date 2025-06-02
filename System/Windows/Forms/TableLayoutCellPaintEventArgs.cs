namespace System.Windows.Forms
{
    using System.Drawing;

    public class TableLayoutCellPaintEventArgs : PaintEventArgs
    {
        private Rectangle bounds;
        private int row;
        private int column;

        public TableLayoutCellPaintEventArgs(Graphics g, Rectangle clipRectangle, Rectangle cellBounds, int column, int row) : base(g, clipRectangle)
        {
            this.bounds = cellBounds;
            this.row = row;
            this.column = column;
        }

        //the bounds of the cell
        public Rectangle CellBounds
        {
            get { return bounds; }
        }

        //the row index of the cell
        public int Row
        {
            get { return row; }
        }

        //the column index of the cell
        public int Column
        {
            get { return column; }
        }
    }
}
