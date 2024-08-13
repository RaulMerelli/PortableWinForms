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
        internal string _Text = "";
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                if (layoutPerformed)
                {
                    Page.Set(identifier, "innerHTML", $"\"{value}\"");
                }
            }
        }

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

            await Page.Add(Parent.identifier, "innerHTML", $"'<div id=\"{identifier}\" style=\"{style}\" class=\"label\">{Text}</div>';".Replace("\u200B", ""));

            await Page.RunScript(script);

            PerformChildLayout();
            layoutPerformed = true;
        }
    }
}
