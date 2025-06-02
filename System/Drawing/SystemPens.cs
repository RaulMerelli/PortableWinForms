namespace System.Drawing
{
    using System.Diagnostics;
    using System;
    using System.Runtime.Versioning;

    public sealed class SystemPens
    {
        static readonly object SystemPensKey = new object();

        private SystemPens()
        {
        }

        public static Pen ActiveBorder
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveBorder);
            }
        }

        public static Pen ActiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveCaption);
            }
        }

        public static Pen ActiveCaptionText
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveCaptionText);
            }
        }

        public static Pen AppWorkspace
        {
            get
            {
                return FromSystemColor(SystemColors.AppWorkspace);
            }
        }

        public static Pen ButtonFace
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonFace);
            }
        }

        public static Pen ButtonHighlight
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonHighlight);
            }
        }

        public static Pen ButtonShadow
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonShadow);
            }
        }

        public static Pen Control
        {
            get
            {
                return FromSystemColor(SystemColors.Control);
            }
        }

        public static Pen ControlText
        {
            get
            {
                return FromSystemColor(SystemColors.ControlText);
            }
        }

        public static Pen ControlDark
        {
            get
            {
                return FromSystemColor(SystemColors.ControlDark);
            }
        }

        public static Pen ControlDarkDark
        {
            get
            {
                return FromSystemColor(SystemColors.ControlDarkDark);
            }
        }

        public static Pen ControlLight
        {
            get
            {
                return FromSystemColor(SystemColors.ControlLight);
            }
        }

        public static Pen ControlLightLight
        {
            get
            {
                return FromSystemColor(SystemColors.ControlLightLight);
            }
        }

        public static Pen Desktop
        {
            get
            {
                return FromSystemColor(SystemColors.Desktop);
            }
        }

        public static Pen GradientActiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.GradientActiveCaption);
            }
        }

        public static Pen GradientInactiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.GradientInactiveCaption);
            }
        }

        public static Pen GrayText
        {
            get
            {
                return FromSystemColor(SystemColors.GrayText);
            }
        }

        public static Pen Highlight
        {
            get
            {
                return FromSystemColor(SystemColors.Highlight);
            }
        }

        public static Pen HighlightText
        {
            get
            {
                return FromSystemColor(SystemColors.HighlightText);
            }
        }

        public static Pen HotTrack
        {
            get
            {
                return FromSystemColor(SystemColors.HotTrack);
            }
        }

        public static Pen InactiveBorder
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveBorder);
            }
        }

        public static Pen InactiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveCaption);
            }
        }

        public static Pen InactiveCaptionText
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveCaptionText);
            }
        }

        public static Pen Info
        {
            get
            {
                return FromSystemColor(SystemColors.Info);
            }
        }

        public static Pen InfoText
        {
            get
            {
                return FromSystemColor(SystemColors.InfoText);
            }
        }

        public static Pen Menu
        {
            get
            {
                return FromSystemColor(SystemColors.Menu);
            }
        }

        public static Pen MenuBar
        {
            get
            {
                return FromSystemColor(SystemColors.MenuBar);
            }
        }

        public static Pen MenuHighlight
        {
            get
            {
                return FromSystemColor(SystemColors.MenuHighlight);
            }
        }

        public static Pen MenuText
        {
            get
            {
                return FromSystemColor(SystemColors.MenuText);
            }
        }

        public static Pen ScrollBar
        {
            get
            {
                return FromSystemColor(SystemColors.ScrollBar);
            }
        }

        public static Pen Window
        {
            get
            {
                return FromSystemColor(SystemColors.Window);
            }
        }

        public static Pen WindowFrame
        {
            get
            {
                return FromSystemColor(SystemColors.WindowFrame);
            }
        }

        public static Pen WindowText
        {
            get
            {
                return FromSystemColor(SystemColors.WindowText);
            }
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process | ResourceScope.AppDomain, ResourceScope.Process | ResourceScope.AppDomain)]
        public static Pen FromSystemColor(Color c)
        {
            if (!c.IsSystemColor)
            {
                throw new ArgumentException("ColorNotSystemColor");
            }

            Pen[] systemPens = (Pen[])SafeNativeMethods.Gdip.ThreadData[SystemPensKey];
            if (systemPens == null)
            {
                systemPens = new Pen[(int)KnownColor.WindowText + (int)KnownColor.MenuHighlight - (int)KnownColor.YellowGreen];
                SafeNativeMethods.Gdip.ThreadData[SystemPensKey] = systemPens;
            }

            int idx = (int)c.ToKnownColor();
            if (idx > (int)KnownColor.YellowGreen)
            {
                idx -= (int)KnownColor.YellowGreen - (int)KnownColor.WindowText;
            }
            idx--;
            Debug.Assert(idx >= 0 && idx < systemPens.Length, "System colors have been added but our system color array has not been expanded.");

            if (systemPens[idx] == null)
            {
                systemPens[idx] = new Pen(c, true);
            }

            return systemPens[idx];
        }
    }
}