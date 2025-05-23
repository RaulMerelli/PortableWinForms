using App1;
using System.ComponentModel;

namespace System.Windows.Forms
{
    public class MessageBox
    {
        private const int IDOK = 1;
        private const int IDCANCEL = 2;
        private const int IDABORT = 3;
        private const int IDRETRY = 4;
        private const int IDIGNORE = 5;
        private const int IDYES = 6;
        private const int IDNO = 7;
        private const int HELP_BUTTON = 16384;

        [ThreadStatic]
        private static HelpInfo[] helpInfoTable;

        internal static HelpInfo HelpInfo
        {
            get
            {
                if (helpInfoTable != null && helpInfoTable.Length != 0)
                {
                    return helpInfoTable[helpInfoTable.Length - 1];
                }

                return null;
            }
        }

        private MessageBox()
        {
        }

        private static DialogResult Win32ToDialogResult(int value)
        {
            DialogResult result;
            switch (value)
            {
                case 1:
                    result = DialogResult.OK;
                    break;
                case 2:
                    result = DialogResult.Cancel;
                    break;
                case 3:
                    result = DialogResult.Abort;
                    break;
                case 4:
                    result = DialogResult.Retry;
                    break;
                case 5:
                    result = DialogResult.Ignore;
                    break;
                case 6:
                    result = DialogResult.Yes;
                    break;
                case 7:
                    result = DialogResult.No;
                    break;
                default:
                    result = DialogResult.No;
                    break;
            }
            return result;
        }

        private static void PopHelpInfo()
        {
            if (helpInfoTable != null)
            {
                if (helpInfoTable.Length == 1)
                {
                    helpInfoTable = null;
                    return;
                }

                int num = helpInfoTable.Length - 1;
                HelpInfo[] destinationArray = new HelpInfo[num];
                Array.Copy(helpInfoTable, destinationArray, num);
                helpInfoTable = destinationArray;
            }
        }

