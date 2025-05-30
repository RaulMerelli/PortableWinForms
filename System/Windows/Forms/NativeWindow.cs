﻿namespace System.Windows.Forms
{
    using System.Threading;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using System.ComponentModel;
    using Microsoft.Win32;
    using System.Text;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Versioning;

    [
        SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode),
        SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)
    ]
    public class NativeWindow : MarshalByRefObject, IWin32Window
    {
#if DEBUG
        private static readonly BooleanSwitch AlwaysUseNormalWndProc = new BooleanSwitch("AlwaysUseNormalWndProc", "Skips checking for the debugger when choosing the debuggable WndProc handler");
        private static readonly TraceSwitch WndProcChoice = new TraceSwitch("WndProcChoice", "Info about choice of WndProc");
#else
                private static readonly TraceSwitch WndProcChoice;
#endif

        /**
         * Table of prime numbers to use as hash table sizes. Each entry is the
         * smallest prime number larger than twice the previous entry.
         */
        private readonly static int[] primes = {
                    11,17,23,29,37,47,59,71,89,107,131,163,197,239,293,353,431,521,631,761,919,
                    1103,1327,1597,1931,2333,2801,3371,4049,4861,5839,7013,8419,10103,12143,14591,
                    17519,21023,25229,30293,36353,43627,52361,62851,75431,90523, 108631, 130363,
                    156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
                    968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287,
                    4999559, 5999471, 7199369
                };

        const int InitializedFlags = 0x01;
        const int DebuggerPresent = 0x02;
        const int UseDebuggableWndProc = 0x04;
        const int LoadConfigSettings = 0x08;
        const int AssemblyIsDebuggable = 0x10;

        // do we have any active HWNDs? 
        //       
        [ThreadStatic]
        static bool anyHandleCreated;
        static bool anyHandleCreatedInApp;

        const float hashLoadFactor = .72F;

        private static int handleCount;
        private static int hashLoadSize;
        private static HandleBucket[] hashBuckets;
        private static IntPtr userDefWindowProc;
        [ThreadStatic]
        private static byte wndProcFlags = 0;
        [ThreadStatic]
        private static byte userSetProcFlags = 0;
        private static byte userSetProcFlagsForApp;

        //nned to Store Table of Ids and Handles
        private static short globalID = 1;
        private static Dictionary<short, IntPtr> hashForIdHandle;
        private static Dictionary<IntPtr, short> hashForHandleId;
        private static object internalSyncObject = new object();
        private static object createWindowSyncObject = new object();

#if DEBUG
        AppDomain handleCreatedIn = null;
        string subclassStatus = "None";
#endif
        IntPtr handle;
        NativeMethods.WndProc windowProc;
        IntPtr windowProcPtr;
        IntPtr defWindowProc;
        bool suppressedGC;
        bool ownHandle;
        NativeWindow previousWindow; // doubly linked list of subclasses.
        NativeWindow nextWindow;
        WeakReference weakThisPtr;
        //private DpiAwarenessContext windowDpiAwarenessContext = DpiHelper.EnableDpiChangedHighDpiImprovements ?
        //                                                          CommonUnsafeNativeMethods.TryGetThreadDpiAwarenessContext() :
        //                                                          DpiAwarenessContext.DPI_AWARENESS_CONTEXT_UNSPECIFIED;

        //        static NativeWindow()
        //        {
        //            EventHandler shutdownHandler = new EventHandler(OnShutdown);
        //            AppDomain.CurrentDomain.ProcessExit += shutdownHandler;
        //            AppDomain.CurrentDomain.DomainUnload += shutdownHandler;

        //            // Initialize our static hash of handles.  I have chosen
        //            // a prime bucket based on a typical number of window handles
        //            // needed to start up a decent sized app.
        //            int hashSize = primes[4];
        //            hashBuckets = new HandleBucket[hashSize];

        //            hashLoadSize = (int)(hashLoadFactor * hashSize);
        //            if (hashLoadSize >= hashSize)
        //            {
        //                hashLoadSize = hashSize - 1;
        //            }

        //            //Intilialize the Hashtable for Id...
        //            hashForIdHandle = new Dictionary<short, IntPtr>();
        //            hashForHandleId = new Dictionary<IntPtr, short>();
        //        }

        public NativeWindow()
        {
            weakThisPtr = new WeakReference(this);
        }

        //        internal DpiAwarenessContext DpiAwarenessContext
        //        {
        //            get
        //            {
        //                return windowDpiAwarenessContext;
        //            }
        //        }

        //        ~NativeWindow()
        //        {
        //            ForceExitMessageLoop();
        //        }

        //        internal void ForceExitMessageLoop()
        //        {
        //            IntPtr h;
        //            bool ownedHandle;

        //            // 
        //            lock (this)
        //            {
        //                h = handle;
        //                ownedHandle = ownHandle;
        //            }

        //            if (handle != IntPtr.Zero)
        //            {
        //                //now, before we set handle to zero and finish the finalizer, let's send
        //                // a WM_NULL to the window.  Why?  Because if the main ui thread is INSIDE
        //                // the wndproc for this control during our unsubclass, then we could AV
        //                // when control finally reaches us.
        //                if (UnsafeNativeMethods.IsWindow(new HandleRef(null, handle)))
        //                {
        //                    int lpdwProcessId;  //unused
        //                    int id = SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, handle), out lpdwProcessId);
        //                    Application.ThreadContext ctx = Application.ThreadContext.FromId(id);
        //                    IntPtr threadHandle = (ctx == null ? IntPtr.Zero : ctx.GetHandle());

        //                    if (threadHandle != IntPtr.Zero)
        //                    {
        //                        int exitCode = 0;
        //                        SafeNativeMethods.GetExitCodeThread(new HandleRef(null, threadHandle), out exitCode);
        //                        if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && exitCode == NativeMethods.STATUS_PENDING)
        //                        {
        //                            IntPtr result;
        //                            if (UnsafeNativeMethods.SendMessageTimeout(new HandleRef(null, handle),
        //                                NativeMethods.WM_UIUNSUBCLASS, IntPtr.Zero, IntPtr.Zero,
        //                                UnsafeNativeMethods.SMTO_ABORTIFHUNG, 100, out result) == IntPtr.Zero)
        //                            {

        //                                //Debug.Fail("unable to ping HWND:" + handle.ToString() + " during finalization");
        //                            }
        //                        }
        //                    }
        //                }
        //                if (handle != IntPtr.Zero)
        //                {
        //                    //if the dest thread is gone, it should be safe to unsubclass here.
        //                    ReleaseHandle(true);
        //                }
        //            }

        //            if (h != IntPtr.Zero && ownedHandle)
        //            {
        //                // If we owned the handle, post a 
        //                // WM_CLOSE to get rid of it.
        //                //
        //                UnsafeNativeMethods.PostMessage(new HandleRef(this, h), NativeMethods.WM_CLOSE, 0, 0);
        //            }
        //        }

        internal static bool AnyHandleCreated
        {
            get
            {
                return anyHandleCreated;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        internal NativeWindow PreviousWindow
        {
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
            get
            {
                return previousWindow;
            }
        }

        //        internal static IntPtr UserDefindowProc
        //        {
        //            get
        //            {
        //                return userDefWindowProc;
        //            }
        //        }

        //        private static int WndProcFlags
        //        {
        //            get
        //            {
        //                // upcast for easy bit masking...
        //                //
        //                int intWndProcFlags = wndProcFlags;

        //                // Check to see if a debugger is installed.  If there is, then use
        //                // DebuggableCallback instead; this callback has no try/catch around it
        //                // so exceptions go to the debugger.
        //                //
        //                if (intWndProcFlags == 0)
        //                {
        //                    Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Init wndProcFlags");
        //                    Debug.Indent();

        //                    if (userSetProcFlags != 0)
        //                    {
        //                        intWndProcFlags = userSetProcFlags;
        //                    }
        //                    else if (userSetProcFlagsForApp != 0)
        //                    {
        //                        intWndProcFlags = userSetProcFlagsForApp;
        //                    }
        //                    else if (!Application.CustomThreadExceptionHandlerAttached)
        //                    {
        //                        if (Debugger.IsAttached)
        //                        {
        //                            Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Debugger is attached, using debuggable WndProc");
        //                            intWndProcFlags |= UseDebuggableWndProc;
        //                        }
        //                        else
        //                        {
        //                            intWndProcFlags = AdjustWndProcFlagsFromRegistry(intWndProcFlags);
        //                            Debug.WriteLineIf(WndProcChoice.TraceVerbose, "After reg check 0x" + intWndProcFlags.ToString("X", CultureInfo.InvariantCulture));
        //                            if ((intWndProcFlags & DebuggerPresent) != 0)
        //                            {
        //                                Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Debugger present");

        //                                intWndProcFlags = AdjustWndProcFlagsFromMetadata(intWndProcFlags);

        //                                if ((intWndProcFlags & AssemblyIsDebuggable) != 0)
        //                                {
        //                                    if ((intWndProcFlags & LoadConfigSettings) != 0)
        //                                    {
        //                                        intWndProcFlags = AdjustWndProcFlagsFromConfig(intWndProcFlags);
        //                                        Debug.WriteLineIf(WndProcChoice.TraceVerbose, "After config check 0x" + intWndProcFlags.ToString("X", CultureInfo.InvariantCulture));
        //                                    }
        //                                    else
        //                                    {
        //                                        intWndProcFlags |= UseDebuggableWndProc;
        //                                    }
        //                                }
        //                            }

        //                            Debug.WriteLineIf(WndProcChoice.TraceVerbose, "After config & debugger check 0x" + intWndProcFlags.ToString("X", CultureInfo.InvariantCulture));
        //                        }
        //                    }

        //#if DEBUG
        //                    if (AlwaysUseNormalWndProc.Enabled)
        //                    {
        //                        Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Stripping debuggablewndproc due to AlwaysUseNormalWndProc switch");
        //                        intWndProcFlags &= ~UseDebuggableWndProc;
        //                    }
        //#endif
        //                    intWndProcFlags |= InitializedFlags;
        //                    Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Final 0x" + intWndProcFlags.ToString("X", CultureInfo.InvariantCulture));
        //                    wndProcFlags = (byte)intWndProcFlags;
        //                    Debug.Unindent();
        //                }

        //                return intWndProcFlags;
        //            }
        //        }

        //        internal static bool WndProcShouldBeDebuggable
        //        {
        //            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        //            get
        //            {
        //                return (WndProcFlags & UseDebuggableWndProc) != 0;
        //            }
        //        }

        //        private static void AddWindowToTable(IntPtr handle, NativeWindow window)
        //        {

        //            Debug.Assert(handle != IntPtr.Zero, "Should never insert a zero handle into the hash");

        //            lock (internalSyncObject)
        //            {

        //                if (handleCount >= hashLoadSize)
        //                {
        //                    ExpandTable();
        //                }

        //                // set a flag that this thread is tracking an HWND
        //                //
        //                anyHandleCreated = true;
        //                // ...same for the application
        //                anyHandleCreatedInApp = true;

        //                uint seed;
        //                uint incr;
        //                // Assume we only have one thread writing concurrently.  Modify
        //                // buckets to contain new data, as long as we insert in the right order.
        //                uint hashcode = InitHash(handle, hashBuckets.Length, out seed, out incr);

        //                int ntry = 0;
        //                int emptySlotNumber = -1; // We use the empty slot number to cache the first empty slot. We chose to reuse slots
        //                                          // create by remove that have the collision bit set over using up new slots.

        //                GCHandle root = GCHandle.Alloc(window, GCHandleType.Weak);

        //                do
        //                {
        //                    int bucketNumber = (int)(seed % (uint)hashBuckets.Length);

        //                    if (emptySlotNumber == -1 && (hashBuckets[bucketNumber].handle == new IntPtr(-1)) && (hashBuckets[bucketNumber].hash_coll < 0))
        //                        emptySlotNumber = bucketNumber;

        //                    //We need to check if the collision bit is set because we have the possibility where the first
        //                    //item in the hash-chain has been deleted.
        //                    if ((hashBuckets[bucketNumber].handle == IntPtr.Zero) ||
        //                        (hashBuckets[bucketNumber].handle == new IntPtr(-1) && ((hashBuckets[bucketNumber].hash_coll & unchecked(0x80000000)) == 0)))
        //                    {

        //                        if (emptySlotNumber != -1)
        //                        { // Reuse slot
        //                            bucketNumber = emptySlotNumber;
        //                        }

        //                        // Always set the hash_coll last because there may be readers
        //                        // reading the table right now on other threads.
        //                        hashBuckets[bucketNumber].window = root;
        //                        hashBuckets[bucketNumber].handle = handle;
        //#if DEBUG
        //                        hashBuckets[bucketNumber].owner = window.ToString();
        //#endif
        //                        hashBuckets[bucketNumber].hash_coll |= (int)hashcode;
        //                        handleCount++;
        //                        return;
        //                    }

        //                    // If there is an existing window in this slot, reuse it.  Be sure to hook up the previous and next
        //                    // window pointers so we can get back to the right window.
        //                    //
        //                    if (((hashBuckets[bucketNumber].hash_coll & 0x7FFFFFFF) == hashcode) && handle == hashBuckets[bucketNumber].handle)
        //                    {
        //                        GCHandle prevWindow = hashBuckets[bucketNumber].window;
        //                        if (prevWindow.IsAllocated)
        //                        {
        //                            if (prevWindow.Target != null)
        //                            {
        //                                window.previousWindow = ((NativeWindow)prevWindow.Target);
        //                                Debug.Assert(window.previousWindow.nextWindow == null, "Last window in chain should have null next ptr");
        //                                window.previousWindow.nextWindow = window;
        //                            }
        //                            prevWindow.Free();
        //                        }
        //                        hashBuckets[bucketNumber].window = root;
        //#if DEBUG
        //                        string ownerString = string.Empty;
        //                        NativeWindow w = window;
        //                        while (w != null)
        //                        {
        //                            ownerString += ("->" + w.ToString());
        //                            w = w.previousWindow;
        //                        }
        //                        hashBuckets[bucketNumber].owner = ownerString;
        //#endif
        //                        return;
        //                    }

        //                    if (emptySlotNumber == -1)
        //                    {// We don't need to set the collision bit here since we already have an empty slot
        //                        hashBuckets[bucketNumber].hash_coll |= unchecked((int)0x80000000);
        //                    }

        //                    seed += incr;

        //                } while (++ntry < hashBuckets.Length);

        //                if (emptySlotNumber != -1)
        //                {
        //                    // Always set the hash_coll last because there may be readers
        //                    // reading the table right now on other threads.
        //                    hashBuckets[emptySlotNumber].window = root;
        //                    hashBuckets[emptySlotNumber].handle = handle;
        //#if DEBUG
        //                    hashBuckets[emptySlotNumber].owner = window.ToString();
        //#endif
        //                    hashBuckets[emptySlotNumber].hash_coll |= (int)hashcode;
        //                    handleCount++;
        //                    return;
        //                }
        //            }

        //            // If you see this assert, make sure load factor & count are reasonable.
        //            // Then verify that our double hash function (h2, described at top of file)
        //            // meets the requirements described above. You should never see this assert.
        //            Debug.Fail("native window hash table insert failed!  Load factor too high, or our double hashing function is incorrect.");
        //        }

        //        internal static void AddWindowToIDTable(object wrapper, IntPtr handle)
        //        {
        //            NativeWindow.hashForIdHandle[NativeWindow.globalID] = handle;
        //            NativeWindow.hashForHandleId[handle] = NativeWindow.globalID;
        //            UnsafeNativeMethods.SetWindowLong(new HandleRef(wrapper, handle), NativeMethods.GWL_ID, new HandleRef(wrapper, (IntPtr)globalID));
        //            globalID++;

        //        }

        //        // disable inlining optimization - VSW 454165
        //        // This method introduces a dependency on System.Configuration.dll
        //        // due to its usage of the type WindowsFormsSection
        //        //
        //        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        //        private static int AdjustWndProcFlagsFromConfig(int wndProcFlags)
        //        {
        //            if (WindowsFormsSection.GetSection().JitDebugging)
        //            {
        //                wndProcFlags |= UseDebuggableWndProc;
        //            }
        //            return wndProcFlags;
        //        }

        //        private static int AdjustWndProcFlagsFromRegistry(int wndProcFlags)
        //        {
        //            // This is the enum used to define the meaning of the debug flag...
        //            //

        //            /*
        //            //
        //            // We split the value of DbgJITDebugLaunchSetting between the value for whether or not to ask the user and between a
        //            // mask of places to ask. The places to ask are specified in the UnhandledExceptionLocation enum in excep.h.
        //            //
        //            enum DebuggerLaunchSetting
        //            {
        //                DLS_ASK_USER          = 0x00000000,
        //                DLS_TERMINATE_APP     = 0x00000001,
        //                DLS_ATTACH_DEBUGGER   = 0x00000002,
        //                DLS_QUESTION_MASK     = 0x0000000F,
        //                DLS_ASK_WHEN_SERVICE  = 0x00000010,
        //                DLS_MODIFIER_MASK     = 0x000000F0,
        //                DLS_LOCATION_MASK     = 0xFFFFFF00,
        //                DLS_LOCATION_SHIFT    = 8 // Shift right 8 bits to get a UnhandledExceptionLocation value from the location part.
        //            };
        //            */

        //            new RegistryPermission(PermissionState.Unrestricted).Assert();
        //            try
        //            {

        //                Debug.Assert(wndProcFlags == 0x00, "Re-entrancy into IsDebuggerInstalled()");

        //                RegistryKey debugKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework");
        //                if (debugKey == null)
        //                {
        //                    Debug.WriteLineIf(WndProcChoice.TraceVerbose, ".NETFramework key not found");
        //                    return wndProcFlags;
        //                }
        //                try
        //                {
        //                    object value = debugKey.GetValue("DbgJITDebugLaunchSetting");
        //                    if (value != null)
        //                    {
        //                        Debug.WriteLineIf(WndProcChoice.TraceVerbose, "DbgJITDebugLaunchSetting value found, debugger is installed");
        //                        int dbgJit = 0;
        //                        try
        //                        {
        //                            dbgJit = (int)value;
        //                        }
        //                        catch (InvalidCastException)
        //                        {
        //                            // If the value isn't a DWORD, then we will 
        //                            // continue to use the non-debuggable wndproc
        //                            //
        //                            dbgJit = 1;
        //                        }

        //                        // From the enum above, 0x01 == "Terminate App"... for
        //                        // anything else, we should flag that the debugger
        //                        // will catch unhandled exceptions
        //                        //
        //                        if (dbgJit != 1)
        //                        {
        //                            wndProcFlags |= DebuggerPresent;
        //                            wndProcFlags |= LoadConfigSettings;
        //                        }
        //                    }
        //                    else if (debugKey.GetValue("DbgManagedDebugger") != null)
        //                    {
        //                        //if there is a debugger installed, check the config files to decide
        //                        //whether to allow JIT debugging.
        //                        wndProcFlags |= DebuggerPresent;
        //                        wndProcFlags |= LoadConfigSettings;
        //                    }
        //                }
        //                finally
        //                {
        //                    debugKey.Close();
        //                }
        //            }
        //            finally
        //            {
        //                System.Security.CodeAccessPermission.RevertAssert();
        //            }

        //            return wndProcFlags;
        //        }

        //        static int AdjustWndProcFlagsFromMetadata(int wndProcFlags)
        //        {
        //            if ((wndProcFlags & DebuggerPresent) != 0)
        //            {
        //                Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Debugger present, checking for debuggable entry assembly");
        //                Assembly entry = Assembly.GetEntryAssembly();
        //                if (entry != null)
        //                {
        //                    if (Attribute.IsDefined(entry, typeof(DebuggableAttribute)))
        //                    {
        //                        Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Debuggable attribute on assembly");
        //                        Attribute[] attr = Attribute.GetCustomAttributes(entry, typeof(DebuggableAttribute));
        //                        if (attr.Length > 0)
        //                        {
        //                            DebuggableAttribute dbg = (DebuggableAttribute)attr[0];
        //                            if (dbg.IsJITTrackingEnabled)
        //                            {
        //                                // Only if the assembly is really setup for debugging
        //                                // does it really make sense to enable the jitDebugging
        //                                //
        //                                Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Entry assembly is debuggable");
        //                                wndProcFlags |= AssemblyIsDebuggable;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            return wndProcFlags;
        //        }

        //        public void AssignHandle(IntPtr handle)
        //        {
        //            AssignHandle(handle, true);
        //        }

        //        // 




        //        internal void AssignHandle(IntPtr handle, bool assignUniqueID)
        //        {
        //            // 
        //            lock (this)
        //            {
        //                CheckReleased();
        //                Debug.Assert(handle != IntPtr.Zero, "handle is 0");
        //#if DEBUG
        //                handleCreatedIn = AppDomain.CurrentDomain;
        //#endif
        //                this.handle = handle;

        //                // Synchronizing the handle DPiAwarenessContext with control DpiAwarenessContext. They can be out of sync in case control has a parent set before the handle was created.
        //                if (DpiHelper.EnableDpiChangedHighDpiImprovements && windowDpiAwarenessContext != DpiAwarenessContext.DPI_AWARENESS_CONTEXT_UNSPECIFIED)
        //                {
        //                    var handleDpiContext = CommonUnsafeNativeMethods.TryGetDpiAwarenessContextForWindow(this.handle);
        //                    if (handleDpiContext != DpiAwarenessContext.DPI_AWARENESS_CONTEXT_UNSPECIFIED && !CommonUnsafeNativeMethods.TryFindDpiAwarenessContextsEqual(windowDpiAwarenessContext, handleDpiContext))
        //                    {
        //                        windowDpiAwarenessContext = handleDpiContext;
        //                    }
        //                }

        //                if (userDefWindowProc == IntPtr.Zero)
        //                {
        //                    string defproc = (Marshal.SystemDefaultCharSize == 1 ? "DefWindowProcA" : "DefWindowProcW");

        //                    userDefWindowProc = UnsafeNativeMethods.GetProcAddress(new HandleRef(null, UnsafeNativeMethods.GetModuleHandle("user32.dll")), defproc);
        //                    if (userDefWindowProc == IntPtr.Zero)
        //                    {
        //                        throw new Win32Exception();
        //                    }
        //                }

        //                defWindowProc = UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC);
        //                Debug.Assert(defWindowProc != IntPtr.Zero, "defWindowProc is 0");


        //                if (WndProcShouldBeDebuggable)
        //                {
        //                    Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Using debuggable wndproc");
        //                    windowProc = new NativeMethods.WndProc(this.DebuggableCallback);
        //                }
        //                else
        //                {
        //                    Debug.WriteLineIf(WndProcChoice.TraceVerbose, "Using normal wndproc");
        //                    windowProc = new NativeMethods.WndProc(this.Callback);
        //                }

        //                AddWindowToTable(handle, this);

        //                UnsafeNativeMethods.SetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC, windowProc);
        //                windowProcPtr = UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC);
        //                Debug.Assert(defWindowProc != windowProcPtr, "Uh oh! Subclassed ourselves!!!");
        //                if (assignUniqueID &&
        //                    (unchecked((int)((long)UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_STYLE))) & NativeMethods.WS_CHILD) != 0 &&
        //                     unchecked((int)((long)UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_ID))) == 0)
        //                {
        //                    UnsafeNativeMethods.SetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_ID, new HandleRef(this, handle));
        //                }


        //                if (suppressedGC)
        //                {
        //                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
        //                    try
        //                    {
        //                        GC.ReRegisterForFinalize(this);
        //                    }
        //                    finally
        //                    {
        //                        CodeAccessPermission.RevertAssert();
        //                    }
        //                    suppressedGC = false;
        //                }

        //                OnHandleChange();
        //            }
        //        }

        //        private IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        //        {

        //            // Note: if you change this code be sure to change the 
        //            // corresponding code in DebuggableCallback below!

        //            Message m = Message.Create(hWnd, msg, wparam, lparam);

        //            try
        //            {
        //                if (weakThisPtr.IsAlive && weakThisPtr.Target != null)
        //                {
        //                    WndProc(ref m);
        //                }
        //                else
        //                {
        //                    DefWndProc(ref m);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                OnThreadException(e);
        //            }
        //            finally
        //            {
        //                if (msg == NativeMethods.WM_NCDESTROY) ReleaseHandle(false);
        //                if (msg == NativeMethods.WM_UIUNSUBCLASS) ReleaseHandle(true);
        //            }

        //            return m.Result;
        //        }

        //        private void CheckReleased()
        //        {
        //            if (handle != IntPtr.Zero)
        //            {
        //                throw new InvalidOperationException(SR.GetString(SR.HandleAlreadyExists));
        //            }
        //        }

        //        [
        //            // The call to UnsafeNativeMethods.CreateWindowEx is actually an interop call...
        //            System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")
        //        ]
        //        public virtual void CreateHandle(CreateParams cp)
        //        {

        //            Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "CreateAnyWindow Demanded");
        //            IntSecurity.CreateAnyWindow.Demand();

        //            if ((cp.Style & NativeMethods.WS_CHILD) != NativeMethods.WS_CHILD
        //                || cp.Parent == IntPtr.Zero)
        //            {

        //                Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "TopLevelWindow Demanded");
        //                IntSecurity.TopLevelWindow.Demand();
        //            }

        //            // 
        //            lock (this)
        //            {
        //                CheckReleased();
        //                WindowClass windowClass = WindowClass.Create(cp.ClassName, cp.ClassStyle);
        //                lock (createWindowSyncObject)
        //                {

        //                    // VSWhidbey 528778
        //                    // Why are we checking the handle again after calling CheckReleased()?  It turns                     
        //                    // out the CLR will sometimes pump messages while we're waiting on the lock.  If
        //                    // a message comes through (say a WM_ACTIVATE for the parent) which causes the 
        //                    // handle to be created, we can try to create the handle twice for the same 
        //                    // NativeWindow object. See bug for more details.

        //                    if (this.handle != IntPtr.Zero)
        //                    {
        //                        return;
        //                    }
        //                    windowClass.targetWindow = this;
        //                    IntPtr createResult = IntPtr.Zero;
        //                    int lastWin32Error = 0;

        //                    // Parking window dpi awarness context need to match with dpi awarenss context of control being 
        //                    // parented to this parkign window. Otherwise, reparenting of control will fail.
        //                    using (DpiHelper.EnterDpiAwarenessScope(this.windowDpiAwarenessContext))
        //                    {
        //                        IntPtr modHandle = UnsafeNativeMethods.GetModuleHandle(null);

        //                        // Win98 apparently doesn't believe in returning E_OUTOFMEMORY.  They'd much
        //                        // rather just AV.  So we catch this and then we re-throw an out of memory error.
        //                        //
        //                        try
        //                        {

        //                            //(bug 109840)
        //                            //CreateWindowEx() is throwing because we're passing the WindowText arg with a string of length  > 32767.  
        //                            //It looks like the Windows call (CreateWindowEx) will only work 
        //                            //for string lengths no greater than the max length of a 16 bit int (32767).

        //                            //We need to check the length of the string we're passing into CreateWindowEx().  
        //                            //If it exceeds the max, we should take the substring....

        //                            if (cp.Caption != null && cp.Caption.Length > Int16.MaxValue)
        //                            {
        //                                cp.Caption = cp.Caption.Substring(0, Int16.MaxValue);
        //                            }

        //                            createResult = UnsafeNativeMethods.CreateWindowEx(cp.ExStyle, windowClass.windowClassName,
        //                                                                              cp.Caption, cp.Style, cp.X, cp.Y, cp.Width, cp.Height, new HandleRef(cp, cp.Parent), NativeMethods.NullHandleRef,
        //                                                                              new HandleRef(null, modHandle), cp.Param);

        //                            lastWin32Error = Marshal.GetLastWin32Error();
        //                        }
        //                        catch (NullReferenceException e)
        //                        {
        //                            throw new OutOfMemoryException(SR.GetString(SR.ErrorCreatingHandle), e);
        //                        }
        //                    }
        //                    windowClass.targetWindow = null;

        //                    Debug.WriteLineIf(CoreSwitches.PerfTrack.Enabled, "Handle created of type '" + cp.ClassName + "' with caption '" + cp.Caption + "' from NativeWindow of type '" + GetType().FullName + "'");

        //                    if (createResult == IntPtr.Zero)
        //                    {
        //                        throw new Win32Exception(lastWin32Error, SR.GetString(SR.ErrorCreatingHandle));
        //                    }
        //                    ownHandle = true;
        //                    System.Internal.HandleCollector.Add(createResult, NativeMethods.CommonHandles.Window);
        //                }
        //            }
        //        }

        //        private IntPtr DebuggableCallback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        //        {

        //            // Note: if you change this code be sure to change the 
        //            // corresponding code in Callback above!

        //            Message m = Message.Create(hWnd, msg, wparam, lparam);

        //            try
        //            {
        //                if (weakThisPtr.IsAlive && weakThisPtr.Target != null)
        //                {
        //                    WndProc(ref m);
        //                }
        //                else
        //                {
        //                    DefWndProc(ref m);
        //                }
        //            }
        //            finally
        //            {
        //                if (msg == NativeMethods.WM_NCDESTROY) ReleaseHandle(false);
        //                if (msg == NativeMethods.WM_UIUNSUBCLASS) ReleaseHandle(true);
        //            }

        //            return m.Result;
        //        }

        //        public void DefWndProc(ref Message m)
        //        {
        //            if (previousWindow == null)
        //            {
        //                if (defWindowProc == IntPtr.Zero)
        //                {

        //#if DEBUG
        //                    Debug.Fail("Can't find a default window procedure for message " + m.ToString() + " on class " + GetType().Name + " subclass status: " + subclassStatus);
        //#endif

        //                    // At this point, there isn't much we can do.  There's a
        //                    // small chance the following line will allow the rest of
        //                    // the program to run, but don't get your hopes up.
        //                    m.Result = UnsafeNativeMethods.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
        //                    return;
        //                }
        //                m.Result = UnsafeNativeMethods.CallWindowProc(defWindowProc, m.HWnd, m.Msg, m.WParam, m.LParam);
        //            }
        //            else
        //            {
        //                Debug.Assert(previousWindow != this, "Looping in our linked list");
        //                m.Result = previousWindow.Callback(m.HWnd, m.Msg, m.WParam, m.LParam);
        //            }
        //        }

        //        public virtual void DestroyHandle()
        //        {
        //            // 
        //            lock (this)
        //            {
        //                if (handle != IntPtr.Zero)
        //                {
        //                    if (!UnsafeNativeMethods.DestroyWindow(new HandleRef(this, handle)))
        //                    {
        //                        UnSubclass();
        //                        //then post a close and let it do whatever it needs to do on its own.
        //                        UnsafeNativeMethods.PostMessage(new HandleRef(this, handle), NativeMethods.WM_CLOSE, 0, 0);
        //                    }
        //                    handle = IntPtr.Zero;
        //                    ownHandle = false;
        //                }

        //                // Now that we have disposed, there is no need to finalize us any more.  So
        //                // Mark to the garbage collector that we no longer need finalization.
        //                //
        //                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
        //                try
        //                {
        //                    GC.SuppressFinalize(this);
        //                }
        //                finally
        //                {
        //                    CodeAccessPermission.RevertAssert();
        //                }
        //                suppressedGC = true;
        //            }
        //        }

        //        private static void ExpandTable()
        //        {
        //            // Allocate new Array 
        //            int oldhashsize = hashBuckets.Length;

        //            int hashsize = GetPrime(1 + oldhashsize * 2);

        //            // Don't replace any internal state until we've finished adding to the 
        //            // new bucket[].  This serves two purposes: 1) Allow concurrent readers
        //            // to see valid hashtable contents at all times and 2) Protect against
        //            // an OutOfMemoryException while allocating this new bucket[].
        //            HandleBucket[] newBuckets = new HandleBucket[hashsize];

        //            // rehash table into new buckets
        //            int nb;
        //            for (nb = 0; nb < oldhashsize; nb++)
        //            {
        //                HandleBucket oldb = hashBuckets[nb];
        //                if ((oldb.handle != IntPtr.Zero) && (oldb.handle != new IntPtr(-1)))
        //                {

        //                    // Now re-fit this entry into the table
        //                    //
        //                    uint seed = (uint)oldb.hash_coll & 0x7FFFFFFF;
        //                    uint incr = (uint)(1 + (((seed >> 5) + 1) % ((uint)newBuckets.Length - 1)));

        //                    do
        //                    {
        //                        int bucketNumber = (int)(seed % (uint)newBuckets.Length);

        //                        if ((newBuckets[bucketNumber].handle == IntPtr.Zero) || (newBuckets[bucketNumber].handle == new IntPtr(-1)))
        //                        {
        //                            newBuckets[bucketNumber].window = oldb.window;
        //                            newBuckets[bucketNumber].handle = oldb.handle;
        //                            newBuckets[bucketNumber].hash_coll |= oldb.hash_coll & 0x7FFFFFFF;
        //                            break;
        //                        }
        //                        newBuckets[bucketNumber].hash_coll |= unchecked((int)0x80000000);
        //                        seed += incr;
        //                    } while (true);
        //                }
        //            }

        //            // New bucket[] is good to go - replace buckets and other internal state.
        //            hashBuckets = newBuckets;

        //            hashLoadSize = (int)(hashLoadFactor * hashsize);
        //            if (hashLoadSize >= hashsize)
        //            {
        //                hashLoadSize = hashsize - 1;
        //            }
        //        }

        //        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        //        public static NativeWindow FromHandle(IntPtr handle)
        //        {
        //            if (handle != IntPtr.Zero && handleCount > 0)
        //            {
        //                return GetWindowFromTable(handle);
        //            }
        //            return null;
        //        }

        //        private static int GetPrime(int minSize)
        //        {
        //            if (minSize < 0)
        //            {
        //                Debug.Fail("NativeWindow hashtable capacity overflow");
        //                throw new OutOfMemoryException();
        //            }
        //            for (int i = 0; i < primes.Length; i++)
        //            {
        //                int size = primes[i];
        //                if (size >= minSize) return size;
        //            }
        //            //outside of our predefined table. 
        //            //compute the hard way. 
        //            for (int j = ((minSize - 2) | 1); j < Int32.MaxValue; j += 2)
        //            {
        //                bool prime = true;

        //                if ((j & 1) != 0)
        //                {
        //                    int target = (int)Math.Sqrt(j);
        //                    for (int divisor = 3; divisor < target; divisor += 2)
        //                    {
        //                        if ((j % divisor) == 0)
        //                        {
        //                            prime = false;
        //                            break;
        //                        }
        //                    }
        //                    if (prime) return j;
        //                }
        //                else
        //                {
        //                    if (j == 2)
        //                    {
        //                        return j;
        //                    }
        //                }
        //            }
        //            return minSize;
        //        }

        //        private static NativeWindow GetWindowFromTable(IntPtr handle)
        //        {

        //            Debug.Assert(handle != IntPtr.Zero, "Zero handles cannot be stored in the table");

        //            // Take a snapshot of buckets, in case another thread does a resize
        //            HandleBucket[] buckets = hashBuckets;
        //            uint seed;
        //            uint incr;
        //            int ntry = 0;
        //            uint hashcode = InitHash(handle, buckets.Length, out seed, out incr);

        //            HandleBucket b;
        //            do
        //            {
        //                int bucketNumber = (int)(seed % (uint)buckets.Length);
        //                b = buckets[bucketNumber];
        //                if (b.handle == IntPtr.Zero)
        //                {
        //                    return null;
        //                }
        //                if (((b.hash_coll & 0x7FFFFFFF) == hashcode) && handle == b.handle)
        //                {
        //                    if (b.window.IsAllocated)
        //                    {
        //                        return (NativeWindow)b.window.Target;
        //                    }
        //                }
        //                seed += incr;
        //            }
        //            while (b.hash_coll < 0 && ++ntry < buckets.Length);
        //            return null;
        //        }

        //        internal IntPtr GetHandleFromID(short id)
        //        {
        //            IntPtr handle;
        //            if (NativeWindow.hashForIdHandle == null || !NativeWindow.hashForIdHandle.TryGetValue(id, out handle))
        //            {
        //                handle = IntPtr.Zero;
        //            }

        //            return handle;
        //        }

        //        private static uint InitHash(IntPtr handle, int hashsize, out uint seed, out uint incr)
        //        {
        //            // Hashcode must be positive.  Also, we must not use the sign bit, since
        //            // that is used for the collision bit.
        //            uint hashcode = ((uint)handle.GetHashCode()) & 0x7FFFFFFF;
        //            seed = (uint)hashcode;
        //            // Restriction: incr MUST be between 1 and hashsize - 1, inclusive for
        //            // the modular arithmetic to work correctly.  This guarantees you'll
        //            // visit every bucket in the table exactly once within hashsize 
        //            // iterations.  Violate this and it'll cause obscure bugs forever.
        //            // If you change this calculation for h2(key), update putEntry too!
        //            incr = (uint)(1 + (((seed >> 5) + 1) % ((uint)hashsize - 1)));
        //            return hashcode;
        //        }

        //        protected virtual void OnHandleChange()
        //        {
        //        }

        //        [PrePrepareMethod]
        //        private static void OnShutdown(object sender, EventArgs e)
        //        {

        //            // If we still have windows allocated, we must sling them to userDefWindowProc
        //            // or else they will AV if they get a message after the managed code has been
        //            // removed.  In debug builds, we assert and give the "ToString" of the native
        //            // window. In retail we just detatch the window proc and let it go.  Note that
        //            // we cannot call DestroyWindow because this API will fail if called from
        //            // an incorrect thread.
        //            //
        //            if (handleCount > 0)
        //            {

        //                Debug.Assert(userDefWindowProc != IntPtr.Zero, "We have active windows but no user window proc?");

        //                lock (internalSyncObject)
        //                {
        //                    for (int i = 0; i < hashBuckets.Length; i++)
        //                    {
        //                        HandleBucket b = hashBuckets[i];
        //                        if (b.handle != IntPtr.Zero && b.handle != new IntPtr(-1))
        //                        {
        //                            HandleRef href = new HandleRef(b, b.handle);
        //                            UnsafeNativeMethods.SetWindowLong(href, NativeMethods.GWL_WNDPROC, new HandleRef(null, userDefWindowProc));
        //                            UnsafeNativeMethods.SetClassLong(href, NativeMethods.GCL_WNDPROC, userDefWindowProc);
        //                            UnsafeNativeMethods.PostMessage(href, NativeMethods.WM_CLOSE, 0, 0);

        //                            // Fish out the Window object, if it is valid, and NULL the handle pointer.  This
        //                            // way the rest of Microsoft won't think the handle is still valid here.
        //                            if (b.window.IsAllocated)
        //                            {
        //                                NativeWindow w = (NativeWindow)b.window.Target;
        //                                if (w != null)
        //                                {
        //                                    w.handle = IntPtr.Zero;
        //                                }
        //                            }

        //#if DEBUG && FINALIZATION_WATCH
        //                            Debug.Fail("Window did not clean itself up: " + b.owner);
        //#endif

        //                            b.window.Free();
        //                        }
        //                        hashBuckets[i].handle = IntPtr.Zero;
        //                        hashBuckets[i].hash_coll = 0;
        //                    }

        //                    handleCount = 0;
        //                }
        //            }

        //            WindowClass.DisposeCache();
        //        }

        protected virtual void OnThreadException(Exception e)
        {
        }

        //        public virtual void ReleaseHandle()
        //        {
        //            ReleaseHandle(true);
        //        }

        //        private void ReleaseHandle(bool handleValid)
        //        {
        //            if (handle != IntPtr.Zero)
        //            {
        //                // 
        //                lock (this)
        //                {
        //                    if (handle != IntPtr.Zero)
        //                    {
        //                        if (handleValid)
        //                        {
        //                            UnSubclass();
        //                        }

        //                        RemoveWindowFromTable(handle, this);

        //                        if (ownHandle)
        //                        {
        //                            System.Internal.HandleCollector.Remove(handle, NativeMethods.CommonHandles.Window);
        //                            ownHandle = false;
        //                        }

        //                        handle = IntPtr.Zero;
        //                        //if not finalizing already.
        //                        if (weakThisPtr.IsAlive && weakThisPtr.Target != null)
        //                        {
        //                            OnHandleChange();

        //                            // Now that we have disposed, there is no need to finalize us any more.  So
        //                            // Mark to the garbage collector that we no longer need finalization.
        //                            //
        //                            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
        //                            try
        //                            {
        //                                GC.SuppressFinalize(this);
        //                            }
        //                            finally
        //                            {
        //                                CodeAccessPermission.RevertAssert();
        //                            }
        //                            suppressedGC = true;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        private static void RemoveWindowFromTable(IntPtr handle, NativeWindow window)
        //        {

        //            Debug.Assert(handle != IntPtr.Zero, "Incorrect handle");

        //            lock (internalSyncObject)
        //            {

        //                uint seed;
        //                uint incr;
        //                // Assuming only one concurrent writer, write directly into buckets.
        //                uint hashcode = InitHash(handle, hashBuckets.Length, out seed, out incr);
        //                int ntry = 0;
        //                NativeWindow prevWindow = window.PreviousWindow;
        //                HandleBucket b;

        //                int bn; // bucketNumber
        //                do
        //                {
        //                    bn = (int)(seed % (uint)hashBuckets.Length);  // bucketNumber
        //                    b = hashBuckets[bn];
        //                    if (((b.hash_coll & 0x7FFFFFFF) == hashcode) && handle == b.handle)
        //                    {

        //                        bool shouldRemoveBucket = (window.nextWindow == null);
        //                        bool shouldReplaceBucket = IsRootWindowInListWithChildren(window);

        //                        // We need to fixup the link pointers of window here.
        //                        //
        //                        if (window.previousWindow != null)
        //                        {
        //                            window.previousWindow.nextWindow = window.nextWindow;
        //                        }
        //                        if (window.nextWindow != null)
        //                        {
        //                            window.nextWindow.defWindowProc = window.defWindowProc;
        //                            window.nextWindow.previousWindow = window.previousWindow;
        //                        }

        //                        window.nextWindow = null;
        //                        window.previousWindow = null;

        //                        if (shouldReplaceBucket)
        //                        {
        //                            // Free the existing GC handle
        //                            if (hashBuckets[bn].window.IsAllocated)
        //                            {
        //                                hashBuckets[bn].window.Free();
        //                            }

        //                            hashBuckets[bn].window = GCHandle.Alloc(prevWindow, GCHandleType.Weak);
        //                        }
        //                        else if (shouldRemoveBucket)
        //                        {

        //                            // Clear hash_coll field, then key, then value
        //                            hashBuckets[bn].hash_coll &= unchecked((int)0x80000000);
        //                            if (hashBuckets[bn].hash_coll != 0)
        //                            {
        //                                hashBuckets[bn].handle = new IntPtr(-1);
        //                            }
        //                            else
        //                            {
        //                                hashBuckets[bn].handle = IntPtr.Zero;
        //                            }

        //                            if (hashBuckets[bn].window.IsAllocated)
        //                            {
        //                                hashBuckets[bn].window.Free();
        //                            }

        //                            Debug.Assert(handleCount > 0, "Underflow on handle count");
        //                            handleCount--;
        //                        }
        //                        return;
        //                    }
        //                    seed += incr;
        //                } while (hashBuckets[bn].hash_coll < 0 && ++ntry < hashBuckets.Length);
        //            }
        //        }


        private static bool IsRootWindowInListWithChildren(NativeWindow window)
        {
            // This seems backwards, but it isn't.  When a new subclass comes in, 
            // it's previousWindow field is set to the previous subclass.  Therefore,
            // the top of the subclass chain has nextWindow == null and previousWindow
            // == the first child subclass.
            return ((window.PreviousWindow != null) && (window.nextWindow == null));
        }


        /* No one is calling this private method, so it is safe to comment it out.
        private static bool IsRootWindowInListWithNoChildren(NativeWindow window)
        {
            return ((window.PreviousWindow == null) && (window.nextWindow == null));     
        }       
        */

        internal static void RemoveWindowFromIDTable(IntPtr handle)
        {
            short id = (short)NativeWindow.hashForHandleId[handle];
            NativeWindow.hashForHandleId.Remove(handle);
            NativeWindow.hashForIdHandle.Remove(id);
        }

        //        internal static void SetUnhandledExceptionModeInternal(UnhandledExceptionMode mode, bool threadScope)
        //        {

        //            if (!threadScope && anyHandleCreatedInApp)
        //            {
        //                throw new InvalidOperationException(SR.GetString(SR.ApplicationCannotChangeApplicationExceptionMode));
        //            }
        //            if (threadScope && anyHandleCreated)
        //            {
        //                throw new InvalidOperationException(SR.GetString(SR.ApplicationCannotChangeThreadExceptionMode));
        //            }

        //            switch (mode)
        //            {
        //                case UnhandledExceptionMode.Automatic:
        //                    if (threadScope)
        //                    {
        //                        userSetProcFlags = 0;
        //                    }
        //                    else
        //                    {
        //                        userSetProcFlagsForApp = 0;
        //                    }
        //                    break;
        //                case UnhandledExceptionMode.ThrowException:
        //                    if (threadScope)
        //                    {
        //                        userSetProcFlags = UseDebuggableWndProc | InitializedFlags;
        //                    }
        //                    else
        //                    {
        //                        userSetProcFlagsForApp = UseDebuggableWndProc | InitializedFlags;
        //                    }
        //                    break;
        //                case UnhandledExceptionMode.CatchException:
        //                    if (threadScope)
        //                    {
        //                        userSetProcFlags = InitializedFlags;
        //                    }
        //                    else
        //                    {
        //                        userSetProcFlagsForApp = InitializedFlags;
        //                    }
        //                    break;
        //                default:
        //                    throw new InvalidEnumArgumentException("mode", (int)mode, typeof(UnhandledExceptionMode));
        //            }
        //        }

        //        private void UnSubclass()
        //        {
        //            bool finalizing = (!weakThisPtr.IsAlive || weakThisPtr.Target == null);
        //            HandleRef href = new HandleRef(this, handle);

        //            // Don't touch if the current window proc is not ours.
        //            //
        //            IntPtr currentWinPrc = UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_WNDPROC);
        //            if (windowProcPtr == currentWinPrc)
        //            {
        //                if (previousWindow == null)
        //                {

        //#if DEBUG
        //                    subclassStatus = "Unsubclassing back to native defWindowProc";
        //#endif

        //                    // If the defWindowProc points to a native window proc, previousWindow will
        //                    // be null.  In this case, it is completely safe to assign defWindowProc
        //                    // to the current wndproc.
        //                    //
        //                    UnsafeNativeMethods.SetWindowLong(href, NativeMethods.GWL_WNDPROC, new HandleRef(this, defWindowProc));
        //                }
        //                else
        //                {
        //                    if (finalizing)
        //                    {

        //#if DEBUG
        //                        subclassStatus = "Setting back to userDefWindowProc -- next chain is managed";
        //#endif

        //                        // Here, we are finalizing and defWindowProc is pointing to a managed object.  We must assume
        //                        // that the object defWindowProc is pointing to is also finalizing.  Why?  Because we're
        //                        // holding a ref to it, and it is holding a ref to us.  The only way this cycle will
        //                        // finalize is if no one else is hanging onto it.  So, we re-assign the window proc to
        //                        // userDefWindowProc.
        //                        UnsafeNativeMethods.SetWindowLong(href, NativeMethods.GWL_WNDPROC, new HandleRef(this, userDefWindowProc));
        //                    }
        //                    else
        //                    {

        //#if DEBUG
        //                        subclassStatus = "Setting back to next managed subclass object";
        //#endif

        //                        // Here we are not finalizing so we use the windowProc for our previous window.  This may
        //                        // DIFFER from the value we are currently storing in defWindowProc because someone may
        //                        // have re-subclassed.
        //                        UnsafeNativeMethods.SetWindowLong(href, NativeMethods.GWL_WNDPROC, previousWindow.windowProc);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // cutting the subclass chain anyway, even if we're not the last one in the chain
        //                // if the whole chain is all managed NativeWindow it doesnt matter, 
        //                // if the chain is not, then someone has been dirty and didn't clean up properly, too bad for them...

        //                //We will cut off the chain if we cannot unsubclass.
        //                //If we find previouswindow pointing to us, then we can let RemoveWindowFromTable reassign the
        //                //defwndproc pointers properly when this guy gets removed (thereby unsubclassing ourselves)

        //                if (nextWindow == null || nextWindow.defWindowProc != windowProcPtr)
        //                {
        //                    // we didn't find it... let's unhook anyway and cut the chain... this prevents crashes
        //                    UnsafeNativeMethods.SetWindowLong(href, NativeMethods.GWL_WNDPROC, new HandleRef(this, userDefWindowProc));
        //                }
        //#if DEBUG
        //                subclassStatus = "FORCE unsubclass -- we do not own the subclass";
        //#endif
        //            }
        //        }


        //        protected virtual void WndProc(ref Message m)
        //        {
        //            DefWndProc(ref m);
        //        }

        private struct HandleBucket
        {
            public IntPtr handle; // Win32 window handle
            public GCHandle window; // a weak GC handle to the NativeWindow class
            public int hash_coll;   // Store hash code; sign bit means there was a collision.
#if DEBUG
            public string owner;    // owner of this handle
#endif
        }

        [
        System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class WindowClass
        {
            internal static WindowClass cache;

            internal WindowClass next;
            internal string className;
            internal int classStyle;
            internal string windowClassName;
            internal int hashCode;
            internal IntPtr defWindowProc;
            internal NativeMethods.WndProc windowProc;
            internal bool registered;
            internal NativeWindow targetWindow;

            private static object wcInternalSyncObject = new object();
            private static int domainQualifier = 0;

            internal WindowClass(string className, int classStyle)
            {
                this.className = className;
                this.classStyle = classStyle;
                RegisterClass();
            }

            //public IntPtr Callback(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
            //{
            //    Debug.Assert(hWnd != IntPtr.Zero, "Windows called us with an HWND of 0");
            //    UnsafeNativeMethods.SetWindowLong(new HandleRef(null, hWnd), NativeMethods.GWL_WNDPROC, new HandleRef(this, defWindowProc));
            //    targetWindow.AssignHandle(hWnd);
            //    return targetWindow.Callback(hWnd, msg, wparam, lparam);
            //}

            internal static WindowClass Create(string className, int classStyle)
            {
                lock (wcInternalSyncObject)
                {
                    WindowClass wc = cache;
                    if (className == null)
                    {
                        while (wc != null && (wc.className != null ||
                                              wc.classStyle != classStyle)) wc = wc.next;
                    }
                    else
                    {
                        while (wc != null &&
                               !className.Equals(wc.className))
                        {
                            wc = wc.next;
                        }
                    }
                    if (wc == null)
                    {
                        wc = new WindowClass(className, classStyle);
                        wc.next = cache;
                        cache = wc;
                    }
                    else
                    {
                        if (!wc.registered)
                        {
                            wc.RegisterClass();
                        }
                    }
                    return wc;
                }
            }

            internal static void DisposeCache()
            {
                lock (wcInternalSyncObject)
                {
                    WindowClass wc = cache;

                    while (wc != null)
                    {
                        wc.UnregisterClass();
                        wc = wc.next;
                    }
                }
            }

            private string GetFullClassName(string className)
            {
                StringBuilder b = new StringBuilder(50);
                b.Append(Application.WindowsFormsVersion);
                b.Append('.');
                b.Append(className);
                b.Append(".app.");
                b.Append(domainQualifier);
                b.Append('.');
                String appDomain = Convert.ToString(AppDomain.CurrentDomain.GetHashCode(), 16);
                b.Append(VersioningHelper.MakeVersionSafeName(appDomain, ResourceScope.Process, ResourceScope.AppDomain));
                return b.ToString();
            }

            private void RegisterClass()
            {
                NativeMethods.WNDCLASS_D wndclass = new NativeMethods.WNDCLASS_D();

                //if (userDefWindowProc == IntPtr.Zero)
                //{
                //    string defproc = (Marshal.SystemDefaultCharSize == 1 ? "DefWindowProcA" : "DefWindowProcW");

                //    userDefWindowProc = UnsafeNativeMethods.GetProcAddress(new HandleRef(null, UnsafeNativeMethods.GetModuleHandle("user32.dll")), defproc);
                //    if (userDefWindowProc == IntPtr.Zero)
                //    {
                //        throw new Win32Exception();
                //    }
                //}

                //string localClassName = className;

                //if (localClassName == null)
                //{

                //    // If we don't use a hollow brush here, Windows will "pre paint" us with COLOR_WINDOW which
                //    // creates a little bit if flicker.  This happens even though we are overriding wm_erasebackgnd.
                //    // Make this hollow to avoid all flicker.
                //    //
                //    wndclass.hbrBackground = UnsafeNativeMethods.GetStockObject(NativeMethods.HOLLOW_BRUSH); //(IntPtr)(NativeMethods.COLOR_WINDOW + 1);
                //    wndclass.style = classStyle;

                //    defWindowProc = userDefWindowProc;
                //    localClassName = "Window." + Convert.ToString(classStyle, 16);
                //    hashCode = 0;
                //}
                //else
                //{
                //    NativeMethods.WNDCLASS_I wcls = new NativeMethods.WNDCLASS_I();
                //    bool ok = UnsafeNativeMethods.GetClassInfo(NativeMethods.NullHandleRef, className, wcls);
                //    int error = Marshal.GetLastWin32Error();
                //    if (!ok)
                //    {
                //        throw new Win32Exception(error, SR.GetString(SR.InvalidWndClsName));
                //    }
                //    wndclass.style = wcls.style;
                //    wndclass.cbClsExtra = wcls.cbClsExtra;
                //    wndclass.cbWndExtra = wcls.cbWndExtra;
                //    wndclass.hIcon = wcls.hIcon;
                //    wndclass.hCursor = wcls.hCursor;
                //    wndclass.hbrBackground = wcls.hbrBackground;
                //    wndclass.lpszMenuName = Marshal.PtrToStringAuto(wcls.lpszMenuName);
                //    localClassName = className;
                //    defWindowProc = wcls.lpfnWndProc;
                //    hashCode = className.GetHashCode();
                //}

                //windowClassName = GetFullClassName(localClassName);
                //windowProc = new NativeMethods.WndProc(this.Callback);
                //wndclass.lpfnWndProc = windowProc;
                //wndclass.hInstance = UnsafeNativeMethods.GetModuleHandle(null);
                //wndclass.lpszClassName = windowClassName;

                //short atom = UnsafeNativeMethods.RegisterClass(wndclass);
                //if (atom == 0)
                //{

                //    int err = Marshal.GetLastWin32Error();
                //    if (err == NativeMethods.ERROR_CLASS_ALREADY_EXISTS)
                //    {
                //        // Check to see if the window class window
                //        // proc points to DefWndProc.  If it does, then
                //        // this is a class from a rudely-terminated app domain
                //        // and we can safely reuse it.  If not, we've got
                //        // to throw.
                //        NativeMethods.WNDCLASS_I wcls = new NativeMethods.WNDCLASS_I();
                //        bool ok = UnsafeNativeMethods.GetClassInfo(new HandleRef(null, UnsafeNativeMethods.GetModuleHandle(null)), windowClassName, wcls);
                //        if (ok && wcls.lpfnWndProc == NativeWindow.UserDefindowProc)
                //        {

                //            // We can just reuse this class because we have marked it
                //            // as being a nop in another domain.  All we need to do is call SetClassLong.
                //            // Only one problem:  SetClassLong takes an HWND, which we don't have.  That leaves
                //            // us with some tricky business. First, try this the easy way and see
                //            // if we can simply unregister and re-register the class.  This might
                //            // work because the other domain shutdown would have posted WM_CLOSE to all
                //            // the windows of the class.
                //            if (UnsafeNativeMethods.UnregisterClass(windowClassName, new HandleRef(null, UnsafeNativeMethods.GetModuleHandle(null))))
                //            {
                //                atom = UnsafeNativeMethods.RegisterClass(wndclass);
                //                // If this fails, we will always raise the first err above.  No sense exposing our twiddling.
                //            }
                //            else
                //            {
                //                // This is a little harder.  We cannot reuse the class because it is
                //                // already in use.  We must create a new class.  We bump our domain qualifier
                //                // here to account for this, so we only do this expensive search once for the
                //                // domain.  
                //                do
                //                {
                //                    domainQualifier++;
                //                    windowClassName = GetFullClassName(localClassName);
                //                    wndclass.lpszClassName = windowClassName;
                //                    atom = UnsafeNativeMethods.RegisterClass(wndclass);
                //                } while (atom == 0 && Marshal.GetLastWin32Error() == NativeMethods.ERROR_CLASS_ALREADY_EXISTS);
                //            }
                //        }
                //    }

                //    if (atom == 0)
                //    {
                //        windowProc = null;
                //        throw new Win32Exception(err);
                //    }
                //}
                //registered = true;
            }

            private void UnregisterClass()
            {
                //if (registered && UnsafeNativeMethods.UnregisterClass(windowClassName, new HandleRef(null, UnsafeNativeMethods.GetModuleHandle(null))))
                //{
                //    windowProc = null;
                //    registered = false;
                //}
            }
        }
    }

    public enum UnhandledExceptionMode
    {
        Automatic,
        ThrowException,
        CatchException
    }
}