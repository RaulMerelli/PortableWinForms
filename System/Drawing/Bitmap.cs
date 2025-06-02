using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Drawing.Internal;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Windows.Forms;

namespace System.Drawing
{
    /**
     * Represent a bitmap image
     */
    [Serializable, ComVisible(true)]
    public sealed class Bitmap : Image
    {
        private static Color defaultTransparentColor = Color.LightGray;

        /*
         * Predefined bitmap data formats
         */

        /**
         * Create a new bitmap object from URL
         */
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(String filename)
        {
  
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(String filename, bool useIcm)
        {

        }


        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(Type type, string resource)
        {
            Stream stream = type.Module.Assembly.GetManifestResourceStream(type, resource);
            if (stream == null)
                throw new ArgumentException("ResourceNotFound");

            IntPtr bitmap = IntPtr.Zero;
        }

        /**
         * Create a new bitmap object from a stream
         */
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(Stream stream)
        {

            if (stream == null)
                throw new ArgumentException("InvalidArgument");

            IntPtr bitmap = IntPtr.Zero;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(Stream stream, bool useIcm)
        {

            if (stream == null)
                throw new ArgumentException("InvalidArgument");

            IntPtr bitmap = IntPtr.Zero;
            int status;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();

            IntPtr bitmap = IntPtr.Zero;
        }


        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(int width, int height, PixelFormat format)
        {
            IntPtr bitmap = IntPtr.Zero;

        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(int width, int height) : this(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(int width, int height, Graphics g)
        {
            if (g == null)
                throw new ArgumentNullException("InvalidArgument");

            IntPtr bitmap = IntPtr.Zero;

        }

        //[ResourceExposure(ResourceScope.Machine)]
        //[ResourceConsumption(ResourceScope.Machine)]
        //public Bitmap(Image original) : this(original, original.Width, original.Height)
        //{
        //}

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(Image original, int width, int height) : this(width, height)
        {
            Graphics g = null;
        }

        /**
         * Constructor used in deserialization
         */
        //[ResourceExposure(ResourceScope.Machine)]
        //[ResourceConsumption(ResourceScope.Machine)]
        //private Bitmap(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //}

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Bitmap FromHicon(IntPtr hicon)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();

            IntPtr bitmap = IntPtr.Zero;

            return Bitmap.FromGDIplus(bitmap);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Bitmap FromResource(IntPtr hinstance, String bitmapName)
        {
            IntSecurity.ObjectFromWin32Handle.Demand();

            IntPtr bitmap;

            IntPtr name = Marshal.StringToHGlobalUni(bitmapName);

            bitmap = IntPtr.Zero;

            return Bitmap.FromGDIplus(bitmap);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public IntPtr GetHbitmap()
        {
            return GetHbitmap(Color.LightGray);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public IntPtr GetHbitmap(Color background)
        {
            IntPtr hBitmap = IntPtr.Zero;

            return hBitmap;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public IntPtr GetHicon()
        {
            IntPtr hIcon = IntPtr.Zero;

            return hIcon;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap(Image original, Size newSize) :
        this(original, (object)newSize != null ? newSize.Width : 0, (object)newSize != null ? newSize.Height : 0)
        {
        }

        // for use with CreateFromGDIplus
        private Bitmap()
        {
        }

        /*
         * Create a new bitmap object from a native bitmap handle.
         * This is only for internal purpose.
         */
        internal static Bitmap FromGDIplus(IntPtr handle)
        {
            Bitmap result = new Bitmap();
            return result;
        }

        // int version
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap Clone(Rectangle rect, PixelFormat format)
        {

            //validate the rect
            if (rect.Width == 0 || rect.Height == 0)
            {
                throw new ArgumentException("GdiplusInvalidRectangle");
            }

            IntPtr dstHandle = IntPtr.Zero;

            return Bitmap.FromGDIplus(dstHandle);
        }

        // float version
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Bitmap Clone(RectangleF rect, PixelFormat format)
        {

            //validate the rect
            if (rect.Width == 0 || rect.Height == 0)
            {
                throw new ArgumentException("GdiplusInvalidRectangle");
            }

            IntPtr dstHandle = IntPtr.Zero;

            return Bitmap.FromGDIplus(dstHandle);
        }

        public void MakeTransparent()
        {
            Color transparent = defaultTransparentColor;

            if (transparent.A < 255)
            {
                // It's already transparent, and if we proceeded, we will do something
                // unintended like making black transparent
                return;
            }
            MakeTransparent(transparent);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void MakeTransparent(Color transparentColor)
        {

        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format)
        {
            Contract.Ensures(Contract.Result<BitmapData>() != null);

            BitmapData bitmapData = new BitmapData();

            return LockBits(rect, flags, format, bitmapData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format, BitmapData bitmapData)
        {
            Contract.Ensures(Contract.Result<BitmapData>() != null);

            GPRECT gprect = new GPRECT(rect);
            //int status = SafeNativeMethods.Gdip.GdipBitmapLockBits(new HandleRef(this, nativeImage), ref gprect,
            //                                        flags, format, bitmapData);

            //if (status != SafeNativeMethods.Gdip.Ok)
            //    throw SafeNativeMethods.Gdip.StatusException(status);

            return bitmapData;
        }


        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void UnlockBits(BitmapData bitmapdata)
        {

        }

        public Color GetPixel(int x, int y)
        {

            int color = 0;

            return Color.FromArgb(color);
        }

        public void SetPixel(int x, int y, Color color)
        {

        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public void SetResolution(float xDpi, float yDpi)
        {

        }
    }
}
