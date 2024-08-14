using App1;

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

            identifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += $"document.getElementById('{identifier}').addEventListener('click', function() {{ eventHandler('{identifier}', 'Click');}});​";

            await Page.Add(Parent.identifier, "innerHTML", $"'<div class=\"option-button\" style=\"{style}\" id=\"{identifier}\"><input type=\"radio\" id=\"{identifier}-input\" name=\"{Parent.identifier}-option\" value=\"test\"><label for=\"{identifier}-input\">{Text}</label></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
