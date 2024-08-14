using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace System.Drawing
{
    public sealed class FontFamily : MarshalByRefObject, IDisposable
    {
        private const int LANG_NEUTRAL = 0;

        private IntPtr nativeFamily;

        private bool createDefaultOnFail;

        internal IntPtr NativeFamily => nativeFamily;

        private static int CurrentLanguage => CultureInfo.CurrentUICulture.LCID;

        public string Name => GetName(CurrentLanguage);

        //public static FontFamily[] Families => new InstalledFontCollection().Families;

        public static FontFamily GenericSansSerif => new FontFamily(GetGdipGenericSansSerif());

        public static FontFamily GenericSerif => new FontFamily(GetNativeGenericSerif());

        public static FontFamily GenericMonospace => new FontFamily(GetNativeGenericMonospace());

        private void SetNativeFamily(IntPtr family)
        {
            nativeFamily = family;
        }

        internal FontFamily(IntPtr family)
        {
            SetNativeFamily(family);
        }

        internal FontFamily(string name, bool createDefaultOnFail)
        {
            this.createDefaultOnFail = createDefaultOnFail;
            //CreateFontFamily(name, null);
        }

        public FontFamily(string name)
        {
            //CreateFontFamily(name, null);
        }
        /*
        public FontFamily(string name, FontCollection fontCollection)
        {
            CreateFontFamily(name, fontCollection);
        }
        */
        /*
        private void CreateFontFamily(string name, FontCollection fontCollection)
        {
            IntPtr FontFamily = IntPtr.Zero;
            IntPtr handle = fontCollection?.nativeFontCollection ?? IntPtr.Zero;
            int num = 0; // SafeNativeMethods.Gdip.GdipCreateFontFamilyFromName(name, new HandleRef(fontCollection, handle), out FontFamily);
            if (num != 0)
            {
                if (!createDefaultOnFail)
                {
                    
                    //switch (num)
                    //{
                    //    case 14:
                    //        throw new ArgumentException(SR.GetString("GdiplusFontFamilyNotFound", name));
                    //    case 16:
                    //        throw new ArgumentException(SR.GetString("GdiplusNotTrueTypeFont", name));
                    //    default:
                    //        throw SafeNativeMethods.Gdip.StatusException(num);
                    //}
                    
                }

                FontFamily = GetGdipGenericSansSerif();
            }

            SetNativeFamily(FontFamily);
        }
*/
        /*
        public FontFamily(GenericFontFamilies genericFamily)
        {
            IntPtr fontfamily = IntPtr.Zero;
            int num = genericFamily switch
            {
                GenericFontFamilies.Serif => SafeNativeMethods.Gdip.GdipGetGenericFontFamilySerif(out fontfamily),
                GenericFontFamilies.SansSerif => SafeNativeMethods.Gdip.GdipGetGenericFontFamilySansSerif(out fontfamily),
                _ => SafeNativeMethods.Gdip.GdipGetGenericFontFamilyMonospace(out fontfamily),
            };
            if (num != 0)
            {
                throw SafeNativeMethods.Gdip.StatusException(num);
            }

            SetNativeFamily(fontfamily);
        }
        */
        ~FontFamily()
        {
            Dispose(disposing: false);
        }

        //
        // Riepilogo:
        //     Indica se l'oggetto specificato è un oggetto System.Drawing.FontFamily ed è identico
        //     a questo oggetto System.Drawing.FontFamily.
        //
        // Parametri:
        //   obj:
        //     Oggetto da sottoporre a test.
        //
        // Valori restituiti:
        //     true se obj è un oggetto System.Drawing.FontFamily ed è uguale a System.Drawing.FontFamily;
        //     in caso contrario false.
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (!(obj is FontFamily fontFamily))
            {
                return false;
            }

            return fontFamily.NativeFamily == NativeFamily;
        }

        //
        // Riepilogo:
        //     Converte la classe System.Drawing.FontFamily in una rappresentazione in forma
        //     di stringa leggibile.
        //
        // Valori restituiti:
        //     Stringa che rappresenta questo oggetto System.Drawing.FontFamily.
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}: Name={1}]", GetType().Name, Name);
        }

        //
        // Riepilogo:
        //     Ottiene un codice hash per System.Drawing.FontFamily.
        //
        // Valori restituiti:
        //     Codice hash per System.Drawing.FontFamily.
        public override int GetHashCode()
        {
            return GetName(0).GetHashCode();
        }

        //
        // Riepilogo:
        //     Rilascia tutte le risorse utilizzate da questo oggetto System.Drawing.FontFamily.
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!(nativeFamily != IntPtr.Zero))
            {
                return;
            }

            try
            {
                //SafeNativeMethods.Gdip.GdipDeleteFontFamily(new HandleRef(this, nativeFamily));
            }
            catch (Exception ex)
            {
                if (ClientUtils.IsCriticalException(ex))
                {
                    throw;
                }
            }
            finally
            {
                nativeFamily = IntPtr.Zero;
            }
        }

        public string GetName(int language)
        {
            StringBuilder stringBuilder = new StringBuilder(32);
            return stringBuilder.ToString();
        }

        private static IntPtr GetGdipGenericSansSerif()
        {
            IntPtr fontfamily = IntPtr.Zero;
            return fontfamily;
        }

        private static IntPtr GetNativeGenericSerif()
        {
            IntPtr fontfamily = IntPtr.Zero;
            return fontfamily;
        }

        private static IntPtr GetNativeGenericMonospace()
        {
            IntPtr fontfamily = IntPtr.Zero;
            return fontfamily;
        }

        /*[Obsolete("Do not use method GetFamilies, use property Families instead")]
        public static FontFamily[] GetFamilies(Graphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }

            return new InstalledFontCollection().Families;
        }*/

        public bool IsStyleAvailable(FontStyle style)
        {
            int isStyleAvailable = 0;

            return isStyleAvailable != 0;
        }

        public int GetEmHeight(FontStyle style)
        {
            int EmHeight = 0;
            return EmHeight;
        }

        public int GetCellAscent(FontStyle style)
        {
            int CellAscent = 0;
            return CellAscent;
        }

        public int GetCellDescent(FontStyle style)
        {
            int CellDescent = 0;
            return CellDescent;
        }

        public int GetLineSpacing(FontStyle style)
        {
            int LineSpaceing = 0;
            return LineSpaceing;
        }
    }
}
