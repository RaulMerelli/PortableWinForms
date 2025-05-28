namespace System.Windows.Forms
{
    using System.Diagnostics.CodeAnalysis;

    [Flags]
    [
        SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")  // Maps to native enum.
    ]
    public enum ArrangeStartingPosition
    {
        BottomLeft = NativeMethods.ARW_BOTTOMLEFT,
        BottomRight = NativeMethods.ARW_BOTTOMRIGHT,
        Hide = NativeMethods.ARW_HIDE,
        TopLeft = NativeMethods.ARW_TOPLEFT,
        TopRight = NativeMethods.ARW_TOPRIGHT,
    }
}