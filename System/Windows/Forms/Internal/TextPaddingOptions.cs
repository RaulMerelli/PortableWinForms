﻿namespace System.Windows.Forms.Internal
{
    [Flags]
    public enum TextPaddingOptions
    {
        // Add some extra points to account for some glyphs overhanging (like for letter f in some fonts or 
        // when italized).
        // For an illustration, type letter f in Wordpad and make it 72-point "Times New Roman" italic
        // observe that the lower left part of the letter is clipped.  Also, try selecting the letter,
        // both the lower-left and the upper-right parts are clipped.
        // The default value.
        GlyphOverhangPadding = 0x00000000,

        NoPadding = 0x00000001,

        // Adds padding to the text bounding box (inflating the bounding box).
        // This is to render text similar to GDI+.
        // Implies GlypOverhangPadding.
        LeftAndRightPadding = 0x00000002
    }
}
