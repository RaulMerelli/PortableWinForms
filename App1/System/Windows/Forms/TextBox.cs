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
                        Page.Set(identifier, Multiline ? "innerHTML" : "value", $"\"{value}\"");
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

            identifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += $"document.getElementById('{identifier}').addEventListener('click', function() {{ eventHandler('{identifier}', 'Click');}});​";
            script += $"document.getElementById('{identifier}').addEventListener('input', function() {{ eventHandler('{identifier}', 'Input');}});";
            script += $"document.getElementById('{identifier}').addEventListener('mouseenter', function() {{ eventHandler('{identifier}', 'MouseEnter');}});​";
            script += $"document.getElementById('{identifier}').addEventListener('mouseleave', function() {{ eventHandler('{identifier}', 'MouseLeave');}});​";

            if (Multiline)
            {
                await Page.Add(Parent.identifier, "innerHTML", $"'<textarea style=\"{style}\" autocomplete=\"off\" autocorrect=\"off\" autocapitalize=\"off\" spellcheck=\"false\" id=\"{identifier}\">{Text}</textarea>';");
            }
            else
            {
                await Page.Add(Parent.identifier, "innerHTML", $"'<input id=\"{identifier}\" style=\"{style}\" class=\"textbox\" type=\"text\" value=\"{Text}\" autocomplete=\"off\" autocorrect=\"off\" autocapitalize=\"off\" spellcheck=\"false\">';");
            }

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
