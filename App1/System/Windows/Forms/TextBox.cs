using App1;

namespace System.Windows.Forms
{
    public class TextBox : TextBoxBase
    {
        internal string text;
        public string Text { 
            get 
            {
                return text;
            }
            set
            {
                if (value != text)
                {
                    text = value;
                    if (layoutPerformed)
                    {
                        Page.Set(WebviewIdentifier, Multiline ? "innerHTML" : "value", $"\"{value}\"");
                        OnTextChanged(EventArgs.Empty);
                    }
                }
            }
        }
        public bool UseCompatibleTextRendering = true;
        public bool Multiline = false;
        public ScrollBars ScrollBars = ScrollBars.None;

        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            WebviewIdentifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += preLayoutScriptString;

            script += $"document.getElementById('{WebviewIdentifier}').addEventListener('input', textChangedEvent);"; // Always, in order to keep updated the Text Property

            if (Multiline)
            {
                await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<textarea style=\"{style}\" autocomplete=\"off\" autocorrect=\"off\" autocapitalize=\"off\" spellcheck=\"false\" id=\"{WebviewIdentifier}\">{Text}</textarea>';");
            }
            else
            {
                await Page.Add(Parent.WebviewIdentifier, "innerHTML", $"'<input id=\"{WebviewIdentifier}\" style=\"{style}\" class=\"textbox\" type=\"text\" value=\"{Text}\" autocomplete=\"off\" autocorrect=\"off\" autocapitalize=\"off\" spellcheck=\"false\">';");
            }

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
