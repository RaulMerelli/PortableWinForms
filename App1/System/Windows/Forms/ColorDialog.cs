using System.Drawing;

namespace System.Windows.Forms
{
    public class ColorDialog : CommonDialog
    {
        private int options;

        private int[] customColors;

        private Color color;

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                if (!value.IsEmpty)
                {
                    color = value;
                }
                else
                {
                    color = Color.Black;
                }
            }
        }

        public int[] CustomColors
        {
            get
            {
                return (int[])customColors.Clone();
            }
            set
            {
                int num = ((value != null) ? Math.Min(value.Length, 16) : 0);
                if (num > 0)
                {
                    Array.Copy(value, 0, customColors, 0, num);
                }

                for (int i = num; i < 16; i++)
                {
                    customColors[i] = 16777215;
                }
            }
        }

        public ColorDialog()
        {
            customColors = new int[16];
            Reset();
        }

        public override void Reset()
        {
            options = 0;
            color = Color.Black;
            CustomColors = null;
        }

        private void ResetColor()
        {
            Color = Color.Black;
        }

        public override string ToString()
        {
            string text = base.ToString();
            return text + ",  Color: " + Color.ToString();
        }
    }
}
