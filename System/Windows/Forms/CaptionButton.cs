namespace System.Windows.Forms
{
    public enum CaptionButton
    {
        Close = NativeMethods.DFCS_CAPTIONCLOSE,
        Help = NativeMethods.DFCS_CAPTIONHELP,
        Maximize = NativeMethods.DFCS_CAPTIONMAX,
        Minimize = NativeMethods.DFCS_CAPTIONMIN,
        Restore = NativeMethods.DFCS_CAPTIONRESTORE,
    }
}