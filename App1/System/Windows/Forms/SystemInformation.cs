namespace System.Windows.Forms
{
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using Microsoft.Win32;
    using System.Drawing;
    using System.ComponentModel;
    using System.Runtime.Versioning;

    public class SystemInformation
    {

        // private constructor to prevent creation
        //
        private SystemInformation()
        {
        }

        // Figure out if all the multimon stuff is supported on the OS
        //
        private static bool checkMultiMonitorSupport = false;
        private static bool multiMonitorSupport = false;
        private static bool checkNativeMouseWheelSupport = false;
        private static bool nativeMouseWheelSupport = true;
        private static bool highContrast = false;
        private static bool systemEventsAttached = false;
        private static bool systemEventsDirty = true;

        private static IntPtr processWinStation = IntPtr.Zero;
        private static bool isUserInteractive = false;

        private static PowerStatus powerStatus = null;

        private const int DefaultMouseWheelScrollLines = 3;

        ////////////////////////////////////////////////////////////////////////////
        // SystemParametersInfo
        //

        public static bool DragFullWindows
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETDRAGFULLWINDOWS, 0, ref data, 0);
                return data != 0;
            }
        }

        public static bool HighContrast
        {
            get
            {
                EnsureSystemEvents();
                if (systemEventsDirty)
                {
                    NativeMethods.HIGHCONTRAST_I data = new NativeMethods.HIGHCONTRAST_I();
                    data.cbSize = Marshal.SizeOf(data);
                    data.dwFlags = 0;
                    data.lpszDefaultScheme = IntPtr.Zero;

                    bool b = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETHIGHCONTRAST, data.cbSize, ref data, 0);

                    // NT4 does not support this parameter, so we always force
                    // it to false if we fail to get the parameter.
                    //
                    if (b)
                    {
                        highContrast = (data.dwFlags & NativeMethods.HCF_HIGHCONTRASTON) != 0;
                    }
                    else
                    {
                        highContrast = false;
                    }
                    systemEventsDirty = false;
                }

                return highContrast;
            }
        }

        public static int MouseWheelScrollLines
        {
            get
            {
                if (NativeMouseWheelSupport)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETWHEELSCROLLLINES, 0, ref data, 0);
                    return data;
                }
                else
                {
                    IntPtr hWndMouseWheel = IntPtr.Zero;

                    // Check for the MouseZ "service". This is a little app that generated the
                    // MSH_MOUSEWHEEL messages by monitoring the hardware. If this app isn't
                    // found, then there is no support for MouseWheels on the system.
                    //
                    //hWndMouseWheel = UnsafeNativeMethods.FindWindow(NativeMethods.MOUSEZ_CLASSNAME, NativeMethods.MOUSEZ_TITLE);

                    //if (hWndMouseWheel != IntPtr.Zero)
                    //{

                    //    // Register the MSH_SCROLL_LINES message...
                    //    //
                    //    int message = SafeNativeMethods.RegisterWindowMessage(NativeMethods.MSH_SCROLL_LINES);


                    //    int lines = (int)UnsafeNativeMethods.SendMessage(new HandleRef(null, hWndMouseWheel), message, 0, 0);

                    //    // this fails under terminal server, so we default to 3, which is the windows
                    //    // default.  Nobody seems to pay attention to this value anyways...
                    //    if (lines != 0)
                    //    {
                    //        return lines;
                    //    }
                    //}
                }

                return DefaultMouseWheelScrollLines;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // SystemMetrics
        //

        public static Size PrimaryMonitorSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN));
            }
        }

        public static int VerticalScrollBarWidth
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXVSCROLL);
            }
        }

        public static int GetVerticalScrollBarWidthForDpi(int dpi)
        {
            //if (DpiHelper.EnableDpiChangedMessageHandling)
            //{
            //    return UnsafeNativeMethods.TryGetSystemMetricsForDpi(NativeMethods.SM_CXVSCROLL, (uint)dpi);
            //}
            //else
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXVSCROLL);
            }
        }

        public static int HorizontalScrollBarHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYHSCROLL);
            }
        }

        public static int GetHorizontalScrollBarHeightForDpi(int dpi)
        {
            //if (DpiHelper.EnableDpiChangedMessageHandling)
            //{
            //    return UnsafeNativeMethods.TryGetSystemMetricsForDpi(NativeMethods.SM_CYHSCROLL, (uint)dpi);
            //}
            //else
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYHSCROLL);
            }
        }

        public static int CaptionHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYCAPTION);
            }
        }

        public static Size BorderSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXBORDER),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYBORDER));
            }
        }

        public static Size GetBorderSizeForDpi(int dpi)
        {
            //if (DpiHelper.EnableDpiChangedMessageHandling)
            //{
            //    return new Size(UnsafeNativeMethods.TryGetSystemMetricsForDpi(NativeMethods.SM_CXBORDER, (uint)dpi),
            //                    UnsafeNativeMethods.TryGetSystemMetricsForDpi(NativeMethods.SM_CYBORDER, (uint)dpi));
            //}
            //else
            {
                return BorderSize;
            }
        }

        public static Size FixedFrameBorderSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXFIXEDFRAME),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYFIXEDFRAME));
            }
        }

        public static int VerticalScrollBarThumbHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYVTHUMB);
            }
        }

        public static int HorizontalScrollBarThumbWidth
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXHTHUMB);
            }
        }
       
        public static Size IconSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXICON),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYICON));
            }
        }

        public static Size CursorSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXCURSOR),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYCURSOR));
            }
        }

        public static Font MenuFont
        {
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get
            {
                return GetMenuFontHelper(0, false);
            }
        }

        //public static Font GetMenuFontForDpi(int dpi)
        //{
        //    return GetMenuFontHelper((uint)dpi, DpiHelper.EnableDpiChangedMessageHandling);
        //}

        private static Font GetMenuFontHelper(uint dpi, bool useDpi)
        {
            Font menuFont = null;

            //we can get the system's menu font through the NONCLIENTMETRICS structure via SystemParametersInfo
            //
            NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
            bool result;
            if (useDpi)
            {
                //result = UnsafeNativeMethods.TrySystemParametersInfoForDpi(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0, dpi);
                result = false;
            }
            else
            {
                result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);
            }

            if (result && data.lfMenuFont != null)
            {
                // SECREVIEW : This assert is safe since we created the NONCLIENTMETRICS struct.
                //
                IntSecurity.ObjectFromWin32Handle.Assert();
                try
                {
                    //menuFont = Font.FromLogFont(data.lfMenuFont);
                }
                catch
                {
                    // menu font is not true type.  Default to standard control font.
                    //
                    //menuFont = Control.DefaultFont;
                }
                finally
                {
                    //CodeAccessPermission.RevertAssert();
                }
            }
            return menuFont;
        }

        public static int MenuHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMENU);
            }
        }


        public static PowerStatus PowerStatus
        {
            get
            {
                if (powerStatus == null)
                {
                    powerStatus = new PowerStatus();
                }
                return powerStatus;
            }
        }


        public static Rectangle WorkingArea
        {
            get
            {
                NativeMethods.RECT rc = new NativeMethods.RECT();
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETWORKAREA, 0, ref rc, 0);
                return Rectangle.FromLTRB(rc.left, rc.top, rc.right, rc.bottom);
            }
        }

        public static int KanjiWindowHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYKANJIWINDOW);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool MousePresent
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_MOUSEPRESENT) != 0;
            }
        }

        public static int VerticalScrollBarArrowHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYVSCROLL);
            }
        }

        public static int VerticalScrollBarArrowHeightForDpi(int dpi)
        {
            //return UnsafeNativeMethods.TryGetSystemMetricsForDpi(NativeMethods.SM_CXHSCROLL, (uint)dpi);
            return 0;
        }
        public static int HorizontalScrollBarArrowWidth
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXHSCROLL);
            }
        }

        public static int GetHorizontalScrollBarArrowWidthForDpi(int dpi)
        {
            //if (DpiHelper.EnableDpiChangedMessageHandling)
            //{
            //    return UnsafeNativeMethods.TryGetSystemMetricsForDpi(NativeMethods.SM_CXHSCROLL, (uint)dpi);
            //}
            //else
            //{
            //    return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXHSCROLL);
            //}
            return 0;
        }

        public static bool DebugOS
        {
            get
            {
                IntSecurity.SensitiveSystemInformation.Demand();
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_DEBUG) != 0;
            }
        }

        public static bool MouseButtonsSwapped
        {
            get
            {
                return (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_SWAPBUTTON) != 0);
            }
        }

        public static Size MinimumWindowSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMIN),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMIN));
            }
        }

        public static Size CaptionButtonSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXSIZE),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSIZE));
            }
        }

        public static Size FrameBorderSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXFRAME),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYFRAME));
            }
        }

        public static Size MinWindowTrackSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMINTRACK),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMINTRACK));
            }
        }

        public static Size DoubleClickSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXDOUBLECLK),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYDOUBLECLK));
            }
        }

        public static int DoubleClickTime
        {
            get
            {
                //return SafeNativeMethods.GetDoubleClickTime();
                return 0;
            }
        }

        public static Size IconSpacingSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXICONSPACING),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYICONSPACING));
            }
        }

        public static bool RightAlignedMenus
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_MENUDROPALIGNMENT) != 0;
            }
        }

        public static bool PenWindows
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_PENWINDOWS) != 0;
            }
        }

        public static bool DbcsEnabled
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_DBCSENABLED) != 0;
            }
        }

        public static int MouseButtons
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CMOUSEBUTTONS);
            }
        }

        public static bool Secure
        {
            get
            {
                IntSecurity.SensitiveSystemInformation.Demand();
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_SECURE) != 0;
            }
        }

        public static Size Border3DSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXEDGE),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYEDGE));
            }
        }

        public static Size MinimizedWindowSpacingSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMINSPACING),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMINSPACING));
            }
        }

        public static Size SmallIconSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXSMICON),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSMICON));
            }
        }

        public static int ToolWindowCaptionHeight
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSMCAPTION);
            }
        }

        public static Size ToolWindowCaptionButtonSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXSMSIZE),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSMSIZE));
            }
        }

        public static Size MenuButtonSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMENUSIZE),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMENUSIZE));
            }
        }

        public static ArrangeStartingPosition ArrangeStartingPosition
        {
            get
            {
                ArrangeStartingPosition mask = ArrangeStartingPosition.BottomLeft | ArrangeStartingPosition.BottomRight | ArrangeStartingPosition.Hide | ArrangeStartingPosition.TopLeft | ArrangeStartingPosition.TopRight;
                int compoundValue = UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_ARRANGE);
                return mask & (ArrangeStartingPosition)compoundValue;
            }
        }

        public static ArrangeDirection ArrangeDirection
        {
            get
            {
                ArrangeDirection mask = ArrangeDirection.Down | ArrangeDirection.Left | ArrangeDirection.Right | ArrangeDirection.Up;
                int compoundValue = UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_ARRANGE);
                return mask & (ArrangeDirection)compoundValue;
            }
        }

        public static Size MinimizedWindowSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMINIMIZED),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMINIMIZED));
            }
        }

        public static Size MaxWindowTrackSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMAXTRACK),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMAXTRACK));
            }
        }

        public static Size PrimaryMonitorMaximizedWindowSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMAXIMIZED),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMAXIMIZED));
            }
        }

        public static bool Network
        {
            get
            {
                return (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_NETWORK) & 0x00000001) != 0;
            }
        }

        public static bool TerminalServerSession
        {
            get
            {
                return (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_REMOTESESSION) & 0x00000001) != 0;
            }
        }



        public static BootMode BootMode
        {
            get
            {
                IntSecurity.SensitiveSystemInformation.Demand();
                return (BootMode)UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CLEANBOOT);
            }
        }

        public static Size DragSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXDRAG),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYDRAG));
            }
        }

        public static bool ShowSounds
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_SHOWSOUNDS) != 0;
            }
        }

        public static Size MenuCheckSize
        {
            get
            {
                return new Size(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXMENUCHECK),
                                UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYMENUCHECK));
            }
        }

        public static bool MidEastEnabled
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_MIDEASTENABLED) != 0;
            }
        }

        private static bool MultiMonitorSupport
        {
            get
            {
                if (!checkMultiMonitorSupport)
                {
                    multiMonitorSupport = (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CMONITORS) != 0);
                    checkMultiMonitorSupport = true;
                }
                return multiMonitorSupport;
            }
        }

        public static bool NativeMouseWheelSupport
        {
            get
            {
                if (!checkNativeMouseWheelSupport)
                {
                    nativeMouseWheelSupport = (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_MOUSEWHEELPRESENT) != 0);
                    checkNativeMouseWheelSupport = true;
                }
                return nativeMouseWheelSupport;
            }
        }


        public static bool MouseWheelPresent
        {
            get
            {

                bool mouseWheelPresent = false;

                if (!NativeMouseWheelSupport)
                {
                    IntPtr hWndMouseWheel = IntPtr.Zero;

                    // Check for the MouseZ "service". This is a little app that generated the
                    // MSH_MOUSEWHEEL messages by monitoring the hardware. If this app isn't
                    // found, then there is no support for MouseWheels on the system.
                    //
                    //hWndMouseWheel = UnsafeNativeMethods.FindWindow(NativeMethods.MOUSEZ_CLASSNAME, NativeMethods.MOUSEZ_TITLE);

                    if (hWndMouseWheel != IntPtr.Zero)
                    {
                        mouseWheelPresent = true;
                    }
                }
                else
                {
                    mouseWheelPresent = (UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_MOUSEWHEELPRESENT) != 0);
                }
                return mouseWheelPresent;
            }
        }


        public static Rectangle VirtualScreen
        {
            get
            {
                if (MultiMonitorSupport)
                {
                    return new Rectangle(UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN),
                                         UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN),
                                         UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN),
                                         UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN));
                }
                else
                {
                    Size size = PrimaryMonitorSize;
                    return new Rectangle(0, 0, size.Width, size.Height);
                }
            }
        }


        public static int MonitorCount
        {
            get
            {
                if (MultiMonitorSupport)
                {
                    return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CMONITORS);
                }
                else
                {
                    return 1;
                }
            }
        }


        public static bool MonitorsSameDisplayFormat
        {
            get
            {
                if (MultiMonitorSupport)
                {
                    return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_SAMEDISPLAYFORMAT) != 0;
                }
                else
                {
                    return true;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Misc
        //


        public static string ComputerName
        {
            get
            {
                IntSecurity.SensitiveSystemInformation.Demand();

                StringBuilder sb = new StringBuilder(256);
                //UnsafeNativeMethods.GetComputerName(sb, new int[] { sb.Capacity });
                return sb.ToString();
            }
        }


        public static string UserDomainName
        {
            get
            {
                return Environment.UserDomainName;
            }
        }

        public static bool UserInteractive
        {
            get
            {
                if (Environment.OSVersion.Platform == System.PlatformID.Win32NT)
                {
                    IntPtr hwinsta = IntPtr.Zero;

                    //hwinsta = UnsafeNativeMethods.GetProcessWindowStation();
                    //if (hwinsta != IntPtr.Zero && processWinStation != hwinsta)
                    //{
                    //    isUserInteractive = true;

                    //    int lengthNeeded = 0;
                    //    NativeMethods.USEROBJECTFLAGS flags = new NativeMethods.USEROBJECTFLAGS();

                    //    if (UnsafeNativeMethods.GetUserObjectInformation(new HandleRef(null, hwinsta), NativeMethods.UOI_FLAGS, flags, Marshal.SizeOf(flags), ref lengthNeeded))
                    //    {
                    //        if ((flags.dwFlags & NativeMethods.WSF_VISIBLE) == 0)
                    //        {
                    //            isUserInteractive = false;
                    //        }
                    //    }
                    //    processWinStation = hwinsta;
                    //}
                }
                else
                {
                    isUserInteractive = true;
                }
                return isUserInteractive;
            }
        }

        public static string UserName
        {
            get
            {
                IntSecurity.SensitiveSystemInformation.Demand();

                StringBuilder sb = new StringBuilder(256);
                //UnsafeNativeMethods.GetUserName(sb, new int[] { sb.Capacity });
                return sb.ToString();
            }
        }

        private static void EnsureSystemEvents()
        {
            if (!systemEventsAttached)
            {
                //SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemInformation.OnUserPreferenceChanged);
                systemEventsAttached = true;
            }
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            systemEventsDirty = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        // NEW ADDITIONS FOR WHIDBEY                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool IsDropShadowEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETDROPSHADOW, 0, ref data, 0);
                    return data != 0;
                }
                return false;
            }
        }

        public static bool IsFlatMenuEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETFLATMENU, 0, ref data, 0);
                    return data != 0;
                }
                return false;
            }
        }

        public static bool IsFontSmoothingEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETFONTSMOOTHING, 0, ref data, 0);
                return data != 0;
            }
        }

        public static int FontSmoothingContrast
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETFONTSMOOTHINGCONTRAST, 0, ref data, 0);
                    return data;
                }
                else
                {
                    throw new NotSupportedException("SystemInformationFeatureNotSupported");
                }
            }
        }

        public static int FontSmoothingType
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETFONTSMOOTHINGTYPE, 0, ref data, 0);
                    return data;
                }
                else
                {
                    throw new NotSupportedException("SystemInformationFeatureNotSupported");
                }
            }
        }

        public static int IconHorizontalSpacing
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_ICONHORIZONTALSPACING, 0, ref data, 0);
                return data;
            }
        }

        public static int IconVerticalSpacing
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_ICONVERTICALSPACING, 0, ref data, 0);
                return data;
            }
        }

        public static bool IsIconTitleWrappingEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETICONTITLEWRAP, 0, ref data, 0);
                return data != 0;
            }
        }

        public static bool MenuAccessKeysUnderlined
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETKEYBOARDCUES, 0, ref data, 0);
                return data != 0;
            }
        }

        public static int KeyboardDelay
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETKEYBOARDDELAY, 0, ref data, 0);
                return data;
            }
        }

        public static bool IsKeyboardPreferred
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETKEYBOARDPREF, 0, ref data, 0);
                return data != 0;
            }
        }

        public static int KeyboardSpeed
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETKEYBOARDSPEED, 0, ref data, 0);
                return data;
            }
        }

        public static Size MouseHoverSize
        {
            get
            {
                int height = 0;
                int width = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMOUSEHOVERHEIGHT, 0, ref height, 0);
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMOUSEHOVERWIDTH, 0, ref width, 0);
                return new Size(width, height);
            }
        }

        public static int MouseHoverTime
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMOUSEHOVERTIME, 0, ref data, 0);
                return data;
            }

        }

        public static int MouseSpeed
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMOUSESPEED, 0, ref data, 0);
                return data;
            }

        }


        public static bool IsSnapToDefaultEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETSNAPTODEFBUTTON, 0, ref data, 0);
                return data != 0;
            }
        }


        public static LeftRightAlignment PopupMenuAlignment
        {
            get
            {
                bool data = false;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMENUDROPALIGNMENT, 0, ref data, 0);
                if (data)
                {
                    return LeftRightAlignment.Left;
                }
                else
                {
                    return LeftRightAlignment.Right;
                }

            }
        }

        public static bool IsMenuFadeEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp || OSFeature.Feature.OnWin2k)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMENUFADE, 0, ref data, 0);
                    return data != 0;
                }
                return false;
            }
        }

        public static int MenuShowDelay
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMENUSHOWDELAY, 0, ref data, 0);
                return data;
            }

        }

        public static bool IsComboBoxAnimationEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETCOMBOBOXANIMATION, 0, ref data, 0);
                return data != 0;
            }
        }

        public static bool IsTitleBarGradientEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETGRADIENTCAPTIONS, 0, ref data, 0);
                return data != 0;
            }
        }


        public static bool IsHotTrackingEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETHOTTRACKING, 0, ref data, 0);
                return data != 0;
            }
        }

        public static bool IsListBoxSmoothScrollingEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETLISTBOXSMOOTHSCROLLING, 0, ref data, 0);
                return data != 0;
            }
        }

        public static bool IsMenuAnimationEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETMENUANIMATION, 0, ref data, 0);
                return data != 0;
            }
        }

        public static bool IsSelectionFadeEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp || OSFeature.Feature.OnWin2k)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETSELECTIONFADE, 0, ref data, 0);
                    return data != 0;
                }
                return false;
            }
        }

        public static bool IsToolTipAnimationEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp || OSFeature.Feature.OnWin2k)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETTOOLTIPANIMATION, 0, ref data, 0);
                    return data != 0;
                }
                return false;

            }
        }

        public static bool UIEffectsEnabled
        {
            get
            {
                if (OSFeature.Feature.OnXp || OSFeature.Feature.OnWin2k)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETUIEFFECTS, 0, ref data, 0);
                    return data != 0;
                }
                return false;
            }
        }


        public static bool IsActiveWindowTrackingEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETACTIVEWINDOWTRACKING, 0, ref data, 0);
                return data != 0;
            }
        }

        public static int ActiveWindowTrackingDelay
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETACTIVEWNDTRKTIMEOUT, 0, ref data, 0);
                return data;
            }

        }


        public static bool IsMinimizeRestoreAnimationEnabled
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETANIMATION, 0, ref data, 0);
                return data != 0;
            }
        }

        public static int BorderMultiplierFactor
        {
            get
            {
                int data = 0;
                UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETBORDER, 0, ref data, 0);
                return data;
            }

        }

        public static int CaretBlinkTime
        {
            get
            {
                //return unchecked((int)SafeNativeMethods.GetCaretBlinkTime());
                return 0;
            }

        }

        public static int CaretWidth
        {
            get
            {
                if (OSFeature.Feature.OnXp || OSFeature.Feature.OnWin2k)
                {
                    int data = 0;
                    UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETCARETWIDTH, 0, ref data, 0);
                    return data;
                }
                else
                {
                    throw new NotSupportedException("SystemInformationFeatureNotSupported");
                }
            }

        }

        public static int MouseWheelScrollDelta
        {
            get
            {
                return NativeMethods.WHEEL_DELTA;
            }

        }


        public static int VerticalFocusThickness
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYFOCUSBORDER);
                }
                else
                {
                    throw new NotSupportedException("SystemInformationFeatureNotSupported");
                }
            }

        }

        public static int HorizontalFocusThickness
        {
            get
            {
                if (OSFeature.Feature.OnXp)
                {
                    return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXFOCUSBORDER);
                }
                else
                {
                    throw new NotSupportedException("SystemInformationFeatureNotSupported");
                }
            }

        }

        public static int VerticalResizeBorderThickness
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CYSIZEFRAME);
            }

        }

        public static int HorizontalResizeBorderThickness
        {
            get
            {
                return UnsafeNativeMethods.GetSystemMetrics(NativeMethods.SM_CXSIZEFRAME);
            }

        }

        public static ScreenOrientation ScreenOrientation
        {
            get
            {
                ScreenOrientation so = ScreenOrientation.Angle0;
                NativeMethods.DEVMODE dm = new NativeMethods.DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(NativeMethods.DEVMODE));
                dm.dmDriverExtra = 0;
                try
                {
                    //SafeNativeMethods.EnumDisplaySettings(null, -1 /*ENUM_CURRENT_SETTINGS*/, ref dm);
                    if ((dm.dmFields & NativeMethods.DM_DISPLAYORIENTATION) > 0)
                    {
                        so = dm.dmDisplayOrientation;
                    }
                }
                catch
                {
                    // empty catch, we'll just return the default if the call to EnumDisplaySettings fails.
                }

                return so;
            }
        }

        public static int SizingBorderWidth
        {
            get
            {
                //we can get the system's menu font through the NONCLIENTMETRICS structure via SystemParametersInfo
                //
                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);
                if (result && data.iBorderWidth > 0)
                {
                    return data.iBorderWidth;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static Size SmallCaptionButtonSize
        {
            get
            {

                //we can get the system's menu font through the NONCLIENTMETRICS structure via SystemParametersInfo
                //
                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);
                if (result && data.iSmCaptionHeight > 0 && data.iSmCaptionWidth > 0)
                {
                    return new Size(data.iSmCaptionWidth, data.iSmCaptionHeight);
                }
                else
                {
                    return Size.Empty;
                }


            }
        }

        public static Size MenuBarButtonSize
        {
            get
            {

                //we can get the system's menu font through the NONCLIENTMETRICS structure via SystemParametersInfo
                //
                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);
                if (result && data.iMenuHeight > 0 && data.iMenuWidth > 0)
                {
                    return new Size(data.iMenuWidth, data.iMenuHeight);
                }
                else
                {
                    return Size.Empty;
                }


            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        // End ADDITIONS FOR WHIDBEY                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////


        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal static bool InLockedTerminalSession()
        {
            bool retVal = false;

            //if (SystemInformation.TerminalServerSession)
            //{
            //    // Let's try to open the input desktop, it it fails with access denied assume
            //    // the app is running on a secure desktop.
            //    IntPtr hDsk = SafeNativeMethods.OpenInputDesktop(0, false, NativeMethods.DESKTOP_SWITCHDESKTOP);

            //    if (hDsk == IntPtr.Zero)
            //    {
            //        int error = Marshal.GetLastWin32Error();
            //        retVal = error == NativeMethods.ERROR_ACCESS_DENIED;
            //    }

            //    if (hDsk != IntPtr.Zero)
            //    {
            //        SafeNativeMethods.CloseDesktop(hDsk);
            //    }
            //}

            return retVal;
        }
    }
}