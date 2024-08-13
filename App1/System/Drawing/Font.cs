//using System.ComponentModel;
//using System.Globalization;
//using System.Runtime.InteropServices;
//using System.Runtime.Serialization;
//using System.Security.Permissions;
//using System.Windows.Forms;

//namespace System.Drawing
//{
//    public sealed class Font : MarshalByRefObject, ICloneable, ISerializable, IDisposable
//    {
//        private const int LogFontCharSetOffset = 23;

//        private const int LogFontNameOffset = 28;

//        private IntPtr nativeFont;

//        private float fontSize;

//        private FontStyle fontStyle;

//        private FontFamily fontFamily;

//        private GraphicsUnit fontUnit;

//        private byte gdiCharSet = 1;

//        private bool gdiVerticalFont;

//        private string systemFontName = "";

//        private string originalFontName;

//        internal IntPtr NativeFont => nativeFont;

//        [Browsable(false)]
//        public FontFamily FontFamily => fontFamily;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public bool Bold => (Style & FontStyle.Bold) != 0;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public byte GdiCharSet => gdiCharSet;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public bool GdiVerticalFont => gdiVerticalFont;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public bool Italic => (Style & FontStyle.Italic) != 0;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public string Name => FontFamily.Name;

//        [Browsable(false)]
//        public string OriginalFontName => originalFontName;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public bool Strikeout => (Style & FontStyle.Strikeout) != 0;

//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public bool Underline => (Style & FontStyle.Underline) != 0;

//        [Browsable(false)]
//        public FontStyle Style => fontStyle;

//        public float Size => fontSize;

//        /*[Browsable(false)]
//        public float SizeInPoints
//        {
//            get
//            {
//                if (Unit == GraphicsUnit.Point)
//                {
//                    return Size;
//                }

//                IntPtr dC = UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
//                try
//                {
//                    using (Graphics graphics = Graphics.FromHdcInternal(dC))
//                    {
//                        float num = (float)((double)graphics.DpiY / 72.0);
//                        float height = GetHeight(graphics);
//                        float num2 = height * (float)FontFamily.GetEmHeight(Style) / (float)FontFamily.GetLineSpacing(Style);
//                        return num2 / num;
//                    }
//                }
//                finally
//                {
//                    UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, dC));
//                }
//            }
//        }*/
        
//        public GraphicsUnit Unit => fontUnit;
//        /*
//        [Browsable(false)]
//        public int Height => (int)Math.Ceiling(GetHeight());
//        */
//        [Browsable(false)]
//        public bool IsSystemFont => !string.IsNullOrEmpty(systemFontName);

//        [Browsable(false)]
//        public string SystemFontName => systemFontName;

//        private void CreateNativeFont()
//        {
//            /*int num = SafeNativeMethods.Gdip.GdipCreateFont(new HandleRef(this, fontFamily.NativeFamily), fontSize, fontStyle, fontUnit, out nativeFont);
//            switch (num)
//            {
//                case 15:
//                    throw new ArgumentException(SR.GetString("GdiplusFontStyleNotFound", fontFamily.Name, fontStyle.ToString()));
//                default:
//                    throw SafeNativeMethods.Gdip.StatusException(num);
//                case 0:
//                    break;
//            }*/
//        }

//        private Font(SerializationInfo info, StreamingContext context)
//        {
//            string familyName = null;
//            float emSize = -1f;
//            FontStyle style = FontStyle.Regular;
//            GraphicsUnit unit = GraphicsUnit.Point;
//            SingleConverter singleConverter = new SingleConverter();
//            SerializationInfoEnumerator enumerator = info.GetEnumerator();
//            while (enumerator.MoveNext())
//            {
//                if (string.Equals(enumerator.Name, "Name", StringComparison.OrdinalIgnoreCase))
//                {
//                    familyName = (string)enumerator.Value;
//                }
//                else if (string.Equals(enumerator.Name, "Size", StringComparison.OrdinalIgnoreCase))
//                {
//                    emSize = ((!(enumerator.Value is string)) ? ((float)enumerator.Value) : ((float)singleConverter.ConvertFrom(enumerator.Value)));
//                }
//                else if (string.Compare(enumerator.Name, "Style", ignoreCase: true, CultureInfo.InvariantCulture) == 0)
//                {
//                    style = (FontStyle)enumerator.Value;
//                }
//                else if (string.Compare(enumerator.Name, "Unit", ignoreCase: true, CultureInfo.InvariantCulture) == 0)
//                {
//                    unit = (GraphicsUnit)enumerator.Value;
//                }
//            }

//            Initialize(familyName, emSize, style, unit, 1, IsVerticalName(familyName));
//        }

