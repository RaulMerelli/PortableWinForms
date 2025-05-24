namespace System.Windows.Forms
{

    using System.Diagnostics;
    using System.Text;
    using System;

    public class CreateParams
    {
        string className;
        string caption;
        int style;
        int exStyle;
        int classStyle;
        int x;
        int y;
        int width;
        int height;
        IntPtr parent;
        object param;

        public string ClassName
        {
            get { return className; }
            set { className = value; }
        }

        public string Caption
        {
            get { return caption; }
            set { caption = value; }
        }

        public int Style
        {
            get { return style; }
            set { style = value; }
        }

        public int ExStyle
        {
            get { return exStyle; }
            set { exStyle = value; }
        }

        public int ClassStyle
        {
            get { return classStyle; }
            set { classStyle = value; }
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public IntPtr Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public object Param
        {
            get { return param; }
            set { param = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(64);
            sb.Append("CreateParams {'");
            sb.Append(className);
            sb.Append("', '");
            sb.Append(caption);
            sb.Append("', 0x");
            sb.Append(Convert.ToString(style, 16));
            sb.Append(", 0x");
            sb.Append(Convert.ToString(exStyle, 16));
            sb.Append(", {");
            sb.Append(x);
            sb.Append(", ");
            sb.Append(y);
            sb.Append(", ");
            sb.Append(width);
            sb.Append(", ");
            sb.Append(height);
            sb.Append("}");
            sb.Append("}");
            return sb.ToString();
        }
    }
}