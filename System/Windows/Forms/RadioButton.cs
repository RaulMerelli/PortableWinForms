#if WINDOWS_UWP
using App1;
#else
using AndroidWinForms;
#endif

namespace System.Windows.Forms
{
    public class RadioButton : ButtonBase
    {
        internal bool checkState = false;
        public bool Checked
        {
            get
            {
                return checkState;
            }
            set
            {
                if (value != checkState)
                {
                    checkState = value;
                    if (layoutPerformed)
                    {

                    }
                }
            }
        }
        public bool TabStop;

        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            WebviewIdentifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += preLayoutScriptString;

            await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<div class=\"option-button\" style=\"{style}\" id=\"{WebviewIdentifier}\"><input type=\"radio\" id=\"{WebviewIdentifier}-input\" name=\"{Parent.WebviewIdentifier}-option\" value=\"test\"><label for=\"{WebviewIdentifier}-input\">{Text}</label></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
