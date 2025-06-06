﻿using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;

namespace System.Windows.Forms
{
    public sealed class WindowsFormsSynchronizationContext : SynchronizationContext, IDisposable
    {
        private Control controlToSendTo;
        private WeakReference destinationThreadRef;

        //ThreadStatics won't get initialized per thread: easiest to just invert the value.
        [ThreadStatic]
        private static bool dontAutoInstall;

        [ThreadStatic]
        private static bool inSyncContextInstallation;

        [ThreadStatic]
        private static SynchronizationContext previousSyncContext;


        public WindowsFormsSynchronizationContext()
        {
            DestinationThread = Thread.CurrentThread;   //store the current thread to ensure its still alive during an invoke.
            Application.ThreadContext context = Application.ThreadContext.FromCurrent();
            Debug.Assert(context != null);
            if (context != null)
            {
                controlToSendTo = context.MarshalingControl;
            }
            Debug.Assert(controlToSendTo.IsHandleCreated, "Marshaling control should have created its handle in its ctor.");
        }

        private WindowsFormsSynchronizationContext(Control marshalingControl, Thread destinationThread)
        {
            controlToSendTo = marshalingControl;
            this.DestinationThread = destinationThread;
            Debug.Assert(controlToSendTo.IsHandleCreated, "Marshaling control should have created its handle in its ctor.");
        }

        // VSWhidbey 476889: Directly holding onto the Thread can prevent ThreadStatics from finalizing.
        private Thread DestinationThread
        {
            get
            {
                if ((destinationThreadRef != null) && (destinationThreadRef.IsAlive))
                {
                    return destinationThreadRef.Target as Thread;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    destinationThreadRef = new WeakReference(value);
                }
            }
        }
        public void Dispose()
        {
            if (controlToSendTo != null)
            {
                if (!controlToSendTo.IsDisposed)
                {
                    controlToSendTo.Dispose();
                }
                controlToSendTo = null;
            }
        }

        // This is never called because we decide whether to Send or Post and we always post
        public override void Send(SendOrPostCallback d, Object state)
        {
            Thread destinationThread = DestinationThread;
            if (destinationThread == null || !destinationThread.IsAlive)
            {
                throw new InvalidAsynchronousStateException("ThreadNoLongerValid");
            }

            Debug.Assert(controlToSendTo != null, "Should always have the marshaling control by this point");

            if (controlToSendTo != null)
            {
                //controlToSendTo.Invoke(d, new object[] { state });
            }
        }

        public override void Post(SendOrPostCallback d, Object state)
        {
            Debug.Assert(controlToSendTo != null, "Should always have the marshaling control by this point");

            if (controlToSendTo != null)
            {
                //controlToSendTo.BeginInvoke(d, new object[] { state });
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return new WindowsFormsSynchronizationContext(controlToSendTo, DestinationThread);
        }

        // Determines whether we install the WindowsFormsSynchronizationContext when we create a control, or
        // when we start a message loop.  Default: true.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static bool AutoInstall
        {
            get
            {
                return !dontAutoInstall;
            }
            set
            {
                dontAutoInstall = !value;
            }
        }

        // Instantiate and install a WF op sync context, and save off the old one.
        internal static void InstallIfNeeded()
        {
            // Exit if we shouldn't auto-install, if we've already installed and we haven't uninstalled, 
            // or if we're being called recursively (creating the WF
            // async op sync context can create a parking window control).
            if (!AutoInstall || inSyncContextInstallation)
            {
                return;
            }

            if (SynchronizationContext.Current == null)
            {
                previousSyncContext = null;
            }

            if (previousSyncContext != null)
            {
                return;
            }

            inSyncContextInstallation = true;
            try
            {
                SynchronizationContext currentContext = AsyncOperationManager.SynchronizationContext;
                //Make sure we either have no sync context or that we have one of type SynchronizationContext
                if (currentContext == null || currentContext.GetType() == typeof(SynchronizationContext))
                {
                    previousSyncContext = currentContext;

                    // SECREVIEW : WindowsFormsSynchronizationContext.cctor generates a call to SetTopLevel on the MarshallingControl (hidden sycn hwnd), 
                    //             this call demands TopLevelWindow (UIPermissionWindow.SafeTopLevelWindows) so an assert for that permission is needed here.
                    //             The assert is safe, we are creating a thread context and the sync window is hidden.
                    new PermissionSet(PermissionState.Unrestricted).Assert();
                    try
                    {
                        AsyncOperationManager.SynchronizationContext = new WindowsFormsSynchronizationContext();
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            finally
            {
                inSyncContextInstallation = false;
            }
        }

        public static void Uninstall()
        {
            Uninstall(true);
        }

        internal static void Uninstall(bool turnOffAutoInstall)
        {
            if (AutoInstall)
            {
                WindowsFormsSynchronizationContext winFormsSyncContext = AsyncOperationManager.SynchronizationContext as WindowsFormsSynchronizationContext;
                if (winFormsSyncContext != null)
                {
                    try
                    {
                        new PermissionSet(PermissionState.Unrestricted).Assert();
                        if (previousSyncContext == null)
                        {
                            AsyncOperationManager.SynchronizationContext = new SynchronizationContext();
                        }
                        else
                        {
                            AsyncOperationManager.SynchronizationContext = previousSyncContext;
                        }
                    }
                    finally
                    {
                        previousSyncContext = null;
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            if (turnOffAutoInstall)
            {
                AutoInstall = false;
            }
        }
    }
}