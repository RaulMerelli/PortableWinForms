namespace System.Drawing.Imaging
{
    using System.Diagnostics.CodeAnalysis;

    //
    // Access modes used when calling IImage::LockBits
    //
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum ImageLockMode
    {
        ReadOnly = 0x0001,
        WriteOnly = 0x0002,
        ReadWrite = ReadOnly | WriteOnly,
        UserInputBuffer = 0x0004,
    }
}