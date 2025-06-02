using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
    public sealed class StringFormat : MarshalByRefObject, ICloneable, IDisposable
    {
        internal IntPtr nativeFormat;
        StringFormatFlags format;

        private StringFormat(IntPtr format)
        {
            nativeFormat = format;
        }

        public StringFormat()
        {
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public StringFormatFlags FormatFlags
        {
            get
            {
                return format;
            }
            set
            {
                Debug.Assert(nativeFormat != IntPtr.Zero, "NativeFormat is null!");
                format = value;
            }
        }

        public HotkeyPrefix HotkeyPrefix { get; internal set; }
        public StringTrimming Trimming { get; internal set; }
        public StringAlignment Alignment { get; internal set; }
        public StringAlignment LineAlignment { get; internal set; }
    }
}
