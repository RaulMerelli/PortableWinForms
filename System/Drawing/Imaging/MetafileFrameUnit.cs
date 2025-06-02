namespace System.Drawing.Imaging
{
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;

    /**
     * Page unit constants
     */
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum MetafileFrameUnit
    {
        Pixel = GraphicsUnit.Pixel,
        Point = GraphicsUnit.Point,
        Inch = GraphicsUnit.Inch,
        Document = GraphicsUnit.Document,
        Millimeter = GraphicsUnit.Millimeter,
        GdiCompatible
    }
}