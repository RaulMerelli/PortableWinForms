[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA905:SystemAndMicrosoftNamespacesRequireApproval", Scope = "namespace", Target = "System.Windows.Forms.VisualStyles")]

namespace System.Windows.Forms.VisualStyles
{
    public enum VisualStyleState
    {
        NoneEnabled = 0,
        ClientAreaEnabled = NativeMethods.STAP_ALLOW_CONTROLS,
        NonClientAreaEnabled = NativeMethods.STAP_ALLOW_NONCLIENT,
        ClientAndNonClientAreasEnabled = NativeMethods.STAP_ALLOW_NONCLIENT | NativeMethods.STAP_ALLOW_CONTROLS
    }
}