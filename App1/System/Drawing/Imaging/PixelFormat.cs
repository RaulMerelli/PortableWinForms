namespace System.Drawing.Imaging
{
    /*
     * In-memory pixel data formats:
     *  bits 0-7 = format index
     *  bits 8-15 = pixel size (in bits)
     *  bits 16-23 = flags
     *  bits 24-31 = reserved
     */

    // Suggestion: don't use SlickEdit to format this file -- it does a horrible job.
    // Visual Studio works ok, though.
    //
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    // XX                                                                         XX
    // XX  If you modify this file, please update Image.GetColorDepth()           XX
    // XX                                                                         XX
    // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
    //
    public enum PixelFormat
    {
        Indexed = 0x00010000,
        Gdi = 0x00020000,
        Alpha = 0x00040000,
        PAlpha = 0x00080000, // What's this?
        Extended = 0x00100000,
        Canonical = 0x00200000,
        Undefined = 0,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        DontCare = 0,
        // makes it into devtools, we can change this.
        Format1bppIndexed = 1 | (1 << 8) | (int)Indexed | (int)Gdi,
        Format4bppIndexed = 2 | (4 << 8) | (int)Indexed | (int)Gdi,
        Format8bppIndexed = 3 | (8 << 8) | (int)Indexed | (int)Gdi,
        Format16bppGrayScale = 4 | (16 << 8) | (int)Extended,
        Format16bppRgb555 = 5 | (16 << 8) | (int)Gdi,
        Format16bppRgb565 = 6 | (16 << 8) | (int)Gdi,
        Format16bppArgb1555 = 7 | (16 << 8) | (int)Alpha | (int)Gdi,
        Format24bppRgb = 8 | (24 << 8) | (int)Gdi,
        Format32bppRgb = 9 | (32 << 8) | (int)Gdi,
        Format32bppArgb = 10 | (32 << 8) | (int)Alpha | (int)Gdi | (int)Canonical,
        Format32bppPArgb = 11 | (32 << 8) | (int)Alpha | (int)PAlpha | (int)Gdi,
        Format48bppRgb = 12 | (48 << 8) | (int)Extended,
        Format64bppArgb = 13 | (64 << 8) | (int)Alpha | (int)Canonical | (int)Extended,
        Format64bppPArgb = 14 | (64 << 8) | (int)Alpha | (int)PAlpha | (int)Extended,
        Max = 15,
    }
}