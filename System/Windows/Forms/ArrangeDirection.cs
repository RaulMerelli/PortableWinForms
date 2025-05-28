namespace System.Windows.Forms
{
    using System.Diagnostics.CodeAnalysis;

    [System.Runtime.InteropServices.ComVisible(true)]
    [Flags]
    [
        SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")  // Maps to native enum.
    ]
    public enum ArrangeDirection
    {
        Down = NativeMethods.ARW_DOWN,
        Left = NativeMethods.ARW_LEFT,
        Right = NativeMethods.ARW_RIGHT,
        Up = NativeMethods.ARW_UP,
    }
}