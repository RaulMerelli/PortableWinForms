﻿#if DEBUG_HANDLECOLLECTOR
  using System.Diagnostics;
#endif
using System.Collections.Generic;
using System.Diagnostics;

using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Internal")]
namespace System.Internal
{

    internal sealed class HandleCollector
    {
        private static HandleType[] handleTypes;
        private static int handleTypeCount;
        private static int suspendCount;

        internal static event HandleChangeEventHandler HandleAdded;

        internal static event HandleChangeEventHandler HandleRemoved;

        private static object internalSyncObject = new object();

        internal static IntPtr Add(IntPtr handle, int type)
        {
            handleTypes[type - 1].Add(handle);
            return handle;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void SuspendCollect()
        {
            lock (internalSyncObject)
            {
                suspendCount++;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static void ResumeCollect()
        {
            bool performCollect = false;
            lock (internalSyncObject)
            {
                if (suspendCount > 0)
                {
                    suspendCount--;
                }

                if (suspendCount == 0)
                {
                    for (int i = 0; i < handleTypeCount; i++)
                    {
                        lock (handleTypes[i])
                        {
                            if (handleTypes[i].NeedCollection())
                            {
                                performCollect = true;
                            }
                        }
                    }
                }
            }

            if (performCollect)
            {
                GC.Collect();
            }
        }

        internal static int RegisterType(string typeName, int expense, int initialThreshold)
        {
            lock (internalSyncObject)
            {
                if (handleTypeCount == 0 || handleTypeCount == handleTypes.Length)
                {
                    HandleType[] newTypes = new HandleType[handleTypeCount + 10];
                    if (handleTypes != null)
                    {
                        Array.Copy(handleTypes, 0, newTypes, 0, handleTypeCount);
                    }
                    handleTypes = newTypes;
                }

                handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
                return handleTypeCount;
            }
        }

        internal static IntPtr Remove(IntPtr handle, int type)
        {
            return handleTypes[type - 1].Remove(handle);
        }

        private class HandleType
        {
            internal readonly string name;

            private int initialThreshHold;
            private int threshHold;
            private int handleCount;
            private readonly int deltaPercent;

#if DEBUG_HANDLECOLLECTOR
            private List<IntPtr> handles = new List<IntPtr>();
#endif

            internal HandleType(string name, int expense, int initialThreshHold)
            {
                this.name = name;
                this.initialThreshHold = initialThreshHold;
                this.threshHold = initialThreshHold;
                this.deltaPercent = 100 - expense;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
            internal void Add(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    return;
                }

                bool performCollect = false;
                int currentCount = 0;

                lock (this)
                {
                    handleCount++;
#if DEBUG_HANDLECOLLECTOR
                    Debug.Assert(!handles.Contains(handle));
                    handles.Add(handle);
#endif
                    performCollect = NeedCollection();
                    currentCount = handleCount;
                }
                lock (internalSyncObject)
                {
                    if (HandleCollector.HandleAdded != null)
                    {
                        HandleCollector.HandleAdded(name, handle, currentCount);
                    }
                }

                if (!performCollect)
                {
                    return;
                }


                if (performCollect)
                {
#if DEBUG_HANDLECOLLECTOR
                    Debug.WriteLine("HC> Forcing garbage collect");
                    Debug.WriteLine("HC>     name        :" + name);
                    Debug.WriteLine("HC>     threshHold  :" + (threshHold).ToString());
                    Debug.WriteLine("HC>     handleCount :" + (handleCount).ToString());
                    Debug.WriteLine("HC>     deltaPercent:" + (deltaPercent).ToString());
#endif                  
                    GC.Collect();

                    // We just performed a GC.  If the main thread is in a tight
                    // loop there is a this will cause us to increase handles forever and prevent handle collector
                    // from doing its job.  Yield the thread here.  This won't totally cause
                    // a finalization pass but it will effectively elevate the priority
                    // of the finalizer thread just for an instant.  But how long should
                    // we sleep?  We base it on how expensive the handles are because the
                    // more expensive the handle, the more critical that it be reclaimed.
                    int sleep = (100 - deltaPercent) / 4;
                    System.Threading.Thread.Sleep(sleep);
                }
            }


            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal int GetHandleCount()
            {
                lock (this)
                {
                    return handleCount;
                }
            }

            internal bool NeedCollection()
            {

                if (suspendCount > 0)
                {
                    return false;
                }
                if (handleCount > threshHold)
                {
                    threshHold = handleCount + ((handleCount * deltaPercent) / 100);
#if DEBUG_HANDLECOLLECTOR
                    Debug.WriteLine("HC> NeedCollection: increase threshHold to " + threshHold);
#endif                  
                    return true;
                }

                // If handle count < threshHold, we don't
                // need to collect, but if it 10% below the next lowest threshhold we
                // will bump down a rung.  We need to choose a percentage here or else
                // we will oscillate.
                //
                int oldThreshHold = (100 * threshHold) / (100 + deltaPercent);
                if (oldThreshHold >= initialThreshHold && handleCount < (int)(oldThreshHold * .9F))
                {
#if DEBUG_HANDLECOLLECTOR
                    Debug.WriteLine("HC> NeedCollection: throttle threshhold " + threshHold + " down to " + oldThreshHold);
#endif                  
                    threshHold = oldThreshHold;
                }

                return false;
            }

            internal IntPtr Remove(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    return handle;
                }
                int currentCount = 0;
                lock (this)
                {
                    handleCount--;
#if DEBUG_HANDLECOLLECTOR
                    Debug.Assert(handles.Contains(handle));
                    handles.Remove(handle);
#endif
                    if (handleCount < 0)
                    {
                        System.Diagnostics.Debug.Fail("Handle collector underflow for type '" + name + "'");
                        handleCount = 0;
                    }
                    currentCount = handleCount;
                }
                lock (internalSyncObject)
                {
                    if (HandleCollector.HandleRemoved != null)
                    {
                        HandleCollector.HandleRemoved(name, handle, currentCount);
                    }
                }
                return handle;

            }
        }
    }

    internal delegate void HandleChangeEventHandler(string handleType, IntPtr handleValue, int currentHandleCount);
}