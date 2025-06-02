namespace System.Windows.Forms
{
    public enum ScrollButton
    {
        Down = NativeMethods.DFCS_SCROLLDOWN,
        Left = NativeMethods.DFCS_SCROLLLEFT,
        Right = NativeMethods.DFCS_SCROLLRIGHT,
        Up = NativeMethods.DFCS_SCROLLUP,
        Min = NativeMethods.DFCS_SCROLLUP,
        Max = NativeMethods.DFCS_SCROLLRIGHT,
    }
}