//        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
//        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
//        {
//            si.AddValue("Name", string.IsNullOrEmpty(OriginalFontName) ? Name : OriginalFontName);
//            si.AddValue("Size", Size);
//            si.AddValue("Style", Style);
//            si.AddValue("Unit", Unit);
//        }

//        public Font(Font prototype, FontStyle newStyle)
//        {
//            originalFontName = prototype.OriginalFontName;
//            Initialize(prototype.FontFamily, prototype.Size, newStyle, prototype.Unit, 1, gdiVerticalFont: false);
//        }

//        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
//        {
//            Initialize(family, emSize, style, unit, 1, gdiVerticalFont: false);
//        }

//        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
//        {
//            Initialize(family, emSize, style, unit, gdiCharSet, gdiVerticalFont: false);
//        }

//        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
//        {
//            Initialize(family, emSize, style, unit, gdiCharSet, gdiVerticalFont);
//        }

//        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet)
//        {
//            Initialize(familyName, emSize, style, unit, gdiCharSet, IsVerticalName(familyName));
//        }

//        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
//        {
//            if (float.IsNaN(emSize) || float.IsInfinity(emSize) || emSize <= 0f)
//            {
//                throw new ArgumentException("InvalidBoundArgument", "emSize");
//            }

//            Initialize(familyName, emSize, style, unit, gdiCharSet, gdiVerticalFont);
//        }

//        public Font(FontFamily family, float emSize, FontStyle style)
//        {
//            Initialize(family, emSize, style, GraphicsUnit.Point, 1, gdiVerticalFont: false);
//        }

//        public Font(FontFamily family, float emSize, GraphicsUnit unit)
//        {
//            Initialize(family, emSize, FontStyle.Regular, unit, 1, gdiVerticalFont: false);
//        }

//        public Font(FontFamily family, float emSize)
//        {
//            Initialize(family, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, gdiVerticalFont: false);
//        }

//        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
//        {
//            Initialize(familyName, emSize, style, unit, 1, IsVerticalName(familyName));
//        }

//        public Font(string familyName, float emSize, FontStyle style)
//        {
//            Initialize(familyName, emSize, style, GraphicsUnit.Point, 1, IsVerticalName(familyName));
//        }

//        public Font(string familyName, float emSize, GraphicsUnit unit)
//        {
//            Initialize(familyName, emSize, FontStyle.Regular, unit, 1, IsVerticalName(familyName));
//        }

//        public Font(string familyName, float emSize)
//        {
//            Initialize(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, 1, IsVerticalName(familyName));
//        }

//        private Font(IntPtr nativeFont, byte gdiCharSet, bool gdiVerticalFont)
//        {
//            int num = 0;
//            float size = 0f;
//            GraphicsUnit unit = GraphicsUnit.Point;
//            FontStyle style = FontStyle.Regular;
//            IntPtr family = IntPtr.Zero;
//            this.nativeFont = nativeFont;
            
//            /*num = SafeNativeMethods.Gdip.GdipGetFontUnit(new HandleRef(this, nativeFont), out unit);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }

//            num = SafeNativeMethods.Gdip.GdipGetFontSize(new HandleRef(this, nativeFont), out size);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }

//            num = SafeNativeMethods.Gdip.GdipGetFontStyle(new HandleRef(this, nativeFont), out style);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }

//            num = SafeNativeMethods.Gdip.GdipGetFamily(new HandleRef(this, nativeFont), out family);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }*/

//            SetFontFamily(new FontFamily(family));
//            Initialize(fontFamily, size, style, unit, gdiCharSet, gdiVerticalFont);
//        }

//        private void Initialize(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
//        {
//            originalFontName = familyName;
//            SetFontFamily(new FontFamily(StripVerticalName(familyName), createDefaultOnFail: true));
//            Initialize(fontFamily, emSize, style, unit, gdiCharSet, gdiVerticalFont);
//        }

//        private void Initialize(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
//        {
//            if (family == null)
//            {
//                throw new ArgumentNullException("family");
//            }

//            if (float.IsNaN(emSize) || float.IsInfinity(emSize) || emSize <= 0f)
//            {
//                throw new ArgumentException("InvalidBoundArgument", "emSize");
//            }

//            fontSize = emSize;
//            fontStyle = style;
//            fontUnit = unit;
//            this.gdiCharSet = gdiCharSet;
//            this.gdiVerticalFont = gdiVerticalFont;
//            if (fontFamily == null)
//            {
//                SetFontFamily(new FontFamily(family.NativeFamily));
//            }

