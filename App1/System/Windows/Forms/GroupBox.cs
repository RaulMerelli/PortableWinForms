using App1;

namespace System.Windows.Forms
{
    public class GroupBox : Control
    {
        public string Text = "";
        public bool UseCompatibleTextRendering = true;
        public bool TabStop = false;

        public async override void PerformLayout()
        {
            string style = "";
            string titlestyle = "";
            string script = "";

            WebviewIdentifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"margin: 0px;";
            style += $"padding: 0px;";
            style += $"background-color: {System.Drawing.ColorTranslator.ToHtml(BackColor)};";
            titlestyle += $"background-color: {System.Drawing.ColorTranslator.ToHtml(BackColor)};";
            style += CssLocationAndSize();

            script += $"document.getElementById('{WebviewIdentifier}').addEventListener('click', function() {{ eventHandler('{WebviewIdentifier}', 'Click');}});​";

            await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<div id=\"{WebviewIdentifier}\" style=\"{style}\" class=\"frame\"><div id=\"{WebviewIdentifier}-title\" style=\"{titlestyle}\" class=\"frame-title\">{Text}</div></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
