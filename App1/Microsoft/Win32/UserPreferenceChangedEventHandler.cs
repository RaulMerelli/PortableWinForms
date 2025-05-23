namespace Microsoft.Win32
{
    using System.Security.Permissions;

    [HostProtectionAttribute(MayLeakOnAbort = true)]
    public delegate void UserPreferenceChangedEventHandler(object sender, UserPreferenceChangedEventArgs e);
}