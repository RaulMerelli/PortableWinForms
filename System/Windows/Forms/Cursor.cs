﻿namespace System.Windows.Forms
{
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Drawing;
    using System.Drawing.Design;
    using CodeAccessPermission = System.Security.CodeAccessPermission;
    using System.Security.Permissions;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.Win32;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Security;

    [
    TypeConverterAttribute(typeof(CursorConverter)),
    Serializable,
    ]
    public sealed class Cursor : IDisposable, ISerializable
    {
        private static Size cursorSize = System.Drawing.Size.Empty;

        private byte[] cursorData;
        private IntPtr handle = IntPtr.Zero;       // handle to loaded image
        private bool ownHandle = true;
        private int resourceId = 0;

        private object userData;

        /**
         * Constructor used in deserialization
         */
        internal Cursor(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator sie = info.GetEnumerator();
            if (sie == null)
            {
                return;
            }
            for (; sie.MoveNext();)
            {
                // Dont catch any exceptions while Deserialising objects from stream.
                if (String.Equals(sie.Name, "CursorData", StringComparison.OrdinalIgnoreCase))
                {
                    cursorData = (byte[])sie.Value;
                    if (cursorData != null)
                    {
                        LoadPicture(new UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(cursorData)));
                    }
                }
                else if (String.Compare(sie.Name, "CursorResourceId", true, CultureInfo.InvariantCulture) == 0)
                {
                    LoadFromResourceId((int)sie.Value);
                }
            }
        }

        internal Cursor(int nResourceId, int dummy)
        {
            LoadFromResourceId(nResourceId);
        }

        // Private constructor.  We have a private constructor here for
        // static cursors that are loaded through resources.  The only reason
        // to use the private constructor is so that we can assert, rather
        // than throw, if the cursor couldn't be loaded.  Why?  Because
        // throwing in <clinit/> is really rude and will prevent any of windows forms
        // from initializing.  This seems extreme just because we fail to
        // load a cursor.
        internal Cursor(string resource, int dummy)
        {
            Stream stream = typeof(Cursor).Module.Assembly.GetManifestResourceStream(typeof(Cursor), resource);
            Debug.Assert(stream != null, "couldn't get stream for resource " + resource);
            cursorData = new byte[stream.Length];
            stream.Read(cursorData, 0, Convert.ToInt32(stream.Length)); // we assume that a cursor is less than 4gig big
            LoadPicture(new UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(cursorData)));
        }

        public Cursor(IntPtr handle)
        {
            IntSecurity.UnmanagedCode.Demand();
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException("InvalidGDIHandle");
            }

            this.handle = handle;
            this.ownHandle = false;
        }

        public Cursor(string fileName)
        {
            //Filestream demands the correct FILEIO access here
            //
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                cursorData = new byte[f.Length];
                f.Read(cursorData, 0, Convert.ToInt32(f.Length)); // assume that a cursor is less than 4gig...
            }
            finally
            {
                f.Close();
            }
            LoadPicture(new UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(cursorData)));
        }

        public Cursor(Type type, string resource) : this(type.Module.Assembly.GetManifestResourceStream(type, resource))
        {
        }

        public Cursor(Stream stream)
        {
            cursorData = new byte[stream.Length];
            stream.Read(cursorData, 0, Convert.ToInt32(stream.Length));// assume that a cursor is less than 4gig...
            LoadPicture(new UnsafeNativeMethods.ComStreamFromDataStream(new MemoryStream(cursorData)));
        }

        public static Rectangle Clip
        {
            get
            {
                return ClipInternal;
            }
            set
            {
                if (!value.IsEmpty)
                {
                    //Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "AdjustCursorClip Demanded");
                    IntSecurity.AdjustCursorClip.Demand();
                }
                ClipInternal = value;
            }
        }

        internal static Rectangle ClipInternal
        {
            get
            {
                NativeMethods.RECT r = new NativeMethods.RECT();
                //SafeNativeMethods.GetClipCursor(ref r);
                return Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
            }
            set
            {
                if (value.IsEmpty)
                {
                    //UnsafeNativeMethods.ClipCursor(null);
                }
                else
                {
                    //NativeMethods.RECT rcClip = NativeMethods.RECT.FromXYWH(value.X, value.Y, value.Width, value.Height);
                    //UnsafeNativeMethods.ClipCursor(ref rcClip);
                }
            }
        }

        public static Cursor Current
        {
            get
            {
                return CurrentInternal;
            }

            set
            {
                IntSecurity.ModifyCursor.Demand();
                CurrentInternal = value;
            }
        }

        internal static Cursor CurrentInternal;
        //{
        //    get
        //    {
        //        IntPtr curHandle = SafeNativeMethods.GetCursor();

        //        // SECREVIEW : This method is to be used internally from safe places.
        //        //
        //        IntSecurity.UnmanagedCode.Assert();
        //        return Cursors.KnownCursorFromHCursor(curHandle);
        //        // SECREVIEW RevertAssert automatically called on return from function.
        //    }
        //    set
        //    {
        //        IntPtr handle = (value == null) ? IntPtr.Zero : value.handle;
        //        UnsafeNativeMethods.SetCursor(new HandleRef(value, handle));
        //    }
        //}

        public IntPtr Handle
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException("ObjectDisposed");
                }
                return handle;
            }
        }

        //[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")] //Minor, not worth breaking change
        //public Point HotSpot
        //{
        //    get
        //    {
        //        Point hotSpot = Point.Empty;
        //        NativeMethods.ICONINFO info = new NativeMethods.ICONINFO();
        //        Icon currentIcon = null;

        //        // SECREVIEW : Safe to assert here, the handle used was created by us and the Icon created is not exposed.
        //        //
        //        IntSecurity.ObjectFromWin32Handle.Assert();
        //        try
        //        {
        //            currentIcon = Icon.FromHandle(this.Handle);
        //        }
        //        finally
        //        {
        //            CodeAccessPermission.RevertAssert();
        //        }

        //        try
        //        {
        //            SafeNativeMethods.GetIconInfo(new HandleRef(this, currentIcon.Handle), info);
        //            hotSpot = new Point(info.xHotspot, info.yHotspot);
        //        }
        //        finally
        //        {
        //            // GetIconInfo creates bitmaps for the hbmMask and hbmColor members of ICONINFO.
        //            // The calling application must manage these bitmaps and delete them when they are no longer necessary. 

        //            if (info.hbmMask != IntPtr.Zero)
        //            {
        //                // ExternalDelete to prevent Handle underflow
        //                SafeNativeMethods.ExternalDeleteObject(new HandleRef(null, info.hbmMask));
        //                info.hbmMask = IntPtr.Zero;
        //            }
        //            if (info.hbmColor != IntPtr.Zero)
        //            {
        //                // ExternalDelete to prevent Handle underflow
        //                SafeNativeMethods.ExternalDeleteObject(new HandleRef(null, info.hbmColor));
        //                info.hbmColor = IntPtr.Zero;
        //            }
        //            currentIcon.Dispose();

        //        }
        //        return hotSpot;
        //    }
        //}

        //public static Point Position
        //{
        //    get
        //    {
        //        NativeMethods.POINT p = new NativeMethods.POINT();
        //        UnsafeNativeMethods.GetCursorPos(p);
        //        return new Point(p.x, p.y);
        //    }
        //    set
        //    {
        //        IntSecurity.AdjustCursorPosition.Demand();
        //        UnsafeNativeMethods.SetCursorPos(value.X, value.Y);
        //    }
        //}

        public Size Size
        {
            get
            {
                if (cursorSize.IsEmpty)
                {
                    //cursorSize = new Size(
                    //                    UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXCURSOR),
                    //                    UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYCURSOR)
                    //                    );

                }
                return cursorSize;
            }
        }

        [
        Localizable(false),
        Bindable(true),
        DefaultValue(null),
        TypeConverter(typeof(StringConverter)),
        ]
        public object Tag
        {
            get
            {
                return userData;
            }
            set
            {
                userData = value;
            }
        }

        //public IntPtr CopyHandle()
        //{
        //    Size sz = Size;
        //    return SafeNativeMethods.CopyImage(new HandleRef(this, Handle), NativeMethods.IMAGE_CURSOR, sz.Width, sz.Height, 0);
        //}

        private void DestroyHandle()
        {
            if (ownHandle)
            {
                //UnsafeNativeMethods.DestroyCursor(new HandleRef(this, handle));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            /*if (picture != null) {
                picture = null;
 
                // If we have no message loop, OLE may block on this call.
                // Let pent up SendMessage calls go through here.
                //
                NativeMethods.MSG msg = new NativeMethods.MSG();
                UnsafeNativeMethods.PeekMessage(ref msg, NativeMethods.NullHandleRef, 0, 0, NativeMethods.PM_NOREMOVE | NativeMethods.PM_NOYIELD);
            }*/ // do we still keep that?

            if (handle != IntPtr.Zero)
            {
                DestroyHandle();
                handle = IntPtr.Zero;
            }
        }

        //private void DrawImageCore(Graphics graphics, Rectangle imageRect, Rectangle targetRect, bool stretch)
        //{
        //    // Support GDI+ Translate method
        //    targetRect.X += (int)graphics.Transform.OffsetX;
        //    targetRect.Y += (int)graphics.Transform.OffsetY;

        //    int rop = 0xcc0020; // RasterOp.SOURCE.GetRop();
        //    IntPtr dc = graphics.GetHdc();

        //    try
        //    { // want finally clause to release dc
        //        int imageX = 0;
        //        int imageY = 0;
        //        int imageWidth;
        //        int imageHeight;
        //        int targetX = 0;
        //        int targetY = 0;
        //        int targetWidth = 0;
        //        int targetHeight = 0;

        //        Size cursorSize = Size;

        //        // compute the dimensions of the icon, if needed
        //        //
        //        if (!imageRect.IsEmpty)
        //        {
        //            imageX = imageRect.X;
        //            imageY = imageRect.Y;
        //            imageWidth = imageRect.Width;
        //            imageHeight = imageRect.Height;
        //        }
        //        else
        //        {
        //            imageWidth = cursorSize.Width;
        //            imageHeight = cursorSize.Height;
        //        }

        //        if (!targetRect.IsEmpty)
        //        {
        //            targetX = targetRect.X;
        //            targetY = targetRect.Y;
        //            targetWidth = targetRect.Width;
        //            targetHeight = targetRect.Height;
        //        }
        //        else
        //        {
        //            targetWidth = cursorSize.Width;
        //            targetHeight = cursorSize.Height;
        //        }

        //        int drawWidth, drawHeight;
        //        int clipWidth, clipHeight;

        //        if (stretch)
        //        {
        //            // Short circuit the simple case of blasting an icon to the
        //            // screen
        //            //
        //            if (targetWidth == imageWidth && targetHeight == imageHeight
        //                && imageX == 0 && imageY == 0 && rop == NativeMethods.SRCCOPY
        //                && imageWidth == cursorSize.Width && imageHeight == cursorSize.Height)
        //            {
        //                SafeNativeMethods.DrawIcon(new HandleRef(graphics, dc), targetX, targetY, new HandleRef(this, handle));
        //                return;
        //            }

        //            drawWidth = cursorSize.Width * targetWidth / imageWidth;
        //            drawHeight = cursorSize.Height * targetHeight / imageHeight;
        //            clipWidth = targetWidth;
        //            clipHeight = targetHeight;
        //        }
        //        else
        //        {
        //            // Short circuit the simple case of blasting an icon to the
        //            // screen
        //            //
        //            if (imageX == 0 && imageY == 0 && rop == NativeMethods.SRCCOPY
        //                && cursorSize.Width <= targetWidth && cursorSize.Height <= targetHeight
        //                && cursorSize.Width == imageWidth && cursorSize.Height == imageHeight)
        //            {
        //                SafeNativeMethods.DrawIcon(new HandleRef(graphics, dc), targetX, targetY, new HandleRef(this, handle));
        //                return;
        //            }

        //            drawWidth = cursorSize.Width;
        //            drawHeight = cursorSize.Height;
        //            clipWidth = targetWidth < imageWidth ? targetWidth : imageWidth;
        //            clipHeight = targetHeight < imageHeight ? targetHeight : imageHeight;
        //        }

        //        if (rop == NativeMethods.SRCCOPY)
        //        {
        //            // The ROP is SRCCOPY, so we can be simple here and take
        //            // advantage of clipping regions.  Drawing the cursor
        //            // is merely a matter of offsetting and clipping.
        //            //
        //            SafeNativeMethods.IntersectClipRect(new HandleRef(this, Handle), targetX, targetY, targetX + clipWidth, targetY + clipHeight);
        //            SafeNativeMethods.DrawIconEx(new HandleRef(graphics, dc), targetX - imageX, targetY - imageY,
        //                               new HandleRef(this, handle), drawWidth, drawHeight, 0, NativeMethods.NullHandleRef, NativeMethods.DI_NORMAL);
        //            // Let GDI+ restore clipping
        //            return;
        //        }

        //        Debug.Fail("Cursor.Draw does not support raster ops.  How did you even pass one in?");
        //    }
        //    finally
        //    {
        //        graphics.ReleaseHdcInternal(dc);
        //    }
        //}

        //public void Draw(Graphics g, Rectangle targetRect)
        //{
        //    DrawImageCore(g, Rectangle.Empty, targetRect, false);
        //}

        //public void DrawStretched(Graphics g, Rectangle targetRect)
        //{
        //    DrawImageCore(g, Rectangle.Empty, targetRect, true);
        //}

        ~Cursor()
        {
            Dispose(false);
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (cursorData != null)
            {
                si.AddValue("CursorData", cursorData, typeof(byte[]));
            }
            else if (resourceId != 0)
            {
                si.AddValue("CursorResourceId", resourceId, typeof(int));
            }
            else
            {
                Debug.Fail("Why are we trying to serialize an empty cursor?");
                throw new SerializationException("CursorNonSerializableHandle");
            }
        }

        public static void Hide()
        {
            Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "AdjustCursorClip Demanded");
            IntSecurity.AdjustCursorClip.Demand();

            //UnsafeNativeMethods.ShowCursor(false);
        }

        private void LoadFromResourceId(int nResourceId)
        {
            ownHandle = false;  // we don't delete stock cursors.

            // We assert here on exception -- this constructor is used during clinit,
            // and it would be a shame if we failed to initialize all of windows forms just
            // just because a cursor couldn't load.
            //
            try
            {
                resourceId = nResourceId;
                //handle = SafeNativeMethods.LoadCursor(NativeMethods.NullHandleRef, nResourceId);
            }
            catch (Exception e)
            {
                handle = IntPtr.Zero;
                Debug.Fail(e.ToString());
            }
        }

        // this code is adapted from Icon.GetIconSize please take this into account when changing this
        private Size GetIconSize(IntPtr iconHandle)
        {
            Size iconSize = Size;

            //NativeMethods.ICONINFO info = new NativeMethods.ICONINFO();
            //SafeNativeMethods.GetIconInfo(new HandleRef(this, iconHandle), info);
            //NativeMethods.BITMAP bmp = new NativeMethods.BITMAP();

            //if (info.hbmColor != IntPtr.Zero)
            //{
            //    UnsafeNativeMethods.GetObject(new HandleRef(null, info.hbmColor), Marshal.SizeOf(typeof(NativeMethods.BITMAP)), bmp);
            //    SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmColor));
            //    iconSize = new Size(bmp.bmWidth, bmp.bmHeight);
            //}
            //else if (info.hbmMask != IntPtr.Zero)
            //{
            //    UnsafeNativeMethods.GetObject(new HandleRef(null, info.hbmMask), Marshal.SizeOf(typeof(NativeMethods.BITMAP)), bmp);
            //    iconSize = new Size(bmp.bmWidth, bmp.bmHeight / 2);
            //}

            //if (info.hbmMask != IntPtr.Zero)
            //{
            //    SafeNativeMethods.IntDeleteObject(new HandleRef(null, info.hbmMask));
            //}
            return iconSize;
        }



        private void LoadPicture(UnsafeNativeMethods.IStream stream)
        {

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            try
            {
                Guid g = typeof(UnsafeNativeMethods.IPicture).GUID;
                UnsafeNativeMethods.IPicture picture = null;

                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

                try
                {
                    //picture = UnsafeNativeMethods.OleCreateIPictureIndirect(null, ref g, true);
                    //UnsafeNativeMethods.IPersistStream ipictureAsIPersist = (UnsafeNativeMethods.IPersistStream)picture;
                    //ipictureAsIPersist.Load(stream);

                    //if (picture != null && picture.GetPictureType() == NativeMethods.Ole.PICTYPE_ICON)
                    //{
                    //    IntPtr cursorHandle = picture.GetHandle();
                    //    Size picSize = GetIconSize(cursorHandle);
                    //    if (DpiHelper.IsScalingRequired)
                    //    {
                    //        picSize = DpiHelper.LogicalToDeviceUnits(picSize);
                    //    }

                    //    handle = SafeNativeMethods.CopyImageAsCursor(new HandleRef(this, cursorHandle), NativeMethods.IMAGE_CURSOR,
                    //        picSize.Width, picSize.Height, 0);
                    //    ownHandle = true;
                    //}
                    //else
                    //{
                    //    throw new ArgumentException("InvalidPictureType");
                    //}
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                    // destroy the picture...
                    if (picture != null)
                    {
                        Marshal.ReleaseComObject(picture);
                    }
                }
            }
            catch (COMException e)
            {
                Debug.Fail(e.ToString());
                throw new ArgumentException("InvalidPictureFormat");
            }
        }

        internal void SavePicture(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.resourceId != 0)
            {
                throw new FormatException("CursorCannotCovertToBytes");
            }
            try
            {
                stream.Write(cursorData, 0, cursorData.Length);
            }
            catch (SecurityException)
            {
                // VSWhidbey 424904 - dont eat security exceptions.
                throw;
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
                throw new InvalidOperationException("InvalidPictureFormat");
            }
        }

        public static void Show()
        {
            //UnsafeNativeMethods.ShowCursor(true);
        }

        public override string ToString()
        {
            string s = null;

            if (!this.ownHandle)
                s = TypeDescriptor.GetConverter(typeof(Cursor)).ConvertToString(this);
            else
                s = base.ToString();

            return "[Cursor: " + s + "]";
        }

        public static bool operator ==(Cursor left, Cursor right)
        {
            if (object.ReferenceEquals(left, null) != object.ReferenceEquals(right, null))
            {
                return false;
            }

            if (!object.ReferenceEquals(left, null))
            {
                return (left.handle == right.handle);
            }
            else
            {
                return true;
            }
        }

        public static bool operator !=(Cursor left, Cursor right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            // Handle is a 64-bit value in 64-bit machines, uncheck here to avoid overflow exceptions.
            return unchecked((int)handle);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Cursor))
            {
                return false;
            }
            return (this == (Cursor)obj);
        }
    }
}