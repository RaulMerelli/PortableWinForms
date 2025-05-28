namespace System.Drawing
{
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable
    {
        // handle to native GDI+ brush object to be used on demand.
        private IntPtr nativeBrush;

        public abstract object Clone();

        protected internal void SetNativeBrush(IntPtr brush)
        {
            IntSecurity.UnmanagedCode.Demand();

            SetNativeBrushInternal(brush);
        }

        internal void SetNativeBrushInternal(IntPtr brush)
        {
            Debug.Assert(brush != IntPtr.Zero, "WARNING: Assigning null to the GDI+ native brush object.");
            Debug.Assert(this.nativeBrush == IntPtr.Zero, "WARNING: Initialized GDI+ native brush object being assigned a new value.");

            this.nativeBrush = brush;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal IntPtr NativeBrush
        {
            get
            {
                //Need to comment this line out to allow for checking this.NativePen == IntPtr.Zero.
                //Debug.Assert(this.nativeBrush != IntPtr.Zero, "this.nativeBrush == null." );
                return this.nativeBrush;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.nativeBrush != IntPtr.Zero)
            {
                try
                {
                    //SafeNativeMethods.Gdip.GdipDeleteBrush(new HandleRef(this, this.nativeBrush));
                }
                catch (Exception ex)
                {
                    if (ClientUtils.IsSecurityOrCriticalException(ex))
                    {
                        throw;
                    }

                    Debug.Fail("Exception thrown during Dispose: " + ex.ToString());
                }
                finally
                {
                    this.nativeBrush = IntPtr.Zero;
                }
            }
        }

        ~Brush()
        {
            Dispose(false);
        }
    }
}