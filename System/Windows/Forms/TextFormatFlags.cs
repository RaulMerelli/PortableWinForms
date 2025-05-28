namespace System.Windows.Forms
{
    using System;
    using System.Windows.Forms.Internal;
    using System.Diagnostics.CodeAnalysis;

    [Flags]
    // PM team has reviewed and decided on naming changes already
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [
        SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")  // Maps to native enum.
    ]
    public enum TextFormatFlags
    {
        Bottom = IntTextFormatFlags.Bottom,
        // NOTE: This flag is used for measuring text and TextRenderer has methods for doing that
        // so we don't expose it to avoid confusion.
        // CalculateRectangle          = IntTextFormatFlags.CalculateRectangle,
        EndEllipsis = IntTextFormatFlags.EndEllipsis,
        ExpandTabs = IntTextFormatFlags.ExpandTabs,
        ExternalLeading = IntTextFormatFlags.ExternalLeading,
        Default = IntTextFormatFlags.Default,
        HidePrefix = IntTextFormatFlags.HidePrefix,
        HorizontalCenter = IntTextFormatFlags.HorizontalCenter,
        Internal = IntTextFormatFlags.Internal,
        Left = IntTextFormatFlags.Left,  // default
        ModifyString = IntTextFormatFlags.ModifyString,
        NoClipping = IntTextFormatFlags.NoClipping,
        NoPrefix = IntTextFormatFlags.NoPrefix,
        NoFullWidthCharacterBreak = IntTextFormatFlags.NoFullWidthCharacterBreak,
        PathEllipsis = IntTextFormatFlags.PathEllipsis,
        PrefixOnly = IntTextFormatFlags.PrefixOnly,
        Right = IntTextFormatFlags.Right,
        RightToLeft = IntTextFormatFlags.RightToLeft,
        SingleLine = IntTextFormatFlags.SingleLine,
        // NOTE: TextRenderer does not expose a way to set the tab stops (VSW#481267). Observe that ExapandTabs is available.
        // TabStop                     = IntTextFormatFlags.TabStop,
        TextBoxControl = IntTextFormatFlags.TextBoxControl,
        Top = IntTextFormatFlags.Top, // default
        VerticalCenter = IntTextFormatFlags.VerticalCenter,
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")] //Not a compound word
        WordBreak = IntTextFormatFlags.WordBreak,
        WordEllipsis = IntTextFormatFlags.WordEllipsis,
        PreserveGraphicsClipping = 0x01000000,
        PreserveGraphicsTranslateTransform = 0x02000000,
        GlyphOverhangPadding = 0x00000000, // default.
        NoPadding = 0x10000000,
        LeftAndRightPadding = 0x20000000
    }
}