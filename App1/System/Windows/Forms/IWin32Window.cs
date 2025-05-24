namespace System.Windows.Forms
{
    using System;

    [System.Runtime.InteropServices.Guid("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IWin32Window
    {
        IntPtr Handle { get; }
    }
}
