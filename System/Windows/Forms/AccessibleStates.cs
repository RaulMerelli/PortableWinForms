namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum AccessibleStates
    {

        None = 0,
        Unavailable = (0x1),
        Selected = (0x2),
        Focused = (0x4),
        Pressed = (0x8),
        Checked = (0x10),
        Mixed = (0x20),
        Indeterminate = (Mixed),
        ReadOnly = (0x40),
        HotTracked = (0x80),
        Default = (0x100),
        Expanded = (0x200),
        Collapsed = (0x400),
        Busy = (0x800),
        Floating = (0x1000),
        Marqueed = (0x2000),
        Animated = (0x4000),
        Invisible = (0x8000),
        Offscreen = (0x10000),
        Sizeable = (0x20000),
        Moveable = (0x40000),
        SelfVoicing = (0x80000),
        Focusable = (0x100000),
        Selectable = (0x200000),
        Linked = (0x400000),
        Traversed = (0x800000),
        MultiSelectable = (0x1000000),
        ExtSelectable = (0x2000000),
        AlertLow = (0x4000000),
        AlertMedium = (0x8000000),
        AlertHigh = (0x10000000),
        Protected = (0x20000000),
        HasPopup = (0x40000000),
        [Obsolete("This enumeration value has been deprecated. There is no replacement. http://go.microsoft.com/fwlink/?linkid=14202")]
        Valid = (0x3fffffff),
    }
}