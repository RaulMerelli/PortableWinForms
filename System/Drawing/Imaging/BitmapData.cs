namespace System.Drawing.Imaging
{
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BitmapData
    {
        int width;
        int height;
        int stride;
        int pixelFormat;
        IntPtr scan0;
        int reserved;

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

        public int Stride
        {
            get { return stride; }
            set { stride = value; }
        }

        public PixelFormat PixelFormat
        {
            get { return (PixelFormat)pixelFormat; }
            [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
            set
            {
                switch (value)
                {
                    case PixelFormat.DontCare:
                    // case PixelFormat.Undefined: same as DontCare
                    case PixelFormat.Max:
                    case PixelFormat.Indexed:
                    case PixelFormat.Gdi:
                    case PixelFormat.Format16bppRgb555:
                    case PixelFormat.Format16bppRgb565:
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                    case PixelFormat.Format1bppIndexed:
                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Alpha:
                    case PixelFormat.Format16bppArgb1555:
                    case PixelFormat.PAlpha:
                    case PixelFormat.Format32bppPArgb:
                    case PixelFormat.Extended:
                    case PixelFormat.Format16bppGrayScale:
                    case PixelFormat.Format48bppRgb:
                    case PixelFormat.Format64bppPArgb:
                    case PixelFormat.Canonical:
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format64bppArgb:
                        break;
                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException("value", unchecked((int)value), typeof(PixelFormat));
                }


                pixelFormat = (int)value;
            }
        }

        public IntPtr Scan0
        {
            get { return scan0; }
            set { scan0 = value; }
        }

        public int Reserved
        {
            // why make public??
            //
            get { return reserved; }
            set { reserved = value; }
        }
    }
}