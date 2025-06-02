using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Internal;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace System.Drawing
{
    public sealed class Pen : MarshalByRefObject, ISystemColorTracker, ICloneable, IDisposable
    {
        // handle to native GDI+ pen object.
        private IntPtr nativePen;

        // GDI+ doesn't understand system colors, so we need to cache the value here
        private Color color;
        private bool immutable;
        private Brush brush;
        private int v;

        public Color Color { get; set; }
        public float Width { get; set; } = 1f;

        public Pen(Color color)
        {
            Color = color;
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal Pen(Color color, bool immutable) : this(color)
        {
            this.immutable = immutable;
        }

        public Pen(Color color, float width)
        {
            Color = color;
            Width = width;
        }

        public Pen(Brush brush, int v)
        {
            this.brush = brush;
            this.v = v;
        }

        public DashStyle DashStyle
        {
            get
            {
                int dashstyle = 0;

                //int status = SafeNativeMethods.Gdip.GdipGetPenDashStyle(new HandleRef(this, this.NativePen), out dashstyle);

                //if (status != SafeNativeMethods.Gdip.Ok)
                //    throw SafeNativeMethods.Gdip.StatusException(status);

                return (DashStyle)dashstyle;
            }

            set
            {

                //valid values are 0x0 to 0x5
                if (!ClientUtils.IsEnumValid(value, unchecked((int)value), (int)DashStyle.Solid, (int)DashStyle.Custom))
                {
                    throw new InvalidEnumArgumentException("value", unchecked((int)value), typeof(DashStyle));
                }

                if (immutable)
                    throw new ArgumentException("CantChangeImmutableObjects");

                //int status = SafeNativeMethods.Gdip.GdipSetPenDashStyle(new HandleRef(this, this.NativePen), unchecked((int)value));

                //if (status != SafeNativeMethods.Gdip.Ok)
                //{
                //    throw SafeNativeMethods.Gdip.StatusException(status);
                //}

                //if we just set pen style to "custom" without defining the custom dash pattern,
                //lets make sure we can return a valid value...
                //
                if (value == DashStyle.Custom)
                {
                    EnsureValidDashPattern();
                }
            }
        }

        private void EnsureValidDashPattern()
        {
            int retval = 0;
            //int status = SafeNativeMethods.Gdip.GdipGetPenDashCount(new HandleRef(this, this.NativePen), out retval);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            if (retval == 0)
            {
                //just set to a solid pattern
                DashPattern = new float[] { 1 };
            }
        }


        public float[] DashPattern
        {
            get
            {
                float[] dashArray;

                // Figure out how many dash elements we have

                int retval = 0;
                //int status = SafeNativeMethods.Gdip.GdipGetPenDashCount(new HandleRef(this, this.NativePen), out retval);

                //if (status != SafeNativeMethods.Gdip.Ok)
                //    throw SafeNativeMethods.Gdip.StatusException(status);

                int count = retval;

                // Allocate temporary native memory buffer
                // and pass it to GDI+ to retrieve dash array elements

                IntPtr buf = Marshal.AllocHGlobal(checked(4 * count));
                //status = SafeNativeMethods.Gdip.GdipGetPenDashArray(new HandleRef(this, this.NativePen), buf, count);

                try
                {
                    //if (status != SafeNativeMethods.Gdip.Ok)
                    //{
                    //    throw SafeNativeMethods.Gdip.StatusException(status);
                    //}

                    dashArray = new float[count];

                    Marshal.Copy(buf, dashArray, 0, count);
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }

                return dashArray;
            }

            set
            {
                if (immutable)
                    throw new ArgumentException("CantChangeImmutableObjects");


                //validate the DashPattern value being set
                if (value == null || value.Length == 0)
                {
                    throw new ArgumentException("InvalidDashPattern");
                }

                int count = value.Length;

                IntPtr buf = Marshal.AllocHGlobal(checked(4 * count));

                try
                {
                    Marshal.Copy(value, 0, buf, count);

                    //int status = SafeNativeMethods.Gdip.GdipSetPenDashArray(new HandleRef(this, this.NativePen), new HandleRef(buf, buf), count);

                    //if (status != SafeNativeMethods.Gdip.Ok)
                    //{
                    //    throw SafeNativeMethods.Gdip.StatusException(status);
                    //}
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }
        }

        public void OnSystemColorChanged()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
