namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum ValidationConstraints
    {
        None = 0x00,
        Selectable = 0x01,
        Enabled = 0x02,
        Visible = 0x04,
        TabStop = 0x08,
        ImmediateChildren = 0x10,
    }
}