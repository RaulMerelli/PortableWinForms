//[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA905:SystemAndMicrosoftNamespacesRequireApproval", Scope = "namespace", Target = "System.Windows.Forms.VisualStyles")]

//namespace System.Windows.Forms.VisualStyles
//{

//    using System;
//    using System.Text;
//    using System.Drawing;
//    using System.Windows.Forms;
//    using System.Runtime.InteropServices;
//    using System.Diagnostics.CodeAnalysis;


//    public static class VisualStyleInformation
//    {
//        //Make this per-thread, so that different threads can safely use these methods.
//        [ThreadStatic]
//        private static VisualStyleRenderer visualStyleRenderer = null;

//        public static bool IsSupportedByOS
//        {
//            get
//            {
//                return (OSFeature.Feature.IsPresent(OSFeature.Themes));
//            }
//        }

//        public static bool IsEnabledByUser
//        {
//            get
//            {
//                if (!IsSupportedByOS)
//                {
//                    return false;
//                }

//                return (SafeNativeMethods.IsAppThemed());
//            }
//        }

//        internal static string ThemeFilename
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder filename = new StringBuilder(512);
//                    SafeNativeMethods.GetCurrentThemeName(filename, filename.Capacity, null, 0, null, 0);
//                    return (filename.ToString());
//                }

//                return String.Empty;
//            }
//        }

//        public static string ColorScheme
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder colorScheme = new StringBuilder(512);
//                    SafeNativeMethods.GetCurrentThemeName(null, 0, colorScheme, colorScheme.Capacity, null, 0);
//                    return (colorScheme.ToString());
//                }

//                return String.Empty;
//            }
//        }

//        public static string Size
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder size = new StringBuilder(512);
//                    SafeNativeMethods.GetCurrentThemeName(null, 0, null, 0, size, size.Capacity);
//                    return (size.ToString());
//                }

//                return String.Empty;
//            }
//        }

//        public static string DisplayName
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder name = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.DisplayName, name, name.Capacity);
//                    return name.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        public static string Company
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder company = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Company, company, company.Capacity);
//                    return company.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        public static string Author
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder author = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Author, author, author.Capacity);
//                    return author.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        public static string Copyright
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder copyright = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Copyright, copyright, copyright.Capacity);
//                    return copyright.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
//        public static string Url
//        {
//            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder url = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Url, url, url.Capacity);
//                    return url.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        public static string Version
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder version = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Version, version, version.Capacity);
//                    return version.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        public static string Description
//        {
//            get
//            {
//                if (IsEnabledByUser)
//                {
//                    StringBuilder description = new StringBuilder(512);
//                    SafeNativeMethods.GetThemeDocumentationProperty(ThemeFilename, VisualStyleDocProperty.Description, description, description.Capacity);
//                    return description.ToString();
//                }

//                return String.Empty;
//            }
//        }

//        public static bool SupportsFlatMenus
//        {
//            get
//            {
//                if (Application.RenderWithVisualStyles)
//                {
//                    if (visualStyleRenderer == null)
//                    {
//                        visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
//                    }
//                    else
//                    {
//                        visualStyleRenderer.SetParameters(VisualStyleElement.Window.Caption.Active);
//                    }

//                    return (SafeNativeMethods.GetThemeSysBool(new HandleRef(null, visualStyleRenderer.Handle), VisualStyleSystemProperty.SupportsFlatMenus));
//                }

//                return false;
//            }
//        }

//        public static int MinimumColorDepth
//        {
//            get
//            {
//                if (Application.RenderWithVisualStyles)
//                {
//                    if (visualStyleRenderer == null)
//                    {
//                        visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
//                    }
//                    else
//                    {
//                        visualStyleRenderer.SetParameters(VisualStyleElement.Window.Caption.Active);
//                    }

//                    int mcDepth = 0;

//                    SafeNativeMethods.GetThemeSysInt(new HandleRef(null, visualStyleRenderer.Handle), VisualStyleSystemProperty.MinimumColorDepth, ref mcDepth);
//                    return mcDepth;
//                }

//                return 0;
//            }
//        }

//        public static Color TextControlBorder
//        {
//            get
//            {
//                if (Application.RenderWithVisualStyles)
//                {
//                    if (visualStyleRenderer == null)
//                    {
//                        visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.TextBox.TextEdit.Normal);
//                    }
//                    else
//                    {
//                        visualStyleRenderer.SetParameters(VisualStyleElement.TextBox.TextEdit.Normal);
//                    }
//                    Color borderColor = visualStyleRenderer.GetColor(ColorProperty.BorderColor);
//                    return borderColor;
//                }

//                return SystemColors.WindowFrame;
//            }
//        }


//        public static Color ControlHighlightHot
//        {
//            get
//            {
//                if (Application.RenderWithVisualStyles)
//                {
//                    if (visualStyleRenderer == null)
//                    {
//                        visualStyleRenderer = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);

//                    }
//                    else
//                    {
//                        visualStyleRenderer.SetParameters(VisualStyleElement.Button.PushButton.Normal);
//                    }
//                    Color accentColor = visualStyleRenderer.GetColor(ColorProperty.AccentColorHint);
//                    return accentColor;
//                }

//                return SystemColors.ButtonHighlight;
//            }
//        }
//    }
//}