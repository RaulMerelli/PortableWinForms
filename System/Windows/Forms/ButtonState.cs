namespace System.Windows.Forms
{
    using System;

    [Flags]
    public enum ButtonState
    {
        Checked = NativeMethods.DFCS_CHECKED,
        Flat = NativeMethods.DFCS_FLAT,
        Inactive = NativeMethods.DFCS_INACTIVE,
        Normal = 0,
        Pushed = NativeMethods.DFCS_PUSHED,
        All = Flat | Checked | Pushed | Inactive,
    }
}