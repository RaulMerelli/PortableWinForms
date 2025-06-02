namespace System.Drawing
{
    using System.Diagnostics.CodeAnalysis;

    [System.Runtime.InteropServices.ComVisible(true)]
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum CopyPixelOperation
    {

        Blackness = SafeNativeMethods.BLACKNESS,
        CaptureBlt = SafeNativeMethods.CAPTUREBLT,
        DestinationInvert = SafeNativeMethods.DSTINVERT,
        MergeCopy = SafeNativeMethods.MERGECOPY,
        MergePaint = SafeNativeMethods.MERGEPAINT,
        NoMirrorBitmap = SafeNativeMethods.NOMIRRORBITMAP,
        NotSourceCopy = SafeNativeMethods.NOTSRCCOPY,
        NotSourceErase = SafeNativeMethods.NOTSRCERASE,
        PatCopy = SafeNativeMethods.PATCOPY,
        PatInvert = SafeNativeMethods.PATINVERT,
        PatPaint = SafeNativeMethods.PATPAINT,
        SourceAnd = SafeNativeMethods.SRCAND,
        SourceCopy = SafeNativeMethods.SRCCOPY,
        SourceErase = SafeNativeMethods.SRCERASE,
        SourceInvert = SafeNativeMethods.SRCINVERT,
        SourcePaint = SafeNativeMethods.SRCPAINT,
        Whiteness = SafeNativeMethods.WHITENESS,
    }
}