//            if (nativeFont == IntPtr.Zero)
//            {
//                CreateNativeFont();
//            }
//            /*
//            int num = SafeNativeMethods.Gdip.GdipGetFontSize(new HandleRef(this, nativeFont), out fontSize);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }*/
//        }
//        /*
//        public static Font FromHfont(IntPtr hfont)
//        {
//            IntSecurity.ObjectFromWin32Handle.Demand();
//            SafeNativeMethods.LOGFONT lOGFONT = new SafeNativeMethods.LOGFONT();
//            SafeNativeMethods.GetObject(new HandleRef(null, hfont), lOGFONT);
//            IntPtr dC = UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
//            try
//            {
//                return FromLogFont(lOGFONT, dC);
//            }
//            finally
//            {
//                UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, dC));
//            }
//        }

//        public static Font FromLogFont(object lf)
//        {
//            IntPtr dC = UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
//            try
//            {
//                return FromLogFont(lf, dC);
//            }
//            finally
//            {
//                UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, dC));
//            }
//        }

//        public static Font FromLogFont(object lf, IntPtr hdc)
//        {
//            IntSecurity.ObjectFromWin32Handle.Demand();
//            IntPtr font = IntPtr.Zero;
//            int num = ((Marshal.SystemDefaultCharSize != 1) ? SafeNativeMethods.Gdip.GdipCreateFontFromLogfontW(new HandleRef(null, hdc), lf, out font) : SafeNativeMethods.Gdip.GdipCreateFontFromLogfontA(new HandleRef(null, hdc), lf, out font));
//            switch (num)
//            {
//                case 16:
//                    throw new ArgumentException(SR.GetString("GdiplusNotTrueTypeFont_NoName"));
//                default:
//                    throw SafeNativeMethods.Gdip.StatusException(num);
//                case 0:
//                    if (font == IntPtr.Zero)
//                    {
//                        throw new ArgumentException(SR.GetString("GdiplusNotTrueTypeFont", lf.ToString()));
//                    }

//                    return new Font(gdiVerticalFont: (Marshal.SystemDefaultCharSize != 1) ? (Marshal.ReadInt16(lf, 28) == 64) : (Marshal.ReadByte(lf, 28) == 64), nativeFont: font, gdiCharSet: Marshal.ReadByte(lf, 23));
//            }
//        }
        
//        public static Font FromHdc(IntPtr hdc)
//        {
//            IntSecurity.ObjectFromWin32Handle.Demand();
//            IntPtr font = IntPtr.Zero;
//            int num = SafeNativeMethods.Gdip.GdipCreateFontFromDC(new HandleRef(null, hdc), ref font);
//            return num switch
//            {
//                16 => throw new ArgumentException(SR.GetString("GdiplusNotTrueTypeFont_NoName")),
//                0 => new Font(font, 0, gdiVerticalFont: false),
//                _ => throw SafeNativeMethods.Gdip.StatusException(num),
//            };
//        }
//        */
//        public object Clone()
//        {
//            IntPtr cloneFont = IntPtr.Zero;
//            /*int num = SafeNativeMethods.Gdip.GdipCloneFont(new HandleRef(this, nativeFont), out cloneFont);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }*/

//            return new Font(cloneFont, gdiCharSet, gdiVerticalFont);
//        }

//        private void SetFontFamily(FontFamily family)
//        {
//            fontFamily = family;
//            //new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
//            GC.SuppressFinalize(fontFamily);
//        }

//        ~Font()
//        {
//            Dispose(disposing: false);
//        }

//        public void Dispose()
//        {
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }

//        private void Dispose(bool disposing)
//        {
//            if (!(nativeFont != IntPtr.Zero))
//            {
//                return;
//            }

//            try
//            {
//                //SafeNativeMethods.Gdip.GdipDeleteFont(new HandleRef(this, nativeFont));
//            }
//            catch (Exception ex)
//            {
//                if (ClientUtils.IsCriticalException(ex))
//                {
//                    throw;
//                }
//            }
//            finally
//            {
//                nativeFont = IntPtr.Zero;
//            }
//        }

//        private static bool IsVerticalName(string familyName)
//        {
//            if (familyName != null && familyName.Length > 0)
//            {
//                return familyName[0] == '@';
//            }

//            return false;
//        }

//        public override bool Equals(object obj)
//        {
//            if (obj == this)
//            {
//                return true;
//            }

//            if (!(obj is Font font))
//            {
//                return false;
//            }

//            if (font.FontFamily.Equals(FontFamily) && font.GdiVerticalFont == GdiVerticalFont && font.GdiCharSet == GdiCharSet && font.Style == Style && font.Size == Size)
//            {
//                return font.Unit == Unit;
//            }

//            return false;
//        }

