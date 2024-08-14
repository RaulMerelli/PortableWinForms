using App1;

namespace System.Windows.Forms
{
    public class Button : ButtonBase
    {
        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            identifier += Name;
                        
            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            style += CssLocationAndSize();

            script += $"document.getElementById('{identifier}').addEventListener('click', function() {{ eventHandler('{identifier}', 'Click');}});​";

            await Page.Add(Parent.identifier, "innerHTML", $"'<button id=\"{identifier}\" class=\"button\" style=\"{style}\" ><p>{Text}</p></button>';");

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }

        protected internal override void OnClick(EventArgs e)
        {
            /*
            Form form = FindFormInternal();
            if (form != null)
            {
                form.DialogResult = dialogResult;
            }

            AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
            */
            base.OnClick(e);
        }
    }
}
