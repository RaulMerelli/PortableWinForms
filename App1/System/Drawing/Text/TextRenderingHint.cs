namespace System.Drawing.Text
{
    public enum TextRenderingHint
    {
        SystemDefault = 0,        // Glyph with system default rendering hint
        SingleBitPerPixelGridFit, // Glyph bitmap with hinting
        SingleBitPerPixel,        // Glyph bitmap without hinting
        AntiAliasGridFit,         //Anti-aliasing with hinting
        AntiAlias,                // Glyph anti-alias bitmap without hinting
        ClearTypeGridFit          // Glyph CT bitmap with hinting
    }
}