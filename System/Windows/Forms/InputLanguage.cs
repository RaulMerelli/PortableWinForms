﻿namespace System.Windows.Forms
{
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    using System.Diagnostics;

    using System;
    using Microsoft.Win32;
    using System.Globalization;
    using System.ComponentModel;
    using System.Drawing;

    public sealed class InputLanguage
    {

        private readonly IntPtr handle;

        internal InputLanguage(IntPtr handle)
        {
            this.handle = handle;
        }

        public CultureInfo Culture
        {
            get
            {
                return new CultureInfo((int)handle & 0xFFFF);
            }
        }

        public static InputLanguage CurrentInputLanguage
        {
            get
            {
                //Application.OleRequired();
                // note we can obtain the KeyboardLayout for a given thread...
                //return new InputLanguage(SafeNativeMethods.GetKeyboardLayout(0));
                return InputLanguage.DefaultInputLanguage;
            }
            set
            {
                IntSecurity.AffectThreadBehavior.Demand();

                // (NDPWhidbey 8362) OleInitialize needs to be called before we can call ActivateKeyboardLayout.
                //Application.OleRequired();
                if (value == null)
                {
                    value = InputLanguage.DefaultInputLanguage;
                }
                //IntPtr handleOld = SafeNativeMethods.ActivateKeyboardLayout(new HandleRef(value, value.handle), 0);
                //if (handleOld == IntPtr.Zero)
                //{
                //    throw new ArgumentException("ErrorBadInputLanguage");
                //}
            }
        }

        public static InputLanguage DefaultInputLanguage
        {
            get
            {
                IntPtr[] data = new IntPtr[1];
                //UnsafeNativeMethods.SystemParametersInfo(NativeMethods.SPI_GETDEFAULTINPUTLANG, 0, data, 0);
                return new InputLanguage(data[0]);
            }
        }

        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        public static InputLanguageCollection InstalledInputLanguages
        {
            get
            {
                int size = 0;
                //int size = SafeNativeMethods.GetKeyboardLayoutList(0, null);

                IntPtr[] handles = new IntPtr[size];
                //SafeNativeMethods.GetKeyboardLayoutList(size, handles);

                InputLanguage[] ils = new InputLanguage[size];
                for (int i = 0; i < size; i++)
                    ils[i] = new InputLanguage(handles[i]);

                return new InputLanguageCollection(ils);
            }
        }

        //public string LayoutName
        //{
        //    get
        //    {
        //        // There is no good way to do this in Win32...  GetKeyboardLayoutName does what we want,
        //        // but only for the current input language; setting and resetting the current input language
        //        // would generate spurious InputLanguageChanged events.

        //        /*
        //                    HKL is a 32 bit value. HIWORD is a Device Handle. LOWORD is Language ID.
                
        //        HKL
        //        +------------------------+-------------------------+
        //        |     Device Handle      |       Language ID       |
        //        +------------------------+-------------------------+
        //        31                     16 15                      0   bit
                
                
        //        Language ID
        //        +---------------------------+-----------------------+
        //        |     Sublanguage ID        | Primary Language ID   |
        //        +---------------------------+-----------------------+
        //        15                        10 9                     0   bit
                
        //        WORD LangId  = MAKELANGID(primary, sublang)
        //        BYTE primary = PRIMARYLANGID(LangId)
        //        BYTE sublang = PRIMARYLANGID(LangID)
                
        //        How Preload is interpreted: example US-Dvorak
        //        Look in HKEY_CURRENT_USER\Keyboard Layout\Preload
        //        Name="4"  (may vary)
        //        Value="d0000409"  -> Language ID = 0409
        //        Look in HKEY_CURRENT_USER\Keyboard Layout\Substitutes
        //        Name="d0000409"
        //        Value="00010409"
        //        Look in HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\00010409
        //        "Layout File": name of keyboard layout DLL (KBDDV.DLL)
        //        "Layout Id": ID of this layout (0002)
        //        Win32k will change the top nibble of layout ID to F, which makes F002.
        //        Combined with Language ID, the final HKL is F0020409.
        //        */

        //        string layoutName = null;

        //        IntPtr currentHandle = handle;
        //        int language = unchecked((int)(long)currentHandle) & 0xffff;
        //        int device = (unchecked((int)(long)currentHandle) >> 16) & 0x0fff;

        //        // SECREVIEW : We have to get the input information from the registry. These two 
        //        //           : keys only contain keyboard information. This is safe to do.
        //        //
        //        new RegistryPermission(PermissionState.Unrestricted).Assert();

        //        try
        //        {
        //            if (device == language || device == 0)
        //            {
        //                // Default keyboard for language
        //                string keyName = Convert.ToString(language, 16);
        //                keyName = PadWithZeroes(keyName, 8);
        //                RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts\\" + keyName);

        //                // Attempt to extract the localized keyboard layout name
        //                // using the SHLoadIndirectString API...
        //                layoutName = GetLocalizedKeyboardLayoutName(key.GetValue("Layout Display Name") as string);

        //                // Default back to our legacy codepath and obtain the name
        //                // directly through the registry value
        //                if (layoutName == null)
        //                {
        //                    layoutName = (string)key.GetValue("Layout Text");
        //                }

        //                key.Close();
        //            }
        //            else
        //            {
        //                // Look for a substitution 
        //                //
        //                RegistryKey substitutions = Registry.CurrentUser.OpenSubKey("Keyboard Layout\\Substitutes");
        //                string[] encodings = null;
        //                if (substitutions != null)
        //                {
        //                    encodings = substitutions.GetValueNames();

        //                    foreach (string encoding in encodings)
        //                    {
        //                        int encodingValue = Convert.ToInt32(encoding, 16);
        //                        if (encodingValue == unchecked((int)(long)currentHandle) ||
        //                            (encodingValue & 0x0FFFFFFF) == (unchecked((int)(long)currentHandle) & 0x0FFFFFFF) ||
        //                            (encodingValue & 0xFFFF) == language)
        //                        {

        //                            currentHandle = (IntPtr)Convert.ToInt32((string)substitutions.GetValue(encoding), 16);
        //                            language = unchecked((int)(long)currentHandle) & 0xFFFF;
        //                            device = (unchecked((int)(long)currentHandle) >> 16) & 0xFFF;
        //                            break;
        //                        }
        //                    }

        //                    substitutions.Close();
        //                }

        //                RegistryKey layouts = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts");
        //                if (layouts != null)
        //                {
        //                    encodings = layouts.GetSubKeyNames();

        //                    // Check to see if the encoding directly matches the handle -- some do.
        //                    //
        //                    foreach (string encoding in encodings)
        //                    {
        //                        Debug.Assert(encoding.Length == 8, "unexpected key in registry: hklm\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts\\" + encoding);
        //                        if (currentHandle == (IntPtr)Convert.ToInt32(encoding, 16))
        //                        {
        //                            RegistryKey key = layouts.OpenSubKey(encoding);

        //                            // Attempt to extract the localized keyboard layout name
        //                            // using the SHLoadIndirectString API...
        //                            layoutName = GetLocalizedKeyboardLayoutName(key.GetValue("Layout Display Name") as string);

        //                            // Default back to our legacy codepath and obtain the name
        //                            // directly through the registry value
        //                            if (layoutName == null)
        //                            {
        //                                layoutName = (string)key.GetValue("Layout Text");
        //                            }

        //                            key.Close();
        //                            break;
        //                        }
        //                    }
        //                }

        //                if (layoutName == null)
        //                {

        //                    // No luck there.  Match the language first, then try to find a layout ID
        //                    //
        //                    foreach (string encoding in encodings)
        //                    {
        //                        Debug.Assert(encoding.Length == 8, "unexpected key in registry: hklm\\SYSTEM\\CurrentControlSet\\Control\\Keyboard Layouts\\" + encoding);
        //                        if (language == (0xffff & Convert.ToInt32(encoding.Substring(4, 4), 16)))
        //                        {
        //                            //RegistryKey key = layouts.OpenSubKey(encoding);
        //                            //string codeValue = (string)key.GetValue("Layout Id");
        //                            //if (codeValue != null)
        //                            //{
        //                            //    int value = Convert.ToInt32(codeValue, 16);
        //                            //    if (value == device)
        //                            //    {

        //                            //        // Attempt to extract the localized keyboard layout name
        //                            //        // using the SHLoadIndirectString API...
        //                            //        layoutName = GetLocalizedKeyboardLayoutName(key.GetValue("Layout Display Name") as string);

        //                            //        // Default back to our legacy codepath and obtain the name
        //                            //        // directly through the registry value
        //                            //        if (layoutName == null)
        //                            //        {
        //                            //            layoutName = (string)key.GetValue("Layout Text");
        //                            //        }
        //                            //    }
        //                            //}
        //                            //key.Close();
        //                            if (layoutName != null)
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }

        //                layouts.Close();
        //            }
        //        }
        //        finally
        //        {
        //            System.Security.CodeAccessPermission.RevertAssert();
        //        }

        //        if (layoutName == null)
        //        {
        //            layoutName = "UnknownInputLanguageLayout";
        //        }
        //        return layoutName;
        //    }
        //}

        private static string GetLocalizedKeyboardLayoutName(string layoutDisplayName)
        {

            if (layoutDisplayName != null && Environment.OSVersion.Version.Major >= 5)
            {

                StringBuilder sb = new StringBuilder(512);

                //uint res = UnsafeNativeMethods.SHLoadIndirectString(layoutDisplayName, sb, (uint)sb.Capacity, IntPtr.Zero);

                //if (res == NativeMethods.S_OK)
                {
                    return sb.ToString();
                }
            }

            return null;
        }

        //internal static InputLanguageChangedEventArgs CreateInputLanguageChangedEventArgs(Message m)
        //{
        //    return new InputLanguageChangedEventArgs(new InputLanguage(m.LParam), unchecked((byte)(long)m.WParam));
        //}

        //internal static InputLanguageChangingEventArgs CreateInputLanguageChangingEventArgs(Message m)
        //{
        //    InputLanguage inputLanguage = new InputLanguage(m.LParam);

        //    // NOTE: by default we should allow any locale switch
        //    //
        //    bool localeSupportedBySystem = !(m.WParam == IntPtr.Zero);
        //    return new InputLanguageChangingEventArgs(inputLanguage, localeSupportedBySystem);

        //}

        public override bool Equals(object value)
        {
            if (value is InputLanguage)
            {
                return (this.handle == ((InputLanguage)value).handle);
            }
            return false;
        }

        public static InputLanguage FromCulture(CultureInfo culture)
        {

            // KeyboardLayoutId is the LCID for built-in cultures, but it  
            // is the CU-preferred keyboard language for custom cultures.
            int lcid = culture.KeyboardLayoutId;

            foreach (InputLanguage lang in InstalledInputLanguages)
            {
                if ((unchecked((int)(long)lang.handle) & 0xFFFF) == lcid)
                {
                    return lang;
                }
            }

            return null;
        }

        public override int GetHashCode()
        {
            return unchecked((int)(long)handle);
        }

        private static string PadWithZeroes(string input, int length)
        {
            return "0000000000000000".Substring(0, length - input.Length) + input;
        }
    }
}