using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Imaging
{

    [StructLayout(LayoutKind.Sequential)]
    public sealed class ImageAttributes : ICloneable, IDisposable
    {
        internal IntPtr nativeImageAttributes;

        internal void SetNativeImageAttributes(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentNullException("handle");

            nativeImageAttributes = handle;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void SetColorMatrix(ColorMatrix matrix)
        {
            throw new NotImplementedException();
        }

        internal void SetWrapMode(WrapMode tileFlipXY)
        {
            throw new NotImplementedException();
        }

        internal void SetRemapTable(ColorMap[] colorMaps, ColorAdjustType bitmap)
        {
            throw new NotImplementedException();
        }

        internal void ClearColorKey()
        {
            throw new NotImplementedException();
        }
    }
}
