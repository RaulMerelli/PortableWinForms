using App1;

namespace System.Windows.Forms
{
    public class GroupBox : Control
    {
        internal string text;
        public bool UseCompatibleTextRendering = true;
        public bool TabStop = false;

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value != base.Text)
                {
                    base.Text = value;
                    if (layoutPerformed)
                    {
                        Page.Set(WebviewIdentifier + "-title", "innerHTML", $"\"{value}\"");
                    }
                }
            }
        }

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
            titlestyle += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += preLayoutScriptString;

            await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<div id=\"{WebviewIdentifier}\" style=\"{style}\" class=\"frame\"><div id=\"{WebviewIdentifier}-title\" style=\"{titlestyle}\" class=\"frame-title\">{Text}</div></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
