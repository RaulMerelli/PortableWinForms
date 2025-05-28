namespace System.Windows.Forms
{
    using System.Runtime.InteropServices;

    using System.Diagnostics;

    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using System.ComponentModel;
    using Microsoft.Win32;

    public class InputLanguageChangedEventArgs : EventArgs
    {

        private readonly InputLanguage inputLanguage;

        private readonly CultureInfo culture;
        private readonly byte charSet;

        /**
         * @deprecated.  Use the other constructor instead.
         */
        public InputLanguageChangedEventArgs(CultureInfo culture, byte charSet)
        {
            this.inputLanguage = System.Windows.Forms.InputLanguage.FromCulture(culture);
            this.culture = culture;
            this.charSet = charSet;
        }

        public InputLanguageChangedEventArgs(InputLanguage inputLanguage, byte charSet)
        {
            this.inputLanguage = inputLanguage;
            this.culture = inputLanguage.Culture;
            this.charSet = charSet;
        }

        public InputLanguage InputLanguage
        {
            get
            {
                return inputLanguage;
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return culture;
            }
        }

        public byte CharSet
        {
            get
            {
                return charSet;
            }
        }
    }
}