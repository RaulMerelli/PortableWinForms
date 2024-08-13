using App1;

namespace System.Windows.Forms
{
    public class CheckBox : ButtonBase
    {
        public bool Checked = false;

        public async override void PerformLayout()
        {
            string style = "";
            string script = "";

            identifier += Name;
            // Da fare: Prima rimuovere se con lo stesso id

            style += $"font: 8pt Microsoft Sans Serif;";
            style += $"color: {System.Drawing.ColorTranslator.ToHtml(ForeColor)};";
            if (((Anchor & AnchorStyles.Top) == AnchorStyles.Top) && ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
            {
                style += $"top: {Location.Y}px;";
                if (Parent.Size.Height == 0) // Temporaneo: da sostituire con controllo di tipo se form. un oggetto child può legittimamente avere dimensione 0
                {
                    style += $"bottom: {(Parent as Form).ClientSize.Height - Location.Y - Size.Height}px;";
                }
                else
                {
                    style += $"bottom: {Parent.Size.Height - Location.Y - Size.Height}px;";
                }
                style += $"height: auto;";
            }
            else if ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                if (Parent.Size.Height == 0) // Temporaneo: da sostituire con controllo di tipo se form. un oggetto child può legittimamente avere dimensione 0
                {
                    style += $"bottom: {(Parent as Form).ClientSize.Height - Location.Y - Size.Height}px;";
                }
                else
                {
                    style += $"bottom: {Parent.Size.Height - Location.Y - Size.Height}px;";
                }
                style += $"height: {Size.Height}px;";
            }
            else
            {
                style += $"top: {Location.Y}px;";
                style += $"height: {Size.Height}px;";
            }

            if (((Anchor & AnchorStyles.Left) == AnchorStyles.Left) && ((Anchor & AnchorStyles.Right) == AnchorStyles.Right))
            {
                style += $"left: {Location.X}px;";
                if (Parent.Size.Width == 0) // da sostituire con controllo di tipo se form
                {
                    style += $"right: {(Parent as Form).ClientSize.Width - Location.X - Size.Width}px;";
                }
                else
                {
                    style += $"right: {Parent.Size.Width - Location.X - Size.Width}px;";
                }
                style += $"width: auto;";
            }
            else if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                if (Parent.Size.Width == 0) // da sostituire con controllo di tipo se form
                {
                    style += $"right: {(Parent as Form).ClientSize.Width - Location.X - Size.Width}px;";
                }
                else
                {
                    style += $"right: {Parent.Size.Width - Location.X - Size.Width}px;";
                }
                style += $"width: {Size.Width}px;";
            }
            else
            {
                style += $"left: {Location.X}px;";
                style += $"width: {Size.Width}px;";
            }
            script += $"document.getElementById('{identifier}').addEventListener('click', function() {{ eventHandler('{identifier}', 'Click');}});​";

            await Page.Add(Parent.identifier, "innerHTML", $"'<div class=\"checkbox\" style=\"{style}\" id=\"{identifier}\"><input type=\"checkbox\" id=\"{identifier}-input\" name=\"{Parent.identifier}-option\" value=\"test\"><label for=\"{identifier}-input\">{Text}</label></div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
