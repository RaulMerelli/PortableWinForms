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

    public class InputLanguageChangingEventArgs : CancelEventArgs
    {

        private readonly InputLanguage inputLanguage;

        private readonly CultureInfo culture;
        private readonly bool sysCharSet;

        /**
         * @deprecated Should use the new constructor instead.
         */
        public InputLanguageChangingEventArgs(CultureInfo culture, bool sysCharSet)
        {

            this.inputLanguage = System.Windows.Forms.InputLanguage.FromCulture(culture);
            this.culture = culture;
            this.sysCharSet = sysCharSet;
        }

        public InputLanguageChangingEventArgs(InputLanguage inputLanguage, bool sysCharSet)
        {

            if (inputLanguage == null)
                throw new ArgumentNullException("inputLanguage");

            this.inputLanguage = inputLanguage;
            this.culture = inputLanguage.Culture;
            this.sysCharSet = sysCharSet;
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

        public bool SysCharSet
        {
            get
            {
                return sysCharSet;
            }
        }
    }
}