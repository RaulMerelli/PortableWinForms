﻿namespace System.Windows.Forms
{
    using System.Configuration.Assemblies;
    using System.Diagnostics;
    using System;
    using System.Security;
    using System.Security.Permissions;

    public class OSFeature : FeatureSupport
    {
        public static readonly object LayeredWindows = new object();

        public static readonly object Themes = new object();

        private static OSFeature feature = null;

        private static bool themeSupportTested = false;
        private static bool themeSupport = false;

        protected OSFeature()
        {
        }

        public static OSFeature Feature
        {
            get
            {
                if (feature == null)
                    feature = new OSFeature();

                return feature;
            }
        }

        public override Version GetVersionPresent(object feature)
        {
            Version featureVersion = null;
            if (feature == LayeredWindows)
            {
                if (Environment.OSVersion.Platform == System.PlatformID.Win32NT
                   && Environment.OSVersion.Version.CompareTo(new Version(5, 0, 0, 0)) >= 0)
                {

                    featureVersion = new Version(0, 0, 0, 0);
                }
            }
            else if (feature == Themes)
            {
                if (!themeSupportTested)
                {
                    try
                    {
                        //SafeNativeMethods.IsAppThemed();
                        throw new Exception();
                        themeSupport = true;
                    }
                    catch
                    {
                        themeSupport = false;
                    }
                    themeSupportTested = true;
                }

                if (themeSupport)
                {
                    featureVersion = new Version(0, 0, 0, 0);
                }
            }
            return featureVersion;
        }

        internal bool OnXp
        {
            get
            {
                bool onXp = false;
                if (Environment.OSVersion.Platform == System.PlatformID.Win32NT)
                {
                    onXp = Environment.OSVersion.Version.CompareTo(new Version(5, 1, 0, 0)) >= 0;
                }
                return onXp;
            }
        }

        internal bool OnWin2k
        {
            get
            {
                bool onWin2k = false;
                if (Environment.OSVersion.Platform == System.PlatformID.Win32NT)
                {
                    onWin2k = Environment.OSVersion.Version.CompareTo(new Version(5, 0, 0, 0)) >= 0;
                }
                return onWin2k;
            }
        }

        public static bool IsPresent(SystemParameter enumVal)
        {
            switch (enumVal)
            {
                case SystemParameter.DropShadow:
                    return Feature.OnXp;


                case SystemParameter.FlatMenu:
                    return Feature.OnXp;


                case SystemParameter.FontSmoothingContrastMetric:
                    return Feature.OnXp;


                case SystemParameter.FontSmoothingTypeMetric:
                    return Feature.OnXp;


                case SystemParameter.MenuFadeEnabled:
                    return Feature.OnWin2k;


                case SystemParameter.SelectionFade:
                    return Feature.OnWin2k;


                case SystemParameter.ToolTipAnimationMetric:
                    return Feature.OnWin2k;


                case SystemParameter.UIEffects:
                    return Feature.OnWin2k;


                case SystemParameter.CaretWidthMetric:
                    return Feature.OnWin2k;


                case SystemParameter.VerticalFocusThicknessMetric:
                    return Feature.OnXp;


                case SystemParameter.HorizontalFocusThicknessMetric:
                    return Feature.OnXp;
            }
            return false;
        }

    }
}