using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Windows.Forms;

namespace System.Drawing
{
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    public class Graphics : MarshalByRefObject, IDisposable, IDeviceContext
    {
        //private GraphicsContext previousContext;
        private static readonly object syncObject = new Object();
        private IntPtr nativeGraphics;
        private IntPtr nativeHdc;

        // Object reference used for printing; it could point to a PrintPreviewGraphics to obtain the VisibleClipBounds, or 
        // a DeviceContext holding a printer DC.
        private object printingHelper;

        // GDI+'s preferred HPALETTE.
        private static IntPtr halftonePalette;

        // pointer back to the Image backing a specific graphic object
        private Image backingImage;

        public delegate bool DrawImageAbort(IntPtr callbackdata);

        public delegate bool EnumerateMetafileProc(EmfPlusRecordType recordType,
                                                   int flags,
                                                   int dataSize,
                                                   IntPtr data,
                                                   PlayRecordCallback callbackData);

        private Graphics(IntPtr gdipNativeGraphics)
        {
            if (gdipNativeGraphics == IntPtr.Zero)
            {
                throw new ArgumentNullException("gdipNativeGraphics");
            }
            this.nativeGraphics = gdipNativeGraphics;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Graphics FromHdc(IntPtr hdc)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();

            if (hdc == IntPtr.Zero)
            {
                throw new ArgumentNullException("hdc");
            }

            return FromHdcInternal(hdc);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static Graphics FromHdcInternal(IntPtr hdc)
        {
            Debug.Assert(hdc != IntPtr.Zero, "Must pass in a valid DC");
            IntPtr nativeGraphics = IntPtr.Zero;

            //int status = SafeNativeMethods.Gdip.GdipCreateFromHDC(new HandleRef(null, hdc), out nativeGraphics);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //{
            //    throw SafeNativeMethods.Gdip.StatusException(status);
            //}

            return new Graphics(nativeGraphics);
        }

        internal IntPtr NativeGraphics
        {
            get
            {
                Debug.Assert(this.nativeGraphics != IntPtr.Zero, "this.nativeGraphics == IntPtr.Zero.");
                return this.nativeGraphics;
            }
        }

        public Region Clip { get; internal set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IntPtr GetHdc()
        {
            throw new NotImplementedException();
        }

        public void ReleaseHdc()
        {

        }

        internal void DrawLine(Pen controlDark, int x, int y1, int v, int y2)
        {
            throw new NotImplementedException();
        }

        internal void DrawRectangle(Pen controlDark, Rectangle bound)
        {
            throw new NotImplementedException();
        }

        internal void DrawRectangle(Pen pen, int x, int y, int v1, int v2)
        {
            throw new NotImplementedException();
        }

        internal void DrawImage(Image image, Rectangle dest, int x, int y, int width, int height, GraphicsUnit pixel, ImageAttributes attrs, object value, IntPtr zero)
        {
            throw new NotImplementedException();
        }

        internal void FillRectangle(Brush background, Rectangle offsetRectangle)
        {
            throw new NotImplementedException();
        }

        internal void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        internal void DrawImage(Image backgroundImage, Rectangle imageRectangle)
        {
            throw new NotImplementedException();
        }

        internal void DrawImageUnscaled(Bitmap bmp, Rectangle imageBounds)
        {
            throw new NotImplementedException();
        }

        internal void DrawImage(Image backgroundImage, Rectangle imageRectangle, int x, int y, int width, int height, GraphicsUnit pixel, ImageAttributes imageAttrib)
        {
            throw new NotImplementedException();
        }

        internal void DrawImage(Image backgroundImage, Rectangle imageRect, int x, int y, int width, int height, GraphicsUnit pixel)
        {
            throw new NotImplementedException();
        }

        internal void DrawImage(Bitmap bitmap, int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        internal static Graphics FromImage(Bitmap bitmap)
        {
            throw new NotImplementedException();
        }

        internal void DrawString(string s, Font font, SolidBrush brush, RectangleF layoutRectangle, StringFormat format)
        {
            throw new NotImplementedException();
        }

        internal void DrawImage(Bitmap bitmap, int x, int y)
        {
            throw new NotImplementedException();
        }

        internal void ExcludeClip(Rectangle insideRect)
        {
            throw new NotImplementedException();
        }

        internal void Clear(Color transparent)
        {
            throw new NotImplementedException();
        }
    }
}
