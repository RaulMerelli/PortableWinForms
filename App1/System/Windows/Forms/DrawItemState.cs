namespace System.Windows.Forms
{
    using System;
    [Flags]
    public enum DrawItemState
    {
        Checked = NativeMethods.ODS_CHECKED,
        ComboBoxEdit = NativeMethods.ODS_COMBOBOXEDIT,
        Default = NativeMethods.ODS_DEFAULT,
        Disabled = NativeMethods.ODS_DISABLED,
        Focus = NativeMethods.ODS_FOCUS,
        Grayed = NativeMethods.ODS_GRAYED,
        HotLight = NativeMethods.ODS_HOTLIGHT,
        Inactive = NativeMethods.ODS_INACTIVE,
        NoAccelerator = NativeMethods.ODS_NOACCEL,
        NoFocusRect = NativeMethods.ODS_NOFOCUSRECT,
        Selected = NativeMethods.ODS_SELECTED,
        None = 0,
    }
}