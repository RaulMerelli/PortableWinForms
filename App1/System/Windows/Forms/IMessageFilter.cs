namespace System.Windows.Forms
{
    using System.ServiceModel.Channels;

    public interface IMessageFilter
    {
        [
        System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        bool PreFilterMessage(ref Message m);
    }
}