namespace System.Drawing.Imaging
{
    using System.Drawing;

    public sealed class ColorMap
    {
        Color oldColor;
        Color newColor;

        public ColorMap()
        {
            oldColor = new Color();
            newColor = new Color();
        }

        public Color OldColor
        {
            get { return oldColor; }
            set { oldColor = value; }
        }
        public Color NewColor
        {
            get { return newColor; }
            set { newColor = value; }
        }
    }
}