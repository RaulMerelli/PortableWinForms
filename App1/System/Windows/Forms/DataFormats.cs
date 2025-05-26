namespace System.Windows.Forms
{
    using System.Text;
    using System.Configuration.Assemblies;
    using System.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Globalization;

    public class DataFormats
    {
        public static readonly string Text = "Text";

        public static readonly string UnicodeText = "UnicodeText";

        public static readonly string Dib = "DeviceIndependentBitmap";

        public static readonly string Bitmap = "Bitmap";

        public static readonly string EnhancedMetafile = "EnhancedMetafile";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")] // Would be a breaking change to rename this
        public static readonly string MetafilePict = "MetaFilePict";

        public static readonly string SymbolicLink = "SymbolicLink";

        public static readonly string Dif = "DataInterchangeFormat";

        public static readonly string Tiff = "TaggedImageFileFormat";

        public static readonly string OemText = "OEMText";
        public static readonly string Palette = "Palette";

        public static readonly string PenData = "PenData";

        public static readonly string Riff = "RiffAudio";

        public static readonly string WaveAudio = "WaveAudio";

        public static readonly string FileDrop = "FileDrop";

        public static readonly string Locale = "Locale";

        public static readonly string Html = "HTML Format";

        public static readonly string Rtf = "Rich Text Format";

        public static readonly string CommaSeparatedValue = "Csv";

        // I'm sure upper case "String" is a reserved word in some language that matters
        public static readonly string StringFormat = typeof(string).FullName;
        //C#r: public static readonly String CF_STRINGCLASS   = typeof(String).Name;

        public static readonly string Serializable = Application.WindowsFormsVersion + "PersistentObject";


        private static Format[] formatList;
        private static int formatCount = 0;

        private static object internalSyncObject = new object();

        // not creatable...
        //
        private DataFormats()
        {
        }

        public static Format GetFormat(string format)
        {
            lock (internalSyncObject)
            {
                EnsurePredefined();

                // It is much faster to do a case sensitive search here.  So do 
                // the case sensitive compare first, then the expensive one.
                //
                for (int n = 0; n < formatCount; n++)
                {
                    if (formatList[n].Name.Equals(format))
                        return formatList[n];
                }

                for (int n = 0; n < formatCount; n++)
                {
                    if (String.Equals(formatList[n].Name, format, StringComparison.OrdinalIgnoreCase))
                        return formatList[n];
                }

                // need to add this format string
                //
                //int formatId = SafeNativeMethods.RegisterClipboardFormat(format);
                int formatId = 0;
                if (0 == formatId)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "RegisterCFFailed");
                }

                EnsureFormatSpace(1);
                formatList[formatCount] = new Format(format, formatId);
                return formatList[formatCount++];
            }
        }

        public static Format GetFormat(int id)
        {
            // Win32 uses an unsigned 16 bit type as a format ID, thus stripping off the leading bits.
            // Registered format IDs are in the range 0xC000 through 0xFFFF, thus it's important 
            // to represent format as an unsigned type.  
            return InternalGetFormat(null, (ushort)(id & 0xFFFF));
        }

        private static Format InternalGetFormat(string strName, ushort id)
        {
            lock (internalSyncObject)
            {
                EnsurePredefined();

                for (int n = 0; n < formatCount; n++)
                {
                    if (formatList[n].Id == id)
                        return formatList[n];
                }

                StringBuilder s = new StringBuilder(128);

                // This can happen if windows adds a standard format that we don't know about,
                // so we should play it safe.
                //
                //if (0 == SafeNativeMethods.GetClipboardFormatName(id, s, s.Capacity))
                //{
                //    s.Length = 0;
                //    if (strName == null)
                //    {
                //        s.Append("Format").Append(id);
                //    }
                //    else
                //    {
                //        s.Append(strName);
                //    }
                //}

                EnsureFormatSpace(1);
                formatList[formatCount] = new Format(s.ToString(), id);

                return formatList[formatCount++];
            }
        }


        private static void EnsureFormatSpace(int size)
        {
            if (null == formatList || formatList.Length <= formatCount + size)
            {
                int newSize = formatCount + 20;

                Format[] newList = new Format[newSize];

                for (int n = 0; n < formatCount; n++)
                {
                    newList[n] = formatList[n];
                }
                formatList = newList;
            }
        }

        private static void EnsurePredefined()
        {

            if (0 == formatCount)
            {

                /* not used
                int standardText;
 
                // We must handle text differently for Win 95 and NT.  We should put
                // UnicodeText on the clipboard for NT, and Text for Win 95.
                // So, we figure it out here theh first time someone asks for format info
                //
                if (1 == Marshal.SystemDefaultCharSize) {
                    standardText = NativeMethods.CF_TEXT;
                }
                else {
                    standardText = NativeMethods.CF_UNICODETEXT;
                }
                */

                formatList = new Format[] {
                    //         Text name        Win32 format ID      Data stored as a Win32 handle?
                    new Format(UnicodeText,  NativeMethods.CF_UNICODETEXT),
                    new Format(Text,         NativeMethods.CF_TEXT),
                    new Format(Bitmap,       NativeMethods.CF_BITMAP),
                    new Format(MetafilePict, NativeMethods.CF_METAFILEPICT),
                    new Format(EnhancedMetafile,  NativeMethods.CF_ENHMETAFILE),
                    new Format(Dif,          NativeMethods.CF_DIF),
                    new Format(Tiff,         NativeMethods.CF_TIFF),
                    new Format(OemText,      NativeMethods.CF_OEMTEXT),
                    new Format(Dib,          NativeMethods.CF_DIB),
                    new Format(Palette,      NativeMethods.CF_PALETTE),
                    new Format(PenData,      NativeMethods.CF_PENDATA),
                    new Format(Riff,         NativeMethods.CF_RIFF),
                    new Format(WaveAudio,    NativeMethods.CF_WAVE),
                    new Format(SymbolicLink, NativeMethods.CF_SYLK),
                    new Format(FileDrop,     NativeMethods.CF_HDROP),
                    new Format(Locale,       NativeMethods.CF_LOCALE)
                };

                formatCount = formatList.Length;
            }
        }

        public class Format
        {
            readonly string name;
            readonly int id;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public int Id
            {
                get
                {
                    return id;
                }
            }

            public Format(string name, int id)
            {
                this.name = name;
                this.id = id;
            }
        }
    }
}