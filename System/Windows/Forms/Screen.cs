namespace System.Windows.Forms
{
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections;
    using Microsoft.Win32;
    using Internal;

    public class Screen
    {

        readonly IntPtr hmonitor;
        readonly Rectangle bounds;
        private Rectangle workingArea = Rectangle.Empty;
        readonly bool primary;
        readonly string deviceName;

        readonly int bitDepth;

        private static object syncLock = new object();//used to lock this class before sync'ing to SystemEvents

        private static int desktopChangedCount = -1;//static counter of desktop size changes

        private int currentDesktopChangedCount = -1;//instance-based counter used to invalidate WorkingArea

        // This identifier is just for us, so that we don't try to call the multimon
        // functions if we just need the primary monitor... this is safer for
        // non-multimon OSes.
        //
        private const int PRIMARY_MONITOR = unchecked((int)0xBAADF00D);

        private const int MONITOR_DEFAULTTONULL = 0x00000000;
        private const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const int MONITORINFOF_PRIMARY = 0x00000001;

        private static bool multiMonitorSupport = (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CMONITORS) != 0);
        private static Screen[] screens;

        internal Screen(IntPtr monitor) : this(monitor, IntPtr.Zero)
        {
        }

        internal Screen(IntPtr monitor, IntPtr hdc)
        {

            IntPtr screenDC = hdc;

            if (!multiMonitorSupport || monitor == (IntPtr)PRIMARY_MONITOR)
            {
                // Single monitor system
                //
                bounds = SystemInformation.VirtualScreen;
                primary = true;
                deviceName = "DISPLAY";
            }
            //else
            //{
            //    // MultiMonitor System
            //    // We call the 'A' version of GetMonitorInfoA() because
            //    // the 'W' version just never fills out the struct properly on Win2K.
            //    //
            //    NativeMethods.MONITORINFOEX info = new NativeMethods.MONITORINFOEX();
            //    SafeNativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);
            //    bounds = Rectangle.FromLTRB(info.rcMonitor.left, info.rcMonitor.top, info.rcMonitor.right, info.rcMonitor.bottom);
            //    primary = ((info.dwFlags & MONITORINFOF_PRIMARY) != 0);

            //    deviceName = new string(info.szDevice);
            //    deviceName = deviceName.TrimEnd((char)0);

            //    if (hdc == IntPtr.Zero)
            //    {
            //        screenDC = UnsafeNativeMethods.CreateDC(deviceName);
            //    }
            //}
            hmonitor = monitor;

            //this.bitDepth = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, screenDC), NativeMethods.BITSPIXEL);
            //this.bitDepth *= UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, screenDC), NativeMethods.PLANES);

            bitDepth = 32;

            //if (hdc != screenDC)
            //{
            //    UnsafeNativeMethods.DeleteDC(new HandleRef(null, screenDC));
            //}
        }

        public static Screen[] AllScreens
        {
            get
            {
                if (screens == null)
                {
                    //if (multiMonitorSupport)
                    //{
                    //    MonitorEnumCallback closure = new MonitorEnumCallback();
                    //    NativeMethods.MonitorEnumProc proc = new NativeMethods.MonitorEnumProc(closure.Callback);
                    //    SafeNativeMethods.EnumDisplayMonitors(NativeMethods.NullHandleRef, null, proc, IntPtr.Zero);

                    //    if (closure.screens.Count > 0)
                    //    {
                    //        Screen[] temp = new Screen[closure.screens.Count];
                    //        closure.screens.CopyTo(temp, 0);
                    //        screens = temp;
                    //    }
                    //    else
                    //    {
                    //        screens = new Screen[] { new Screen((IntPtr)PRIMARY_MONITOR) };
                    //    }
                    //}
                    //else
                    {
                        screens = new Screen[] { PrimaryScreen };
                    }

                    // Now that we have our screens, attach a display setting changed
                    // event so that we know when to invalidate them.
                    //
                    //SystemEvents.DisplaySettingsChanging += new EventHandler(OnDisplaySettingsChanging);
                }

                return screens;
            }
        }

        public int BitsPerPixel
        {
            get
            {
                return bitDepth;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
        }

        public string DeviceName
        {
            get
            {
                return deviceName;
            }
        }

        public bool Primary
        {
            get
            {
                return primary;
            }
        }

        public static Screen PrimaryScreen
        {
            get
            {
                if (multiMonitorSupport)
                {
                    Screen[] screens = AllScreens;
                    for (int i = 0; i < screens.Length; i++)
                    {
                        if (screens[i].primary)
                        {
                            return screens[i];
                        }
                    }
                    return null;
                }
                else
                {
                    return new Screen((IntPtr)PRIMARY_MONITOR, IntPtr.Zero);
                }
            }
        }

        public Rectangle WorkingArea
        {
            get
            {

                //if the static Screen class has a different desktop change count 
                //than this instance then update the count and recalculate our working area
                if (currentDesktopChangedCount != Screen.DesktopChangedCount)
                {

                    Interlocked.Exchange(ref currentDesktopChangedCount, Screen.DesktopChangedCount);

                    if (!multiMonitorSupport || hmonitor == (IntPtr)PRIMARY_MONITOR)
                    {
                        // Single monitor system
                        //
                        workingArea = SystemInformation.WorkingArea;
                    }
                    else
                    {
                        // MultiMonitor System
                        // We call the 'A' version of GetMonitorInfoA() because
                        // the 'W' version just never fills out the struct properly on Win2K.
                        //
                        NativeMethods.MONITORINFOEX info = new NativeMethods.MONITORINFOEX();
                        //SafeNativeMethods.GetMonitorInfo(new HandleRef(null, hmonitor), info);
                        //workingArea = Rectangle.FromLTRB(info.rcWork.left, info.rcWork.top, info.rcWork.right, info.rcWork.bottom);
                    }
                }

                return workingArea;
            }
        }

        private static int DesktopChangedCount
        {
            get
            {
                if (desktopChangedCount == -1)
                {

                    lock (syncLock)
                    {

                        //now that we have a lock, verify (again) our changecount...
                        if (desktopChangedCount == -1)
                        {
                            //sync the UserPreference.Desktop change event.  We'll keep count 
                            //of desktop changes so that the WorkingArea property on Screen 
                            //instances know when to invalidate their cache.
                            //SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(OnUserPreferenceChanged);

                            desktopChangedCount = 0;
                        }
                    }
                }
                return desktopChangedCount;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Screen)
            {
                Screen comp = (Screen)obj;
                if (hmonitor == comp.hmonitor)
                {
                    return true;
                }
            }
            return false;
        }

        public static Screen FromPoint(Point point)
        {
            //if (multiMonitorSupport)
            //{
            //    NativeMethods.POINTSTRUCT pt = new NativeMethods.POINTSTRUCT(point.X, point.Y);
            //    return new Screen(SafeNativeMethods.MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST));
            //}
            //else
            {
                return new Screen((IntPtr)PRIMARY_MONITOR);
            }
        }

        public static Screen FromRectangle(Rectangle rect)
        {
            //if (multiMonitorSupport)
            //{
            //    NativeMethods.RECT rc = NativeMethods.RECT.FromXYWH(rect.X, rect.Y, rect.Width, rect.Height);
            //    return new Screen(SafeNativeMethods.MonitorFromRect(ref rc, MONITOR_DEFAULTTONEAREST));
            //}
            //else
            {
                return new Screen((IntPtr)PRIMARY_MONITOR, IntPtr.Zero);
            }
        }

        //public static Screen FromControl(Control control)
        //{
        //    return FromHandleInternal(control.Handle);
        //}

        //public static Screen FromHandle(IntPtr hwnd)
        //{
        //    Debug.WriteLineIf(IntSecurity.SecurityDemand.TraceVerbose, "ObjectFromWin32Handle Demanded");
        //    IntSecurity.ObjectFromWin32Handle.Demand();
        //    return FromHandleInternal(hwnd);
        //}

        //internal static Screen FromHandleInternal(IntPtr hwnd)
        //{
        //    if (multiMonitorSupport)
        //    {
        //        return new Screen(SafeNativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), MONITOR_DEFAULTTONEAREST));
        //    }
        //    else
        //    {
        //        return new Screen((IntPtr)PRIMARY_MONITOR, IntPtr.Zero);
        //    }
        //}

        public static Rectangle GetWorkingArea(Point pt)
        {
            return Screen.FromPoint(pt).WorkingArea;
        }
        public static Rectangle GetWorkingArea(Rectangle rect)
        {
            return Screen.FromRectangle(rect).WorkingArea;
        }
        //public static Rectangle GetWorkingArea(Control ctl)
        //{
        //    return Screen.FromControl(ctl).WorkingArea;
        //}

        public static Rectangle GetBounds(Point pt)
        {
            return Screen.FromPoint(pt).Bounds;
        }
        public static Rectangle GetBounds(Rectangle rect)
        {
            return Screen.FromRectangle(rect).Bounds;
        }
        //public static Rectangle GetBounds(Control ctl)
        //{
        //    return Screen.FromControl(ctl).Bounds;
        //}

        public override int GetHashCode()
        {
            return (int)hmonitor;
        }

        private static void OnDisplaySettingsChanging(object sender, EventArgs e)
        {

            // Now that we've responded to this event, we don't need it again until
            // someone re-queries. We will re-add the event at that time.
            //
            //SystemEvents.DisplaySettingsChanging -= new EventHandler(OnDisplaySettingsChanging);

            // Display settings changed, so the set of screens we have is invalid.
            //
            screens = null;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {

            if (e.Category == UserPreferenceCategory.Desktop)
            {
                Interlocked.Increment(ref desktopChangedCount);
            }
        }

        public override string ToString()
        {
            return GetType().Name + "[Bounds=" + bounds.ToString() + " WorkingArea=" + WorkingArea.ToString() + " Primary=" + primary.ToString() + " DeviceName=" + deviceName;
        }


        private class MonitorEnumCallback
        {
            public ArrayList screens = new ArrayList();

            public virtual bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
            {
                screens.Add(new Screen(monitor, hdc));
                return true;
            }
        }
    }
}