        private static void PushHelpInfo(HelpInfo hpi)
        {
            int num = 0;
            HelpInfo[] array;
            if (helpInfoTable == null)
            {
                array = new HelpInfo[num + 1];
            }
            else
            {
                num = helpInfoTable.Length;
                array = new HelpInfo[num + 1];
                Array.Copy(helpInfoTable, array, num);
            }

            array[num] = hpi;
            helpInfoTable = array;
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, bool displayHelpButton)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options, displayHelpButton);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath);
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath);
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, string keyword)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath, keyword);
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, string keyword)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath, keyword);
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath, navigator);
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath, navigator);
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator, object param)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath, navigator, param);
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator, object param)
        {
            HelpInfo hpi = new HelpInfo(helpFilePath, navigator, param);
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options, hpi);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton, options, showHelp: false);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return ShowCore(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(string text, string caption)
        {
            return ShowCore(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(string text)
        {
            return ShowCore(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, options, showHelp: false);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            return ShowCore(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            return ShowCore(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        public static DialogResult Show(IWin32Window owner, string text)
        {
            return ShowCore(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, showHelp: false);
        }

        private static DialogResult ShowCore(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, HelpInfo hpi)
        {
            DialogResult dialogResult = DialogResult.None;
            try
            {
                PushHelpInfo(hpi);
                return ShowCore(owner, text, caption, buttons, icon, defaultButton, options, showHelp: true);
            }
            finally
            {
                PopHelpInfo();
            }
        }

        private static DialogResult ShowCore(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, bool showHelp)
        {
            if (!ClientUtils.IsEnumValid(buttons, (int)buttons, 0, 5))
            {
                throw new InvalidEnumArgumentException("buttons", (int)buttons, typeof(MessageBoxButtons));
            }

            if (!WindowsFormsUtils.EnumValidator.IsEnumWithinShiftedRange(icon, 4, 0, 4))
            {
                throw new InvalidEnumArgumentException("icon", (int)icon, typeof(MessageBoxIcon));
            }

            if (!WindowsFormsUtils.EnumValidator.IsEnumWithinShiftedRange(defaultButton, 8, 0, 2))
            {
                throw new InvalidEnumArgumentException("defaultButton", (int)defaultButton, typeof(DialogResult));
            }
            /*
            if (!SystemInformation.UserInteractive && (options & (MessageBoxOptions.ServiceNotification | MessageBoxOptions.DefaultDesktopOnly)) == 0)
            {
                throw new InvalidOperationException(SR.GetString("CantShowModalOnNonInteractive"));
            }
            */
            if (owner != null && (options & (MessageBoxOptions.ServiceNotification | MessageBoxOptions.DefaultDesktopOnly)) != 0)
            {
                throw new ArgumentException("CantShowMBServiceWithOwner", "options");
            }

            if (showHelp && (options & (MessageBoxOptions.ServiceNotification | MessageBoxOptions.DefaultDesktopOnly)) != 0)
            {
                throw new ArgumentException("CantShowMBServiceWithHelp", "options");
            }


            int buttonQty = 1;
            string Btn1 = "";
            string Btn2 = "";
            string Btn3 = "";
            string iconName = "";

            switch (icon)
            {
                case MessageBoxIcon.Information:
                    iconName = "Information";
                    break;
                case MessageBoxIcon.Question:
                    iconName = "Question";
                    break;
                case MessageBoxIcon.Warning:
                    iconName = "Warning";
                    break;
                case MessageBoxIcon.Error:
                    iconName = "Error";
                    break;
            }

            if (buttons == MessageBoxButtons.AbortRetryIgnore /*|| buttons == MessageBoxButtons.CancelTryContinue*/ || buttons == MessageBoxButtons.YesNoCancel)
            {
                buttonQty = 3;
            }
            else if (buttons != MessageBoxButtons.OK)
            {
                buttonQty = 2;
            }

            switch (buttons)
            {
                // Need for system aware translation!
                case MessageBoxButtons.OK:
                    Btn1 = "OK";
                    break;
                case MessageBoxButtons.OKCancel:
                    Btn1 = "OK";
                    Btn2 = "Annulla";
                    break;
                case MessageBoxButtons.YesNo:
                    Btn1 = "Sì";
                    Btn2 = "No";
                    break;
                case MessageBoxButtons.RetryCancel:
                    Btn1 = "Riprova";
                    Btn2 = "Annulla";
                    break;
                case MessageBoxButtons.YesNoCancel:
                    Btn1 = "Sì";
                    Btn2 = "No";
                    Btn3 = "Annulla";
                    break;
                /*case MessageBoxButtons.CancelTryContinue:
                    Btn1 = "Continua";
                    Btn2 = "Riprova";
                    Btn3 = "Annulla";
                    break;*/
                case MessageBoxButtons.AbortRetryIgnore:
                    Btn1 = "Annulla";
                    Btn2 = "Riprova";
                    Btn3 = "Ignora";
                    break;
            }

            string htmlContent = "";
            htmlContent += $"<div class=\"MessageBox\">\r\n    " +
                $"<div class=\"MessageBoxInner MissingWidth\">\r\n        " +
                $"<div class=\"MessageBoxInnerTop\">\r\n            ";

            if (icon != MessageBoxIcon.None)
            {
                htmlContent += $"<img id=\"MessageBox-picIcon\" src=\"resources/32x32/{iconName}.png\">\r\n                ";
            }

            htmlContent += $"<div id=\"MessageBox-message\" class=\"label\">{text}</div>\r\n        " +
                $"</div>\r\n        " +
                $"<div class=\"MessageBoxInnerBottom layout{buttonQty}btns\">\r\n            " +
                $"<div id=\"MessageBox-btnsContainer\"></div>\r\n            " +
                $"<button id=\"MessageBox-btn1\" class=\"button\" onclick=\"closeCurrentWindow(this)\"><p>{Btn1}</p></button>\r\n            ";
            if (buttonQty >= 2)
            {
                htmlContent += $"<button id=\"MessageBox-btn2\" class=\"button\" onclick=\"closeCurrentWindow(this)\"><p>{Btn2}</p></button>\r\n                ";
            }
            if (buttonQty == 3)
            {
                htmlContent += $"<button id=\"MessageBox-btn3\" class=\"button\" onclick=\"closeCurrentWindow(this)\"><p>{Btn3}</p></button>\r\n                ";
            }
            htmlContent += $"</div>\r\n    " +
                $"</div>\r\n</div>\r\n<script>\r\n    " +
                $"var MessageBoxes = document.querySelectorAll('.MissingWidth');\r\n    " +
                $"for (var i = 0; i < MessageBoxes.length; i++) {{\r\n        " +
                $"MessageBoxes[i].style[\"min-width\"] = MessageBoxes[i].clientWidth + 'px';\r\n        " +
                $"MessageBoxes[i].style[\"max-width\"] = MessageBoxes[i].clientWidth + 'px';\r\n        " +
                $"MessageBoxes[i].classList.remove('MissingWidth');\r\n    " +
                $"}}\r\n" +
                $"</script>\r\n";

            JsonProperties.Properties properties = new JsonProperties.Properties
            {
                id = "MessageBox",
                identifier = "MessageBox",
                icon = "",
                maximize = false,
                minimize = false,
                mdi = false,
                position = new JsonProperties.Position
                {
                    at = "center",
                    my = "center"
                },
                resizable = false,
                size = new JsonProperties.Size
                {
                    width = "auto",
                    height = "auto"
                },
                title = caption
            };

            launchWindow(htmlContent, properties);
            /*
            if (((uint)options & 0xFFE7FFFFu) != 0)
            {
                IntSecurity.UnmanagedCode.Demand();
            }

            IntSecurity.SafeSubWindows.Demand();
            int num = (showHelp ? 16384 : 0);
            num |= (int)buttons | (int)icon | (int)defaultButton | (int)options;
            IntPtr handle = IntPtr.Zero;
            if (showHelp || (options & (MessageBoxOptions.ServiceNotification | MessageBoxOptions.DefaultDesktopOnly)) == 0)
            {
                handle = ((owner != null) ? Control.GetSafeHandle(owner) : UnsafeNativeMethods.GetActiveWindow());
            }

            IntPtr userCookie = IntPtr.Zero;
            if (Application.UseVisualStyles)
            {
                if (UnsafeNativeMethods.GetModuleHandle("shell32.dll") == IntPtr.Zero && UnsafeNativeMethods.LoadLibraryFromSystemPathIfAvailable("shell32.dll") == IntPtr.Zero)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(lastWin32Error, SR.GetString("LoadDLLError", "shell32.dll"));
                }

                userCookie = UnsafeNativeMethods.ThemingScope.Activate();
            }

            Application.BeginModalMessageLoop();
            DialogResult result;
            try
            {
                result = Win32ToDialogResult(SafeNativeMethods.MessageBox(new HandleRef(owner, handle), text, caption, num));
            }
            finally
            {
                Application.EndModalMessageLoop();
                UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
            }

            UnsafeNativeMethods.SendMessage(new HandleRef(owner, handle), 7, 0, 0);
            
            return result;
            */
            return DialogResult.OK; //temp
        }
        
        async static void launchWindow(string htmlContent, JsonProperties.Properties properties)
        {
            string propertiesSerialized = JsonHelper.Serialize(properties);
            string script = $"launchPostWindowSuccess(`{htmlContent}`,`{propertiesSerialized}`)";
            await Page.RunScript(script);
            //WebView2Extender.pContainer.WebMessageReceived += webView2_WebMessageReceived;
        }
    }
}
