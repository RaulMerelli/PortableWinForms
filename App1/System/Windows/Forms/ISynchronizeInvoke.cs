namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    public interface ISynchronizeInvoke
    {
        bool InvokeRequired { get; }

        [HostProtection(Synchronization = true, ExternalThreading = true)]
        IAsyncResult BeginInvoke(Delegate method, object[] args);

        object EndInvoke(IAsyncResult result);

        object Invoke(Delegate method, object[] args);
    }
}