//        public override int GetHashCode()
//        {
//            return (((int)fontStyle << 13) | (int)((uint)fontStyle >> 19)) ^ (((int)fontUnit << 26) | (int)((uint)fontUnit >> 6)) ^ (int)(((uint)fontSize << 7) | ((uint)fontSize >> 25));
//        }

//        private static string StripVerticalName(string familyName)
//        {
//            if (familyName != null && familyName.Length > 1 && familyName[0] == '@')
//            {
//                return familyName.Substring(1);
//            }

//            return familyName;
//        }

//        public override string ToString()
//        {
//            return string.Format(CultureInfo.CurrentCulture, "[{0}: Name={1}, Size={2}, Units={3}, GdiCharSet={4}, GdiVerticalFont={5}]", GetType().Name, FontFamily.Name, fontSize, (int)fontUnit, gdiCharSet, gdiVerticalFont);
//        }
//        /*
//        public void ToLogFont(object logFont)
//        {
//            IntPtr dC = UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
//            try
//            {
//                Graphics graphics = Graphics.FromHdcInternal(dC);
//                try
//                {
//                    ToLogFont(logFont, graphics);
//                }
//                finally
//                {
//                    graphics.Dispose();
//                }
//            }
//            finally
//            {
//                UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, dC));
//            }
//        }

//        public void ToLogFont(object logFont, Graphics graphics)
//        {
//            IntSecurity.ObjectFromWin32Handle.Demand();
//            if (graphics == null)
//            {
//                throw new ArgumentNullException("graphics");
//            }

//            int num = ((Marshal.SystemDefaultCharSize != 1) ? SafeNativeMethods.Gdip.GdipGetLogFontW(new HandleRef(this, NativeFont), new HandleRef(graphics, graphics.NativeGraphics), logFont) : SafeNativeMethods.Gdip.GdipGetLogFontA(new HandleRef(this, NativeFont), new HandleRef(graphics, graphics.NativeGraphics), logFont));
//            if (gdiVerticalFont)
//            {
//                if (Marshal.SystemDefaultCharSize == 1)
//                {
//                    for (int num2 = 30; num2 >= 0; num2--)
//                    {
//                        Marshal.WriteByte(logFont, 28 + num2 + 1, Marshal.ReadByte(logFont, 28 + num2));
//                    }

//                    Marshal.WriteByte(logFont, 28, 64);
//                }
//                else
//                {
//                    for (int num3 = 60; num3 >= 0; num3 -= 2)
//                    {
//                        Marshal.WriteInt16(logFont, 28 + num3 + 2, Marshal.ReadInt16(logFont, 28 + num3));
//                    }

//                    Marshal.WriteInt16(logFont, 28, 64);
//                }
//            }

//            if (Marshal.ReadByte(logFont, 23) == 0)
//            {
//                Marshal.WriteByte(logFont, 23, gdiCharSet);
//            }

//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }
//        }
        
//        public IntPtr ToHfont()
//        {
//            SafeNativeMethods.LOGFONT lOGFONT = new SafeNativeMethods.LOGFONT();
//            IntSecurity.ObjectFromWin32Handle.Assert();
//            try
//            {
//                ToLogFont(lOGFONT);
//            }
//            finally
//            {
//                CodeAccessPermission.RevertAssert();
//            }

//            IntPtr intPtr = IntUnsafeNativeMethods.IntCreateFontIndirect(lOGFONT);
//            if (intPtr == IntPtr.Zero)
//            {
//                throw new Win32Exception();
//            }

//            return intPtr;
//        }

//        public float GetHeight(Graphics graphics)
//        {
//            if (graphics == null)
//            {
//                throw new ArgumentNullException("graphics");
//            }

//            float size;
//            int num = SafeNativeMethods.Gdip.GdipGetFontHeight(new HandleRef(this, NativeFont), new HandleRef(graphics, graphics.NativeGraphics), out size);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }

//            return size;
//        }

        
//        public float GetHeight()
//        {
//            IntPtr dC = UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
//            float num = 0f;
//            try
//            {
//                using (Graphics graphics = Graphics.FromHdcInternal(dC))
//                {
//                    return GetHeight(graphics);
//                }
//            }
//            finally
//            {
//                UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, dC));
//            }
//        }

//        public float GetHeight(float dpi)
//        {
//            float size;
//            int num = SafeNativeMethods.Gdip.GdipGetFontHeightGivenDPI(new HandleRef(this, NativeFont), dpi, out size);
//            if (num != 0)
//            {
//                throw SafeNativeMethods.Gdip.StatusException(num);
//            }

//            return size;
//        }
//        */
//        internal void SetSystemFontName(string systemFontName)
//        {
//            this.systemFontName = systemFontName;
//        }
//    }
//}
