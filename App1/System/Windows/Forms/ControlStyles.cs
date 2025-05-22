namespace System.Windows.Forms
{
    using System.ComponentModel;
    using System;

    [Flags]
    public enum ControlStyles
    {
        ContainerControl = 0x00000001,
        UserPaint = 0x00000002,
        Opaque = 0x00000004,
        ResizeRedraw = 0x00000010,
        FixedWidth = 0x00000020,
        FixedHeight = 0x00000040,
        StandardClick = 0x00000100,
        Selectable = 0x00000200,
        UserMouse = 0x00000400,
        SupportsTransparentBackColor = 0x00000800,
        StandardDoubleClick = 0x00001000,
        AllPaintingInWmPaint = 0x00002000,
        CacheText = 0x00004000,
        EnableNotifyMessage = 0x00008000,
        [EditorBrowsable(EditorBrowsableState.Never)] // It is recommended that you use the DoubleBuffer property instead.  See VSWhidbey 502873
        DoubleBuffer = 0x00010000,
        OptimizedDoubleBuffer = 0x00020000,
        UseTextForAccessibility = 0x00040000,
    }
}