﻿namespace System.Drawing
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Windows.Forms;

    public sealed class SystemFonts
    {
        private static readonly object SystemFontsKey = new object();

        // Cannot be instantiated.
        private SystemFonts()
        {
        }

        public static Font CaptionFont
        {
            get
            {
                Font captionFont = null;

                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);

                if (result && data.lfCaptionFont != null)
                {
                    // SECREVIEW : This assert is ok.  The LOGFONT struct passed to Font.FromLogFont was obtained from the system.
                    //
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        //captionFont = Font.FromLogFont(data.lfCaptionFont);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalFontException(ex))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }

                    if (captionFont == null)
                    {
                        captionFont = DefaultFont;
                    }
                    else if (captionFont.Unit != GraphicsUnit.Point)
                    {
                        captionFont = FontInPoints(captionFont);
                    }
                }

                captionFont.SetSystemFontName("CaptionFont");
                return captionFont;
            }
        }


        public static Font SmallCaptionFont
        {
            get
            {
                Font smcaptionFont = null;

                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);

                if (result && data.lfSmCaptionFont != null)
                {
                    // SECREVIEW : This assert is ok.  The LOGFONT struct passed to Font.FromLogFont was obtained from the system.
                    //
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        //smcaptionFont = Font.FromLogFont(data.lfSmCaptionFont);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalFontException(ex))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }

                    if (smcaptionFont == null)
                    {
                        smcaptionFont = DefaultFont;
                    }
                    else if (smcaptionFont.Unit != GraphicsUnit.Point)
                    {
                        smcaptionFont = FontInPoints(smcaptionFont);
                    }
                }

                smcaptionFont.SetSystemFontName("SmallCaptionFont");
                return smcaptionFont;
            }
        }

        public static Font MenuFont
        {
            get
            {
                Font menuFont = null;

                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);

                if (result && data.lfMenuFont != null)
                {
                    // SECREVIEW : This assert is ok.  The LOGFONT struct passed to Font.FromLogFont was obtained from the system.
                    //
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        //menuFont = Font.FromLogFont(data.lfMenuFont);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalFontException(ex))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }

                    if (menuFont == null)
                    {
                        menuFont = DefaultFont;
                    }
                    else if (menuFont.Unit != GraphicsUnit.Point)
                    {
                        menuFont = FontInPoints(menuFont);
                    }
                }

                menuFont.SetSystemFontName("MenuFont");
                return menuFont;
            }
        }

        public static Font StatusFont
        {
            get
            {
                Font statusFont = null;

                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);

                if (result && data.lfStatusFont != null)
                {
                    // SECREVIEW : This assert is ok.  The LOGFONT struct passed to Font.FromLogFont was obtained from the system.
                    //
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        //statusFont = Font.FromLogFont(data.lfStatusFont);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalFontException(ex))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }

                    if (statusFont == null)
                    {
                        statusFont = DefaultFont;
                    }
                    else if (statusFont.Unit != GraphicsUnit.Point)
                    {
                        statusFont = FontInPoints(statusFont);
                    }
                }

                statusFont.SetSystemFontName("StatusFont");
                return statusFont;
            }
        }

        public static Font MessageBoxFont
        {
            get
            {
                Font messageboxFont = null;

                NativeMethods.NONCLIENTMETRICS data = new NativeMethods.NONCLIENTMETRICS();
                bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETNONCLIENTMETRICS, data.cbSize, data, 0);

                if (result && data.lfMessageFont != null)
                {
                    // SECREVIEW : This assert is ok.  The LOGFONT struct passed to Font.FromLogFont was obtained from the system.
                    //
                    IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        //messageboxFont = Font.FromLogFont(data.lfMessageFont);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalFontException(ex))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }

                    if (messageboxFont == null)
                    {
                        messageboxFont = DefaultFont;
                    }
                    else if (messageboxFont.Unit != GraphicsUnit.Point)
                    {
                        messageboxFont = FontInPoints(messageboxFont);
                    }
                }

                messageboxFont.SetSystemFontName("MessageBoxFont");
                return messageboxFont;
            }
        }

        private static bool IsCriticalFontException(Exception ex)
        {
            return !(
                // In any of these cases we'll handle the exception.
                ex is ExternalException ||
                ex is ArgumentException ||
                ex is OutOfMemoryException || // GDI+ throws this one for many reasons other than actual OOM.
                ex is InvalidOperationException ||
                ex is NotImplementedException ||
                ex is FileNotFoundException);
        }

        public static Font IconTitleFont
        {
            get
            {
                Font icontitleFont = null;

                //SafeNativeMethods.LOGFONT itfont = new SafeNativeMethods.LOGFONT();
                //bool result = UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETICONTITLELOGFONT, Marshal.SizeOf(itfont), itfont, 0);

                //if (result && itfont != null)
                //{
                //    // SECREVIEW : This assert is ok.  The LOGFONT struct passed to Font.FromLogFont was obtained from the system.
                //    //
                //    IntSecurity.ObjectFromWin32Handle.Assert();
                //    try
                //    {
                //        icontitleFont = Font.FromLogFont(itfont);
                //    }
                //    catch (Exception ex)
                //    {
                //        if (IsCriticalFontException(ex))
                //        {
                //            throw;
                //        }
                //    }
                //    finally
                //    {
                //        CodeAccessPermission.RevertAssert();
                //    }

                //    if (icontitleFont == null)
                //    {
                //        icontitleFont = DefaultFont;
                //    }
                //    else if (icontitleFont.Unit != GraphicsUnit.Point)
                //    {
                //        icontitleFont = FontInPoints(icontitleFont);
                //    }
                //}

                icontitleFont.SetSystemFontName("IconTitleFont");
                return icontitleFont;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                          //
        // SystemFonts.DefaultFont is code moved from System.Windows.Forms.Control.DefaultFont      //
        // System.Windows.Forms.Control.DefaultFont delegates to SystemFonts.DefaultFont now.       //
        // Treat any changes to this code as you would treat breaking changes.                      //
        //                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////

        public static Font DefaultFont
        {
            get
            {
                Font defaultFont = null;

                //special case defaultfont for arabic systems too
                bool systemDefaultLCIDIsArabic = false;

                // For Japanese on Win9x get the MS UI Gothic font
                if (Environment.OSVersion.Platform == System.PlatformID.Win32NT &&
                    Environment.OSVersion.Version.Major <= 4)
                {

                    if ((UnsafeNativeMethods.GetSystemDefaultLCID() & 0x3ff) == 0x0011)
                    {
                        try
                        {
                            defaultFont = new Font("MS UI Gothic", 9);
                        }
                        //fall through here if this fails and we'll get the default
                        //font via the DEFAULT_GUI method
                        catch (Exception ex)
                        {
                            if (IsCriticalFontException(ex))
                            {
                                throw;
                            }
                        }
                    }
                }

                if (defaultFont == null)
                {
                    systemDefaultLCIDIsArabic = ((UnsafeNativeMethods.GetSystemDefaultLCID() & 0x3ff) == 0x0001);
                }

                // For arabic systems, regardless of the platform, always return Tahoma 8.
                // vsWhidbey 82453.
                if (systemDefaultLCIDIsArabic)
                {
                    Debug.Assert(defaultFont == null);
                    // Try TAHOMA 8
                    try
                    {
                        defaultFont = new Font("Tahoma", 8);
                    }
                    catch (Exception ex)
                    {
                        if (IsCriticalFontException(ex))
                        {
                            throw;
                        }
                    }
                }

                //
                // Neither Japanese on Win9x nor Arabic.
                // First try DEFAULT_GUI, then Tahoma 8, then GenericSansSerif 8.
                //

                // first, try DEFAULT_GUI font.
                //
                if (defaultFont == null)
                {
                    //IntPtr handle = UnsafeNativeMethods.GetStockObject(NativeMethods.DEFAULT_GUI_FONT);
                    //try
                    //{
                    //    Font fontInWorldUnits = null;

                    //    // SECREVIEW : We know that we got the handle from the stock object,
                    //    //           : so this is always safe.
                    //    //
                    //    IntSecurity.ObjectFromWin32Handle.Assert();
                    //    try
                    //    {
                    //        fontInWorldUnits = Font.FromHfont(handle);
                    //    }
                    //    finally
                    //    {
                    //        CodeAccessPermission.RevertAssert();
                    //    }

                    //    try
                    //    {
                    //        defaultFont = FontInPoints(fontInWorldUnits);
                    //    }
                    //    finally
                    //    {
                    //        fontInWorldUnits.Dispose();
                    //    }
                    //}
                    //catch (ArgumentException)
                    //{
                    //}
                }

                // If DEFAULT_GUI didn't work, we try Tahoma.
                //
                if (defaultFont == null)
                {
                    try
                    {
                        defaultFont = new Font("Tahoma", 8);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                // Last resort, we use the GenericSansSerif - this will
                // always work.
                //
                if (defaultFont == null)
                {
                    defaultFont = new Font(FontFamily.GenericSansSerif, 8);
                }

                if (defaultFont.Unit != GraphicsUnit.Point)
                {
                    defaultFont = FontInPoints(defaultFont);
                }

                Debug.Assert(defaultFont != null, "defaultFont wasn't set!");

                defaultFont.SetSystemFontName("DefaultFont");
                return defaultFont;
            }
        }

        public static Font DialogFont
        {
            get
            {
                Font dialogFont = null;

                if ((UnsafeNativeMethods.GetSystemDefaultLCID() & 0x3ff) == 0x0011)
                {
                    // for JAPANESE culture always return DefaultFont
                    dialogFont = DefaultFont;
                }
                else if (Environment.OSVersion.Platform == System.PlatformID.Win32Windows)
                {
                    // use DefaultFont for Win9X
                    dialogFont = DefaultFont;
                }
                else
                {
                    try
                    {
                        // use MS Shell Dlg 2, 8pt for anything else than Japanese and Win9x
                        dialogFont = new Font("MS Shell Dlg 2", 8);
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                if (dialogFont == null)
                {
                    dialogFont = DefaultFont;
                }
                else if (dialogFont.Unit != GraphicsUnit.Point)
                {
                    dialogFont = FontInPoints(dialogFont);
                }

                //
                // JAPANESE or Win9x: SystemFonts.DefaultFont returns a new Font object every time it is invoked.
                // So for JAPANESE or Win9x we return the DefaultFont w/ its SystemFontName set to DialogFont.
                //
                dialogFont.SetSystemFontName("DialogFont");
                return dialogFont;
            }
        }

        //Copied from System.Windows.Forms.ControlPaint
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        private static Font FontInPoints(Font font)
        {
            return new Font(font.FontFamily, font.SizeInPoints, font.Style, GraphicsUnit.Point, font.GdiCharSet, font.GdiVerticalFont);
        }

        public static Font GetFontByName(string systemFontName)
        {
            if ("CaptionFont".Equals(systemFontName))
            {
                return CaptionFont;
            }
            else if ("DefaultFont".Equals(systemFontName))
            {
                return DefaultFont;
            }
            else if ("DialogFont".Equals(systemFontName))
            {
                return DialogFont;
            }
            else if ("IconTitleFont".Equals(systemFontName))
            {
                return IconTitleFont;
            }
            else if ("MenuFont".Equals(systemFontName))
            {
                return MenuFont;
            }
            else if ("MessageBoxFont".Equals(systemFontName))
            {
                return MessageBoxFont;
            }
            else if ("SmallCaptionFont".Equals(systemFontName))
            {
                return SmallCaptionFont;
            }
            else if ("StatusFont".Equals(systemFontName))
            {
                return StatusFont;
            }
            else
            {
                return null;
            }
        }
    }
}