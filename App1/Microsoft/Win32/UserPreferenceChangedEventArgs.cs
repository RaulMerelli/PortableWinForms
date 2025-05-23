namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [HostProtectionAttribute(MayLeakOnAbort = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public class UserPreferenceChangedEventArgs : EventArgs
    {

        private readonly UserPreferenceCategory category;

        public UserPreferenceChangedEventArgs(UserPreferenceCategory category)
        {
            this.category = category;
        }

        public UserPreferenceCategory Category
        {
            get
            {
                return category;
            }
        }
    }
}
