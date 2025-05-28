namespace System.Windows.Forms
{
    using System.Drawing;

    //
    // NOTE: internally, this class does double duty as storage for Control's inherited properties.
    public sealed class AmbientProperties
    {

        // Public ambient properties
        private Color backColor;
        private Color foreColor;
        private Cursor cursor;
        private Font font;

        public Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                backColor = value;
            }
        }

        public Cursor Cursor
        {
            get
            {
                return cursor;
            }
            set
            {
                cursor = value;
            }
        }

        public Font Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        public Color ForeColor
        {
            get
            {
                return foreColor;
            }
            set
            {
                foreColor = value;
            }
        }
    }
}