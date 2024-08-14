using System.Drawing;

namespace System.Windows.Forms
{
    internal class LinkUtilities
    {
        // IE fonts and colors 
        static Color ielinkColor = Color.Empty;
        static Color ieactiveLinkColor = Color.Empty;
        static Color ievisitedLinkColor = Color.Empty;

        const string IEAnchorColor = "Anchor Color";
        const string IEAnchorColorVisited = "Anchor Color Visited";
        const string IEAnchorColorHover = "Anchor Color Hover";

        private static Color GetIEColor(string name)
        {
            try
            {
                switch (name)
                {
                    case "Anchor Color":
                        return Color.FromArgb(0, 0, 255);
                    case "Anchor Color Visited":
                        return Color.FromArgb(128, 0, 128);
                    case "Background Color":
                        return Color.FromArgb(192, 192, 192);
                    case "Text Color":
                        return Color.FromArgb(0, 0, 0);
                }
                if (string.Equals(name, IEAnchorColor, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.Blue;
                }
                else if (string.Equals(name, IEAnchorColorVisited, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.Purple;
                }
                else if (string.Equals(name, IEAnchorColorHover, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.Red;
                }
                else
                {
                    return Color.Red;
                }
            }
            finally
            {
                //System.Security.CodeAccessPermission.RevertAssert();
            }

        }

        public static Color IELinkColor
        {
            get
            {
                if (ielinkColor.IsEmpty)
                {
                    ielinkColor = GetIEColor(IEAnchorColor);
                }
                return ielinkColor;
            }
        }

        public static Color IEActiveLinkColor
        {
            get
            {
                if (ieactiveLinkColor.IsEmpty)
                {
                    ieactiveLinkColor = GetIEColor(IEAnchorColorHover);
                }
                return ieactiveLinkColor;
            }
        }
        public static Color IEVisitedLinkColor
        {
            get
            {
                if (ievisitedLinkColor.IsEmpty)
                {
                    ievisitedLinkColor = GetIEColor(IEAnchorColorVisited);
                }
                return ievisitedLinkColor;
            }
        }

        public static LinkBehavior GetIELinkBehavior()
        {
            return LinkBehavior.AlwaysUnderline;
        }

        public static void EnsureLinkFonts(Font baseFont, LinkBehavior link, ref Font linkFont, ref Font hoverLinkFont)
        {
            if (linkFont != null && hoverLinkFont != null)
            {
                return;
            }

            bool underlineLink = true;
            bool underlineHover = true;

            if (link == LinkBehavior.SystemDefault)
            {
                link = GetIELinkBehavior();
            }

            switch (link)
            {
                case LinkBehavior.AlwaysUnderline:
                    underlineLink = true;
                    underlineHover = true;
                    break;
                case LinkBehavior.HoverUnderline:
                    underlineLink = false;
                    underlineHover = true;
                    break;
                case LinkBehavior.NeverUnderline:
                    underlineLink = false;
                    underlineHover = false;
                    break;
            }

            Font f = baseFont;

            // We optimize for the "same" value (never & always) to avoid creating an 
            // extra font object.
            //
            if (underlineHover == underlineLink)
            {
                FontStyle style = f.Style;
                if (underlineHover)
                {
                    style |= FontStyle.Underline;
                }
                else
                {
                    style &= ~FontStyle.Underline;
                }
                hoverLinkFont = new Font(f, style);
                linkFont = hoverLinkFont;
            }
            else
            {
                FontStyle hoverStyle = f.Style;
                if (underlineHover)
                {
                    hoverStyle |= FontStyle.Underline;
                }
                else
                {
                    hoverStyle &= ~FontStyle.Underline;
                }

                hoverLinkFont = new Font(f, hoverStyle);

                FontStyle linkStyle = f.Style;
                if (underlineLink)
                {
                    linkStyle |= FontStyle.Underline;
                }
                else
                {
                    linkStyle &= ~FontStyle.Underline;
                }

                linkFont = new Font(f, linkStyle);
            }
        }
    }
}
