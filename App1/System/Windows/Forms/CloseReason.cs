namespace System.Windows.Forms
{
    using System.Diagnostics.CodeAnalysis;

    public enum CloseReason
    {
        None = 0,
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        WindowsShutDown = 1,
        MdiFormClosing = 2,
        UserClosing = 3,
        TaskManagerClosing = 4,
        FormOwnerClosing = 5,
        ApplicationExitCall = 6
    }
}