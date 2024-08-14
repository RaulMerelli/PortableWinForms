using App1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class Label : Control
    {
        public bool AutoSize;
        public bool UseCompatibleTextRendering = true;
        internal string text = "";
        public string Text
        {
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
                        Page.Set(identifier, "innerHTML", $"\"{value}\"");
                    }
                }
            }
        }

        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            identifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += $"document.getElementById('{identifier}').addEventListener('click', function() {{ eventHandler('{identifier}', 'Click');}});​";

            await Page.Add(Parent.identifier, "innerHTML", $"'<div id=\"{identifier}\" style=\"{style}\" class=\"label\">{Text}</div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
